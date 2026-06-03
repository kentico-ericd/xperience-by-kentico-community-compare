import { IconName } from "@kentico/xperience-admin-components";

export interface WebPageCompareTabProperties {
    readonly contentItemID: number;
    readonly websiteChannelName: string;
    readonly contentTypeClassID: number;
    readonly sourceContentItem: BasicContentItem;
    readonly languages: ContentLanguage[];
    readonly compareTargets: BasicContentItem[];
}

export interface CompareRequest {
    contentItemID: number;
    websiteChannelName: string;
    contentTypeClassID: number;
    sourceContentItem: BasicContentItem;
    targetContentItem?: BasicContentItem;
}

export interface ComparableWebPageData {
    readonly errorMessage?: string;
    readonly fields: Field[];
    readonly sourcePageBuilderWidgets?: string;
    readonly targetPageBuilderWidgets?: string;
}

export interface ContentLanguage {
    readonly languageID: number;
    readonly languageName: string;
    readonly languageDisplayName: string;
    readonly flagName: IconName;
}

export interface BasicContentItem {
    readonly language: ContentLanguage;
    readonly versionStatus: VersionStatus;
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
