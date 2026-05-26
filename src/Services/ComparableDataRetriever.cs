using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.Websites;
using CMS.Websites.Internal;

using XperienceCommunity.Compare.Models;

namespace XperienceCommunity.Compare.Services;

public class ComparableDataRetriever(
    IContentQueryExecutor contentQueryExecutor,
    IInfoProvider<WebPageItemInfo> webPageItemInfoProvider) : IComparableDataRetriever
{
    public async Task<ComparableWebPageData> GetWebPageCompareResult(CompareRequest compareRequest)
    {
        ArgumentException.ThrowIfNullOrEmpty(compareRequest.SourceLanguageName);
        ArgumentException.ThrowIfNullOrEmpty(compareRequest.TargetLanguageName);

        string contentTypeName = DataClassInfoProvider.GetDataClassInfo(compareRequest.ContentTypeClassID)?.ClassName
            ?? throw new InvalidOperationException($"Failed to retrieve data class for ID {compareRequest.ContentTypeClassID}.");
        var fieldNames = GetFieldNamesForCompare(contentTypeName);
        int contentItemId = GetWebPageContentItemID(compareRequest.WebPageID);
        if (contentItemId == default)
        {
            throw new InvalidOperationException($"Failed to retrieve content item ID for web page {compareRequest.WebPageID}.");
        }

        var targetPageData = await GetWebPageData(
            contentTypeName,
            contentItemId,
            compareRequest.TargetLanguageName,
            compareRequest.TargetVersionStatus,
            fieldNames) ?? throw new InvalidOperationException("Failed to retrieve values for target page.");
        var sourcePageData = await GetWebPageData(
            contentTypeName,
            contentItemId,
            compareRequest.SourceLanguageName,
            compareRequest.SourceVersionStatus,
            fieldNames) ?? throw new InvalidOperationException("Failed to retrieve values for source page.");
        var fields = new List<Field>();
        foreach (string field in fieldNames)
        {
            bool sourceHasValue = sourcePageData.FieldValues.TryGetValue(field, out string? sourceValue);
            bool targetHasValue = targetPageData.FieldValues.TryGetValue(field, out string? targetValue);
            if ((!sourceHasValue && !targetHasValue) ||
                (sourceValue?.Equals(targetValue) ?? false)) // Skip exact match
            {
                continue;
            }

            fields.Add(new(field, sourceValue ?? string.Empty, targetValue ?? string.Empty));
        }
        var comparableWebPageData = new ComparableWebPageData
        {
            Fields = fields
        };

        // If page builder widgets are an exact match, set them to null. They will be ignored in the template
        if (sourcePageData.PageBuilderWidgets.Equals(targetPageData.PageBuilderWidgets))
        {
            comparableWebPageData.SourcePageBuilderWidgets = null;
            comparableWebPageData.TargetPageBuilderWidgets = null;
        }
        else
        {
            comparableWebPageData.SourcePageBuilderWidgets = sourcePageData.PageBuilderWidgets;
            comparableWebPageData.TargetPageBuilderWidgets = targetPageData.PageBuilderWidgets;
        }

        return comparableWebPageData;
    }


    private async Task<PageData?> GetWebPageData(
        string contentTypeName,
        int contentItemId,
        string languageName,
        VersionStatus versionStatus,
        IEnumerable<string> fieldNames)
    {
        bool isPreview = versionStatus is VersionStatus.Draft or VersionStatus.InitialDraft;
        var builder = new ContentItemQueryBuilder()
            .ForContentType(contentTypeName, q => q.WithLinkedItems(1))
            .InLanguage(languageName, false)
            .Parameters(p => p.Where(w => w
                .WhereEquals(nameof(IWebPageFieldsSource.SystemFields.ContentItemID), contentItemId)));
        var result = await contentQueryExecutor.GetResult(
            builder,
            container => PageDataBinder(container, fieldNames),
            new()
            {
                ForPreview = isPreview,
                IncludeSecuredItems = true
            });

        return result.FirstOrDefault();
    }


    private static PageData PageDataBinder(
        IContentQueryDataContainer container,
        IEnumerable<string> fieldNames)
    {
        // Build field values
        var fieldValues = new Dictionary<string, string>();
        foreach (string field in fieldNames)
        {
            //TODO: Format linked items in human-readable way
            object rawValue = container.GetValue<object>(field);
            string? stringRepresentation = ValidationHelper.GetString(rawValue, null);
            if (!string.IsNullOrEmpty(stringRepresentation))
            {
                fieldValues.Add(field, stringRepresentation);
            }
        }

        // Get page builder data
        string pageBuilderWidgets =
            container.GetValue<string>(nameof(ContentItemCommonDataInfo.ContentItemCommonDataVisualBuilderWidgets)) ?? string.Empty;

        return new PageData(fieldValues, pageBuilderWidgets);
    }


    private static IEnumerable<string> GetFieldNamesForCompare(string contentTypeName)
    {
        string prefixedContentTypeName = ReusableFieldSchemaUtils.GetPrefixedContentTypeName(contentTypeName);
        var formInfoWithSchema = FormHelper.GetFormInfo(prefixedContentTypeName, false);

        return formInfoWithSchema.GetFields(true, false).Select(f => f.Name);
    }


    private int GetWebPageContentItemID(int webPageId) =>
        webPageItemInfoProvider.Get()
            .WhereEquals(nameof(WebPageItemInfo.WebPageItemID), webPageId)
            .AsSingleColumn(nameof(WebPageItemInfo.WebPageItemContentItemID))
            .GetScalarResult<int>();
}

public readonly record struct PageData(Dictionary<string, string> FieldValues, string PageBuilderWidgets);
