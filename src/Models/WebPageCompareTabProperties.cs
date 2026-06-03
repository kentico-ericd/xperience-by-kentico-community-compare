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
    /// The source web page's content item data.
    /// </summary>
    public BasicContentItem SourceContentItem { get; set; }


    /// <summary>
    /// A collection of all languages registered in the system.
    /// </summary>
    public IEnumerable<ContentLanguage> Languages { get; set; } = [];


    /// <summary>
    /// A collection of content items available for comparison.
    /// </summary>
    public IEnumerable<BasicContentItem> CompareTargets { get; set; } = [];
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
/// Represents the basic data of a content item.
/// </summary>
/// <param name="Language">The language of the content item.</param>
/// <param name="VersionStatus">The version status of the content item.</param>
public readonly record struct BasicContentItem(ContentLanguage Language, VersionStatus VersionStatus);
