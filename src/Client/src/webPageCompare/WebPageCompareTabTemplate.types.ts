import { IconName } from "@kentico/xperience-admin-components";

export interface WebPageCompareTabProperties {
    readonly webPageID: number;
    readonly contentTypeClassID: number;
    readonly sourceLanguageName: string;
    readonly sourceVersionStatus: VersionStatus;
    readonly languages: ContentLanguage[];
}

export interface CompareRequest {
    webPageID: number;
    contentTypeClassID: number;
    sourceLanguageName: string;
    targetLanguageName?: string;
    sourceVersionStatus: VersionStatus;
    targetVersionStatus?: VersionStatus;
}

export interface ComparableWebPageData {
    readonly fields: Field[];
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

export enum VersionStatus {
    InitialDraft,
    Draft,
    Published,
    Unpublished,
    NotTranslated
}
