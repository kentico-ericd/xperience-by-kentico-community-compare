using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.Websites;

using Newtonsoft.Json;

using XperienceCommunity.Compare.Models;

namespace XperienceCommunity.Compare.Services;

/// <summary>
/// Default implementation of <see cref="IComparableDataRetriever"/>.
/// </summary>
public class ComparableDataRetriever(
    IProgressiveCache progressiveCache,
    IContentQueryExecutor contentQueryExecutor,
    ICacheDependencyBuilderFactory dependencyBuilderFactory) : IComparableDataRetriever
{
    public async Task<ComparableContentItemData> GetContentItemCompareResultAsync(ContentItemCompareRequest compareRequest, CancellationToken ct)
    {
        string contentTypeClassName = DataClassInfoProvider.GetDataClassInfo(compareRequest.ContentTypeClassID)?.ClassName
            ?? throw new InvalidOperationException($"Failed to retrieve data class for ID {compareRequest.ContentTypeClassID}.");
        var fieldsForCompare = GetFieldsForCompare(contentTypeClassName);

        var targetItemData = await GetContentItemData(
            false,
            compareRequest,
            fieldsForCompare,
            contentTypeClassName,
            ct) ?? throw new InvalidOperationException("Failed to retrieve values for target item.");
        var sourceItemData = await GetContentItemData(
            true,
            compareRequest,
            fieldsForCompare,
            contentTypeClassName,
            ct) ?? throw new InvalidOperationException("Failed to retrieve values for source item.");

        return BuildComparableContentItemData(sourceItemData, targetItemData, fieldsForCompare);
    }


    /// <summary>
    /// Creates a ComparableContentItemData instance representing differences between two ItemData objects based on specified fields.
    /// </summary>
    /// <param name="sourceItemData">The source ItemData to compare.</param>
    /// <param name="targetItemData">The target ItemData to compare.</param>
    /// <param name="fieldsForCompare">The collection of FormFieldInfo specifying which fields to compare.</param>
    /// <returns>A ComparableContentItemData object containing only differing fields and page builder widgets.</returns>
    private static ComparableContentItemData BuildComparableContentItemData(
        ItemData sourceItemData,
        ItemData targetItemData,
        IEnumerable<FormFieldInfo> fieldsForCompare)
    {
        var fields = new List<ComparableField>();
        foreach (string field in fieldsForCompare.Select(f => f.Name))
        {
            bool sourceHasValue = sourceItemData.FieldValues.TryGetValue(field, out string? sourceValue);
            bool targetHasValue = targetItemData.FieldValues.TryGetValue(field, out string? targetValue);
            if ((!sourceHasValue && !targetHasValue) ||
                (sourceValue?.Equals(targetValue) ?? false)) // Skip exact match
            {
                continue;
            }

            fields.Add(new ComparableField
            {
                FieldName = field,
                SourceValue = sourceValue ?? string.Empty,
                TargetValue = targetValue ?? string.Empty
            });
        }
        var comparableContentItemData = new ComparableContentItemData
        {
            Fields = fields
        };

        // If page builder widgets are an exact match, set them to null. They will be ignored in the template
        if (sourceItemData.PageBuilderWidgets.Equals(targetItemData.PageBuilderWidgets))
        {
            comparableContentItemData.SourcePageBuilderWidgets = null;
            comparableContentItemData.TargetPageBuilderWidgets = null;
        }
        else
        {
            comparableContentItemData.SourcePageBuilderWidgets = sourceItemData.PageBuilderWidgets;
            comparableContentItemData.TargetPageBuilderWidgets = targetItemData.PageBuilderWidgets;
        }

        return comparableContentItemData;
    }


    /// <summary>
    /// Retrieves content item data for a specified content item and language context.
    /// </summary>
    /// <param name="isSourceItem">Indicates whether to use the source or target content item version.</param>
    /// <param name="compareRequest">The compare request containing language and version information.</param>
    /// <param name="fields">The collection of form field metadata to bind to the content item data.</param>
    /// <param name="contentTypeClassName">The class name of the content type to retrieve.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    private async Task<ItemData?> GetContentItemData(
        bool isSourceItem,
        ContentItemCompareRequest compareRequest,
        IEnumerable<FormFieldInfo> fields,
        string contentTypeClassName,
        CancellationToken ct)
    {
        var language = isSourceItem ? compareRequest.SourceContentItem?.Language : compareRequest.TargetContentItem?.Language;
        var versionStatus = isSourceItem ? compareRequest.SourceContentItem?.VersionStatus : compareRequest.TargetContentItem?.VersionStatus;
        bool isPreview = versionStatus is VersionStatus.Draft or VersionStatus.InitialDraft;

        var builder = new ContentItemQueryBuilder()
            .ForContentType(contentTypeClassName)
            .InLanguage(language?.LanguageName)
            .Parameters(p => p.Where(w => w
                .WhereEquals(nameof(IWebPageFieldsSource.SystemFields.ContentItemID), compareRequest.ContentItemID)));
        var result = await contentQueryExecutor.GetResult(
            builder,
            container => ContentItemDataBinder(container, fields, language, ct),
            new()
            {
                ForPreview = isPreview,
                IncludeSecuredItems = true
            },
            ct);

        return result.FirstOrDefault();
    }


    private async Task<ItemData?> ContentItemDataBinder(
        IContentQueryDataContainer container,
        IEnumerable<FormFieldInfo> fields,
        ContentLanguage? language,
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

        return new ItemData(fieldValues, pageBuilderWidgets);
    }


    private async Task<string?> GetStringRepresentation(
        FormFieldInfo field,
        object value,
        ContentLanguage? language,
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
                string name = await GetContentItemDisplayName(reference, language?.LanguageID, ct) ?? "[Not translated]";
                referenceNames.Add($"{name} ({reference})");
            }

            return string.Join(", ", referenceNames);
        }

        return stringRepresentation;
    }


    private async Task<string?> GetContentItemDisplayName(Guid contentItemGuid, int? languageId, CancellationToken ct)
    {
        var query = new DataQuery()
            .From(new QuerySource(new QuerySourceTable(ContentItemLanguageMetadataInfo.TYPEINFO.ClassStructureInfo.TableName)))
            .Source(source => source
                .LeftJoin<ContentItemInfo>(
                    nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataContentItemID),
                    nameof(ContentItemInfo.ContentItemID))
            )
            .WhereEquals(nameof(ContentItemInfo.ContentItemGUID), contentItemGuid)
            .WhereEquals(nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataContentLanguageID), languageId)
            .Columns(nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataDisplayName));

        return await progressiveCache.LoadAsync(
            async (cs) =>
            {
                var cacheDependencyBuilder = dependencyBuilderFactory.Create();
                cs.CacheDependency = cacheDependencyBuilder.ForContentItems().ByGuid(contentItemGuid).Builder().Build();

                var data = (await query.GetDataContainerResultAsync(cancellationToken: ct)).FirstOrDefault();
                if (data is null)
                {
                    return null;
                }

                return ValidationHelper.GetString(
                    data.GetValue(nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataDisplayName)), null);
            },
            new CacheSettings(
                30,
                $"{nameof(ComparableDataRetriever)}|{nameof(GetContentItemDisplayName)}|{contentItemGuid}|{languageId}"));
    }


    private static List<FormFieldInfo> GetFieldsForCompare(string contentTypeName)
    {
        string prefixedContentTypeName = ReusableFieldSchemaUtils.GetPrefixedContentTypeName(contentTypeName);
        var formInfoWithSchema = FormHelper.GetFormInfo(prefixedContentTypeName, false);

        return formInfoWithSchema.GetFields(true, false);
    }
}

public readonly record struct ItemData(Dictionary<string, string> FieldValues, string PageBuilderWidgets);
