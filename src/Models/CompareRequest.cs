namespace XperienceCommunity.Compare.Models;

public class CompareRequest
{
    public string? SourceLanguageName { get; set; }


    public string? TargetLanguageName { get; set; }


    public int SourceWorkflowStepID { get; set; }


    public int TargetWorkflowStepID { get; set; }
}
