using CMS.ContentEngine;

namespace XperienceCommunity.Compare.Models;

/// <summary>
/// Represents a simplified <see cref="ContentLanguageInfo"/> object.
/// </summary>
public class ContentLanguage
{
    /// <summary>
    /// The ID of the language.
    /// </summary>
    public int LanguageID { get; set; }


    /// <summary>
    /// The name of the language.
    /// </summary>
    public string? LanguageName { get; set; }


    /// <summary>
    /// The display name of the language.
    /// </summary>
    public string? LanguageDisplayName { get; set; }


    /// <summary>
    /// The name of the flag associated with the language.
    /// </summary>
    public string? FlagName { get; set; }
}
