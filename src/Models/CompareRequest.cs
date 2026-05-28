using CMS.ContentEngine;

namespace XperienceCommunity.Compare.Models;

public class CompareRequest
{
    public int WebPageID { get; set; }


    public string? WebsiteChannelName { get; set; }


    public int ContentTypeClassID { get; set; }


    public string? SourceLanguageName { get; set; }


    public string? TargetLanguageName { get; set; }


    public VersionStatus SourceVersionStatus { get; set; }


    public VersionStatus TargetVersionStatus { get; set; }
}
