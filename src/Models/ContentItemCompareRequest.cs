namespace XperienceCommunity.Compare.Models;

/// <summary>
/// Model containing the necessary information to compare two versions of content items.
/// </summary>
public class ContentItemCompareRequest
{
    /// <summary>
    /// The ID of the source content item.
    /// </summary>
    public int ContentItemID { get; set; }


    /// <summary>
    /// The ID of the source content item's content type class.
    /// </summary>
    public int ContentTypeClassID { get; set; }


    /// <summary>
    /// The source content item.
    /// </summary>
    public BasicContentItem? SourceContentItem { get; set; }


    /// <summary>
    /// The target content item.
    /// </summary>
    public BasicContentItem? TargetContentItem { get; set; }
}
