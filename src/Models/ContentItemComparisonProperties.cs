using Kentico.Xperience.Admin.Websites.UIPages;

namespace XperienceCommunity.Compare.Models;

/// <summary>
/// The properties for UI pages which support comparing content items.
/// </summary>
public class ContentItemComparisonProperties : WebPageBaseClientProperties
{
    /// <summary>
    /// The source content item's ID.
    /// </summary>
    public int ContentItemID { get; set; }


    /// <summary>
    /// The source content item's content type class ID.
    /// </summary>
    public int ContentTypeClassID { get; set; }


    /// <summary>
    /// The source content item data.
    /// </summary>
    public BasicContentItem? SourceContentItem { get; set; }


    /// <summary>
    /// A collection of all languages registered in the system.
    /// </summary>
    public IEnumerable<ContentLanguage> Languages { get; set; } = [];


    /// <summary>
    /// A collection of content items available for comparison.
    /// </summary>
    public IEnumerable<BasicContentItem> CompareTargets { get; set; } = [];
}
