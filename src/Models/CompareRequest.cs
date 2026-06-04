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
    /// The source content item.
    /// </summary>
    public BasicContentItem SourceContentItem { get; set; }


    /// <summary>
    /// The target content item.
    /// </summary>
    public BasicContentItem TargetContentItem { get; set; }
}
