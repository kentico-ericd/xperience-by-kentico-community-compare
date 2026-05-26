import React, { useRef, useState } from "react";
import ReactDiffViewer, { ReactDiffViewerStylesOverride } from 'react-diff-viewer';
import {
    Box,
    Button,
    ButtonColor,
    ButtonSize,
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

const Commands = {
    Compare: "Compare",
};

//TODO: Render a message if the compare ran, but no differences were found
//TODO: Handle exceptions gracefully, ie when the selected target doesn't exist
export const WebPageCompareTabTemplate = (props: WebPageCompareTabProperties) => {
    const compareRequest: CompareRequest = {
        webPageID: props.webPageID,
        contentTypeClassID: props.contentTypeClassID,
        sourceLanguageName: props.sourceLanguageName,
        sourceVersionStatus: props.sourceVersionStatus
    };
    let compareButtonOriginalContent: string;
    const compareButtonRef = useRef<HTMLButtonElement>(null);
    const [comparableData, setComparableData] = useState<ComparableWebPageData>();
    const [targetLanguageName, setTargetLanguageName] = useState(props.sourceLanguageName);
    const [targetVersionStatus, setTargetVersionStatus] = useState(0);
    const { execute: compare } = usePageCommand<ComparableWebPageData, CompareRequest>(Commands.Compare, {
        before: () => {
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

    /**
     * Returns true if the comparison has been executed and at least one difference was found.
     */
    const shouldRenderDiffs = () => !!(
        comparableData &&
        (comparableData.fields.length > 0 ||
            (comparableData.sourcePageBuilderWidgets && comparableData.targetPageBuilderWidgets))
    );

    return (
        <Stack>
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
                    <Row alignX={LayoutAlignment.Center}>
                        <Box spacingY={Spacing.XXXL}>
                            <Button
                                buttonRef={compareButtonRef}
                                label='Compare'
                                color={ButtonColor.Primary}
                                size={ButtonSize.M}
                                onClick={() => compare()} icon='xp-doc-copy' />
                        </Box>
                    </Row>
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

            {shouldRenderDiffs() &&
                <Row>
                    <Stack>
                        {comparableData && comparableData.fields.length > 0 && comparableData.fields.map(f =>
                            <Box spacing={Spacing.L}>
                                <Row alignX={LayoutAlignment.Center}>
                                    <Headline size={HeadlineSize.M}>{f.fieldName}</Headline>
                                    <ReactDiffViewer
                                        splitView={true}
                                        oldValue={f.sourceValue}
                                        newValue={f.targetValue} />
                                </Row>
                            </Box>
                        )}
                    
                        {comparableData && comparableData.sourcePageBuilderWidgets && comparableData.targetPageBuilderWidgets &&
                            <Box spacing={Spacing.L}>
                                <Row alignX={LayoutAlignment.Center}>
                                    <Headline size={HeadlineSize.M}>Widgets</Headline>
                                    <ReactDiffViewer
                                        splitView={true}
                                        oldValue={comparableData.sourcePageBuilderWidgets}
                                        newValue={comparableData.targetPageBuilderWidgets} />
                                </Row>
                            </Box>
                        }
                    </Stack>
                </Row>
            }

        </Stack>
    );
};
