import { IconName } from "@kentico/xperience-admin-components";

export interface WebPageCompareTabProperties {
    readonly sourcePageData: ComparableWebPageData;
}

export interface CompareRequest {
    sourceLanguageName?: string;
    targetLanguageName?: string;
    sourceWorkflowStepID?: number;
    targetWorkflowStepID?: number;
}

export interface CompareResult {
}

export interface ComparableWebPageData {
    readonly languageName: string;
    readonly versionStatus: number;
    readonly isUnderWorkflow: boolean;
    readonly currentWorkflowStep: number;
    readonly workflowSteps: WorkflowStep[];
    readonly languages: ContentLanguage[];
}

export interface WorkflowStep {
    readonly stepID: number;
    readonly stepDisplayName: string;
}

export interface ContentLanguage {
    readonly languageName: string;
    readonly languageDisplayName: string;
    readonly flagName: IconName;
}
