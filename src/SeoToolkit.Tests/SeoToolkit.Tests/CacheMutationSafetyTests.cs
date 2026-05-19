using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Html;
using Moq;
using Newtonsoft.Json.Linq;
using SeoToolkit.Umbraco.Common.Core.Models.Business;
using SeoToolkit.Umbraco.Common.Core.Repositories.Domains;
using SeoToolkit.Umbraco.Common.Core.Services.Domains;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using SeoToolkit.Umbraco.MetaFields.Core.Collections;
using SeoToolkit.Umbraco.MetaFields.Core.Common.FieldProviders;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using SeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.Business;
using SeoToolkit.Umbraco.MetaFields.Core.Repositories.DocumentTypeSettingsRepository;
using SeoToolkit.Umbraco.MetaFields.Core.Repositories.SeoValueRepository;
using SeoToolkit.Umbraco.MetaFields.Core.Services.DocumentTypeSettings;
using SeoToolkit.Umbraco.MetaFields.Core.Services.SeoValueService;
using SeoToolkit.Umbraco.ScriptManager.Core.Config.Models;
using SeoToolkit.Umbraco.ScriptManager.Core.Enums;
using SeoToolkit.Umbraco.ScriptManager.Core.Interfaces;
using SeoToolkit.Umbraco.ScriptManager.Core.Models.Business;
using SeoToolkit.Umbraco.ScriptManager.Core.Services;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Sync;

namespace SeoToolkit.Tests
{
    [TestFixture]
    public class CacheMutationSafetyTests
    {
        [Test]
        public void SeoDomainsService_GetAll_ReturnsDetachedCopies()
        {
            var repository = new Mock<ISeoDomainsRepository>();
            repository.Setup(x => x.GetAll()).Returns([
                new SeoDomainCollection
                {
                    Id = Guid.NewGuid(),
                    Name = "Main",
                    DomainIds = [1],
                    Settings = new Dictionary<string, string> { ["enabled"] = "true" }
                }
            ]);

            var service = new SeoDomainsService(repository.Object, CreateAppCaches(), CreateDistributedCache());

            var first = service.GetAll();
            first[0].Name = "Updated";
            first[0].DomainIds.Add(2);
            first[0].Settings["enabled"] = "false";

            var second = service.GetAll();

            Assert.Multiple(() =>
            {
                Assert.That(second[0].Name, Is.EqualTo("Main"));
                Assert.That(second[0].DomainIds, Is.EqualTo(new[] { 1 }));
                Assert.That(second[0].Settings["enabled"], Is.EqualTo("true"));
            });
            repository.Verify(x => x.GetAll(), Times.Once);
        }

        [Test]
        public void MetaFieldsValueService_GetUserValues_ReturnsDetachedCopies()
        {
            var repository = new Mock<IMetaFieldsValueRepository>();
            var contentId = Guid.NewGuid();
            repository.Setup(x => x.GetAllValues(contentId, "en-US")).Returns(new Dictionary<string, object>
            {
                ["robots"] = new JArray("noindex")
            });

            var variationContextAccessor = new Mock<IVariationContextAccessor>();
            variationContextAccessor.SetupProperty(x => x.VariationContext, new VariationContext("en-US"));

            var service = new MetaFieldsValueService(repository.Object, variationContextAccessor.Object, CreateAppCaches(), CreateDistributedCache());

            var first = service.GetUserValues(contentId, "en-US");
            ((JArray)first["robots"]).Add("nofollow");
            first["title"] = "Changed";

            var second = service.GetUserValues(contentId, "en-US");

            Assert.Multiple(() =>
            {
                Assert.That(((JArray)second["robots"]).Values<string>(), Is.EqualTo(new[] { "noindex" }));
                Assert.That(second.ContainsKey("title"), Is.False);
            });
            repository.Verify(x => x.GetAllValues(contentId, "en-US"), Times.Once);
        }

