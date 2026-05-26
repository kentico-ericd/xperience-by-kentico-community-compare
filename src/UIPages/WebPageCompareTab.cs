using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
using CMS.ContentWorkflowEngine;
using CMS.ContentWorkflowEngine.Internal;
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
    ICoveringWorkflowRetriever coveringWorkflowRetriever,
    IInfoProvider<ContentLanguageInfo> contentLanguageInfoProvider,
    IInfoProvider<ContentWorkflowStepInfo> contentWorkflowStepInfoProvider,
    IInfoProvider<ChannelInfo> channelInfoProvider,
    IInfoProvider<WebsiteChannelInfo> websiteChannelInfoProvider,
    IComparableDataRetriever comparableDataRetriever,
    IAuthenticatedUserAccessor authenticatedUserAccessor,
    IWebPageManagerFactory webPageManagerFactory,
    IPageLinkGenerator pageLinkGenerator) : WebPageBase<WebPageCompareTabProperties>(
            authenticatedUserAccessor,
            webPageManagerFactory,
            pageLinkGenerator)
{
    public const string SLUG = "compare";


    public override async Task<WebPageCompareTabProperties> ConfigureTemplateProperties(WebPageCompareTabProperties properties)
    {
        await base.ConfigureTemplateProperties(properties);

        properties.PreventRefetch = true;

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

        properties.SourcePageData = await GetSourceWebPageData(
            WebPageIdentifier.WebPageItemID,
            WebPageIdentifier.LanguageName,
            ApplicationIdentifier.WebsiteChannelID);

        return properties;
    }


    [PageCommand]
    public async Task<ICommandResponse<ComparableWebPageData>> Compare(CompareRequest request) =>
        ResponseFrom(await comparableDataRetriever.GetWebPageCompareResult(request));


    private void RedirectTo(Type targetPage, WebPageCompareTabProperties properties)
    {
        var parameters = new PageParameterValues()
        {
            { typeof(WebPagesApplication), ApplicationIdentifier.Slug },
            { typeof(WebPageLayout), WebPageIdentifier.ToString() }
        };
        properties.RedirectUrl = pageLinkGenerator.GetPath(targetPage, parameters);
    }


    public async Task<SourceWebPageData> GetSourceWebPageData(int webPageId, string languageName, int websiteChannelId)
    {
        var data = new SourceWebPageData
        {
            WebPageID = webPageId,
            LanguageName = languageName
        };

        // Get website channel
        int channelId = (await websiteChannelInfoProvider.GetAsync(websiteChannelId))?.WebsiteChannelChannelID
            ?? throw new InvalidOperationException($"Website channel '({websiteChannelId})' not found.");
        data.ChannelName = (await channelInfoProvider.GetAsync(channelId))?.ChannelName
            ?? throw new InvalidOperationException($"Channel '({channelId})' not found.");

        // Get languages
        var languages = await contentLanguageInfoProvider.Get().GetEnumerableTypedResultAsync();
        var webPageLanguage = languages.FirstOrDefault(l => l.ContentLanguageName.Equals(languageName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Language '({languageName})' not found.");
        data.Languages = languages.Select(l =>
            new ContentLanguage(l.ContentLanguageName, l.ContentLanguageDisplayName, l.ContentLanguageFlagIconName));

        // Get basic web page data
        var query = GetWebPageDataQuery(webPageId, websiteChannelId, webPageLanguage.ContentLanguageID);
        var dataContainer = (await query.GetDataContainerResultAsync()).FirstOrDefault()
            ?? throw new InvalidOperationException($"Failed to retrieve metadata info for web page {webPageId}.");
        data.VersionStatus = ValidationHelper.GetInteger(
            dataContainer.GetValue(nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataLatestVersionStatus)),
            0);
        data.ContentTypeClassID = ValidationHelper.GetInteger(
            dataContainer.GetValue(nameof(ContentItemInfo.ContentItemContentTypeID)),
            0);

        return data;
    }


    //TODO: Limit columns of query
    private static DataQuery GetWebPageDataQuery(int webPageId, int websiteChannelId, int languageId) =>
        new DataQuery()
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
            .WhereEquals(nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataContentLanguageID), languageId);
}


/// <summary>
/// Extends web page side navigation tabs. Disables the "compare" tab if the "content" tab is disabled or not present.
/// </summary>
internal class WebPageLayoutExtender : PageExtender<WebPageLayout>
{
    public override async Task<TemplateClientProperties> ConfigureTemplateProperties(TemplateClientProperties properties)
    {
        await base.ConfigureTemplateProperties(properties);

        var myTab = properties.Navigation.Items
            .FirstOrDefault(i => i.Path.Equals(WebPageCompareTab.SLUG, StringComparison.OrdinalIgnoreCase));
        if (myTab is null)
        {
            return properties;
        }

        var contentTab = properties.Navigation.Items
            .FirstOrDefault(i => i.Path.Equals("content", StringComparison.OrdinalIgnoreCase));
        if (contentTab is null || (contentTab is not null && contentTab.Disabled))
        {
            myTab.Disabled = true;
        }

        return properties;
    }
}
