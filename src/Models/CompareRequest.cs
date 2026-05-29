using CMS.ContentEngine;

namespace XperienceCommunity.Compare.Models;

/// <summary>
/// Model containing the necessary information to compare two versions of webpage content.
/// </summary>
public class CompareRequest
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
    /// The name of the language for the target web page.
    /// </summary>
    public string? TargetLanguageName { get; set; }


    /// <summary>
    /// The version status of the selected web page.
    /// </summary>
    public VersionStatus SourceVersionStatus { get; set; }


    /// <summary>
    /// The version status of the target web page.
    /// </summary>
    public VersionStatus TargetVersionStatus { get; set; }
}
