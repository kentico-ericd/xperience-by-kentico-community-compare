namespace XperienceCommunity.Compare.Models;

public class CompareResult
{
    public IEnumerable<Field> Fields { get; set; } = [];
}


public readonly record struct Field(string FieldName, string SourceValue, string TargetValue);
