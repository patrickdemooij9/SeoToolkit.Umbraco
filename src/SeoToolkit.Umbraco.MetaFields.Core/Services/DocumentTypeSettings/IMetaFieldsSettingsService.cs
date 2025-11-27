using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using SeoToolkit.Umbraco.MetaFields.Core.Common.FieldProviders;
using SeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.Business;
using System;

namespace SeoToolkit.Umbraco.MetaFields.Core.Services.DocumentTypeSettings
{
    public interface IMetaFieldsSettingsService
    {
        void Set(DocumentTypeSettingsDto model);

        [Obsolete("Use the overload with Guid key instead")]
        DocumentTypeSettingsDto Get(int id);
        DocumentTypeSettingsDto Get(Guid id);

        IEnumerable<FieldItemViewModel> GetAdditionalFieldItems();
    }
}
