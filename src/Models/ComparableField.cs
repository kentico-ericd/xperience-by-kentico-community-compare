namespace XperienceCommunity.Compare.Models;

/// <summary>
/// Represents a field mapping with its name, source value, and target value.
/// </summary>
public class ComparableField
{
    /// <summary>
    /// The name of the field.
    /// </summary>
    public string? FieldName { get; set; }


    /// <summary>
    /// The value of the field from the source.
    /// </summary>
    public string? SourceValue { get; set; }


    /// <summary>
    /// The value of the field from the target.
    /// </summary>
    public string? TargetValue { get; set; }
}
