using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.Business;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.Database;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Repositories.DocumentTypeSettingsRepository
{
    public class DocumentTypeSettingsRepository : IDocumentTypeSettingsRepository
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly ServiceContext _services;
        private readonly Lazy<UmbracoMapper> _mapper;

        public DocumentTypeSettingsRepository(IScopeProvider scopeProvider, ServiceContext services, Lazy<UmbracoMapper> mapper)
        {
            _scopeProvider = scopeProvider;
            _services = services;
            _mapper = mapper;
        }

        public IEnumerable<DocumentTypeSettingsDto> GetAll()
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                return scope.Database.Fetch<DocumentTypeSettingsEntity>(scope.SqlContext.Sql()
                    .SelectAll()
                    .From<DocumentTypeSettingsEntity>()).Select(it => _mapper.Value.Map<DocumentTypeSettingsDto>(it));
            }
        }

        public DocumentTypeSettingsDto Get(int id)
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                return _mapper.Value.Map<DocumentTypeSettingsDto>(scope.Database.FirstOrDefault<DocumentTypeSettingsEntity>(scope.SqlContext.Sql()
                    .SelectAll()
                    .From<DocumentTypeSettingsEntity>()
                    .Where<DocumentTypeSettingsEntity>(it => it.NodeId == id)));
            }
        }

        public DocumentTypeSettingsDto Add(DocumentTypeSettingsDto model)
        {
            var entity = _mapper.Value.Map<DocumentTypeSettingsEntity>(model);
            using (var scope = _scopeProvider.CreateScope())
            {
                scope.Database.Insert(entity);
                scope.Complete();
            }
            return model;
        }

        public DocumentTypeSettingsDto Update(DocumentTypeSettingsDto model)
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                scope.Database.Update(_mapper.Value.Map<DocumentTypeSettingsEntity>(model));
                scope.Complete();
            }

            return model;
        }

        public void Delete(int id)
        {
            var entity = Get(id);
            if (entity is null)
                return;
            using (var scope = _scopeProvider.CreateScope())
            {
                scope.Database.Delete(entity);
            }
        }
    }
}
