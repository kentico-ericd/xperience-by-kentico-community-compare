using CMS.ContentEngine.Internal;

namespace XperienceCommunity.Compare.Models;

/// <summary>
/// The results of comparing two content items, including any error messages and the differences in page builder widgets and fields.
/// </summary>
public class ComparableContentItemData
{
    /// <summary>
    /// An error message encountered during the comparison, if any.
    /// </summary>
    public string? ErrorMessage { get; set; }


    /// <summary>
    /// The raw value of the source content item's <see cref="ContentItemCommonDataInfo.ContentItemCommonDataVisualBuilderWidgets"/>.
    /// If the value is the same as the target content item's value, it will be null to indicate no difference.
    /// </summary>
    public string? SourcePageBuilderWidgets { get; set; }


    /// <summary>
    /// The raw value of the target content item's <see cref="ContentItemCommonDataInfo.ContentItemCommonDataVisualBuilderWidgets"/>.
    /// If the value is the same as the source content item's value, it will be null to indicate no difference.
    /// </summary>
    public string? TargetPageBuilderWidgets { get; set; }


    /// <summary>
    /// A collection of content type fields and the source/target values. Contains only fields in which the source and target values
    /// differ.
    /// </summary>
    public IEnumerable<ComparableField> Fields { get; set; } = [];
}
