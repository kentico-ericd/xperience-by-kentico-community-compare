import React from "react";
import {
    Button,
    ButtonColor,
    ButtonSize,
    Cols,
    Column,
    MenuItem,
    Row,
    Select,
    Spacing,
    Stack
} from "@kentico/xperience-admin-components";
import { usePageCommand } from "@kentico/xperience-admin-base";

interface WebPageCompareTabProperties {
}

interface CompareRequest {
}

interface CompareResult {
}

interface WorkflowStep {
    readonly StepID: number;
    readonly StepName: string;
}

const Commands = {
    Compare: "Compare",
};

export const CompareTabTemplate = (props: WebPageCompareTabProperties) => {
    const compareRequest: CompareRequest = {};
    const { execute: compare } = usePageCommand<CompareResult, CompareRequest>(Commands.Compare, {
        before: () => {
        },
        after: result => {
        },
        data: compareRequest
    });

    return (
        <Stack spacing={Spacing.XL}>

            <Row>
                <Column cols={Cols.Col1}>
                    <Button
                        label='Compare'
                        color={ButtonColor.Primary}
                        size={ButtonSize.M}
                        onClick={() => compare()} icon='xp-doc-copy' />
                </Column>
                <Column cols={Cols.Col1}>
                    <Select>
                        <MenuItem primaryLabel="A" value="A"></MenuItem>
                        <MenuItem primaryLabel="B" value="B"></MenuItem>
                    </Select>
                </Column>
                <Column cols={Cols.Col1}>
                    <Select>
                        <MenuItem primaryLabel="C" value="C"></MenuItem>
                        <MenuItem primaryLabel="D" value="D"></MenuItem>
                    </Select>
                </Column>
            </Row>

            <Row>
                <Column cols={Cols.Col6}>
                    
                </Column>
                <Column cols={Cols.Col6}>
                    
                </Column>
            </Row>

        </Stack>
    );
};
