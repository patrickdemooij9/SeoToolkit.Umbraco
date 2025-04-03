using System;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SeoToolkit.Umbraco.NotFound.Core.Migrations
{
    public class NotFoundUmbraco13Migration : MigrationBase
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

        protected override void Migrate()
        {
            var notFoundValue = _keyValueService.GetValue(NotFoundConstants.NotFoundKeyValueKey);
            if (string.IsNullOrWhiteSpace(notFoundValue)) return;
            if (Guid.TryParse(notFoundValue, out _)) return;
            if (!int.TryParse(notFoundValue, out var notFoundId)) return;

            var content = _contentService.GetById(notFoundId);
            if (content is null) return;

            _keyValueService.SetValue(NotFoundConstants.NotFoundKeyValueKey, content.Key.ToString());
        }
    }
}
