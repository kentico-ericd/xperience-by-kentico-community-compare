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

        var targetPageFieldValues = await GetWebPageFieldValues(
            contentTypeName,
            contentItemId,
            compareRequest.TargetLanguageName,
            compareRequest.TargetVersionStatus,
            fieldNames) ?? throw new InvalidOperationException("Failed to retrieve values for target page.");
        var sourcePageFieldValues = await GetWebPageFieldValues(
            contentTypeName,
            contentItemId,
            compareRequest.SourceLanguageName,
            compareRequest.SourceVersionStatus,
            fieldNames) ?? throw new InvalidOperationException("Failed to retrieve values for source page.");
        var fields = new List<Field>();
        foreach (string field in fieldNames)
        {
            bool sourceHasValue = sourcePageFieldValues.TryGetValue(field, out string? sourceValue);
            bool targetHasValue = targetPageFieldValues.TryGetValue(field, out string? targetValue);
            if ((!sourceHasValue && !targetHasValue) ||
                (sourceValue?.Equals(targetValue) ?? false)) // Skip exact match
            {
                continue;
            }

            fields.Add(new(field, sourceValue ?? string.Empty, targetValue ?? string.Empty));
        }

        return new ComparableWebPageData { Fields = fields };
    }


    private async Task<Dictionary<string, string>?> GetWebPageFieldValues(
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
            container => FieldValueCollectionBinder(container, fieldNames),
            new()
            {
                ForPreview = isPreview,
                IncludeSecuredItems = true
            });

        return result.FirstOrDefault();
    }


    private static Dictionary<string, string> FieldValueCollectionBinder(
        IContentQueryDataContainer container,
        IEnumerable<string> fieldNames)
    {
        var dict = new Dictionary<string, string>();
        foreach (string field in fieldNames)
        {
            //TODO: Format linked items in human-readable way
            object rawValue = container.GetValue<object>(field);
            string? stringRepresentation = ValidationHelper.GetString(rawValue, null);
            if (!string.IsNullOrEmpty(stringRepresentation))
            {
                dict.Add(field, stringRepresentation);
            }
        }

        return dict;
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
