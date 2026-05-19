using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Websites.UIPages;

using XperienceCommunity.Compare.UIPages;

[assembly: UIPage(
    typeof(WebPageLayout),
    "compare",
    typeof(WebPageCompareTab),
    "Compare",
    "@xperiencecommunity/compare/CompareTab",
    999,
    Icon = Icons.DocCopy)]
namespace XperienceCommunity.Compare.UIPages;

public class WebPageCompareTab : Page<WebPageCompareTabProperties>
{
    public override async Task<WebPageCompareTabProperties> ConfigureTemplateProperties(WebPageCompareTabProperties properties)
    {
        properties.PreventRefetch = true;

        return properties;
    }


    [PageCommand]
    public async Task<ICommandResponse<CompareResult>> Compare(string input)
    {
        // Here is where we'll compare
        return ResponseFrom(new CompareResult { Result = input.ToUpper() });
    }
}


public readonly record struct CompareResult(string Result);


public class WebPageCompareTabProperties : WebPageBaseClientProperties
{
}
