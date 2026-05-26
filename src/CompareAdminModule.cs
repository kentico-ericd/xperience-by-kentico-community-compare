using Kentico.Xperience.Admin.Base;

using XperienceCommunity.Compare;

[assembly: CMS.RegisterModule(typeof(CompareAdminModule))]
namespace XperienceCommunity.Compare;

internal class CompareAdminModule : AdminModule
{
    public CompareAdminModule() : base(nameof(CompareAdminModule))
    {
    }


    protected override void OnInit()
    {
        base.OnInit();

        RegisterClientModule("xperiencecommunity", "compare");
    }
}
