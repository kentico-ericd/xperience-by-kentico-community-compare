using XperienceCommunity.Compare.Models;

namespace XperienceCommunity.Compare.Services;

public interface IComparableDataRetriever
{
    public Task<SourceWebPageData> GetSourceWebPageData(int webPageId, string languageName, int websiteChannelId);


    public Task<CompareResult> GetWebPageCompareResult(CompareRequest compareRequest);
}
