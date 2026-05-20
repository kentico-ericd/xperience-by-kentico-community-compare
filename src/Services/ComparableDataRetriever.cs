using CMS.ContentEngine.Internal;
using CMS.ContentWorkflowEngine;
using CMS.ContentWorkflowEngine.Internal;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Websites.Internal;

using Kentico.Content.Web.Mvc;

using XperienceCommunity.Compare.Models;

namespace XperienceCommunity.Compare.Services;

public class ComparableDataRetriever(
    IContentRetriever contentRetriever,
    ICoveringWorkflowRetriever coveringWorkflowRetriever,
    IContentLanguageRetriever contentLanguageRetriever,
    IInfoProvider<ContentItemInfo> contentItemInfoProvider,
    IInfoProvider<ContentItemLanguageMetadataInfo> contentItemLanguageMetadataInfoProvider,
    IInfoProvider<ContentWorkflowStepInfo> contentWorkflowStepInfoProvider,
    IInfoProvider<WebPageItemInfo> webPageItemInfoProvider) : IComparableDataRetriever
{
    public async Task<ComparableWebPageData> GetComparableWebPageData(int webPageId, string languageName, int websiteChannelId)
    {
        // Get basic web page data
        var language = await contentLanguageRetriever.GetContentLanguage(languageName)
            ?? throw new InvalidOperationException($"Language '({languageName})' not found.");
        var query = GetWebPageDataQuery(webPageId, websiteChannelId, language.ContentLanguageID);
        var dataContainer = (await query.GetDataContainerResultAsync()).FirstOrDefault()
            ?? throw new InvalidOperationException($"Failed to retrieve metadata info for web page {webPageId}.");

        var data = new ComparableWebPageData
        {
            VersionStatus = ValidationHelper.GetInteger(
                dataContainer.GetValue(nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataLatestVersionStatus)),
                0),
            CurrentWorkflowStep = ValidationHelper.GetInteger(
                dataContainer.GetValue(nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataContentWorkflowStepID)),
                0)
        };

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
