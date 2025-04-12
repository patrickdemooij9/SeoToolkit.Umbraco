using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Infrastructure.Manifest;

namespace SeoToolkit.Umbraco.RobotsTxt
{
    internal class ManifestLoader : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddSingleton<IPackageManifestReader, ManifestFilter>();
        }
    }

    internal class ManifestFilter : IPackageManifestReader
    {
        public Task<IEnumerable<PackageManifest>> ReadPackageManifestsAsync()
        {
            var entrypoint = JsonNode.Parse(@"{""name"": ""seoToolkit.robotstxt.entrypoint"",
            ""alias"": ""SeoToolkit.RobotsTxt.EntryPoint"",
            ""type"": ""entryPoint"",
            ""js"": ""/App_Plugins/SeoToolkit/entry/robotstxt/robotstxt.js""}");

            List<PackageManifest> manifest = [
                new PackageManifest
            {
                Id = "SeoToolkit.Umbraco.RobotsTxt",
                Name = "SeoToolkit RobotsTxt",
                AllowTelemetry = true,
                Version = "4.0.0",
                Extensions = [ entrypoint!],
            }
            ];

            return Task.FromResult(manifest.AsEnumerable());
        }
    }
}
