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

export const WebPageCompareTabTemplate = (props: WebPageCompareTabProperties) => {
    const compareRequest: CompareRequest = {
        webPageID: props.sourcePageData.webPageID,
        channelName: props.sourcePageData.channelName,
        contentTypeClassID: props.sourcePageData.contentTypeClassID,
        sourceLanguageName: props.sourcePageData.languageName,
        sourceVersionStatus: props.sourcePageData.versionStatus
    };
    const [comparableData, setComparableData] = useState<ComparableWebPageData>();
    const [targetLanguageName, setTargetLanguageName] = useState(props.sourcePageData.languageName);
    const [targetVersionStatus, setTargetVersionStatus] = useState(0);
    const { execute: compare } = usePageCommand<ComparableWebPageData, CompareRequest>(Commands.Compare, {
        before: () => {
            compareRequest.targetLanguageName = targetLanguageName;
            compareRequest.targetVersionStatus = targetVersionStatus;
        },
        after: setComparableData,
        data: compareRequest
    });

    const getSourcePageLanguageDisplayName = () => props.sourcePageData.languages
        .find(l => l.languageName === props.sourcePageData.languageName)?.languageDisplayName ?? '(Current language)';

    const getSourcePageVersionStatusName = () => {
        switch (props.sourcePageData.versionStatus) {
            case VersionStatus.InitialDraft:
            case VersionStatus.Draft: return 'Draft';
            case VersionStatus.Published: return 'Published';
        };

        return '(Current version)';
    }

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
                                    props.sourcePageData.languages.map(l =>
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
                                    value={VersionStatus.Draft.toString()} />
                                    <MenuItem
                                    primaryLabel='Published'
                                    value={VersionStatus.Published.toString()} />
                            </Select>
                        </Row>
                    </Box>
                </Column>
            </Row>

            <Stack>
                {comparableData &&
                    comparableData.fields.map(f =>
                        <Box spacing={Spacing.L}>
                            <Row alignX={LayoutAlignment.Center}>
                                <Headline size={HeadlineSize.L}>{f.fieldName}</Headline>
                                <ReactDiffViewer oldValue={f.sourceValue} newValue={f.targetValue} splitView={true} />
                            </Row>
                        </Box>
                    )
                }
            </Stack>
        </Stack>
    );
};
