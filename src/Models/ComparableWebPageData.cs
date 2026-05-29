using CMS.ContentEngine.Internal;

namespace XperienceCommunity.Compare.Models;

/// <summary>
/// The results of comparing two web pages, including any error messages and the differences in page builder widgets and fields.
/// </summary>
public class ComparableWebPageData
{
    /// <summary>
    /// An error message encountered during the comparison, if any.
    /// </summary>
    public string? ErrorMessage { get; set; }


    /// <summary>
    /// The raw value of the source page's <see cref="ContentItemCommonDataInfo.ContentItemCommonDataVisualBuilderWidgets"/>.
    /// If the value is the same as the target page's value, it will be null to indicate no difference.
    /// </summary>
    public string? SourcePageBuilderWidgets { get; set; }


    /// <summary>
    /// The raw value of the target page's <see cref="ContentItemCommonDataInfo.ContentItemCommonDataVisualBuilderWidgets"/>.
    /// If the value is the same as the source page's value, it will be null to indicate no difference.
    /// </summary>
    public string? TargetPageBuilderWidgets { get; set; }


    /// <summary>
    /// A collection of content type fields and the source/target values. Contains only fields in which the source and target values
    /// differ.
    /// </summary>
    public IEnumerable<Field> Fields { get; set; } = [];
}


/// <summary>
/// Represents a field mapping with its name, source value, and target value.
/// </summary>
/// <param name="FieldName">The name of the field.</param>
/// <param name="SourceValue">The value of the field from the source.</param>
/// <param name="TargetValue">The value of the field for the target.</param>
public readonly record struct Field(string FieldName, string SourceValue, string TargetValue);
