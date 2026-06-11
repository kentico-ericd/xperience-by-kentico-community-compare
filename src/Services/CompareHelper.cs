using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.Websites.Internal;

using XperienceCommunity.Compare.Models;

namespace XperienceCommunity.Compare.Services;

/// <summary>
/// Default implementation of <see cref="ICompareHelper"/>.
/// </summary>
public class CompareHelper(
    IProgressiveCache progressiveCache,
    IInfoProvider<ContentLanguageInfo> contentLanguageInfoProvider) : ICompareHelper
{
    public async Task<IEnumerable<ContentLanguage>> GetContentLanguagesAsync(CancellationToken ct) =>
        (await contentLanguageInfoProvider.Get()
            .GetEnumerableTypedResultAsync(cancellationToken: ct))
            .Select(l =>
                new ContentLanguage
                {
                    LanguageID = l.ContentLanguageID,
                    LanguageName = l.ContentLanguageName,
                    LanguageDisplayName = l.ContentLanguageDisplayName,
                    FlagName = l.ContentLanguageFlagIconName
                });


    public async Task<List<BasicContentItem>> GetContentItemVariantsAsync(
        int contentItemId,
        IEnumerable<ContentLanguage> languages,
        CancellationToken ct)
    {
        var query = new DataQuery()
            .From(new QuerySource(new QuerySourceTable(ContentItemCommonDataInfo.TYPEINFO.ClassStructureInfo.TableName)))
            .Source(s => s
                .Join(
                    new QuerySourceTable(ContentItemLanguageMetadataInfo.TYPEINFO.ClassStructureInfo.TableName),
                    new WhereCondition(
                        $"{nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataContentItemID)} = {nameof(ContentItemCommonDataInfo.ContentItemCommonDataContentItemID)}" +
                        $" AND {nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataContentLanguageID)} = {nameof(ContentItemCommonDataInfo.ContentItemCommonDataContentLanguageID)}"
                    )
                )
                .LeftJoin(
                    new QuerySourceTable(UserInfo.TYPEINFO.ClassStructureInfo.TableName),
                    nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataModifiedByUserID),
                    nameof(UserInfo.UserID)
                )
            )
            .Columns(
                nameof(ContentItemCommonDataInfo.ContentItemCommonDataContentLanguageID),
                nameof(ContentItemCommonDataInfo.ContentItemCommonDataVersionStatus),
                nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataModifiedWhen),
                nameof(UserInfo.UserName)
            )
            .WhereEquals(nameof(ContentItemCommonDataInfo.ContentItemCommonDataContentItemID), contentItemId);

        return (await query.GetDataContainerResultAsync(cancellationToken: ct))
                    .Select(c =>
                    {
                        int languageId = ValidationHelper.GetInteger(
                            c.GetValue(nameof(ContentItemCommonDataInfo.ContentItemCommonDataContentLanguageID)), 0);
                        var matchingLanguage = languages.FirstOrDefault(l =>
                            l.LanguageID == languageId);
                        int versionStatus = ValidationHelper.GetInteger(
                            c.GetValue(nameof(ContentItemCommonDataInfo.ContentItemCommonDataVersionStatus)), 0);
                        var lastModified = ValidationHelper.GetDateTime(
                            c.GetValue(nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataModifiedWhen)), DateTime.MinValue);
                        string lastModifiedByUser = ValidationHelper.GetString(c.GetValue(nameof(UserInfo.UserName)), string.Empty);

                        return new BasicContentItem
                        {
                            Language = matchingLanguage,
                            VersionStatus = (VersionStatus)versionStatus,
                            LastModified = lastModified,
                            LastModifiedByUser = lastModifiedByUser
                        };
                    })
                    .ToList();
    }


    public Task<(int contentTypeId, int versionStatus)?> GetContentItemDataAsync(int contentItemId, int languageId, CancellationToken ct)
    {
        var query = new DataQuery()
            .From(new QuerySource(new QuerySourceTable(ContentItemLanguageMetadataInfo.TYPEINFO.ClassStructureInfo.TableName)))
            .Source(source => source
                .LeftJoin<ContentItemInfo>(
                    nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataContentItemID),
                    nameof(ContentItemInfo.ContentItemID))
            )
            .WhereEquals(nameof(ContentItemInfo.ContentItemID), contentItemId)
            .WhereEquals(nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataContentLanguageID), languageId)
            .Columns(
                nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataLatestVersionStatus),
                nameof(ContentItemInfo.ContentItemContentTypeID),
                nameof(ContentItemInfo.ContentItemID));

        return progressiveCache.LoadAsync<(int contentTypeId, int versionStatus)?>(
            async (cs) =>
            {
                var result = (await query.GetDataContainerResultAsync(cancellationToken: ct)).FirstOrDefault();
                if (result is null)
                {
                    return null;
                }

                int contentTypeId = ValidationHelper.GetInteger(result.GetValue(nameof(ContentItemInfo.ContentItemContentTypeID)), 0);
                int versionStatus = ValidationHelper.GetInteger(result.GetValue(nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataLatestVersionStatus)), 0);

                return (contentTypeId, versionStatus);
            },
            new CacheSettings(
                60,
                $"{nameof(CompareHelper)}|{nameof(GetContentItemDataAsync)}|{contentItemId}|{languageId}"));
    }


    public Task<(int contentItemId, int contentTypeId, int versionStatus)?> GetWebPageDataAsync(int webPageId, int websiteChannelId, int languageId, CancellationToken ct)
    {
        var query = new DataQuery()
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
            .WhereEquals(nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataContentLanguageID), languageId)
            .Columns(
                nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataLatestVersionStatus),
                nameof(ContentItemInfo.ContentItemContentTypeID),
                nameof(ContentItemInfo.ContentItemID));

        return progressiveCache.LoadAsync<(int contentItemId, int contentTypeId, int versionStatus)?>(
            async (cs) =>
            {
                var result = (await query.GetDataContainerResultAsync(cancellationToken: ct)).FirstOrDefault();
                if (result is null)
                {
                    return null;
                }

                int contentItemId = ValidationHelper.GetInteger(result.GetValue(nameof(ContentItemInfo.ContentItemID)), 0);
                int contentTypeId = ValidationHelper.GetInteger(result.GetValue(nameof(ContentItemInfo.ContentItemContentTypeID)), 0);
                int versionStatus = ValidationHelper.GetInteger(result.GetValue(nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataLatestVersionStatus)), 0);

                return (contentItemId, contentTypeId, versionStatus);
            },
            new CacheSettings(
                60,
                $"{nameof(CompareHelper)}|{nameof(GetWebPageDataAsync)}|{webPageId}|{websiteChannelId}|{languageId}"));
    }
}
