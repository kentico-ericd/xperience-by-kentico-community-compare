using XperienceCommunity.Compare.Models;

namespace XperienceCommunity.Compare.Services;

/// <summary>
/// Contains methods for comparing Xperience by Kentico objects.
/// </summary>
public interface IComparableDataRetriever
{
    /// <summary>
    /// Compares content items based on the specified request and returns the comparison result.
    /// </summary>
    /// <param name="compareRequest">The request containing parameters for the content item comparison.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    public Task<ComparableContentItemData> GetContentItemCompareResultAsync(ContentItemCompareRequest compareRequest, CancellationToken ct);
}
