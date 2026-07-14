using Moq;
using NUnit.Framework;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using SeoToolkit.Umbraco.ScriptManager.Core.Caching;
using SeoToolkit.Umbraco.ScriptManager.Core.Config.Models;
using SeoToolkit.Umbraco.ScriptManager.Core.Interfaces;
using SeoToolkit.Umbraco.ScriptManager.Core.Models.Business;
using SeoToolkit.Umbraco.ScriptManager.Core.Services;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Sync;

namespace SeoToolkit.Tests.Deploy
{
    [TestFixture]
    public class ScriptManagerServiceSaveTests
    {
        private static ScriptManagerService CreateService(Mock<IScriptRepository> repository)
        {
            var refresher = new Mock<ICacheRefresher>();
            refresher.SetupGet(r => r.RefresherUniqueId).Returns(ScriptManagerCacheRefresher.CacheGuid);
            var cacheRefreshers = new CacheRefresherCollection(() => new[] { refresher.Object });
            var distributedCache = new DistributedCache(Mock.Of<IServerMessenger>(), cacheRefreshers);
            var settings = new Mock<ISettingsService<ScriptManagerConfigModel>>();

            return new ScriptManagerService(repository.Object, AppCaches.NoCache, distributedCache, settings.Object);
        }

        [Test]
        public void Save_WithPresetKey_NotInTarget_Inserts()
        {
            var key = Guid.NewGuid();
            var repository = new Mock<IScriptRepository>();
            repository.Setup(r => r.Get(key)).Returns((Script?)null);
            repository.Setup(r => r.Add(It.IsAny<Script>())).Returns<Script>(s => s);

            var service = CreateService(repository);
            service.Save(new Script { Key = key, Name = "Transferred", Config = new Dictionary<string, string>() });

            repository.Verify(r => r.Add(It.Is<Script>(s => s.Key == key)), Times.Once);
            repository.Verify(r => r.Update(It.IsAny<Script>()), Times.Never);
        }

        [Test]
        public void Save_WithPresetKey_Existing_Updates()
        {
            var key = Guid.NewGuid();
            var repository = new Mock<IScriptRepository>();
            repository.Setup(r => r.Get(key)).Returns(new Script { Key = key, Config = new Dictionary<string, string>() });
            repository.Setup(r => r.Update(It.IsAny<Script>())).Returns<Script>(s => s);

            var service = CreateService(repository);
            service.Save(new Script { Key = key, Name = "Existing", Config = new Dictionary<string, string>() });

            repository.Verify(r => r.Update(It.Is<Script>(s => s.Key == key)), Times.Once);
            repository.Verify(r => r.Add(It.IsAny<Script>()), Times.Never);
        }
    }
}
