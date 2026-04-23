using System;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SeoToolkit.Umbraco.NotFound.Core.Migrations
{
    public class NotFoundUmbraco13Migration : AsyncMigrationBase
    {
        private readonly IKeyValueService _keyValueService;
        private readonly IContentService _contentService;

        public NotFoundUmbraco13Migration(IMigrationContext context,
            IKeyValueService keyValueService,
            IContentService contentService) : base(context)
        {
            _keyValueService = keyValueService;
            _contentService = contentService;
        }

        protected override Task MigrateAsync()
        {
            var notFoundValue = _keyValueService.GetValue(NotFoundConstants.NotFoundKeyValueKey);
            if (string.IsNullOrWhiteSpace(notFoundValue)) return Task.CompletedTask;
            if (Guid.TryParse(notFoundValue, out _)) return Task.CompletedTask;
            if (!int.TryParse(notFoundValue, out var notFoundId)) return Task.CompletedTask;

            var content = _contentService.GetById(notFoundId);
            if (content is null) return Task.CompletedTask;

            _keyValueService.SetValue(NotFoundConstants.NotFoundKeyValueKey, content.Key.ToString());
            return Task.CompletedTask;
        }
    }
}
