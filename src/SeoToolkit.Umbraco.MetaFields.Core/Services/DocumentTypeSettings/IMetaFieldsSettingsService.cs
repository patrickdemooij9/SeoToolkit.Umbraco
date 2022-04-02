using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using SeoToolkit.Umbraco.MetaFields.Core.Common.FieldProviders;
using SeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.Business;

namespace SeoToolkit.Umbraco.MetaFields.Core.Services.DocumentTypeSettings
{
    public interface IMetaFieldsSettingsService
    {
        void Set(DocumentTypeSettingsDto model);
        DocumentTypeSettingsDto Get(int id);

        IEnumerable<FieldItemViewModel> GetAdditionalFieldItems();
    }
}
