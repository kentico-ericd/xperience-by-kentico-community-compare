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
import { usePageCommand } from "@kentico/xperience-admin-base";
import {
    CompareRequest,
    ComparableWebPageData,
    WebPageCompareTabProperties,
    VersionStatus,
    ContentLanguage
} from "./WebPageCompareTabTemplate.types";
import { WebPageCompareComponent } from "./webPageCompareComponent";

const Commands = {
    /** Command to compare the selected web pages. */
    Compare: "Compare",
};

/**
 * The front-end template for the web page Compare tab.
 */
export const WebPageCompareTabTemplate = (props: WebPageCompareTabProperties) => {
    const compareRequest: CompareRequest = {
        contentItemID: props.contentItemID,
        websiteChannelName: props.websiteChannelName,
        contentTypeClassID: props.contentTypeClassID,
        sourceContentItem: props.sourceContentItem
    };
    let compareButtonOriginalContent: string;
    const compareButtonRef = useRef<HTMLButtonElement>(null);
    const [comparableData, setComparableData] = useState<ComparableWebPageData>();
    const [targetLanguage, setTargetLanguage] = useState<ContentLanguage>();
    const [targetVersionStatus, setTargetVersionStatus] = useState<number | undefined>();
    const [showDiffs, setShowDiffs] = useState(false);
    const { execute: compare } = usePageCommand<ComparableWebPageData, CompareRequest>(Commands.Compare, {
        before: () => {
            // Validate selections, cancel if invalid
            if (!targetLanguage || !targetVersionStatus) {
                setComparableData({ errorMessage: 'Target page selection incomplete.', fields: [] })

                return false;
            }

            if (compareButtonRef.current) {
                compareButtonRef.current.disabled = true;
                compareButtonOriginalContent = compareButtonRef.current.innerHTML;
                compareButtonRef.current.innerHTML = "Loading...";
            }

            compareRequest.targetContentItem = getTargetContentItem();
        },
        after: data => {
            if (compareButtonRef.current) {
                compareButtonRef.current.disabled = false;
                compareButtonRef.current.innerHTML = compareButtonOriginalContent;
            }

            setComparableData(data);
        },
        data: compareRequest
    });

    /**
     * Returns true if any compare target exists for the given version statuses, within the provided language.
     */
    const variantExistsInLanguageAndVersionStatus = (languageName: string, versionStatuses: VersionStatus[]) =>
        props.compareTargets.some(target => versionStatuses.includes(target.versionStatus) &&
            target.language.languageName == languageName);

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

    /**
     * Gets the data of the target content item, or undefined if the target language or version status has not been selected.
     */
    const getTargetContentItem = () => {
        if (!targetLanguage || !targetVersionStatus) {
            return undefined;
        }

        return props.compareTargets.find(target =>
            target.language.languageName === targetLanguage.languageName &&
            target.versionStatus === targetVersionStatus
        );
    };

    return (
        <Stack spacing={Spacing.L}>
            <Row>
                {/* Left column- source page */}
                <Column cols={Cols.Col4}>
                    <Box spacing={Spacing.L}>
                        <Stack spacing={Spacing.S}>
                            <Headline size={HeadlineSize.M}>This page</Headline>
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
                                    buttonRef={compareButtonRef}
                                    label='Compare'
                                    color={ButtonColor.Primary}
                                    size={ButtonSize.M}
                                    onClick={() => compare()} icon='xp-doc-copy' />
                                <Checkbox
                                    label='Show diffs'
                                    checked={showDiffs}
                                    onChange={(_, checked) => setShowDiffs(checked)} />
                            </Stack>
                        </Row>
                    </Box>
                </Column>

                {/* Right column- target page */}
                <Column cols={Cols.Col4}>
                    <Box spacing={Spacing.L}>
                        <Stack align={LayoutAlignment.End} spacing={Spacing.S}>
                            <Headline size={HeadlineSize.M}>Target page</Headline>
                            {(() => {
                                const target = getTargetContentItem();
                                if (!target) {
                                    return <div style={{ color: 'black' }}>No target page selected.</div>;
                                }

                                return (
                                    <>
                                        <div style={{ color: 'black' }}>
                                            <Inline>
                                                <Icon name={target.language.flagName} />
                                                &nbsp;{target.language.languageDisplayName}
                                                &nbsp;{getVersionStatusName(target.versionStatus)}
                                            </Inline>
                                        </div>
                                        <div style={{ color: 'black' }}>Last modified: {getTimestamp(target.lastModified)}</div>
                                        <div style={{ color: 'black' }}>Modified by: {target.lastModifiedByUser ?? 'N/A'}</div>
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

            <WebPageCompareComponent comparableWebPageData={comparableData} showDiffs={showDiffs} />

        </Stack>
    );
};
