using CMS.Core;
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
    "@xperiencecommunity/compare/ContentItemCompareTab",
    999,
    Icon = Icons.DocCopy)]
[assembly: PageExtender(typeof(WebPageLayoutExtender))]
namespace XperienceCommunity.Compare.UIPages;

/// <summary>
/// Template for the web page "Compare" tab.
/// </summary>
public class WebPageCompareTab(
    ICompareHelper compareHelper,
    IEventLogService eventLogService,
    IComparableDataRetriever comparableDataRetriever,
    IAuthenticatedUserAccessor authenticatedUserAccessor,
    IWebPageManagerFactory webPageManagerFactory,
    IPageLinkGenerator pageLinkGenerator) : WebPageBase<ContentItemComparisonProperties>(
            authenticatedUserAccessor,
            webPageManagerFactory,
            pageLinkGenerator)
{
    public const string SLUG = "compare";
    private readonly IPageLinkGenerator pageLinkGenerator = pageLinkGenerator;


    public override async Task<ContentItemComparisonProperties> ConfigureTemplateProperties(ContentItemComparisonProperties properties)
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

        await CheckAclPermission(WebPageIdentifier.WebPageItemID, WebPageAclPermissions.READ);
        await SetProperties(properties);

        return properties;
    }


    /// <summary>
    /// Submits a request to compare two versions of a content item and returns the result. If an exception occurs, an empty result is
    /// constructed passing the error message within <see cref="ComparableContentItemData.ErrorMessage"/>.
    /// </summary>
    [PageCommand]
    public async Task<ICommandResponse<ComparableContentItemData>> Compare(ContentItemCompareRequest request, CancellationToken ct)
    {
        try
        {
            var result = await comparableDataRetriever.GetContentItemCompareResultAsync(request, ct);

            return ResponseFrom(result);
        }
        catch (Exception ex)
        {
            eventLogService.LogException(nameof(WebPageCompareTab), nameof(Compare), ex);

            return ResponseFrom(new ComparableContentItemData { ErrorMessage = ex.Message });
        }
    }


    private void RedirectTo(Type targetPage, ContentItemComparisonProperties properties)
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
    private async Task SetProperties(ContentItemComparisonProperties properties)
    {
        properties.PreventRefetch = true;

        // Get languages
        properties.Languages = await compareHelper.GetContentLanguagesAsync(CancellationToken.None);
        var sourceLanguage = properties.Languages.FirstOrDefault(l =>
            l.LanguageName?.Equals(WebPageIdentifier.LanguageName, StringComparison.OrdinalIgnoreCase) == true)
            ?? throw new InvalidOperationException($"Failed to retrieve language info for web page {ApplicationIdentifier.WebsiteChannelID}.");

        // Get basic web page data
        var (contentItemId, contentTypeId, versionStatus) = await compareHelper.GetWebPageDataAsync(
            WebPageIdentifier.WebPageItemID,
            ApplicationIdentifier.WebsiteChannelID,
            sourceLanguage.LanguageID,
            CancellationToken.None)
            ?? throw new InvalidOperationException($"Failed to retrieve metadata info for web page {ApplicationIdentifier.WebsiteChannelID}.");
        properties.ContentTypeClassID = contentTypeId;
        properties.ContentItemID = contentItemId;

        // Get all existing versions of content item
        var contentItemVariants = await compareHelper.GetContentItemVariantsAsync(properties.ContentItemID, properties.Languages, CancellationToken.None);
        properties.SourceContentItem = contentItemVariants.Find(c =>
            c.Language?.LanguageName == sourceLanguage.LanguageName && (int)c.VersionStatus == versionStatus)
            ?? throw new InvalidOperationException($"Failed to retrieve source content item variant for web page {ApplicationIdentifier.WebsiteChannelID}.");
        // Remove source version from compare targets
        contentItemVariants.Remove(properties.SourceContentItem);
        properties.CompareTargets = contentItemVariants;
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
