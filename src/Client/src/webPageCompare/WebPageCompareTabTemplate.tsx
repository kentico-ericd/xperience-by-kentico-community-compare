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
    Icon,
    LayoutAlignment,
    MenuItem,
    MenuItemWithSubmenu,
    Row,
    SelectMenu,
    Spacing,
    Stack
} from "@kentico/xperience-admin-components";
import { usePageCommand } from "@kentico/xperience-admin-base";
import { CompareRequest, ComparableWebPageData, WebPageCompareTabProperties, VersionStatus, ContentLanguage } from "./WebPageCompareTabTemplate.types";
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

            compareRequest.targetContentItem = {
                language: targetLanguage,
                versionStatus: targetVersionStatus
            };
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

    const getTargetPageLabel = () => {
        if (targetLanguage && targetVersionStatus) {
            return `Target page: ${targetLanguage.languageDisplayName} ${getVersionStatusName(targetVersionStatus)}`;
        }

        return 'Target page not selected';
    }

    return (
        <Stack spacing={Spacing.L}>
            <Row>
                {/* Left column- source page */}
                <Column cols={Cols.Col4}>
                    <Box spacing={Spacing.L}>
                        <Button
                            color={ButtonColor.Quinary}
                            label={`This page: ${props.sourceContentItem.language.languageDisplayName} 
                            ${getVersionStatusName(props.sourceContentItem.versionStatus)}`} />
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
                        <Row alignX={LayoutAlignment.End}>
                            <DropDownActionMenu
                                renderTrigger={(ref, onTriggerClick) => (
                                    <Button
                                        color={ButtonColor.Quinary}
                                        buttonRef={ref as RefObject<HTMLButtonElement>}
                                        onClick={() => onTriggerClick()}
                                        label={getTargetPageLabel()} />
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
                        </Row>
                    </Box>
                </Column>
            </Row>

            <WebPageCompareComponent comparableWebPageData={comparableData} showDiffs={showDiffs} />

        </Stack>
    );
};
