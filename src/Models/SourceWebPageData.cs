namespace XperienceCommunity.Compare.Models;

public class SourceWebPageData
{
    public int WebPageID { get; set; }


    public int ContentTypeClassID { get; set; }


    public string? ChannelName { get; set; }


    public string? LanguageName { get; set; }


    public int VersionStatus { get; set; }


    public IEnumerable<ContentLanguage> Languages { get; set; } = [];
}


public readonly record struct ContentLanguage(string LanguageName, string LanguageDisplayName, string FlagName);
