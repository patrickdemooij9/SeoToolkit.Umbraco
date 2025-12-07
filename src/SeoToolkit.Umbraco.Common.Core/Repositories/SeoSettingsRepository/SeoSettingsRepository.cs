using Umbraco.Extensions;
using SeoToolkit.Umbraco.Common.Core.Models.Database;
using Umbraco.Cms.Infrastructure.Scoping;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using SeoToolkit.Umbraco.Common.Core.Models.Config;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SeoToolkit.Umbraco.Common.Core.Repositories.SeoSettingsRepository
{
    public class SeoSettingsRepository : ISeoSettingsRepository
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly ISettingsService<GlobalConfig> _settingsService;
        private readonly IContentTypeService _contentTypeService;

        public SeoSettingsRepository(IScopeProvider scopeProvider,
            ISettingsService<GlobalConfig> settingsService,
            IContentTypeService contentTypeService)
        {
            _scopeProvider = scopeProvider;
            _settingsService = settingsService;
            _contentTypeService = contentTypeService;
        }

        public bool IsEnabled(IContentType contentType)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            var entity = scope.Database.FirstOrDefault<SeoSettingsEntity>(scope.SqlContext.Sql()
                .SelectAll()
                .From<SeoSettingsEntity>()
                .Where<SeoSettingsEntity>(it => it.ContentTypeId == contentType.Key));

            //Default is disabled.
            if (entity is null && _settingsService.GetSettings().EnableSeoSettingsByDefaultForTemplated)
            {
                if (contentType.DefaultTemplate != null)
                {
                    return true;
                }
            }
            return entity?.Enabled ?? false;
        }

        public void Toggle(Guid contentTypeId, bool value)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            var entity = scope.Database.FirstOrDefault<SeoSettingsEntity>(scope.SqlContext.Sql()
                .SelectAll()
                .From<SeoSettingsEntity>()
                .Where<SeoSettingsEntity>(it => it.ContentTypeId == contentTypeId));

            if (entity is null)
                entity = new SeoSettingsEntity { ContentTypeId = contentTypeId };
            entity.Enabled = value;

            scope.Database.Save(entity);
        }

        public Dictionary<Guid, bool> GetAll()
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);

            return scope.Database.Fetch<SeoSettingsEntity>(scope.SqlContext.Sql()
                .SelectAll()
                .From<SeoSettingsEntity>())
                .ToDictionary(it => it.ContentTypeId, it => it.Enabled);
        }
    }
}
