using System;
using System.Collections.Generic;
using System.Linq;
using SeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.Business;
using SeoToolkit.Umbraco.MetaFields.Core.Models.MetaFieldsSettings.Database;
using SeoToolkit.Umbraco.MetaFields.Core.Repositories.DocumentTypeSettingsRepository;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.MetaFields.Core.Repositories.MetaFieldsSettingsRepository
{
    public class MetaFieldsSettingsDatabaseRepository : IMetaFieldsSettingsRepository
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly ServiceContext _services;
        private readonly Lazy<IUmbracoMapper> _mapper;

        public MetaFieldsSettingsDatabaseRepository(IScopeProvider scopeProvider, ServiceContext services, Lazy<IUmbracoMapper> mapper)
        {
            _scopeProvider = scopeProvider;
            _services = services;
            _mapper = mapper;
        }

        public IEnumerable<DocumentTypeSettingsDto> GetAll()
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                return scope.Database.Fetch<MetaFieldsSettingsEntity>(scope.SqlContext.Sql()
                    .SelectAll()
                    .From<MetaFieldsSettingsEntity>()).Select(it => _mapper.Value.Map<DocumentTypeSettingsDto>(it));
            }
        }

        public DocumentTypeSettingsDto Get(int id)
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                return _mapper.Value.Map<DocumentTypeSettingsDto>(scope.Database.FirstOrDefault<MetaFieldsSettingsEntity>(scope.SqlContext.Sql()
                    .SelectAll()
                    .From<MetaFieldsSettingsEntity>()
                    .Where<MetaFieldsSettingsEntity>(it => it.NodeId == id)));
            }
        }

        public DocumentTypeSettingsDto Add(DocumentTypeSettingsDto model)
        {
            var entity = _mapper.Value.Map<MetaFieldsSettingsEntity>(model);
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
                scope.Database.Update(_mapper.Value.Map<MetaFieldsSettingsEntity>(model));
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
