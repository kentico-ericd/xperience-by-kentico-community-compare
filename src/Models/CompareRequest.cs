using CMS.ContentEngine;

namespace XperienceCommunity.Compare.Models;

public class CompareRequest
{
    public int WebPageID { get; set; }


    //TODO: Channel name not needed?
    public string? ChannelName { get; set; }


    public int ContentTypeClassID { get; set; }


    public string? SourceLanguageName { get; set; }


    public string? TargetLanguageName { get; set; }


    public VersionStatus SourceVersionStatus { get; set; }


    public VersionStatus TargetVersionStatus { get; set; }
}
