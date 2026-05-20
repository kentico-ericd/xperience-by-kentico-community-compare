using XperienceCommunity.Compare.Models;

namespace XperienceCommunity.Compare.Services;

public interface IComparableDataRetriever
{
    public Task<ComparableWebPageData> GetComparableWebPageData(int webPageId, string languageName, int websiteChannelId);
}
