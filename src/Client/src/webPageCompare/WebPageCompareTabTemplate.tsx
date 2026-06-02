import { useRef, useState } from "react";
import {
    Box,
    Button,
    ButtonColor,
    ButtonSize,
    Checkbox,
    Cols,
    Column,
    Headline,
    HeadlineSize,
    LayoutAlignment,
    MenuItem,
    Row,
    Select,
    Spacing,
    Stack
} from "@kentico/xperience-admin-components";
import { usePageCommand } from "@kentico/xperience-admin-base";
import { CompareRequest, ComparableWebPageData, WebPageCompareTabProperties, VersionStatus } from "./WebPageCompareTabTemplate.types";
import { WebPageCompareComponent } from "./webPageCompareComponent";

const Commands = {
    /** Command to compare the selected web pages. */
    Compare: "Compare",
};

/**
 * The front-end template for the web page Compare tab.
 */
export const WebPageCompareTabTemplate = (props: WebPageCompareTabProperties) => {
    console.log(props);
    const compareRequest: CompareRequest = {
        contentItemID: props.contentItemID,
        websiteChannelName: props.websiteChannelName,
        contentTypeClassID: props.contentTypeClassID,
        sourceLanguageName: props.sourceLanguageName,
        sourceVersionStatus: props.sourceVersionStatus
    };
    let compareButtonOriginalContent: string;
    const compareButtonRef = useRef<HTMLButtonElement>(null);
    const [comparableData, setComparableData] = useState<ComparableWebPageData>();
    const [targetLanguageName, setTargetLanguageName] = useState(props.sourceLanguageName);
    const [targetVersionStatus, setTargetVersionStatus] = useState<number | undefined>();
    const [showDiffs, setShowDiffs] = useState(false);
    const { execute: compare } = usePageCommand<ComparableWebPageData, CompareRequest>(Commands.Compare, {
        before: () => {
            // Validate selections, cancel if invalid
            if (!targetLanguageName || !targetVersionStatus) {
                setComparableData({ errorMessage: 'Target page selection incomplete.', fields: [] })

                return false;
            }

            if (compareButtonRef.current) {
                compareButtonRef.current.disabled = true;
                compareButtonOriginalContent = compareButtonRef.current.innerHTML;
                compareButtonRef.current.innerHTML = "Loading...";
            }
            compareRequest.targetLanguageName = targetLanguageName;
            compareRequest.targetVersionStatus = targetVersionStatus;
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
     * Returns true if any compare target exists for the given language.
     */
    const variantExistsInLanguage = (languageName: string) => props.compareTargets.some(target => target.languageName === languageName);

    /**
     * Returns true if any compare target exists for the given version statuses, within the currently selected target language.
     */
    const variantExistsInVersionStatus = (versionStatuses: VersionStatus[]) =>
        props.compareTargets.some(target => versionStatuses.includes(target.versionStatus) &&
            target.languageName == targetLanguageName);

    const getSourcePageLanguageDisplayName = () => props.languages
        .find(l => l.languageName === props.sourceLanguageName)?.languageDisplayName ?? '(Current language)';

    const getSourcePageVersionStatusName = () => {
        switch (props.sourceVersionStatus) {
            case VersionStatus.InitialDraft:
            case VersionStatus.Draft: return 'Draft';
            case VersionStatus.Published: return 'Published';
        };

        return '(Current version)';
    };

    return (
        <Stack spacing={Spacing.L}>
            <Row>
                {/* Left column- source page */}
                <Column cols={Cols.Col4}>
                    <Box spacing={Spacing.L}>
                        <Headline size={HeadlineSize.L}>This page</Headline>
                        <Row>
                            <Select disabled={true} label='Language'>
                                <MenuItem primaryLabel={getSourcePageLanguageDisplayName()} selected />
                            </Select>
                            <Select disabled={true} label='Version'>
                                <MenuItem primaryLabel={getSourcePageVersionStatusName()} selected />
                            </Select>
                        </Row>
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
                            <Headline size={HeadlineSize.L}>Target page</Headline>
                        </Row>
                        <Row alignX={LayoutAlignment.End}>
                            <Select
                                label='Language'
                                value={targetLanguageName}
                                onChange={(e) => setTargetLanguageName(e ?? '')}>
                                {
                                    props.languages.map(l =>
                                        <MenuItem
                                            primaryLabel={l.languageDisplayName}
                                            value={l.languageName}
                                            disabled={!variantExistsInLanguage(l.languageName)} />
                                    )
                                }
                            </Select>
                            <Select
                                label='Version'
                                placeholder='(Select version)'
                                onChange={(e) => setTargetVersionStatus(Number.parseInt(e ?? ''))}>
                                <MenuItem
                                    primaryLabel='Draft'
                                    value={VersionStatus.Draft.toString()}
                                    disabled={!variantExistsInVersionStatus([VersionStatus.InitialDraft, VersionStatus.Draft])} />
                                    <MenuItem
                                    primaryLabel='Published'
                                    value={VersionStatus.Published.toString()}
                                    disabled={!variantExistsInVersionStatus([VersionStatus.Published])} />
                            </Select>
                        </Row>
                    </Box>
                </Column>
            </Row>

            <WebPageCompareComponent comparableWebPageData={comparableData} showDiffs={showDiffs} />

        </Stack>
    );
};
