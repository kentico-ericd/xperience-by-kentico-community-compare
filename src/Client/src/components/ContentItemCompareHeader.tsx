import { RefObject, useRef, useState } from "react";
import {
    Box,
    Button,
    ButtonColor,
    ButtonSize,
    Checkbox,
    Cols,
    Column,
    DropDownActionMenu,
    Headline,
    HeadlineSize,
    Icon,
    Inline,
    LayoutAlignment,
    MenuItem,
    MenuItemWithSubmenu,
    Row,
    SelectMenu,
    Spacing,
    Stack
} from "@kentico/xperience-admin-components";
import { BasicContentItem, ContentLanguage, VersionStatus } from "../types";

interface ContentItemCompareHeaderProperties {
    sourceContentItem: BasicContentItem,
    languages: ContentLanguage[],
    compareTargets: BasicContentItem[],
    onShowDiffChange?: (checked: boolean) => void,
    onCompareClick?: () => Promise<void>,
    onTargetContentItemChange?: (item: BasicContentItem) => void
};

/**
 * Displays a single row header containing the source content item, actions, and target content item.
 */
export const ContentItemCompareHeader = (props: ContentItemCompareHeaderProperties) => {
    let compareButtonOriginalContent: string;
    const compareButtonRef = useRef<HTMLButtonElement>(null);
    const [targetLanguage, setTargetLanguage] = useState<ContentLanguage>();
    const [targetVersionStatus, setTargetVersionStatus] = useState<number | undefined>();

    /**
     * Click handler for center Compare button.
     */
    const compareClick = async () => {
        if (compareButtonRef.current) {
            compareButtonRef.current.disabled = true;
            compareButtonOriginalContent = compareButtonRef.current.innerHTML;
            compareButtonRef.current.innerHTML = "Loading...";
        }

        await props.onCompareClick?.();

        if (compareButtonRef.current) {
            compareButtonRef.current.disabled = false;
            compareButtonRef.current.innerHTML = compareButtonOriginalContent;
        }
    };

    /**
     * Returns true if any compare target exists for the given version statuses, within the provided language.
     */
    const variantExistsInLanguageAndVersionStatus = (languageName: string, versionStatuses: VersionStatus[]) =>
        props.compareTargets.some(target => versionStatuses.includes(target.versionStatus) &&
            target.language.languageName == languageName);

    /**
     * Gets the data of the target content item, or undefined if the target language or version status has not been selected.
     */
    const getTargetContentItem = () => {
        if (!targetLanguage || !targetVersionStatus) {
            return undefined;
        }

        if (targetVersionStatus == VersionStatus.Draft) {
            // If Draft was selected from menu, get the target item in Draft or InitialDraft
            return props.compareTargets.find(target =>
                target.language.languageName === targetLanguage.languageName &&
                (target.versionStatus === VersionStatus.Draft || target.versionStatus === VersionStatus.InitialDraft)
            );
        }

        return props.compareTargets.find(target =>
            target.language.languageName === targetLanguage.languageName &&
            target.versionStatus === targetVersionStatus
        );
    };

    const getVersionStatusName = (versionStatus: number) => {
        switch (versionStatus) {
            case VersionStatus.InitialDraft:
            case VersionStatus.Draft: return 'Draft';
            case VersionStatus.Published: return 'Published';
            default: return '(Current version)'
        };
    };

    const getTimestamp = (dateTime?: string) => {
        if (!dateTime) {
            return 'N/A';
        }

        return new Date(Date.parse(dateTime)).toLocaleString();
    };

    return (
        <>
            <Row>
                {/* Left column- source content item */}
                <Column cols={Cols.Col4}>
                    <Box spacing={Spacing.L}>
                        <Stack spacing={Spacing.S}>
                            <Headline size={HeadlineSize.M}>This item</Headline>
                            <div style={{ color: 'black' }}>
                                <Inline>
                                    <Icon name={props.sourceContentItem.language.flagName} />
                                    &nbsp;{props.sourceContentItem.language.languageDisplayName}
                                    &nbsp;{getVersionStatusName(props.sourceContentItem.versionStatus)}
                                </Inline>
                            </div>
                            <div style={{ color: 'black' }}>Last modified: {getTimestamp(props.sourceContentItem.lastModified)}</div>
                            <div style={{ color: 'black' }}>Modified by: {props.sourceContentItem.lastModifiedByUser ?? 'N/A'}</div>
                        </Stack>
                    </Box>
                </Column>

                {/* Middle column- actions */}
                <Column cols={Cols.Col4}>
                    <Box spacing={Spacing.L}>
                        <Row alignX={LayoutAlignment.Center}>
                            <Stack spacing={Spacing.M} align={LayoutAlignment.Center}>
                                <Button
                                    label='Compare'
                                    icon='xp-doc-copy'
                                    size={ButtonSize.M}
                                    color={ButtonColor.Primary}
                                    onClick={compareClick}
                                    buttonRef={compareButtonRef} />
                                <Checkbox
                                    label='Show diffs'
                                    onChange={(_, checked) => props.onShowDiffChange?.(checked)} />
                            </Stack>
                        </Row>
                    </Box>
                </Column>

                {/* Right column- target content item */}
                <Column cols={Cols.Col4}>
                    <Box spacing={Spacing.L}>
                        <Stack align={LayoutAlignment.End} spacing={Spacing.S}>
                            <Headline size={HeadlineSize.M}>Target item</Headline>
                            {(() => {
                                const targetContentItem = getTargetContentItem();
                                if (!targetContentItem) {
                                    return <div style={{ color: 'black' }}>No target item selected</div>;
                                }

                                {/* Element re-renders when language or version status changes, then propogates the new target item */ }
                                props.onTargetContentItemChange?.(targetContentItem);

                                return (
                                    <>
                                        <div style={{ color: 'black' }}>
                                            <Inline>
                                                <Icon name={targetContentItem.language.flagName} />
                                                &nbsp;{targetContentItem.language.languageDisplayName}
                                                &nbsp;{getVersionStatusName(targetContentItem.versionStatus)}
                                            </Inline>
                                        </div>
                                        <div style={{ color: 'black' }}>Last modified: {getTimestamp(targetContentItem.lastModified)}</div>
                                        <div style={{ color: 'black' }}>Modified by: {targetContentItem.lastModifiedByUser ?? 'N/A'}</div>
                                    </>
                                );
                            })()}

                            <DropDownActionMenu
                                renderTrigger={(ref, onTriggerClick) => (
                                    <Button
                                        size={ButtonSize.XS}
                                        color={ButtonColor.Secondary}
                                        buttonRef={ref as RefObject<HTMLButtonElement>}
                                        onClick={() => onTriggerClick()}
                                        label='Select' />
                                )}>
                                {props.languages.map((language) => (
                                    <MenuItemWithSubmenu
                                        primaryLabel={language.languageDisplayName}
                                        disabled={!variantExistsInLanguageAndVersionStatus(
                                            language.languageName, [VersionStatus.InitialDraft, VersionStatus.Draft, VersionStatus.Published])}
                                        leadingElement={{
                                            type: 'icon',
                                            element: <Icon name={language.flagName} />
                                        }}
                                        submenuContent={
                                            <SelectMenu>
                                                <MenuItem
                                                    primaryLabel='Draft'
                                                    value={VersionStatus.Draft.toString()}
                                                    disabled={!variantExistsInLanguageAndVersionStatus(
                                                        language.languageName, [VersionStatus.InitialDraft, VersionStatus.Draft])}
                                                    onClick={() => {
                                                        setTargetLanguage(language);
                                                        setTargetVersionStatus(VersionStatus.Draft);
                                                    }} />
                                                <MenuItem
                                                    primaryLabel='Published'
                                                    value={VersionStatus.Published.toString()}
                                                    disabled={!variantExistsInLanguageAndVersionStatus(
                                                        language.languageName, [VersionStatus.Published])}
                                                    onClick={() => {
                                                        setTargetLanguage(language);
                                                        setTargetVersionStatus(VersionStatus.Published);
                                                    }} />
                                            </SelectMenu>
                                        }
                                    />
                                ))}
                            </DropDownActionMenu>
                        </Stack>
                    </Box>
                </Column>
            </Row>
        </>
    );
};
