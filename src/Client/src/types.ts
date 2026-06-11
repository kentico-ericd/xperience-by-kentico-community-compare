import { IconName } from "@kentico/xperience-admin-components";

export interface ContentItemCompareRequest {
    contentItemID: number;
    contentTypeClassID: number;
    sourceContentItem: BasicContentItem;
    targetContentItem?: BasicContentItem;
}

export interface ComparableContentItemData {
    readonly errorMessage?: string;
    readonly fields: ComparableField[];
    readonly sourcePageBuilderWidgets?: string;
    readonly targetPageBuilderWidgets?: string;
}

export interface BasicContentItem {
    readonly language: ContentLanguage;
    readonly versionStatus: VersionStatus;
    readonly lastModified?: string;
    readonly lastModifiedByUser?: string;
}

export interface ContentLanguage {
    readonly languageID: number;
    readonly languageName: string;
    readonly languageDisplayName: string;
    readonly flagName: IconName;
}

export interface ComparableField {
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