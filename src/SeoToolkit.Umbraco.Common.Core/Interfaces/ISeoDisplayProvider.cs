using SeoToolkit.Umbraco.Common.Core.Models.ViewModels;
using Umbraco.Cms.Core.Models;

namespace SeoToolkit.Umbraco.Common.Core.Interfaces
{
    public interface ISeoDisplayProvider
    {
        SeoDisplayViewModel Get(IContent content);
    }
}
