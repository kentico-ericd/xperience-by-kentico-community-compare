using XperienceCommunity.Compare.Models;

namespace XperienceCommunity.Compare.Services;

/// <summary>
/// Contains helper methods for the Compare feature.
/// </summary>
public interface ICompareHelper
{
    /// <summary>
    /// Gets all languages registered in the system.
    /// </summary>
    public Task<IEnumerable<ContentLanguage>> GetContentLanguagesAsync(CancellationToken ct);


    /// <summary>
    /// Gets all variants of the provided content item.
    /// </summary>
    /// <param name="contentItemId">The ID of the content item.</param>
    /// <param name="languages">All languages registered in the system.</param>
    /// <param name="ct">The cancellation token.</param>
    public Task<List<BasicContentItem>> GetContentItemVariantsAsync(int contentItemId, IEnumerable<ContentLanguage> languages, CancellationToken ct);


    /// <summary>
    /// Gets the necessary data for a content item to be compared.
    /// </summary>
    /// <param name="contentItemId">The ID of the content item.</param>
    /// <param name="languageId">The ID of the language.</param>
    /// <param name="ct">The cancellation token.</param>
    public Task<(int contentTypeId, int versionStatus)?> GetContentItemDataAsync(int contentItemId, int languageId, CancellationToken ct);


    /// <summary>
    /// Gets the necessary data for a web page to be compared.
    /// </summary>
    /// <param name="webPageId">The ID of the web page.</param>
    /// <param name="websiteChannelId">The ID of the website channel.</param>
    /// <param name="languageId">The ID of the language.</param>
    /// <param name="ct">The cancellation token.</param>
    public Task<(int contentItemId, int contentTypeId, int versionStatus)?> GetWebPageDataAsync(int webPageId, int websiteChannelId, int languageId, CancellationToken ct);
}
