using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;
using SeoToolkit.Umbraco.Common.Core.Collections;
using SeoToolkit.Umbraco.Common.Core.ContentApps;
using SeoToolkit.Umbraco.Common.Core.Dashboards;
using SeoToolkit.Umbraco.Common.Core.Repositories.SeoSettingsRepository;
using SeoToolkit.Umbraco.Common.Core.Sections;
using SeoToolkit.Umbraco.Common.Core.Services.SeoSettingsService;
using Umbraco.Cms.Core.Services;

namespace SeoToolkit.Umbraco.Common.Core.Composers
{
    public class SeoToolkitComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Sections().Append<SeoToolkitSection>();

            builder.Dashboards().Add<WelcomeDashboard>();

            builder.ContentApps().Append<SeoSettingsContentAppFactory>();
            builder.ContentApps().Append<SeoContentAppFactory>();

            builder.Services.AddSingleton<ModuleCollection>();

            builder.Services.AddUnique<ISeoSettingsRepository, SeoSettingsRepository>();
            builder.Services.AddUnique<ISeoSettingsService, SeoSettingsService>();

            /*builder.Services.AddTransient(sp =>
            {
                var languageFiles = new List<LocalizedTextServiceSupplementaryFileSource>();
                var webhostEnvironment = sp.GetRequiredService<IWebHostEnvironment>();

                var seoToolkitFolder = webhostEnvironment.ContentRootFileProvider.GetDirectoryContents("/App_Plugins/SeoToolkit/");
                foreach (var dir in seoToolkitFolder.Where(it => it.IsDirectory))
                {
                    foreach (var langDir in new DirectoryInfo(dir.PhysicalPath).EnumerateDirectories().Where(d => d.Exists && d.Name.InvariantEquals("lang")))
                    {
                        languageFiles.AddRange(langDir.EnumerateFiles("*.xml").Select(langFile => new LocalizedTextServiceSupplementaryFileSource(langFile, false)));
                    }
                }
                languageFiles.Add(new LocalizedTextServiceSupplementaryFileSource(new FileInfo(webhostEnvironment.ContentRootFileProvider.GetFileInfo("/App_Plugins/SeoToolkit/lang/en-us.xml").PhysicalPath), false));
                return (IEnumerable<LocalizedTextServiceSupplementaryFileSource>)languageFiles;
            });*/
        }
    }
}
