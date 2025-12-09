using SeoToolkit.Umbraco.Common.Core.Interfaces;
using SeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.Business;
using System;

namespace SeoToolkit.Umbraco.MetaFields.Core.Repositories.DocumentTypeSettingsRepository
{
    public interface IMetaFieldsSettingsRepository : IRepository<DocumentTypeSettingsDto>
    {
        void Delete(Guid contentTypeGuid);
    }
}
