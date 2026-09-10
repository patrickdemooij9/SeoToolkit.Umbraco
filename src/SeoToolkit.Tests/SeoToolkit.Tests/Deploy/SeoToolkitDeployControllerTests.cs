using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SeoToolkit.Umbraco.Deploy;
using SeoToolkit.Umbraco.Deploy.Controllers;
using SeoToolkit.Umbraco.Deploy.Models;
using SeoToolkit.Umbraco.MetaFields.Core.Repositories.SeoValueRepository;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using SeoToolkit.Umbraco.Sitemap.Core.Services.SitemapService;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Deploy.Core.Connectors.ServiceConnectors;
using Umbraco.Deploy.Core.Transfer.Queue;

namespace SeoToolkit.Tests.Deploy
{
    [TestFixture]
    public class SeoToolkitDeployControllerTests
    {
        private Mock<IMetaFieldsValueRepository> _valueRepository = null!;
        private Mock<ISitemapService> _sitemapService = null!;
        private Mock<IContentService> _contentService = null!;
        private Mock<IServiceConnectorFactory> _serviceConnectorFactory = null!;
        private Mock<ITransferQueue> _transferQueue = null!;
        private Mock<IBackOfficeSecurityAccessor> _backOfficeSecurityAccessor = null!;
        private SeoToolkitDeployController _controller = null!;

        [SetUp]
        public void SetUp()
        {
            _valueRepository = new Mock<IMetaFieldsValueRepository>();
            _sitemapService = new Mock<ISitemapService>();
            _contentService = new Mock<IContentService>();
            _serviceConnectorFactory = new Mock<IServiceConnectorFactory>();
            _transferQueue = new Mock<ITransferQueue>();
            _backOfficeSecurityAccessor = new Mock<IBackOfficeSecurityAccessor>();
            _controller = new SeoToolkitDeployController(
                _valueRepository.Object, _sitemapService.Object, _contentService.Object,
                _serviceConnectorFactory.Object, _transferQueue.Object, _backOfficeSecurityAccessor.Object);
        }

        private void SignInUser(int userId)
        {
            _backOfficeSecurityAccessor.Setup(a => a.BackOfficeSecurity)
                .Returns(Mock.Of<IBackOfficeSecurity>(s => s.CurrentUser == Mock.Of<IUser>(u => u.Id == userId)));
        }

        private void SetUpConnector(string entityType)
        {
            var connector = new Mock<IServiceConnector>();
            connector.Setup(c => c.GetRangeAsync(entityType, It.IsAny<string>(), "this", It.IsAny<CancellationToken>()))
                .ReturnsAsync((string et, string sid, string sel, CancellationToken _) =>
                    new NamedUdiRange(new GuidUdi(et, Guid.Parse(sid)), "name", sel));
            _serviceConnectorFactory.Setup(f => f.GetConnector(entityType)).Returns(connector.Object);
        }

        private static IReadOnlyList<SeoTransferItem> Items(IActionResult result)
        {
            var ok = (OkObjectResult)result;
            return ((SeoTransferItemsViewModel)ok.Value!).Items;
        }

        [Test]
        public void SingleNode_ReturnsOnlyThatNodesSeoItems()
        {
            var key = Guid.NewGuid();
            _valueRepository.Setup(r => r.HasAnyValues(key)).Returns(true);
            _sitemapService.Setup(s => s.GetContentSettings(key)).Returns((SitemapContentSettings?)null);

            var items = Items(_controller.GetSeoTransferItems(key));

            Assert.That(items, Has.Count.EqualTo(1));
            Assert.That(items[0].Id, Is.EqualTo(key.ToString()));
            Assert.That(items[0].EntityType,
                Is.EqualTo(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue));
            _contentService.Verify(s => s.GetById(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public void IncludeDescendants_AddsSeoItemsForDescendantNodes()
        {
            var rootKey = Guid.NewGuid();
            var childKey = Guid.NewGuid();
            _valueRepository.Setup(r => r.HasAnyValues(rootKey)).Returns(true);
            _valueRepository.Setup(r => r.HasAnyValues(childKey)).Returns(true);
            _sitemapService.Setup(s => s.GetContentSettings(It.IsAny<Guid>()))
                .Returns((SitemapContentSettings?)null);

            _contentService.Setup(s => s.GetById(rootKey))
                .Returns(Mock.Of<IContent>(c => c.Id == 100));
            long total = 1;
            _contentService.Setup(s => s.GetPagedDescendants(
                    100, It.IsAny<long>(), It.IsAny<int>(), out total,
                    It.IsAny<IQuery<IContent>?>(), It.IsAny<Ordering?>()))
                .Returns(new[] { Mock.Of<IContent>(c => c.Key == childKey) });

            var items = Items(_controller.GetSeoTransferItems(rootKey, includeDescendants: true));

            Assert.That(items.Select(i => i.Id),
                Is.EquivalentTo(new[] { rootKey.ToString(), childKey.ToString() }));
        }

        [Test]
        public async Task AddSeoToQueue_QueuesEachSeoEntityServerSide_ReturnsCount()
        {
            var key = Guid.NewGuid();
            SignInUser(7);
            _valueRepository.Setup(r => r.HasAnyValues(key)).Returns(true);
            _sitemapService.Setup(s => s.GetContentSettings(key))
                .Returns(new SitemapContentSettings { NodeKey = key });
            SetUpConnector(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue);
            SetUpConnector(SeoToolkitDeployConstants.UdiEntityType.SitemapContent);
            _transferQueue.Setup(q => q.Add(7, It.IsAny<QueueItem>())).Returns(true);

            var result = (SeoQueueAddResult)((OkObjectResult)await _controller.AddSeoToQueue(key)).Value!;

            Assert.That(result.Added, Is.EqualTo(2));
            _transferQueue.Verify(q => q.Add(7, It.IsAny<QueueItem>()), Times.Exactly(2));
        }

        [Test]
        public async Task AddSeoToQueue_IncludeDescendants_QueuesDescendantSeoToo()
        {
            var rootKey = Guid.NewGuid();
            var childKey = Guid.NewGuid();
            SignInUser(7);
            _valueRepository.Setup(r => r.HasAnyValues(It.IsAny<Guid>())).Returns(true);
            _sitemapService.Setup(s => s.GetContentSettings(It.IsAny<Guid>())).Returns((SitemapContentSettings?)null);
            SetUpConnector(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue);

            _contentService.Setup(s => s.GetById(rootKey)).Returns(Mock.Of<IContent>(c => c.Id == 100));
            long total = 1;
            _contentService.Setup(s => s.GetPagedDescendants(
                    100, It.IsAny<long>(), It.IsAny<int>(), out total,
                    It.IsAny<IQuery<IContent>?>(), It.IsAny<Ordering?>()))
                .Returns(new[] { Mock.Of<IContent>(c => c.Key == childKey) });
            _transferQueue.Setup(q => q.Add(7, It.IsAny<QueueItem>())).Returns(true);

            var result = (SeoQueueAddResult)((OkObjectResult)await _controller.AddSeoToQueue(rootKey, includeDescendants: true)).Value!;

            // metafields value for root + child = 2 (no sitemap settings on either)
            Assert.That(result.Added, Is.EqualTo(2));
        }

        [Test]
        public async Task AddSeoToQueue_NoSignedInUser_ReturnsUnauthorized()
        {
            var result = await _controller.AddSeoToQueue(Guid.NewGuid());

            Assert.That(result, Is.InstanceOf<UnauthorizedResult>());
        }
    }
}
