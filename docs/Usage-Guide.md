# Usage Guide

## Configuring options

If you would like to customize options related to the Compare module visuals and enabled features, you can modify your [startup code](../README.md#quick-start):

```cs
builder.Services.AddXperienceCompare(options =>
{
    options.ExpandAllLines = true;
});
```

Below is a list of all available options and their default value:

| Name             | Default value | Description                                                                                                            |
| ---------------- | ------------- | ---------------------------------------------------------------------------------------------------------------------- |
| FontSize         | "12px"        | The size of the font used when displaying comparison field values                                                      |
| ExpandAllLines   | false         | If `true`, headers which allow for collapsing/expanding unchanged lines are hidden, and all lines are always displayed |
| ShowLineNumbers  | false         | If `true`, line numbers (rows) are displayed in comparison field values                                                |
| EnableWebPages   | true          | If `true`, users can [compare web pages](#comparing-web-pages)                                                         |
| EnableContentHub | true          | If `true`, users can [compare content items](#comparing-content-items)                                                 |

## Comparing web pages

To compare web page versions, first select a page in your channel's content tree. On the right side-panel, click the new tab called "Compare." On this page, you will see the current web page details on the left half, and the desired **Target** version on the right half.

Click the **Select** button on the right side of the page to select the target page of the comparison. Then click the **Compare** button in the middle to run the comparison.

![Web page compare example](/images/webPagesCompare.png)

When the comparison finishes, you will see the following information:

- Content type fields: Only fields with _different_ values in the source and target versions are shown. Fields with an exact match are hidden.
- Page builder widgets: The full, unformatted JSON data of each page version will be shown only if the values are different. If the page builder widget JSON matches, this section is hidden.

## Comparing content items

To compare content items, open the **Content hub** and select the a content item. In the left-side navigation tree, click the new **Compare** tab. On this page, you will see the current content item details on the left half, and the desired **Target** version on the right half.

Click the **Select** button on the right side of the page to select the target content item of the comparison. Then click the **Compare** button in the middle to run the comparison.

![Content item compare example](/images/contentHubCompare.png)

When the comparison finishes, you will see the following information:

- Content type fields: Only fields with _different_ values in the source and target versions are shown. Fields with an exact match are hidden.

## Highlighting differences

After you've run the comparison, the data is shown in an "informational" mode. That is, the values are simply displayed as-is for your review. If you'd like to highlight the differences between the source and target values, click the **Show diffs** checkbox at the top of the page.

The diffing tool assumes that data on the left is "old" and on the right is "new." While this _may_ be the case in some comparisons, the data on the left will not always be old, for example when comparing a current Draft to a Published version. This behavior is only visual in nature and does not affect comparisons, but if you wish to reverse the order of the comparison for more accurate highlighting of diffs, you can click the **Reverse order** button next to the "Show diffs" button.
