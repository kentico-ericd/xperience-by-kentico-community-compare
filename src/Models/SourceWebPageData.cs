namespace XperienceCommunity.Compare.Models;

public class SourceWebPageData
{
    public string? LanguageName { get; set; }


    public int VersionStatus { get; set; }


    public bool IsUnderWorkflow { get; set; }


    public int CurrentWorkflowStep { get; set; }


    public IEnumerable<WorkflowStep> WorkflowSteps { get; set; } = [];


    public IEnumerable<ContentLanguage> Languages { get; set; } = [];
}


public readonly record struct WorkflowStep(int StepID, string StepDisplayName);


public readonly record struct ContentLanguage(string LanguageName, string LanguageDisplayName, string FlagName);
