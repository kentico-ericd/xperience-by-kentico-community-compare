using CMS.ContentEngine;

using Kentico.Xperience.Admin.Websites.UIPages;

using XperienceCommunity.Compare.UIPages;

namespace XperienceCommunity.Compare.Models;

/// <summary>
/// The properties of the <see cref="WebPageCompareTab"/>.
/// </summary>
public class WebPageCompareTabProperties : WebPageBaseClientProperties
{
    /// <summary>
    /// The source web page's content item ID.
    /// </summary>
    public int ContentItemID { get; set; }


    /// <summary>
    /// The source web page's website channel name.
    /// </summary>
    public string? WebsiteChannelName { get; set; }


    /// <summary>
    /// The source web page's content type class ID.
    /// </summary>
    public int ContentTypeClassID { get; set; }


    /// <summary>
    /// The source web page's language.
    /// </summary>
    public ContentLanguage SourceLanguage { get; set; }


    /// <summary>
    /// The source web page's version status.
    /// </summary>
    public int SourceVersionStatus { get; set; }


    /// <summary>
    /// A collection of all languages registered in the system.
    /// </summary>
    public IEnumerable<ContentLanguage> Languages { get; set; } = [];


    /// <summary>
    /// A collection of valid comparison targets for the source web page.
    /// </summary>
    public IEnumerable<CompareTarget> CompareTargets { get; set; } = [];
}

/// <summary>
/// Represents a simplified <see cref="ContentLanguageInfo"/> object.
/// </summary>
/// <param name="LanguageID">The ID of the language.</param>
/// <param name="LanguageName">The name of the language.</param>
/// <param name="LanguageDisplayName">The display name of the language.</param>
/// <param name="FlagName">The name of the flag associated with the language.</param>
public readonly record struct ContentLanguage(int LanguageID, string LanguageName, string LanguageDisplayName, string FlagName);

/// <summary>
/// Represents a potential comparison target, consisting of a language and version status.
/// </summary>
/// <param name="LanguageName">The name of the target content item's language.</param>
/// <param name="VersionStatus">The version status of the target content item.</param>
public readonly record struct CompareTarget(string LanguageName, VersionStatus VersionStatus);
