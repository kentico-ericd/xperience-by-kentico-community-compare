using CMS.Base;
using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Websites;
using CMS.Websites.Internal;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Authentication;
using Kentico.Xperience.Admin.Websites.UIPages;

using XperienceCommunity.Compare.Models;
using XperienceCommunity.Compare.Services;
using XperienceCommunity.Compare.UIPages;

[assembly: UIPage(
    typeof(WebPageLayout),
    WebPageCompareTab.SLUG,
    typeof(WebPageCompareTab),
    "Compare",
    "@xperiencecommunity/compare/WebPageCompareTab",
    999,
    Icon = Icons.DocCopy)]
[assembly: PageExtender(typeof(WebPageLayoutExtender))]
namespace XperienceCommunity.Compare.UIPages;

/// <summary>
/// Template for the web page "Compare" tab.
/// </summary>
public class WebPageCompareTab(
    IEventLogService eventLogService,
    IProgressiveCache progressiveCache,
    IComparableDataRetriever comparableDataRetriever,
    IInfoProvider<ChannelInfo> channelInfoProvider,
    IInfoProvider<WebsiteChannelInfo> websiteChannelInfoProvider,
    IInfoProvider<ContentLanguageInfo> contentLanguageInfoProvider,
    IInfoProvider<ContentItemCommonDataInfo> contentItemCommonDataInfoProvider,
    IAuthenticatedUserAccessor authenticatedUserAccessor,
    IWebPageManagerFactory webPageManagerFactory,
    IPageLinkGenerator pageLinkGenerator) : WebPageBase<WebPageCompareTabProperties>(
            authenticatedUserAccessor,
            webPageManagerFactory,
            pageLinkGenerator)
{
    public const string SLUG = "compare";
    private readonly IPageLinkGenerator pageLinkGenerator = pageLinkGenerator;


    public override async Task<WebPageCompareTabProperties> ConfigureTemplateProperties(WebPageCompareTabProperties properties)
    {
        await base.ConfigureTemplateProperties(properties);

        if (WebPageIdentifier.WebPageItemID == WebPageConstants.ROOT_NODE_ID)
        {
            // If on root, redirect to General tab
            RedirectTo(typeof(RootGeneralTab), properties);
        }

        if (properties.WebPageState.VersionStatus == ContentItemVersionStatus.NotTranslated)
        {
            // If page is not translated, redirect to translate page
            RedirectTo(typeof(CreateLanguageVariant), properties);
        }

        await SetProperties(properties);

        return properties;
    }


    /// <summary>
    /// Submits a request to compare two versions of a web page and returns the result. If an exception occurs, an empty result is
    /// constructed passing the error message within <see cref="ComparableWebPageData.ErrorMessage"/>.
    /// </summary>
    [PageCommand]
    public async Task<ICommandResponse<ComparableWebPageData>> Compare(CompareRequest request, CancellationToken ct)
    {
        try
        {
            var result = await comparableDataRetriever.GetWebPageCompareResult(request, ct);

            return ResponseFrom(result);
        }
        catch (Exception ex)
        {
            eventLogService.LogException(nameof(WebPageCompareTab), nameof(Compare), ex);

            return ResponseFrom(new ComparableWebPageData { ErrorMessage = ex.Message });
        }
    }


    private void RedirectTo(Type targetPage, WebPageCompareTabProperties properties)
    {
        var parameters = new PageParameterValues()
        {
            { typeof(WebPagesApplication), ApplicationIdentifier.Slug },
            { typeof(WebPageLayout), WebPageIdentifier.ToString() }
        };
        properties.RedirectUrl = pageLinkGenerator.GetPath(targetPage, parameters);
    }


    /// <summary>
    /// Initializes and sets various properties for a web page comparison tab, including identifiers, language, and channel.
    /// </summary>
    private async Task SetProperties(WebPageCompareTabProperties properties)
    {
        properties.PreventRefetch = true;

        // Get website channel
        int channelId = (await websiteChannelInfoProvider.GetAsync(ApplicationIdentifier.WebsiteChannelID))?.WebsiteChannelChannelID
            ?? throw new InvalidOperationException($"Website channel '({ApplicationIdentifier.WebsiteChannelID})' not found.");
        properties.WebsiteChannelName = (await channelInfoProvider.GetAsync(channelId))?.ChannelName
            ?? throw new InvalidOperationException($"Channel '({channelId})' not found.");

        // Get languages
        properties.Languages = (await contentLanguageInfoProvider.Get()
            .GetEnumerableTypedResultAsync())
            .Select(l =>
                new ContentLanguage(
                    l.ContentLanguageID,
                    l.ContentLanguageName,
                    l.ContentLanguageDisplayName,
                    l.ContentLanguageFlagIconName));
        properties.SourceLanguage = properties.Languages.FirstOrDefault(l =>
            l.LanguageName.Equals(WebPageIdentifier.LanguageName, StringComparison.OrdinalIgnoreCase));

        // Get basic web page data
        var dataContainer = await GetWebPageData(
            WebPageIdentifier.WebPageItemID,
            ApplicationIdentifier.WebsiteChannelID,
            properties.SourceLanguage.LanguageID) ??
            throw new InvalidOperationException($"Failed to retrieve metadata info for web page {ApplicationIdentifier.WebsiteChannelID}.");
        properties.SourceVersionStatus = ValidationHelper.GetInteger(
            dataContainer.GetValue(nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataLatestVersionStatus)),
            0);
        properties.ContentTypeClassID = ValidationHelper.GetInteger(
            dataContainer.GetValue(nameof(ContentItemInfo.ContentItemContentTypeID)),
            0);
        properties.ContentItemID = ValidationHelper.GetInteger(dataContainer.GetValue(nameof(ContentItemInfo.ContentItemID)), 0);

        // Get all existing versions of content item
        var contentItemVariants = (await contentItemCommonDataInfoProvider.Get()
            .WhereEquals(nameof(ContentItemCommonDataInfo.ContentItemCommonDataContentItemID), properties.ContentItemID)
            .Columns(
                nameof(ContentItemCommonDataInfo.ContentItemCommonDataContentLanguageID),
                nameof(ContentItemCommonDataInfo.ContentItemCommonDataVersionStatus))
            .GetEnumerableTypedResultAsync())
            .Select(c =>
            {
                var matchingLanguage = properties.Languages.FirstOrDefault(l =>
                    l.LanguageID == c.ContentItemCommonDataContentLanguageID);

                return new CompareTarget(matchingLanguage.LanguageName, c.ContentItemCommonDataVersionStatus);
            })
            .ToList();
        // Remove source version from compare targets
        contentItemVariants.RemoveAll(v =>
                v.LanguageName == properties.SourceLanguage.LanguageName &&
                (int)v.VersionStatus == properties.SourceVersionStatus);
        properties.CompareTargets = contentItemVariants;

    }


    /// <summary>
    /// Gets the necessary data for the web page to be compared, such as the content type and version status.
    /// This data is used to determine which fields are comparable and to validate the compare request before processing.
    /// </summary>
    private Task<IDataContainer?> GetWebPageData(int webPageId, int websiteChannelId, int languageId)
    {
        var query = new DataQuery()
            .From(new QuerySource(new QuerySourceTable(ContentItemLanguageMetadataInfo.TYPEINFO.ClassStructureInfo.TableName)))
            .Source(source => source
                .LeftJoin<ContentItemInfo>(
                    nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataContentItemID),
                    nameof(ContentItemInfo.ContentItemID))
                .InnerJoin<WebPageItemInfo>(
                    $"{ContentItemInfo.TYPEINFO.ClassStructureInfo.TableName}.{nameof(ContentItemInfo.ContentItemID)}",
                    nameof(WebPageItemInfo.WebPageItemContentItemID), new WhereCondition().WhereEquals(nameof(WebPageItemInfo.WebPageItemWebsiteChannelID), websiteChannelId))
            )
            .WhereEquals(nameof(WebPageItemInfo.WebPageItemID), webPageId)
            .WhereEquals(nameof(WebPageItemInfo.WebPageItemWebsiteChannelID), websiteChannelId)
            .WhereEquals(nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataContentLanguageID), languageId)
            .Columns(
                nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataLatestVersionStatus),
                nameof(ContentItemInfo.ContentItemContentTypeID),
                nameof(ContentItemInfo.ContentItemID));

        return progressiveCache.LoadAsync(
            async (cs) => (await query.GetDataContainerResultAsync()).FirstOrDefault(),
            new CacheSettings(
                60,
                $"{nameof(WebPageCompareTab)}|{nameof(GetWebPageData)}|{webPageId}|{websiteChannelId}|{languageId}"));
    }
}


/// <summary>
/// Extends web page side navigation tabs. Disables the "compare" tab if the "content" tab is disabled or not present.
/// </summary>
internal class WebPageLayoutExtender : PageExtender<WebPageLayout>
{
    public override async Task<TemplateClientProperties> ConfigureTemplateProperties(TemplateClientProperties properties)
    {
        await base.ConfigureTemplateProperties(properties);

        var compareTab = properties.Navigation.Items
            .FirstOrDefault(i => i.Path.Equals(WebPageCompareTab.SLUG, StringComparison.OrdinalIgnoreCase));
        if (compareTab is null)
        {
            return properties;
        }

        var contentTab = properties.Navigation.Items
            .FirstOrDefault(i => i.Path.Equals("content", StringComparison.OrdinalIgnoreCase));
        if (contentTab is null || contentTab.Disabled)
        {
            compareTab.Disabled = true;
        }

        return properties;
    }
}
