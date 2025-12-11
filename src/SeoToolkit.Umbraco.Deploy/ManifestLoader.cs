using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Nodes;
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
            var entrypoint = JsonNode.Parse(@"{""name"": ""seoToolkit.deploy.entrypoint"",
            ""alias"": ""SeoToolkit.Deploy.EntryPoint"",
            ""type"": ""backofficeEntryPoint"",
            ""js"": ""/App_Plugins/SeoToolkit/entry/deploy/deploy.js""}");

            List<PackageManifest> manifest = [
                new PackageManifest
            {
                Id = "SeoToolkit.Umbraco.Deploy",
                Name = "SeoToolkit Deploy",
                AllowTelemetry = true,
                Version = "6.0.1",
                Extensions = [ entrypoint!],
            }
            ];

            return Task.FromResult(manifest.AsEnumerable());
        }
    }
}
