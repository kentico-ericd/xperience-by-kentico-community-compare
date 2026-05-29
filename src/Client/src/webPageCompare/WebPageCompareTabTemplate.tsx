import React, { useRef, useState } from "react";
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
import ReactDiffViewer, { ReactDiffViewerStylesOverride } from "react-diff-viewer";

const Commands = {
    Compare: "Compare",
};

enum RenderState {
    NotRun,
    Error,
    NoDifferences,
    Differences
};

export const WebPageCompareTabTemplate = (props: WebPageCompareTabProperties) => {
    const compareRequest: CompareRequest = {
        webPageID: props.webPageID,
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
    const [enableDiffs, setDiffsEnabled] = useState(false);
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

    const diffViewerStyles: ReactDiffViewerStylesOverride = {
        line: {
            fontSize: '12px'
        },
        diffContainer: {
            tableLayout: 'fixed',
            wordWrap: 'break-word'
        },
        variables: {
            light: {
                addedBackground: '#fafbfc',
                removedBackground: '#fafbfc',
            }
        },
    };

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

    const getRenderState = (): RenderState => {
        if (!comparableData) {
            return RenderState.NotRun;
        }

        if (comparableData.errorMessage) {
            return RenderState.Error;
        }

        if ((comparableData.fields.length > 0 ||
            (comparableData.sourcePageBuilderWidgets && comparableData.targetPageBuilderWidgets)))
        {
            return RenderState.Differences;
        }

        return RenderState.NoDifferences;
    };

    const renderNotRun = () => <Row alignX={LayoutAlignment.Center}>
        {/* Currently there is no message displayed when tool is not run */}
    </Row>

    const renderNoDifferences = () => <Row alignX={LayoutAlignment.Center}>
        <Headline size={HeadlineSize.M}>No differences found</Headline>
    </Row>

    const renderError = () => <Row alignX={LayoutAlignment.Center}>
        <Stack align={LayoutAlignment.Center}>
            <Headline size={HeadlineSize.M}>Something went wrong</Headline>
            <Headline size={HeadlineSize.S}>Error "{comparableData?.errorMessage}" occurred.
                The Event Log may contain more details.</Headline>
        </Stack>
    </Row>

    const renderDifferences = () => <>
        {comparableData && comparableData.fields.length > 0 && comparableData.fields.map(f =>
            <Column cols={Cols.Col12}>
                <Box spacing={Spacing.L}>
                    <Row alignX={LayoutAlignment.Center}>
                        <Headline size={HeadlineSize.M}>{f.fieldName}</Headline>
                        <ReactDiffViewer
                            splitView={true}
                            hideLineNumbers={true}
                            disableWordDiff={!enableDiffs}
                            extraLinesSurroundingDiff={0}
                            styles={diffViewerStyles}
                            oldValue={f.sourceValue}
                            newValue={f.targetValue} />
                    </Row>
                </Box>
            </Column>
        )}
        {comparableData && comparableData.sourcePageBuilderWidgets && comparableData.targetPageBuilderWidgets &&
            <Column cols={Cols.Col12}>
                <Box spacing={Spacing.L}>
                    <Row alignX={LayoutAlignment.Center}>
                        <Headline size={HeadlineSize.M}>Widgets</Headline>
                        <ReactDiffViewer
                            splitView={true}
                            hideLineNumbers={true}
                            disableWordDiff={!enableDiffs}
                            extraLinesSurroundingDiff={0}
                            styles={diffViewerStyles}
                            oldValue={comparableData.sourcePageBuilderWidgets}
                            newValue={comparableData.targetPageBuilderWidgets} />
                    </Row>
                </Box>
            </Column>
        }
    </>


    return (
        <Stack spacing={Spacing.L}>
            <Row>
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
                                    checked={enableDiffs}
                                    onChange={(_, checked) => setDiffsEnabled(checked)} />
                            </Stack>
                        
                        </Row>
                    </Box>
                </Column>

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
                                            value={l.languageName} />
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
                                    disabled={targetLanguageName === props.sourceLanguageName &&
                                        props.sourceVersionStatus === VersionStatus.Draft} />
                                    <MenuItem
                                    primaryLabel='Published'
                                    value={VersionStatus.Published.toString()}
                                    disabled={props.sourceVersionStatus == VersionStatus.InitialDraft ||
                                        (targetLanguageName === props.sourceLanguageName &&
                                        props.sourceVersionStatus === VersionStatus.Published)} />
                            </Select>
                        </Row>
                    </Box>
                </Column>
            </Row>

            {(() => {
                const state = getRenderState();
                switch (state) {
                    case RenderState.Error: return renderError();
                    case RenderState.NoDifferences: return renderNoDifferences();
                    case RenderState.Differences: return renderDifferences();
                    case RenderState.NotRun:
                    default:
                        return renderNotRun();
                }
            })()}

        </Stack>
    );
};
