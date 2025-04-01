using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Infrastructure.Manifest;

namespace SeoToolkit.Umbraco.NotFound
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
            var entrypoint = JsonNode.Parse(@"{""name"": ""seoToolkit.notFound.entrypoint"",
            ""alias"": ""SeoToolkit.NotFound.EntryPoint"",
            ""type"": ""entryPoint"",
            ""js"": ""/App_Plugins/SeoToolkit/entry/notFound/notFound.js""}");

            List<PackageManifest> manifest = [
                new PackageManifest
            {
                Id = "SeoToolkit.Umbraco.NotFound",
                Name = "SeoToolkit NotFound",
                AllowTelemetry = true,
                Version = "4.0.0-beta5",
                Extensions = [ entrypoint!],
            }
            ];

            return Task.FromResult(manifest.AsEnumerable());
        }
    }
}
