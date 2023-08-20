using Microsoft.Extensions.Configuration;
using SeoToolkit.Umbraco.Common.Core.Collections;
using SeoToolkit.Umbraco.Common.Core.Constants;
using SeoToolkit.Umbraco.Sitemap.Core.Common.DisplayProviders;
using SeoToolkit.Umbraco.Sitemap.Core.Config.Models;
using System;
using System.Linq;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace SeoToolkit.Umbraco.Sitemap
{
    internal class Composer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            var section = builder.Config.GetSection("SeoToolkit:Sitemap");
            var disabledModules = section?.Get<SitemapAppSettingsModel>()?.DisabledModules ?? Array.Empty<string>();

            if (!disabledModules.Contains(DisabledModuleConstant.DocumentTypeContextApp))
            {
                builder.WithCollectionBuilder<DisplayCollectionBuilder>()
                    .Add<SitemapDocumentTypeDisplayProvider>();
            }
        }
    }
}
