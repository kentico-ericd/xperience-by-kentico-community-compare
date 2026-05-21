using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
using CMS.ContentWorkflowEngine;
using CMS.ContentWorkflowEngine.Internal;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Websites.Internal;

using XperienceCommunity.Compare.Models;

namespace XperienceCommunity.Compare.Services;

public class ComparableDataRetriever(
    ICoveringWorkflowRetriever coveringWorkflowRetriever,
    IInfoProvider<ContentLanguageInfo> contentLanguageInfoProvider,
    IInfoProvider<ContentWorkflowStepInfo> contentWorkflowStepInfoProvider) : IComparableDataRetriever
{
    public async Task<SourceWebPageData> GetSourceWebPageData(int webPageId, string languageName, int websiteChannelId)
    {
        // Get languages
        var languages = await contentLanguageInfoProvider.Get().GetEnumerableTypedResultAsync();
        var webPageLanguage = languages.FirstOrDefault(l => l.ContentLanguageName.Equals(languageName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Language '({languageName})' not found.");
        var data = new SourceWebPageData
        {
            LanguageName = languageName,
            Languages = languages.Select(l =>
                new ContentLanguage(l.ContentLanguageName, l.ContentLanguageDisplayName, l.ContentLanguageFlagIconName))
        };

        // Get basic web page data
        var query = GetWebPageDataQuery(webPageId, websiteChannelId, webPageLanguage.ContentLanguageID);
        var dataContainer = (await query.GetDataContainerResultAsync()).FirstOrDefault()
            ?? throw new InvalidOperationException($"Failed to retrieve metadata info for web page {webPageId}.");
        data.VersionStatus = ValidationHelper.GetInteger(
                dataContainer.GetValue(nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataLatestVersionStatus)),
                0);
        data.CurrentWorkflowStep = ValidationHelper.GetInteger(
               dataContainer.GetValue(nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataContentWorkflowStepID)),
               0);

        // Get workflow data
        int contentTypeId = ValidationHelper.GetInteger(dataContainer.GetValue(nameof(ContentItemInfo.ContentItemContentTypeID)), 0);
        var workflow = await coveringWorkflowRetriever.GetForNewCycle(contentTypeId);
        if (workflow is not null)
        {
            data.IsUnderWorkflow = true;
            var workflowSteps = await contentWorkflowStepInfoProvider.Get()
                .WhereEquals(nameof(ContentWorkflowStepInfo.ContentWorkflowStepWorkflowID), workflow.ContentWorkflowID)
                .OrderByAscending(nameof(ContentWorkflowStepInfo.ContentWorkflowStepOrder))
                .GetEnumerableTypedResultAsync();

            data.WorkflowSteps = workflowSteps.Select(w => new WorkflowStep(w.ContentWorkflowStepID, w.ContentWorkflowStepDisplayName));
        }

        return data;
    }


    public async Task<CompareResult> GetWebPageCompareResult(CompareRequest compareRequest) =>
        new()
        {
            Fields = [
                new("TestField", "Source value", "Target value"),
                new("TestField2", "Source value2", "Target value2")
            ]
        };


    //TODO: Limit columns of query
    private static DataQuery GetWebPageDataQuery(int webPageId, int websiteChannelId, int languageId) =>
        new DataQuery()
            .From(new QuerySource(new QuerySourceTable(ContentItemLanguageMetadataInfo.TYPEINFO.ClassStructureInfo.TableName)))
            .Source(source => source
                .LeftJoin<ContentItemInfo>(
                    nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataContentItemID),
                    nameof(ContentItemInfo.ContentItemID))
                .InnerJoin<WebPageItemInfo>(
                    $"{ContentItemInfo.TYPEINFO.ClassStructureInfo.TableName}.{nameof(ContentItemInfo.ContentItemID)}",
                    nameof(WebPageItemInfo.WebPageItemContentItemID), new WhereCondition().WhereEquals(nameof(WebPageItemInfo.WebPageItemWebsiteChannelID), websiteChannelId))
            )
            .WhereEquals(nameof(WebPageItemInfo.WebPageItemID), webPageId)
            .WhereEquals(nameof(WebPageItemInfo.WebPageItemWebsiteChannelID), websiteChannelId)
            .WhereEquals(nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataContentLanguageID), languageId);
}
