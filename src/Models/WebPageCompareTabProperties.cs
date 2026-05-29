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
    /// The ID of the web page selected in the administration.
    /// </summary>
    public int WebPageID { get; set; }


    /// <summary>
    /// The name of the website channel for the selected web page.
    /// </summary>
    public string? WebsiteChannelName { get; set; }


    /// <summary>
    /// The ID of the selected web page's content type class.
    /// </summary>
    public int ContentTypeClassID { get; set; }


    /// <summary>
    /// The name of the language for the selected web page.
    /// </summary>
    public string? SourceLanguageName { get; set; }


    /// <summary>
    /// The version status of the selected web page.
    /// </summary>
    public int SourceVersionStatus { get; set; }


    /// <summary>
    /// A collection of all languages registered in the system.
    /// </summary>
    public IEnumerable<ContentLanguage> Languages { get; set; } = [];
}

/// <summary>
/// Represents a simplified <see cref="ContentLanguageInfo"/> object.
/// </summary>
/// <param name="LanguageName">The name of the language.</param>
/// <param name="LanguageDisplayName">The display name of the language.</param>
/// <param name="FlagName">The name of the flag associated with the language.</param>
public readonly record struct ContentLanguage(string LanguageName, string LanguageDisplayName, string FlagName);
