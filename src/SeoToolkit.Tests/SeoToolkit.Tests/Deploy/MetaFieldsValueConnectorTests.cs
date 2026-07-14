using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SeoToolkit.Umbraco.Deploy;
using SeoToolkit.Umbraco.Deploy.Configuration;
using SeoToolkit.Umbraco.Deploy.Connectors.ServiceConnectors;
using SeoToolkit.Umbraco.MetaFields.Core.Repositories.SeoValueRepository;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace SeoToolkit.Tests.Deploy
{
    [TestFixture]
    public class MetaFieldsValueConnectorTests
    {
        private Mock<IMetaFieldsValueRepository> _valueRepository = null!;
        private Mock<IContentService> _contentService = null!;
        private SeoToolkitMetaFieldsValueServiceConnector _connector = null!;

        private static IOptionsMonitor<SeoToolkitDeploySettings> DefaultSettings(bool pruneMissing = false)
        {
            var monitor = new Mock<IOptionsMonitor<SeoToolkitDeploySettings>>();
            monitor.Setup(m => m.CurrentValue).Returns(new SeoToolkitDeploySettings { PruneMissing = pruneMissing });
            return monitor.Object;
        }

        [SetUp]
        public void SetUp()
        {
            _valueRepository = new Mock<IMetaFieldsValueRepository>();
            _valueRepository.Setup(r => r.GetAllValues(It.IsAny<Guid>()))
                .Returns(new Dictionary<string, Dictionary<string, object>>());
            _contentService = new Mock<IContentService>();
            _connector = new SeoToolkitMetaFieldsValueServiceConnector(
                _valueRepository.Object, _contentService.Object, DefaultSettings());
        }

        private IContent SetUpContent(Guid nodeKey, string name = "Some Page")
        {
            var content = new Mock<IContent>();
            content.SetupGet(c => c.Key).Returns(nodeKey);
            content.SetupGet(c => c.Name).Returns(name);
            _contentService.Setup(s => s.GetById(nodeKey)).Returns(content.Object);
            return content.Object;
        }

        [Test]
        public async Task GetArtifact_IncludesAllCulturesAndDocumentDependency()
        {
            var nodeKey = Guid.NewGuid();
            SetUpContent(nodeKey);
            _valueRepository.Setup(r => r.GetAllValues(nodeKey)).Returns(new Dictionary<string, Dictionary<string, object>>
            {
                [""] = new() { ["title"] = "Hello" },
                ["da-DK"] = new() { ["title"] = "Hej", ["description"] = "umb://media/1c6f9c9df1c34b2c8a5a2d9a8e4b0c11" },
            });

            var udi = new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue, nodeKey);
            var artifact = await _connector.GetArtifactAsync(udi, Mock.Of<IContextCache>());

            Assert.That(artifact, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(artifact!.Values, Has.Count.EqualTo(2));
                Assert.That(artifact.Values["da-DK"], Has.Count.EqualTo(2));
                Assert.That(artifact.Dependencies.Select(d => d.Udi),
                    Does.Contain(new GuidUdi(Constants.UdiEntityType.Document, nodeKey)));
                Assert.That(artifact.Dependencies.Select(d => d.Udi.EntityType),
                    Does.Contain(Constants.UdiEntityType.Media));
            });
        }

        [Test]
        public async Task Process_Pass7_WritesValuesPerCultureViaAddOrUpdate()
        {
            var nodeKey = Guid.NewGuid();
            SetUpContent(nodeKey);
            _valueRepository.Setup(r => r.Exists(nodeKey, "title", "")).Returns(false);
            _valueRepository.Setup(r => r.Exists(nodeKey, "title", "da-DK")).Returns(true);

            var udi = new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue, nodeKey);
            var artifact = new SeoToolkit.Umbraco.Deploy.Artifacts.MetaFieldsValueArtifact(udi)
            {
                Name = "Some Page",
                Values = new Dictionary<string, Dictionary<string, string?>>
                {
                    [""] = new() { ["title"] = "\"Hello\"" },
                    ["da-DK"] = new() { ["title"] = "\"Hej\"" },
                },
            };

            var state = await _connector.ProcessInitAsync(artifact, Mock.Of<IDeployContext>());
            await _connector.ProcessAsync(state, Mock.Of<IDeployContext>(), 7);

            _valueRepository.Verify(r => r.Add(nodeKey, "title", "", "Hello"), Times.Once);
            _valueRepository.Verify(r => r.Update(nodeKey, "title", "da-DK", "Hej"), Times.Once);
        }

        [Test]
        public async Task Process_Pass7_PruneMissing_DeletesTargetValuesAbsentFromArtifact()
        {
            var nodeKey = Guid.NewGuid();
            SetUpContent(nodeKey);
            _valueRepository.Setup(r => r.GetAllValues(nodeKey)).Returns(new Dictionary<string, Dictionary<string, object>>
            {
                [""] = new() { ["title"] = "Old title", ["description"] = "Stale" },
            });

            var connector = new SeoToolkitMetaFieldsValueServiceConnector(
                _valueRepository.Object, _contentService.Object, DefaultSettings(pruneMissing: true));

            var udi = new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue, nodeKey);
            var artifact = new SeoToolkit.Umbraco.Deploy.Artifacts.MetaFieldsValueArtifact(udi)
            {
                Name = "Some Page",
                Values = new Dictionary<string, Dictionary<string, string?>> { [""] = new() { ["title"] = "\"New\"" } },
            };

            var state = await connector.ProcessInitAsync(artifact, Mock.Of<IDeployContext>());
            await connector.ProcessAsync(state, Mock.Of<IDeployContext>(), 7);

            _valueRepository.Verify(r => r.Delete(nodeKey, "description", ""), Times.Once);
            _valueRepository.Verify(r => r.Delete(nodeKey, "title", ""), Times.Never);
        }

        [Test]
        public async Task Process_Pass7_PruneMissing_EmptyArtifact_ClearsAllTargetValues()
        {
            var nodeKey = Guid.NewGuid();
            SetUpContent(nodeKey);
            _valueRepository.Setup(r => r.GetAllValues(nodeKey)).Returns(new Dictionary<string, Dictionary<string, object>>
            {
                [""] = new() { ["title"] = "Old", ["description"] = "Old" },
                ["da-DK"] = new() { ["title"] = "Gammel" },
            });

            var connector = new SeoToolkitMetaFieldsValueServiceConnector(
                _valueRepository.Object, _contentService.Object, DefaultSettings(pruneMissing: true));

            var udi = new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue, nodeKey);
            // An artifact carrying no values is a "clear everything for this node" instruction.
            var artifact = new SeoToolkit.Umbraco.Deploy.Artifacts.MetaFieldsValueArtifact(udi) { Name = "Some Page" };

            var state = await connector.ProcessInitAsync(artifact, Mock.Of<IDeployContext>());
            await connector.ProcessAsync(state, Mock.Of<IDeployContext>(), 7);

            _valueRepository.Verify(r => r.Delete(nodeKey, "title", ""), Times.Once);
            _valueRepository.Verify(r => r.Delete(nodeKey, "description", ""), Times.Once);
            _valueRepository.Verify(r => r.Delete(nodeKey, "title", "da-DK"), Times.Once);
            _valueRepository.Verify(r => r.Add(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        [Test]
        public async Task Process_Pass7_OverwriteOnly_DoesNotDeleteTargetValues()
        {
            var nodeKey = Guid.NewGuid();
            SetUpContent(nodeKey);
            _valueRepository.Setup(r => r.GetAllValues(nodeKey)).Returns(new Dictionary<string, Dictionary<string, object>>
            {
                [""] = new() { ["title"] = "Old", ["description"] = "Stale" },
            });

            var udi = new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue, nodeKey);
            var artifact = new SeoToolkit.Umbraco.Deploy.Artifacts.MetaFieldsValueArtifact(udi)
            {
                Name = "Some Page",
                Values = new Dictionary<string, Dictionary<string, string?>> { [""] = new() { ["title"] = "\"New\"" } },
            };

            var state = await _connector.ProcessInitAsync(artifact, Mock.Of<IDeployContext>());
            await _connector.ProcessAsync(state, Mock.Of<IDeployContext>(), 7);

            _valueRepository.Verify(r => r.Delete(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task Process_Pass7_MissingNode_SkipsWithoutThrowing()
        {
            var nodeKey = Guid.NewGuid();
            _contentService.Setup(s => s.GetById(nodeKey)).Returns((IContent?)null);

            var udi = new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue, nodeKey);
            var artifact = new SeoToolkit.Umbraco.Deploy.Artifacts.MetaFieldsValueArtifact(udi)
            {
                Name = "Gone",
                Values = new Dictionary<string, Dictionary<string, string?>> { [""] = new() { ["title"] = "\"x\"" } },
            };

            var state = await _connector.ProcessInitAsync(artifact, Mock.Of<IDeployContext>());
            Assert.DoesNotThrowAsync(() => _connector.ProcessAsync(state, Mock.Of<IDeployContext>(), 7));
            _valueRepository.Verify(r => r.Add(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }
    }
}
