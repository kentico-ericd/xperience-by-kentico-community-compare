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

        properties.SourcePageData = await comparableDataRetriever.GetSourceWebPageData(
            WebPageIdentifier.WebPageItemID,
            WebPageIdentifier.LanguageName,
            ApplicationIdentifier.WebsiteChannelID);

        return properties;
    }


    [PageCommand]
    public async Task<ICommandResponse<CompareResult>> Compare(CompareRequest request) =>
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
