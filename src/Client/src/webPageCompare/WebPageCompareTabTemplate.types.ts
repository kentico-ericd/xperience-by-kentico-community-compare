import { IconName } from "@kentico/xperience-admin-components";

export interface WebPageCompareTabProperties {
    readonly sourcePageData: SourceWebPageData;
}

export interface CompareRequest {
    webPageID: number;
    channelName: string;
    contentTypeClassID: number;
    sourceLanguageName: string;
    targetLanguageName?: string;
    sourceVersionStatus: VersionStatus;
    targetVersionStatus?: VersionStatus;
}

export interface ComparableWebPageData {
    readonly fields: Field[];
}

export interface SourceWebPageData {
    readonly webPageID: number;
    readonly channelName: string;
    readonly contentTypeClassID: number;
    readonly languageName: string;
    readonly versionStatus: VersionStatus;
    readonly languages: ContentLanguage[];
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
