using XperienceCommunity.Compare.Models;

namespace XperienceCommunity.Compare.Services;

public interface IComparableDataRetriever
{
    public Task<ComparableWebPageData> GetWebPageCompareResult(CompareRequest compareRequest);
}
