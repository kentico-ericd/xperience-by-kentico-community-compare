import { IconName } from "@kentico/xperience-admin-components";

export interface WebPageCompareTabProperties {
    readonly sourcePageData: SourceWebPageData;
}

export interface CompareRequest {
    sourceLanguageName?: string;
    targetLanguageName?: string;
    sourceWorkflowStepID?: number;
    targetWorkflowStepID?: number;
}

export interface CompareResult {
    readonly fields: Field[];
}

export interface SourceWebPageData {
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

export interface Field {
    readonly fieldName: string;
    readonly sourceValue: string;
    readonly targetValue: string;
}
