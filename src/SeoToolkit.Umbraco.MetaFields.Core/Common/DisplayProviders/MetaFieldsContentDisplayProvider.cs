using SeoToolkit.Umbraco.Common.Core.Interfaces;
using SeoToolkit.Umbraco.Common.Core.Models.ViewModels;
using Umbraco.Cms.Core.Models;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.DisplayProviders
{
    public class MetaFieldsContentDisplayProvider : ISeoDisplayProvider
    {
        public SeoDisplayViewModel Get(IContent content)
        {
            //Check if content is set

            return new SeoDisplayViewModel()
            {
                Alias = "metaFields",
                Name = "Meta Fields",
                View = "/App_Plugins/SeoToolkit/MetaFields/Interface/ContentApps/SeoSettings/seoSettings.html"
            };
        }
    }
}
