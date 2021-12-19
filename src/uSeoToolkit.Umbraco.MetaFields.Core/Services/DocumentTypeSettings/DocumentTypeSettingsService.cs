using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using uSeoToolkit.Umbraco.Core.Interfaces;
using uSeoToolkit.Umbraco.MetaFields.Core.Collections;
using uSeoToolkit.Umbraco.MetaFields.Core.Common.FieldProviders;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.Business;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Services.DocumentTypeSettings
{
    public class DocumentTypeSettingsService : IDocumentTypeSettingsService
    {
        private readonly IRepository<DocumentTypeSettingsDto> _repository;
        private readonly FieldProviderCollection _fieldProviders;

        public DocumentTypeSettingsService(IRepository<DocumentTypeSettingsDto> repository, FieldProviderCollection fieldProviders)
        {
            _repository = repository;
            _fieldProviders = fieldProviders;
        }

        public void Set(DocumentTypeSettingsDto model)
        {
            var exists = _repository.Get(model.Content.Id) != null;
            if (exists)
                _repository.Update(model);
            else
                _repository.Add(model);
        }

        public DocumentTypeSettingsDto Get(int id)
        {
            return _repository.Get(id);
        }

        public IEnumerable<FieldItemViewModel> GetAdditionalFieldItems()
        {
            return _fieldProviders.GetAllItems();
        }

        public bool IsEnabled(IContent content)
        {
            return Get(content.ContentTypeId)?.EnableSeoSettings is true;
        }
    }
}
