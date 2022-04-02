using SeoToolkit.Umbraco.Common.Core.Models.ViewModels;

namespace SeoToolkit.Umbraco.Common.Core.Interfaces
{
    public interface IDisplayProvider
    {
        SeoSettingsDisplayViewModel Get(int contentTypeId);
    }
}
