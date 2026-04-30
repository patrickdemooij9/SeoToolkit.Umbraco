using Newtonsoft.Json;
using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Services;
using SeoToolkit.Umbraco.Sitemap.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SeoToolkit.Umbraco.Core.Connectors
{
    public class MetaFieldsNoIndexFilter : ISitemapNoIndexFilter
    {
        private readonly IMetaFieldsValueService _metaFieldsValueService;
        private HashSet<Guid> _noIndexKeys;

        public MetaFieldsNoIndexFilter(IMetaFieldsValueService metaFieldsValueService)
        {
            _metaFieldsValueService = metaFieldsValueService;
            _noIndexKeys = new HashSet<Guid>();
        }

        public void Prepare(string culture)
        {
            var allRobotsValues = _metaFieldsValueService.GetAllValuesByFieldAlias(SeoFieldAliasConstants.Robots, culture);
            _noIndexKeys = new HashSet<Guid>(
                allRobotsValues
                    .Where(v => v.UserValue != null)
                    .Where(v =>
                    {
                        var robots = JsonConvert.DeserializeObject<string[]>(v.UserValue);
                        return robots != null && robots.Any(r => r.Equals("noindex", StringComparison.OrdinalIgnoreCase));
                    })
                    .Select(v => v.NodeKey));
        }

        public bool IsNoIndex(Guid contentKey) => _noIndexKeys.Contains(contentKey);
    }
}
