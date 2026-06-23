namespace XperienceCommunity.Compare.Models;

/// <summary>
/// Options for configuring the Compare module during application startup.
/// </summary>
public class CompareModuleOptions
{
    /// <summary>
    /// The font size used in field comparisons.
    /// </summary>
    public string FontSize { get; set; } = "12px";


    /// <summary>
    /// If <c>true</c>, all lines in the comparison will be expanded by default.
    /// </summary>
    public bool ExpandAllLines { get; set; }


    /// <summary>
    /// If <c>true</c>, line numbers will be displayed in the comparisons.
    /// </summary>
    public bool ShowLineNumbers { get; set; }


    /// <summary>
    /// If <c>true</c>, the Compare module is enabled for web pages.
    /// </summary>
    public bool EnableWebPages { get; set; } = true;


    /// <summary>
    /// If <c>true</c>, the Compare module is enabled for the Content Hub.
    /// </summary>
    public bool EnableContentHub { get; set; } = true;
}
