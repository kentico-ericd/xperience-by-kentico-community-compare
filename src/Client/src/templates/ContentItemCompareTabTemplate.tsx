import { useState } from "react";
import { usePageCommandProvider } from "@kentico/xperience-admin-base";
import { Paper, Spacing, Stack } from "@kentico/xperience-admin-components";
import { ContentItemCompareHeader } from "../components/ContentItemCompareHeader";
import { ContentItemCompareComponent } from "../components/ContentItemCompareComponent";
import { BasicContentItem, ComparableContentItemData, CompareModuleOptions, ContentItemCompareRequest, ContentLanguage } from "../types";

const Commands = {
    /** Command to compare the selected content items. */
    Compare: "Compare",
};

interface ContentItemComparisonProperties {
    readonly contentItemID: number;
    readonly contentTypeClassID: number;
    readonly sourceContentItem: BasicContentItem;
    readonly languages: ContentLanguage[];
    readonly compareTargets: BasicContentItem[];
    readonly options: CompareModuleOptions;
};

/**
 * The front-end template for the content item and web page Compare tab.
 */
export const ContentItemCompareTabTemplate = (props: ContentItemComparisonProperties) => {
    const compareRequest: ContentItemCompareRequest = {
        contentItemID: props.contentItemID,
        contentTypeClassID: props.contentTypeClassID,
        sourceContentItem: props.sourceContentItem
    };
    const [showDiffs, setShowDiffs] = useState(false);
    const [isReversed, setIsReversed] = useState(false);
    const [targetContentItem, setTargetContentItem] = useState<BasicContentItem>();
    const [comparableData, setComparableData] = useState<ComparableContentItemData>();
    const { executeCommand } = usePageCommandProvider();

    const compare = async () => {
        if (!targetContentItem) {
            setComparableData({ errorMessage: 'Target selection incomplete.', fields: [] })

            return;
        }

        compareRequest.targetContentItem = targetContentItem;
        const data = await executeCommand<ComparableContentItemData, ContentItemCompareRequest>(Commands.Compare, compareRequest);
        setComparableData(data);
    };
    console.log(isReversed);
    return (
        <>
            <Paper fullHeight>
                <Stack spacing={Spacing.M}>
                    <ContentItemCompareHeader
                        languages={props.languages}
                        compareTargets={props.compareTargets}
                        sourceContentItem={props.sourceContentItem}
                        isReversed={isReversed}
                        onReverseChange={setIsReversed}
                        onCompareClick={compare}
                        onShowDiffChange={setShowDiffs}
                        onTargetContentItemChange={setTargetContentItem} />
                    <ContentItemCompareComponent
                        showDiffs={showDiffs}
                        isReversed={isReversed}
                        fontSize={props.options.fontSize}
                        showLineNumbers={props.options.showLineNumbers}
                        expandAllLines={props.options.expandAllLines}
                        comparableContentItemData={comparableData} />
                </Stack>
            </Paper>
        </>
    );
};
