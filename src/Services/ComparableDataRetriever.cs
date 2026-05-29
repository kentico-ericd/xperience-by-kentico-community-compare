using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.Websites;
using CMS.Websites.Internal;

using Kentico.Content.Web.Mvc;

using XperienceCommunity.Compare.Models;

namespace XperienceCommunity.Compare.Services;

public class ComparableDataRetriever(
    IContentRetriever contentRetriever,
    IInfoProvider<WebPageItemInfo> webPageItemInfoProvider) : IComparableDataRetriever
{
    public async Task<ComparableWebPageData> GetWebPageCompareResult(CompareRequest compareRequest, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrEmpty(compareRequest.SourceLanguageName);
        ArgumentException.ThrowIfNullOrEmpty(compareRequest.TargetLanguageName);
        ArgumentException.ThrowIfNullOrEmpty(compareRequest.WebsiteChannelName);

        string contentTypeName = DataClassInfoProvider.GetDataClassInfo(compareRequest.ContentTypeClassID)?.ClassName
            ?? throw new InvalidOperationException($"Failed to retrieve data class for ID {compareRequest.ContentTypeClassID}.");
        int contentItemId = GetWebPageContentItemID(compareRequest.WebPageID);
        if (contentItemId == default)
        {
            throw new InvalidOperationException($"Failed to retrieve content item ID for web page {compareRequest.WebPageID}.");
        }
        var fieldsForCompare = GetFieldsForCompare(contentTypeName);

        // For performance reasons, get target page first because it might not exist and we can avoid querying the source page
        var targetPageData = await GetWebPageData(
            contentItemId,
            compareRequest.WebsiteChannelName,
            compareRequest.TargetLanguageName,
            compareRequest.TargetVersionStatus,
            fieldsForCompare,
            ct) ?? throw new InvalidOperationException("Failed to retrieve values for target page.");
        var sourcePageData = await GetWebPageData(
            contentItemId,
            compareRequest.WebsiteChannelName,
            compareRequest.SourceLanguageName,
            compareRequest.SourceVersionStatus,
            fieldsForCompare,
            ct) ?? throw new InvalidOperationException("Failed to retrieve values for source page.");

        return BuildComparableWebPageData(sourcePageData, targetPageData, fieldsForCompare);
    }


    private static ComparableWebPageData BuildComparableWebPageData(
        PageData sourcePageData,
        PageData targetPageData,
        IEnumerable<FormFieldInfo> fieldsForCompare)
    {
        var fields = new List<Field>();
        foreach (string field in fieldsForCompare.Select(f => f.Name))
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
        int contentItemId,
        string websiteChannelName,
        string languageName,
        VersionStatus versionStatus,
        IEnumerable<FormFieldInfo> fields,
        CancellationToken ct)
    {
        bool isPreview = versionStatus is VersionStatus.Draft or VersionStatus.InitialDraft;
        var result = await contentRetriever.RetrieveAllPages<PageData?>(
            new RetrieveAllPagesParameters
            {
                ChannelName = websiteChannelName,
                IsForPreview = isPreview,
                LanguageName = languageName,
                IncludeSecuredItems = true,
                UseLanguageFallbacks = false
            },
            q => q.Where(w => w.WhereEquals(nameof(IWebPageFieldsSource.SystemFields.ContentItemID), contentItemId)),
            RetrievalCacheSettings.CacheDisabled,
            (container, mappedResult) => PageDataBinder(container, fields),
            ct);

        return result.FirstOrDefault();
    }


    private static async Task<PageData?> PageDataBinder(
        IContentQueryDataContainer container,
        IEnumerable<FormFieldInfo> fields)
    {
        // Build field values
        var fieldValues = new Dictionary<string, string>();
        foreach (string field in fields.Select(f => f.Name))
        {
            object value = container.GetValue<object>(field);
            string? stringRepresentation = ValidationHelper.GetString(value, null);
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


    private static List<FormFieldInfo> GetFieldsForCompare(string contentTypeName)
    {
        string prefixedContentTypeName = ReusableFieldSchemaUtils.GetPrefixedContentTypeName(contentTypeName);
        var formInfoWithSchema = FormHelper.GetFormInfo(prefixedContentTypeName, false);

        return formInfoWithSchema.GetFields(true, false);
    }


    private int GetWebPageContentItemID(int webPageId) =>
        webPageItemInfoProvider.Get()
            .WhereEquals(nameof(WebPageItemInfo.WebPageItemID), webPageId)
            .AsSingleColumn(nameof(WebPageItemInfo.WebPageItemContentItemID))
            .GetScalarResult<int>();
}

public readonly record struct PageData(Dictionary<string, string> FieldValues, string PageBuilderWidgets);
