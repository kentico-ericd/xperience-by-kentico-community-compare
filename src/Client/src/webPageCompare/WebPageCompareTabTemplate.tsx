import React, { useState } from "react";
import ReactDiffViewer from 'react-diff-viewer';
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

//TODO: Handle exceptions gracefully, ie when the selected target doesn't exist
//TODO: Show loading screen after button click
export const WebPageCompareTabTemplate = (props: WebPageCompareTabProperties) => {
    const compareRequest: CompareRequest = {
        webPageID: props.webPageID,
        contentTypeClassID: props.contentTypeClassID,
        sourceLanguageName: props.sourceLanguageName,
        sourceVersionStatus: props.sourceVersionStatus
    };
    const [comparableData, setComparableData] = useState<ComparableWebPageData>();
    const [targetLanguageName, setTargetLanguageName] = useState(props.sourceLanguageName);
    const [targetVersionStatus, setTargetVersionStatus] = useState(0);
    const { execute: compare } = usePageCommand<ComparableWebPageData, CompareRequest>(Commands.Compare, {
        before: () => {
            compareRequest.targetLanguageName = targetLanguageName;
            compareRequest.targetVersionStatus = targetVersionStatus;
        },
        after: setComparableData,
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

    const renderBody = () => {
        if (comparableData && comparableData.fields.length > 0) {
            // Ran comparison, differences found
            return comparableData.fields.map(f =>
                <Box spacing={Spacing.L}>
                    <Row alignX={LayoutAlignment.Center}>
                        <Headline size={HeadlineSize.M}>{f.fieldName}</Headline>
                        <ReactDiffViewer oldValue={f.sourceValue} newValue={f.targetValue} splitView={true} />
                    </Row>
                </Box>
            );
        }
        else if (comparableData) {
            // Ran comparison, no differences found
            return <Row alignX={LayoutAlignment.Center}>
                <Headline size={HeadlineSize.L}>Nothing to see here...</Headline>
            </Row>
        }
    };

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

            <Stack>
                {renderBody()}
            </Stack>
        </Stack>
    );
};
