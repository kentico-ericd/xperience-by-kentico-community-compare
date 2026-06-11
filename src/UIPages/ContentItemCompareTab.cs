using CMS.Core;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages;

using XperienceCommunity.Compare.Models;
using XperienceCommunity.Compare.Services;
using XperienceCommunity.Compare.UIPages;

[assembly: UIPage(
    typeof(ContentItemEditSection),
    "compare",
    typeof(ContentItemCompareTab),
    "Compare",
    "@xperiencecommunity/compare/ContentItemCompareTab",
    999)]
namespace XperienceCommunity.Compare.UIPages;

/// <summary>
/// Template for the content item "Compare" tab.
/// </summary>
public class ContentItemCompareTab(
    ICompareHelper compareHelper,
    IEventLogService eventLogService,
    IComparableDataRetriever comparableDataRetriever) : Page<ContentItemComparisonProperties>
{
    [PageParameter(typeof(IntPageModelBinder), typeof(ContentItemEditSection))]
    public int ItemID { get; set; }


    [PageParameter(typeof(StringPageModelBinder), typeof(ContentHubContentLanguage))]
    public string? LanguageName { get; set; }


    public override async Task<ContentItemComparisonProperties> ConfigureTemplateProperties(ContentItemComparisonProperties properties)
    {
        properties.PreventRefetch = true;
        properties.ContentItemID = ItemID;

        // Get languages
        properties.Languages = await compareHelper.GetContentLanguagesAsync(CancellationToken.None);
        var sourceLanguage = properties.Languages.FirstOrDefault(l =>
            l.LanguageName?.Equals(LanguageName, StringComparison.OrdinalIgnoreCase) == true)
            ?? throw new InvalidOperationException($"Failed to retrieve language info for content item {ItemID}.");

        // Get basic content item page data
        var (contentTypeId, versionStatus) = await compareHelper.GetContentItemDataAsync(
            ItemID,
            sourceLanguage.LanguageID,
            CancellationToken.None)
            ?? throw new InvalidOperationException($"Failed to retrieve metadata info for content item {ItemID}.");
        properties.ContentTypeClassID = contentTypeId;

        // Get all existing versions of content item
        var contentItemVariants = await compareHelper.GetContentItemVariantsAsync(
            properties.ContentItemID,
            properties.Languages,
            CancellationToken.None);
        var sourceContentItem = contentItemVariants.Find(c =>
            c.Language?.LanguageName == sourceLanguage.LanguageName && (int)c.VersionStatus == versionStatus)
            ?? throw new InvalidOperationException($"Failed to retrieve source content item variant for content item {ItemID}.");
        properties.SourceContentItem = sourceContentItem;
        // Remove source version from compare targets
        contentItemVariants.Remove(sourceContentItem);
        properties.CompareTargets = contentItemVariants;

        return properties;
    }


    /// <summary>
    /// Submits a request to compare two versions of a content item and returns the result. If an exception occurs, an empty result is
    /// constructed passing the error message within <see cref="ComparableContentItemData.ErrorMessage"/>.
    /// </summary>
    [PageCommand]
    public async Task<ICommandResponse<ComparableContentItemData>> Compare(ContentItemCompareRequest request, CancellationToken ct)
    {
        try
        {
            var result = await comparableDataRetriever.GetContentItemCompareResultAsync(request, ct);

            return ResponseFrom(result);
        }
        catch (Exception ex)
        {
            eventLogService.LogException(nameof(ContentItemCompareTab), nameof(Compare), ex);

            return ResponseFrom(new ComparableContentItemData { ErrorMessage = ex.Message });
        }
    }
}
