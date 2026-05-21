import React, { useState } from "react";
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
import { CompareRequest, CompareResult, WebPageCompareTabProperties } from "./WebPageCompareTabTemplate.types";

const Commands = {
    Compare: "Compare",
};

export const WebPageCompareTabTemplate = (props: WebPageCompareTabProperties) => {
    const compareRequest: CompareRequest = {
        sourceLanguageName: props.sourcePageData.languageName,
        sourceWorkflowStepID: props.sourcePageData.currentWorkflowStep
    };
    const [targetLanguageName, setTargetLanguageName] = useState(props.sourcePageData.languageName);
    const [targetWorkflowStepId, setTargetWorkflowStepId] = useState(0);
    const { execute: compare } = usePageCommand<CompareResult, CompareRequest>(Commands.Compare, {
        before: () => {
            compareRequest.targetLanguageName = targetLanguageName;
            compareRequest.targetWorkflowStepID = targetWorkflowStepId;
        },
        after: result => {
        },
        data: compareRequest
    });

    const getSourcePageLanguageDisplayName = () => props.sourcePageData.languages
        .find(l => l.languageName === props.sourcePageData.languageName)?.languageDisplayName ?? '(Current language)';

    const getSourcePageWorkflowStepDisplayName = () => props.sourcePageData.workflowSteps
        .find(s => s.stepID === props.sourcePageData.currentWorkflowStep)?.stepDisplayName ?? '(Current workflow step)';

    return (
        <Stack spacing={Spacing.XL}>
            <Row spacing={Spacing.XL}>
                <Column cols={Cols.Col4}>
                    <Box spacing={Spacing.L}>
                        <Headline size={HeadlineSize.L}>This page</Headline>
                        <Row>
                            <Select disabled={true} label='Language'>
                                <MenuItem primaryLabel={getSourcePageLanguageDisplayName()} selected />
                            </Select>
                            <Select disabled={true} label='Workflow step'>
                                <MenuItem primaryLabel={getSourcePageWorkflowStepDisplayName()} selected />
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
                                label='Workflow step'
                                placeholder='(Select workflow step)'
                                onChange={(e) => setTargetWorkflowStepId(Number.parseInt(e ?? ''))}>
                                {
                                    props.sourcePageData.workflowSteps.map(s =>
                                        <MenuItem
                                            primaryLabel={s.stepDisplayName}
                                            value={s.stepID.toString()} />
                                    )
                                }
                            </Select>
                        </Row>
                    </Box>
                </Column>
             </Row>
        </Stack>
    );
};
