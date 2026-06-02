using CMS.ContentEngine;

namespace XperienceCommunity.Compare.Models;

/// <summary>
/// Model containing the necessary information to compare two versions of webpage content.
/// </summary>
public class CompareRequest
{
    /// <summary>
    /// The ID of the content item selected in the administration.
    /// </summary>
    public int ContentItemID { get; set; }


    /// <summary>
    /// The name of the website channel for the source web page.
    /// </summary>
    public string? WebsiteChannelName { get; set; }


    /// <summary>
    /// The ID of the source web page's content type class.
    /// </summary>
    public int ContentTypeClassID { get; set; }


    /// <summary>
    /// The language for the source web page.
    /// </summary>
    public ContentLanguage SourceLanguage { get; set; }


    /// <summary>
    /// The language for the target web page.
    /// </summary>
    public ContentLanguage TargetLanguage { get; set; }


    /// <summary>
    /// The version status of the selected web page.
    /// </summary>
    public VersionStatus SourceVersionStatus { get; set; }


    /// <summary>
    /// The version status of the target web page.
    /// </summary>
    public VersionStatus TargetVersionStatus { get; set; }
}
