namespace XperienceCommunity.Compare.Models;

public class ComparableWebPageData
{
    public string? SourcePageBuilderWidgets { get; set; }


    public string? TargetPageBuilderWidgets { get; set; }


    public IEnumerable<Field> Fields { get; set; } = [];
}


public readonly record struct Field(string FieldName, string SourceValue, string TargetValue);
