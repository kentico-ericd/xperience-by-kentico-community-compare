using Kentico.Xperience.Admin.Websites.UIPages;

namespace XperienceCommunity.Compare.Models;

public class WebPageCompareTabProperties : WebPageBaseClientProperties
{
    public int WebPageID { get; set; }


    public int ContentTypeClassID { get; set; }


    public string? SourceLanguageName { get; set; }


    public int SourceVersionStatus { get; set; }


    public IEnumerable<ContentLanguage> Languages { get; set; } = [];
}

public readonly record struct ContentLanguage(string LanguageName, string LanguageDisplayName, string FlagName);
