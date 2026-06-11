import { ComparableContentItemData } from "../types";
import ReactDiffViewer, { ReactDiffViewerStylesOverride } from "react-diff-viewer";
import { Row, LayoutAlignment, Headline, HeadlineSize, Stack, Column, Cols, Box, Spacing } from "@kentico/xperience-admin-components";

enum RenderState {
    /** The comparison has not been run yet. */
    NotRun,
    /** An error occurred during the comparison. */
    Error,
    /** No differences were found between the source and target items. */
    NoDifferences,
    /** Differences were found between the source and target items. */
    Differences
};

/**
 * Displays the results of comparing two content items, or an error message if present.
 */
export const ContentItemCompareComponent = (props: {
    comparableContentItemData?: ComparableContentItemData,
    showDiffs: boolean
}) => {
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

    /**
     * Determines what to render based on the current state of comparableData.
     */
    const getRenderState = (): RenderState => {
        if (!props.comparableContentItemData) {
            return RenderState.NotRun;
        }

        if (props.comparableContentItemData.errorMessage) {
            return RenderState.Error;
        }

        if ((props.comparableContentItemData.fields.length > 0 ||
            (props.comparableContentItemData.sourcePageBuilderWidgets && props.comparableContentItemData.targetPageBuilderWidgets))) {
            return RenderState.Differences;
        }

        return RenderState.NoDifferences;
    };

    const renderNotRun = () => <Row alignX={LayoutAlignment.Center}>
        {/* Currently there is no message displayed when tool is not run */}
    </Row>

    /**
     * Renders a friendly message when no differences are found between the source and target items.
     */
    const renderNoDifferences = () => <Row alignX={LayoutAlignment.Center}>
        <Headline size={HeadlineSize.M}>No differences found</Headline>
    </Row>

    /**
     * Renders an error message when an exception occurs during comparison.
     */
    const renderError = () => <Row alignX={LayoutAlignment.Center}>
        <Stack align={LayoutAlignment.Center}>
            <Headline size={HeadlineSize.M}>Something went wrong</Headline>
            <Headline size={HeadlineSize.S}>Error "{props.comparableContentItemData?.errorMessage}" occurred.
                The Event Log may contain more details.</Headline>
        </Stack>
    </Row>

    /**
     * Renders the differences between the source and target items. This includes field differences as well as page builder widget
     * differences (if available).
     */
    const renderDifferences = () =>
        <>
            {props.comparableContentItemData &&
                props.comparableContentItemData.fields.length > 0 &&
                props.comparableContentItemData.fields.map(f =>
                <Column cols={Cols.Col12}>
                    <Box spacing={Spacing.L}>
                        <Row alignX={LayoutAlignment.Center}>
                            <Headline size={HeadlineSize.M}>{f.fieldName}</Headline>
                            <ReactDiffViewer
                                splitView={true}
                                hideLineNumbers={true}
                                disableWordDiff={!props.showDiffs}
                                extraLinesSurroundingDiff={0}
                                styles={diffViewerStyles}
                                oldValue={f.sourceValue}
                                newValue={f.targetValue} />
                        </Row>
                    </Box>
                </Column>
            )}
            {props.comparableContentItemData &&
                props.comparableContentItemData.sourcePageBuilderWidgets &&
                props.comparableContentItemData.targetPageBuilderWidgets &&
                <Column cols={Cols.Col12}>
                    <Box spacing={Spacing.L}>
                        <Row alignX={LayoutAlignment.Center}>
                            <Headline size={HeadlineSize.M}>Widgets</Headline>
                            <ReactDiffViewer
                                splitView={true}
                                hideLineNumbers={true}
                                disableWordDiff={!props.showDiffs}
                                extraLinesSurroundingDiff={0}
                                styles={diffViewerStyles}
                                oldValue={props.comparableContentItemData.sourcePageBuilderWidgets}
                                newValue={props.comparableContentItemData.targetPageBuilderWidgets} />
                        </Row>
                    </Box>
                </Column>
            }
        </>

    const state = getRenderState();
    switch (state) {
        case RenderState.Error: return renderError();
        case RenderState.NoDifferences: return renderNoDifferences();
        case RenderState.Differences: return renderDifferences();
        case RenderState.NotRun:
        default:
            return renderNotRun();
            
    }
}