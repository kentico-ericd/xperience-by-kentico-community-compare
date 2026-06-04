# Usage Guide

## Comparing web pages

To compare web page versions, first select a page in your channel's content tree. On the right side-panel, click the new tab called "Compare." On this page, you will see the current web page details on the left half, and the desired **Target** version on the right half.

Click the **Select** button on the right side of the page to select the target page of the comparison. Then click the **Compare** button in the middle to run the comparison.

![Compare example](/images/compareSample.png)

When the comparison finishes, you will see the following information:

- Content type fields: Only fields with _different_ values in the source and target versions are shown. Fields with an exact match are hidden.
- Page builder widgets: The full, unformatted JSON data of each page version will be shown only if the values are different. If the page build widget JSON matches, this section is hidden.

### Highlighting differences

After you've run the comparison, the page data is shown in an "informational" mode. That is, the values are simply displayed as-is for your review. If you'd like to highlight the differences between the source and target values, click the **Show diffs** checkbox at the top of the page.

> **Note:** The diffing tool assumes that data on the left is "old" and on the right is "new." While this _may_ be the case in some comparisons, the data on the left will not always be old, for example when comparing a current Draft to a Published version.  
> Improvements to this functionality may be made in the future, but please keep in mind that the "deletes" and "additions" highlighting might be reversed in the current version. This is visual only, and has no impact on detecting page differences.
