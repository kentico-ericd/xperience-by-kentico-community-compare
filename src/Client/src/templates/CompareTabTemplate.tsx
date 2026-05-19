import React, { useState } from "react";
import { Box, Button, ButtonColor, ButtonSize } from "@kentico/xperience-admin-components";
import { usePageCommand } from "@kentico/xperience-admin-base";

interface WebPageCompareTabProperties {
}

interface CompareResult {
    readonly result: string;
}

const Commands = {
    Compare: "Compare",
};

export const CompareTabTemplate = (props: WebPageCompareTabProperties) => {
    const { execute: compare } = usePageCommand<CompareResult, string>(Commands.Compare, {
        after: result => {
            alert(result?.result);
        }
    });

    return (
        <Box>
            <Button label='Compare' color={ButtonColor.Primary} size={ButtonSize.S} onClick={() => compare("Test")} icon='xp-doc-copy' />
        </Box>
    );
};
