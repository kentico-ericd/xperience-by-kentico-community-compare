import { ComparableWebPageData } from "./WebPageCompareTabTemplate.types";
import { Row, LayoutAlignment, Headline, HeadlineSize, Stack, Column, Cols, Box, Spacing } from "@kentico/xperience-admin-components";
import ReactDiffViewer, { ReactDiffViewerStylesOverride } from "react-diff-viewer";

enum RenderState {
    /** The comparison has not been run yet. */
    NotRun,
    /** An error occurred during the comparison. */
    Error,
    /** No differences were found between the source and target pages. */
    NoDifferences,
    /** Differences were found between the source and target pages. */
    Differences
};

export const WebPageCompareComponent = (props: {
    comparableWebPageData?: ComparableWebPageData,
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
        if (!props.comparableWebPageData) {
            return RenderState.NotRun;
        }

        if (props.comparableWebPageData.errorMessage) {
            return RenderState.Error;
        }

        if ((props.comparableWebPageData.fields.length > 0 ||
            (props.comparableWebPageData.sourcePageBuilderWidgets && props.comparableWebPageData.targetPageBuilderWidgets))) {
            return RenderState.Differences;
        }

        return RenderState.NoDifferences;
    };

    const renderNotRun = () => <Row alignX={LayoutAlignment.Center}>
        {/* Currently there is no message displayed when tool is not run */}
    </Row>

    /**
     * Renders a friendly message when no differences are found between the source and target pages.
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
            <Headline size={HeadlineSize.S}>Error "{props.comparableWebPageData?.errorMessage}" occurred.
                The Event Log may contain more details.</Headline>
        </Stack>
    </Row>

    /**
     * Renders the differences between the source and target pages. This includes field differences as well as page builder widget
     * differences (if available).
     */
    const renderDifferences = () => <>
        {props.comparableWebPageData && props.comparableWebPageData.fields.length > 0 && props.comparableWebPageData.fields.map(f =>
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
        {props.comparableWebPageData &&
            props.comparableWebPageData.sourcePageBuilderWidgets &&
            props.comparableWebPageData.targetPageBuilderWidgets &&
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
                            oldValue={props.comparableWebPageData.sourcePageBuilderWidgets}
                            newValue={props.comparableWebPageData.targetPageBuilderWidgets} />
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