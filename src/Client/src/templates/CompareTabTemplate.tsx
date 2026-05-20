import React from "react";
import {
    Avatar,
    AvatarSize,
    Box,
    Button,
    ButtonColor,
    ButtonSize,
    Cols,
    Column,
    Gradients,
    Icon,
    KXIconSetNames,
    LayoutAlignment,
    MenuItem,
    Row,
    Select,
    Spacing,
    Stack
} from "@kentico/xperience-admin-components";
import { usePageCommand } from "@kentico/xperience-admin-base";
import { CompareRequest, CompareResult, ContentLanguage, WebPageCompareTabProperties } from "../types/types";

const Commands = {
    Compare: "Compare",
};

export const CompareTabTemplate = (props: WebPageCompareTabProperties) => {
    console.log(props.sourcePageData);
    const compareRequest: CompareRequest = {};
    const { execute: compare } = usePageCommand<CompareResult, CompareRequest>(Commands.Compare, {
        before: () => {
        },
        after: result => {
        },
        data: compareRequest
    });

    const getLanguageFlag = (language: ContentLanguage) => {
        const initialsLength = 2;
        const nameParts = language.languageDisplayName.split(' ');
        const initials =
            nameParts.length === 1
                ? nameParts[0].substring(0, initialsLength)
                : nameParts
                    .map((part) => part.charAt(0))
                    .join('')
                    .substring(0, initialsLength);
        const customContent = language.flagName ? (
            <Box>
                <Icon name={language.flagName} iconSet={KXIconSetNames.flags} />
            </Box>
        ) : (
            initials.toUpperCase()
        );

        return (
            <Avatar
                size={AvatarSize.XS}
                tooltipText={language.languageDisplayName}
                initials={''}
                customContent={customContent}
                background={{ gradient: Gradients.VeryLightWarmGrey }}
            />
        );
    };

    return (
        <Stack spacing={Spacing.XL}>

            <Row spacing={Spacing.XL}>
                <Column cols={Cols.Col6}>
                    <Stack align={LayoutAlignment.Start}>
                        Test
                    </Stack>
                </Column>
                <Column cols={Cols.Col6}>
                    <Stack align={LayoutAlignment.End}>
                        <Row>
                            <Select value={props.sourcePageData.languageName}>
                                {
                                    props.sourcePageData.languages.map(l =>
                                        <MenuItem
                                            primaryLabel={l.languageDisplayName}
                                            value={l.languageName}
                                            leadingElement={{
                                                type: 'avatar',
                                                element: getLanguageFlag(l)
                                            }} />
                                    )
                                }
                            </Select>
                            <Select>
                                {
                                    props.sourcePageData.workflowSteps.map(s =>
                                        <MenuItem
                                            primaryLabel={s.stepDisplayName}
                                            value={s.stepID.toString()} />
                                    )
                                }
                            </Select>
                            <Button
                                label='Compare'
                                color={ButtonColor.Primary}
                                size={ButtonSize.M}
                                onClick={() => compare()} icon='xp-doc-copy' />
                        </Row>
                    </Stack>
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
