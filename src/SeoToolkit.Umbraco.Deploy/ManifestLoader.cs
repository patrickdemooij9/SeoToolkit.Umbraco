using Microsoft.Extensions.DependencyInjection;
using SeoToolkit.Umbraco.Common.Core.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Infrastructure.Manifest;

namespace SeoToolkit.Umbraco.Deploy
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

            var manifestVersion = AssemblyVersionHelper.GetInformationalVersion(typeof(ManifestFilter).Assembly);

            List<PackageManifest> manifest = [
                new PackageManifest
            {
                Id = "SeoToolkit.Umbraco.Deploy",
                Name = "SeoToolkit Deploy",
                AllowTelemetry = true,
                Version = manifestVersion,
                Extensions = [ entrypoint! ],
            }
            ];

            return Task.FromResult(manifest.AsEnumerable());
        }
    }
}
