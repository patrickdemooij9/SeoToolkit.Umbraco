using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SeoToolkit.Umbraco.Deploy;
using SeoToolkit.Umbraco.Deploy.Configuration;
using SeoToolkit.Umbraco.Deploy.Connectors.ServiceConnectors;
using SeoToolkit.Umbraco.ScriptManager.Core.Collections;
using SeoToolkit.Umbraco.ScriptManager.Core.Interfaces;
using SeoToolkit.Umbraco.ScriptManager.Core.Interfaces.Services;
using SeoToolkit.Umbraco.ScriptManager.Core.Models.Business;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;

namespace SeoToolkit.Tests.Deploy
{
    [TestFixture]
    public class ScriptConnectorTests
    {
        private static IOptionsMonitor<SeoToolkitDeploySettings> DefaultSettings()
        {
            var monitor = new Mock<IOptionsMonitor<SeoToolkitDeploySettings>>();
            monitor.Setup(m => m.CurrentValue).Returns(new SeoToolkitDeploySettings());
            return monitor.Object;
        }

        [Test]
        public async Task RoundTrip_PreservesScriptFieldsAndResolvesDefinition()
        {
            var scriptKey = Guid.NewGuid();
            var domainCollectionId = Guid.NewGuid();
            var definition = new Mock<IScriptDefinition>();
            definition.SetupGet(d => d.Alias).Returns("googleAnalytics");

            var scriptService = new Mock<IScriptManagerService>();
            scriptService.Setup(s => s.Get(scriptKey)).Returns(new Script
            {
                Key = scriptKey,
                Name = "GA4",
                Definition = definition.Object,
                Config = new Dictionary<string, string> { ["measurementId"] = "G-123" },
                DomainId = domainCollectionId,
                SortOrder = 3,
            });

            var definitions = CreateDefinitionCollection(definition.Object);

            var connector = new SeoToolkitScriptServiceConnector(scriptService.Object, definitions, DefaultSettings());

            var udi = new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.Script, scriptKey);
            var artifact = await connector.GetArtifactAsync(udi, Mock.Of<IContextCache>());

            Assert.That(artifact, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(artifact!.DefinitionAlias, Is.EqualTo("googleAnalytics"));
                Assert.That(artifact.Config["measurementId"], Is.EqualTo("G-123"));
                Assert.That(artifact.SortOrder, Is.EqualTo(3));
                Assert.That(artifact.DomainCollectionUdi,
                    Is.EqualTo(new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.DomainCollection, domainCollectionId)));
                Assert.That(artifact.Dependencies.Select(d => d.Udi), Does.Contain(artifact.DomainCollectionUdi));
            });

            Script? saved = null;
            scriptService.Setup(s => s.Save(It.IsAny<Script>())).Returns<Script>(s => { saved = s; return s; });

            var state = await connector.ProcessInitAsync(artifact!, Mock.Of<IDeployContext>());
            await connector.ProcessAsync(state, Mock.Of<IDeployContext>(), 2);

            Assert.That(saved, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(saved!.Key, Is.EqualTo(scriptKey));
                Assert.That(saved.Definition.Alias, Is.EqualTo("googleAnalytics"));
                Assert.That(saved.DomainId, Is.EqualTo(domainCollectionId));
            });
        }

        private static ScriptDefinitionCollection CreateDefinitionCollection(params IScriptDefinition[] items)
        {
            var settingsService = new Mock<SeoToolkit.Umbraco.Common.Core.Services.SettingsService
                .ISettingsService<SeoToolkit.Umbraco.ScriptManager.Core.Config.Models.ScriptManagerConfigModel>>();
            return new ScriptDefinitionCollection(() => items, settingsService.Object);
        }
    }
}
