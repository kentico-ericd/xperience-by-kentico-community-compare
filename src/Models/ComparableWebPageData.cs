namespace XperienceCommunity.Compare.Models;

public class ComparableWebPageData
{
    public int VersionStatus { get; set; }


    public bool IsUnderWorkflow { get; set; }


    public IEnumerable<WorkflowStep> WorkflowSteps { get; set; } = [];


    public int CurrentWorkflowStep { get; set; }
}


public readonly record struct WorkflowStep(int StepID, string StepName);
