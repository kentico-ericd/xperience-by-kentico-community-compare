using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.Websites;

using Kentico.Content.Web.Mvc;

using Newtonsoft.Json;

using XperienceCommunity.Compare.Models;

namespace XperienceCommunity.Compare.Services;

/// <summary>
/// Default implementation of <see cref="IComparableDataRetriever"/>.
/// </summary>
public class ComparableDataRetriever(
    IContentRetriever contentRetriever,
    IInfoProvider<ContentItemInfo> contentItemInfoProvider,
    IInfoProvider<ContentItemLanguageMetadataInfo> contentItemLanguageMetadataInfoProvider) : IComparableDataRetriever
{
    public async Task<ComparableWebPageData> GetWebPageCompareResult(CompareRequest compareRequest, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrEmpty(compareRequest.WebsiteChannelName);

        var fieldsForCompare = GetFieldsForCompare(compareRequest.ContentTypeClassID);
        var targetPageData = await GetWebPageData(
            false,
            compareRequest,
            fieldsForCompare,
            ct) ?? throw new InvalidOperationException("Failed to retrieve values for target page.");
        var sourcePageData = await GetWebPageData(
            true,
            compareRequest,
            fieldsForCompare,
            ct) ?? throw new InvalidOperationException("Failed to retrieve values for source page.");

        return BuildComparableWebPageData(sourcePageData, targetPageData, fieldsForCompare);
    }


    /// <summary>
    /// Creates a ComparableWebPageData instance representing differences between two PageData objects based on specified fields.
    /// </summary>
    /// <param name="sourcePageData">The source PageData to compare.</param>
    /// <param name="targetPageData">The target PageData to compare.</param>
    /// <param name="fieldsForCompare">The collection of FormFieldInfo specifying which fields to compare.</param>
    /// <returns>A ComparableWebPageData object containing only differing fields and page builder widgets.</returns>
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


    /// <summary>
    /// Retrieves web page data for a specified content item and language context.
    /// </summary>
    /// <param name="isSourcePage">Indicates whether to use the source or target web page version.</param>
    /// <param name="compareRequest">The compare request containing language and version information.</param>
    /// <param name="fields">The collection of form field metadata to bind to the page data.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    private async Task<PageData?> GetWebPageData(
        bool isSourcePage,
        CompareRequest compareRequest,
        IEnumerable<FormFieldInfo> fields,
        CancellationToken ct)
    {
        var language = isSourcePage ? compareRequest.SourceLanguage : compareRequest.TargetLanguage;
        var versionStatus = isSourcePage ? compareRequest.SourceVersionStatus : compareRequest.TargetVersionStatus;
        bool isPreview = versionStatus is VersionStatus.Draft or VersionStatus.InitialDraft;
        var parameters = new RetrieveAllPagesParameters
        {
            ChannelName = compareRequest.WebsiteChannelName,
            IsForPreview = isPreview,
            LanguageName = language.LanguageName,
            IncludeSecuredItems = true,
            UseLanguageFallbacks = false
        };
        var result = await contentRetriever.RetrieveAllPages<PageData?>(
            parameters,
            q => q.Where(w => w.WhereEquals(nameof(IWebPageFieldsSource.SystemFields.ContentItemID), compareRequest.ContentItemID)),
            RetrievalCacheSettings.CacheDisabled,
            (container, mappedResult) => PageDataBinder(container, fields, language, ct),
            ct);

        return result.FirstOrDefault();
    }


    private async Task<PageData?> PageDataBinder(
        IContentQueryDataContainer container,
        IEnumerable<FormFieldInfo> fields,
        ContentLanguage language,
        CancellationToken ct)
    {
        // Build field values
        var fieldValues = new Dictionary<string, string>();
        foreach (var field in fields)
        {
            object value = container.GetValue<object>(field.Name);
            string? stringRepresentation = await GetStringRepresentation(field, value, language, ct);
            if (!string.IsNullOrEmpty(stringRepresentation))
            {
                fieldValues.Add(field.Name, stringRepresentation);
            }
        }

        // Get page builder data
        string pageBuilderWidgets =
            container.GetValue<string>(nameof(ContentItemCommonDataInfo.ContentItemCommonDataVisualBuilderWidgets)) ?? string.Empty;

        return new PageData(fieldValues, pageBuilderWidgets);
    }


    private async Task<string?> GetStringRepresentation(
        FormFieldInfo field,
        object value,
        ContentLanguage language,
        CancellationToken ct)
    {
        string? stringRepresentation = ValidationHelper.GetString(value, null);
        if (string.IsNullOrEmpty(stringRepresentation))
        {
            return null;
        }

        // Convert content item references (GUIDs) to display names
        if (field.DataType == FieldDataType.ContentItemReference)
        {
            var references = JsonConvert.DeserializeObject<List<ContentItemReference>>(stringRepresentation)
                ?? throw new InvalidOperationException($"Failed to deserialize content item references for field {field.Name}.");
            List<string> referenceNames = [];
            foreach (var reference in references.Select(r => r.Identifier))
            {
                string? name = await GetContentItemDisplayName(reference, language.LanguageID, ct) ?? "[Not translated]";
                referenceNames.Add($"{name} ({reference})");
            }

            return string.Join(", ", referenceNames);
        }

        return stringRepresentation;
    }


    private async Task<string?> GetContentItemDisplayName(Guid contentItemGuid, int languageId, CancellationToken ct)
    {
        int contentItemId = await contentItemInfoProvider.Get()
            .WhereEquals(nameof(ContentItemInfo.ContentItemGUID), contentItemGuid)
            .AsSingleColumn(nameof(ContentItemInfo.ContentItemID))
            .GetScalarResultAsync(0, ct);

        return await contentItemLanguageMetadataInfoProvider.Get()
            .WhereEquals(nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataContentItemID), contentItemId)
            .WhereEquals(nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataContentLanguageID), languageId)
            .AsSingleColumn(nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataDisplayName))
            .GetScalarResultAsync<string?>(null, ct);
    }


    private static List<FormFieldInfo> GetFieldsForCompare(int classId)
    {
        string contentTypeName = DataClassInfoProvider.GetDataClassInfo(classId)?.ClassName
            ?? throw new InvalidOperationException($"Failed to retrieve data class for ID {classId}.");

        string prefixedContentTypeName = ReusableFieldSchemaUtils.GetPrefixedContentTypeName(contentTypeName);
        var formInfoWithSchema = FormHelper.GetFormInfo(prefixedContentTypeName, false);

        return formInfoWithSchema.GetFields(true, false);
    }
}

public readonly record struct PageData(Dictionary<string, string> FieldValues, string PageBuilderWidgets);
