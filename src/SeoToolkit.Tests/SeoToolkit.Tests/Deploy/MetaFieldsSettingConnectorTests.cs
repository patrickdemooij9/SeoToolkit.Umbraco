using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SeoToolkit.Umbraco.Deploy;
using SeoToolkit.Umbraco.Deploy.Configuration;
using SeoToolkit.Umbraco.Deploy.Connectors.ServiceConnectors;
using SeoToolkit.Umbraco.MetaFields.Core.Collections;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using SeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.Business;
using SeoToolkit.Umbraco.MetaFields.Core.Services.DocumentTypeSettings;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace SeoToolkit.Tests.Deploy
{
    [TestFixture]
    public class MetaFieldsSettingConnectorTests
    {
        private static IOptionsMonitor<SeoToolkitDeploySettings> DefaultSettings(bool pruneMissing = false)
        {
            var monitor = new Mock<IOptionsMonitor<SeoToolkitDeploySettings>>();
            monitor.Setup(m => m.CurrentValue).Returns(new SeoToolkitDeploySettings { PruneMissing = pruneMissing });
            return monitor.Object;
        }

        private static Mock<ISeoField> MakeField(string alias)
        {
            var converter = new Mock<IEditorValueConverter>();
            // Pass-through converter (like the text converter): object/editor/database forms coincide.
            converter.Setup(c => c.ConvertDatabaseToObject(It.IsAny<object>())).Returns((object o) => o);
            converter.Setup(c => c.ConvertObjectToEditorValue(It.IsAny<object>())).Returns((object o) => o);
            converter.Setup(c => c.ConvertEditorToDatabaseValue(It.IsAny<object>())).Returns((object o) => o);
            var editor = new Mock<ISeoFieldEditor>();
            editor.SetupGet(e => e.ValueConverter).Returns(converter.Object);

            var field = new Mock<ISeoField>();
            field.SetupGet(f => f.Alias).Returns(alias);
            field.SetupGet(f => f.Editor).Returns(editor.Object);
            return field;
        }

        [Test]
        public async Task GetArtifact_SerializesFieldsAndExtractsUdiDependencies()
        {
            var contentTypeKey = Guid.NewGuid();
            var contentType = new Mock<IContentType>();
            contentType.SetupGet(c => c.Key).Returns(contentTypeKey);
            contentType.SetupGet(c => c.Name).Returns("Article");
            contentType.SetupGet(c => c.Alias).Returns("article");

            var field = MakeField("title");
            var dto = new DocumentTypeSettingsDto
            {
                Content = contentType.Object,
                Fields = new Dictionary<ISeoField, DocumentTypeValueDto>
                {
                    [field.Object] = new DocumentTypeValueDto
                    {
                        UseInheritedValue = false,
                        Value = "umb://media/1c6f9c9df1c34b2c8a5a2d9a8e4b0c11",
                    },
                },
            };

            var settingsService = new Mock<IMetaFieldsSettingsService>();
            settingsService.Setup(s => s.Get(contentTypeKey)).Returns(dto);
            var contentTypeService = new Mock<IContentTypeService>();
            contentTypeService.Setup(s => s.Get(contentTypeKey)).Returns(contentType.Object);
            var fieldCollection = new SeoFieldCollection(() => new[] { field.Object });

            var connector = new SeoToolkitMetaFieldsSettingServiceConnector(
                settingsService.Object, contentTypeService.Object, fieldCollection, DefaultSettings());

            var udi = new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsSetting, contentTypeKey);
            var artifact = await connector.GetArtifactAsync(udi, Mock.Of<IContextCache>());

            Assert.That(artifact, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(artifact!.Fields, Has.Count.EqualTo(1));
                Assert.That(artifact.Fields[0].Alias, Is.EqualTo("title"));
                Assert.That(artifact.Fields[0].Value, Does.Contain("umb://media/"));
                Assert.That(artifact.Dependencies.Select(d => d.Udi.EntityType),
                    Does.Contain(Constants.UdiEntityType.Media));
                Assert.That(artifact.Dependencies.Select(d => d.Udi),
                    Does.Contain(new GuidUdi(Constants.UdiEntityType.DocumentType, contentTypeKey)));
            });
        }

        [Test]
        public async Task Process_Pass2_RebuildsDtoAndSaves()
        {
            var contentTypeKey = Guid.NewGuid();
            var contentType = new Mock<IContentType>();
            contentType.SetupGet(c => c.Key).Returns(contentTypeKey);
            contentType.SetupGet(c => c.Alias).Returns("article");

            var field = MakeField("title");
            var settingsService = new Mock<IMetaFieldsSettingsService>();
            var contentTypeService = new Mock<IContentTypeService>();
            contentTypeService.Setup(s => s.Get(contentTypeKey)).Returns(contentType.Object);
            var fieldCollection = new SeoFieldCollection(() => new[] { field.Object });

            var connector = new SeoToolkitMetaFieldsSettingServiceConnector(
                settingsService.Object, contentTypeService.Object, fieldCollection, DefaultSettings());

            var udi = new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsSetting, contentTypeKey);
            var artifact = new SeoToolkit.Umbraco.Deploy.Artifacts.MetaFieldsSettingArtifact(udi)
            {
                Name = "Article",
                Fields =
                [
                    new SeoToolkit.Umbraco.Deploy.Artifacts.MetaFieldsSettingField
                    {
                        Alias = "title",
                        UseInheritedValue = false,
                        Value = "\"Hello\"",
                    },
                ],
            };

            DocumentTypeSettingsDto? saved = null;
            settingsService.Setup(s => s.Set(It.IsAny<DocumentTypeSettingsDto>()))
                .Callback<DocumentTypeSettingsDto>(d => saved = d);

            var state = await connector.ProcessInitAsync(artifact, Mock.Of<IDeployContext>());
            await connector.ProcessAsync(state, Mock.Of<IDeployContext>(), 2);

            Assert.That(saved, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(saved!.Content.Key, Is.EqualTo(contentTypeKey));
                Assert.That(saved.Fields, Has.Count.EqualTo(1));
                Assert.That(saved.Fields.Single().Value.Value, Is.EqualTo("Hello"));
            });
        }

        [Test]
        public async Task Process_Pass2_PruneMissing_DropsTargetOnlyFieldsAndInheritance()
        {
            var contentTypeKey = Guid.NewGuid();
            var contentType = new Mock<IContentType>();
            contentType.SetupGet(c => c.Key).Returns(contentTypeKey);
            contentType.SetupGet(c => c.Alias).Returns("article");

            var titleField = MakeField("title");
            var staleField = MakeField("description");

            // Target already has a "description" field and an inheritance that the artifact omits.
            var existing = new DocumentTypeSettingsDto
            {
                Content = contentType.Object,
                Inheritance = Mock.Of<IContentType>(),
                Fields = new Dictionary<ISeoField, DocumentTypeValueDto>
                {
                    [staleField.Object] = new DocumentTypeValueDto { Value = "stale" },
                },
            };

            var settingsService = new Mock<IMetaFieldsSettingsService>();
            settingsService.Setup(s => s.Get(contentTypeKey)).Returns(existing);
            var contentTypeService = new Mock<IContentTypeService>();
            contentTypeService.Setup(s => s.Get(contentTypeKey)).Returns(contentType.Object);
            var fieldCollection = new SeoFieldCollection(() => new[] { titleField.Object, staleField.Object });

            var connector = new SeoToolkitMetaFieldsSettingServiceConnector(
                settingsService.Object, contentTypeService.Object, fieldCollection, DefaultSettings(pruneMissing: true));

            var udi = new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsSetting, contentTypeKey);
            var artifact = new SeoToolkit.Umbraco.Deploy.Artifacts.MetaFieldsSettingArtifact(udi)
            {
                Name = "Article",
                Fields =
                [
                    new SeoToolkit.Umbraco.Deploy.Artifacts.MetaFieldsSettingField
                    {
                        Alias = "title",
                        UseInheritedValue = false,
                        Value = "\"Hello\"",
                    },
                ],
            };

            DocumentTypeSettingsDto? saved = null;
            settingsService.Setup(s => s.Set(It.IsAny<DocumentTypeSettingsDto>()))
                .Callback<DocumentTypeSettingsDto>(d => saved = d);

            var state = await connector.ProcessInitAsync(artifact, Mock.Of<IDeployContext>());
            await connector.ProcessAsync(state, Mock.Of<IDeployContext>(), 2);

            Assert.That(saved, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(saved!.Fields, Has.Count.EqualTo(1));
                Assert.That(saved.Fields.Single().Key.Alias, Is.EqualTo("title"));
                Assert.That(saved.Inheritance, Is.Null);
            });
        }
    }
}
