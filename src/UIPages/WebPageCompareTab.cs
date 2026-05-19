using CMS.Websites;
using CMS.Websites.Internal;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Authentication;
using Kentico.Xperience.Admin.Websites.UIPages;

using XperienceCommunity.Compare.UIPages;

[assembly: UIPage(
    typeof(WebPageLayout),
    WebPageCompareTab.SLUG,
    typeof(WebPageCompareTab),
    "Compare",
    "@xperiencecommunity/compare/CompareTab",
    999,
    Icon = Icons.DocCopy)]
[assembly: PageExtender(typeof(WebPageLayoutExtender))]
namespace XperienceCommunity.Compare.UIPages;

/// <summary>
/// Template for the web page "Compare" tab.
/// </summary>
public class WebPageCompareTab(
        IAuthenticatedUserAccessor authenticatedUserAccessor,
        IPageLinkGenerator pageLinkGenerator,
        IWebPageManagerFactory webPageManagerFactory) : WebPageBase<WebPageCompareTabProperties>(
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

        return properties;
    }


    [PageCommand]
    public async Task<ICommandResponse<CompareResult>> Compare(string input)
    {
        // Here is where we'll compare
        return ResponseFrom(new CompareResult { Result = input.ToUpper() });
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
}


public readonly record struct CompareResult(string Result);


public class WebPageCompareTabProperties : WebPageBaseClientProperties
{
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
