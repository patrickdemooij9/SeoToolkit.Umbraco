using uSeoToolkit.Umbraco.Common.Core.Models.ViewModels;

namespace uSeoToolkit.Umbraco.Common.Core.Interfaces
{
    public interface IDisplayProvider
    {
        SeoSettingsDisplayViewModel Get(int contentTypeId);
    }
}
