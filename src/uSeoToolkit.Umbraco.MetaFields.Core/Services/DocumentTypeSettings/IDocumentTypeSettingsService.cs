﻿using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using uSeoToolkit.Umbraco.MetaFields.Core.Common.FieldProviders;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.Business;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Services.DocumentTypeSettings
{
    public interface IDocumentTypeSettingsService
    {
        void Set(DocumentTypeSettingsDto model);
        DocumentTypeSettingsDto Get(int id);

        IEnumerable<FieldItemViewModel> GetAdditionalFieldItems();
        bool IsEnabled(IContent content);
    }
}
