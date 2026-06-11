using CMS.ContentEngine;

namespace XperienceCommunity.Compare.Models;

/// <summary>
/// Represents the basic data of a content item.
/// </summary>
public class BasicContentItem
{
    /// <summary>
    /// The language of the content item.
    /// </summary>
    public ContentLanguage? Language { get; init; }

    /// <summary>
    /// The version status of the content item.
    /// </summary>
    public VersionStatus VersionStatus { get; init; }

    /// <summary>
    /// The date and time when the content item was last modified.
    /// </summary>
    public DateTime LastModified { get; init; }

    /// <summary>
    /// The name of the user who last modified the content item.
    /// </summary>
    public string? LastModifiedByUser { get; init; }
}
