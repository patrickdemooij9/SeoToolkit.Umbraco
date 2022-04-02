using Umbraco.Cms.Core.Composing;
using SeoToolkit.Umbraco.Common.Core.Interfaces;
using SeoToolkit.Umbraco.Common.Core.Models.ViewModels;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.DisplayProviders
{
    [Weight(100)]
    public class MetaFieldsDocumentSettingsDisplayProvider : IDisplayProvider
    {
        public SeoSettingsDisplayViewModel Get(int contentTypeId)
        {
            return new SeoSettingsDisplayViewModel
            {
                Alias = "metaFields",
                Name = "Meta Fields",
                View = "/App_Plugins/SeoToolkit/MetaFields/Interface/ContentApps/DocumentSettings/documentSettings.html"
            };
        }
    }
}