        [Test]
        public void MetaFieldsSettingsService_Get_ReturnsDetachedCopies()
        {
            var repository = new Mock<IMetaFieldsSettingsRepository>();
            var contentTypeKey = Guid.NewGuid();
            var field = new Mock<ISeoField>();
            field.SetupGet(x => x.Alias).Returns("robots");

            repository.Setup(x => x.Get(contentTypeKey)).Returns(new DocumentTypeSettingsDto
            {
                Content = new Mock<IContentType>().Object,
                Fields = new Dictionary<ISeoField, DocumentTypeValueDto>
                {
                    [field.Object] = new()
                    {
                        UseInheritedValue = false,
                        Value = new JArray("noindex")
                    }
                }
            });

            var service = new MetaFieldsSettingsService(
                repository.Object,
                new FieldProviderCollection(() => Enumerable.Empty<IFieldProvider>()),
                CreateAppCaches(),
                CreateDistributedCache(),
                Mock.Of<IEventAggregator>());

            var first = service.Get(contentTypeKey)!;
            first.Fields[field.Object].UseInheritedValue = true;
            ((JArray)first.Fields[field.Object].Value).Add("nofollow");

            var second = service.Get(contentTypeKey)!;

            Assert.Multiple(() =>
            {
                Assert.That(second.Fields[field.Object].UseInheritedValue, Is.False);
                Assert.That(((JArray)second.Fields[field.Object].Value).Values<string>(), Is.EqualTo(new[] { "noindex" }));
            });
            repository.Verify(x => x.Get(contentTypeKey), Times.Once);
        }

        [Test]
        public void ScriptManagerService_Get_ReturnsDetachedCopies()
        {
            var repository = new Mock<IScriptRepository>();
            var scriptKey = Guid.NewGuid();
            repository.Setup(x => x.Get(scriptKey)).Returns(new Script
            {
                Key = scriptKey,
                Name = "Analytics",
                Config = new Dictionary<string, string> { ["src"] = "https://example.com" },
                SortOrder = 1
            });

            var settings = new Mock<ISettingsService<ScriptManagerConfigModel>>();
            settings.Setup(x => x.GetSettings()).Returns(new ScriptManagerConfigModel());

            var service = new ScriptManagerService(repository.Object, CreateAppCaches(), CreateDistributedCache(), settings.Object);

            var first = service.Get(scriptKey)!;
            first.Name = "Changed";
            first.Config["src"] = "https://changed.com";

            var second = service.Get(scriptKey)!;

            Assert.Multiple(() =>
            {
                Assert.That(second.Name, Is.EqualTo("Analytics"));
                Assert.That(second.Config["src"], Is.EqualTo("https://example.com"));
            });
            repository.Verify(x => x.Get(scriptKey), Times.Once);
        }

        [Test]
        public void ScriptManagerService_GetRender_ReturnsDetachedCopies()
        {
            var definition = new Mock<IScriptDefinition>();
            definition.Setup(x => x.Render(It.IsAny<ScriptRenderModel>(), It.IsAny<Dictionary<string, string>>()))
                .Callback<ScriptRenderModel, Dictionary<string, string>>((model, _) =>
                {
                    model.AddScript(ScriptPositionType.HeadBottom, new HtmlString("<script src=\"https://example.com\"></script>"));
                });

            var repository = new Mock<IScriptRepository>();
            repository.Setup(x => x.GetAll(null)).Returns([
                new Script
                {
                    Key = Guid.NewGuid(),
                    Name = "Analytics",
                    Definition = definition.Object,
                    Config = [],
                    SortOrder = 1
                }
            ]);

            var settings = new Mock<ISettingsService<ScriptManagerConfigModel>>();
            settings.Setup(x => x.GetSettings()).Returns(new ScriptManagerConfigModel
            {
                DisableRenderCaching = false
            });

            var service = new ScriptManagerService(repository.Object, CreateAppCaches(), CreateDistributedCache(), settings.Object);

            var first = service.GetRender(null);
            first.AddScript(ScriptPositionType.HeadBottom, new HtmlString("<script src=\"https://changed.com\"></script>"));

            var second = service.GetRender(null);

            Assert.That(second.Get(ScriptPositionType.HeadBottom).Select(x => x.ToString()), Is.EqualTo(new[] { "<script src=\"https://example.com\"></script>" }));
            repository.Verify(x => x.GetAll(null), Times.Once);
        }

        private static AppCaches CreateAppCaches()
        {
            return new AppCaches(
                new ObjectCacheAppCache(),
                Mock.Of<IRequestCache>(),
                new IsolatedCaches(_ => new ObjectCacheAppCache()));
        }

        private static DistributedCache CreateDistributedCache()
        {
            return new DistributedCache(
                Mock.Of<IServerMessenger>(),
                new CacheRefresherCollection(() => Enumerable.Empty<ICacheRefresher>()));
        }
    }
}
