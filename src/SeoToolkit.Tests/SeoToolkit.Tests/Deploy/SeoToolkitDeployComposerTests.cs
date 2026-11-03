using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using SeoToolkit.Umbraco.Deploy;
using SeoToolkit.Umbraco.Deploy.Composing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;

namespace SeoToolkit.Tests.Deploy
{
    public class SeoToolkitDeployComposerTests
    {
        [Test]
        public void Compose_WithoutUmbracoDeploy_RegistersNoHandlers()
        {
            var (builder, services) = CreateBuilder();

            new SeoToolkitDeployComposer().Compose(builder.Object);

            Assert.That(services.Any(IsNotificationHandler), Is.False);
            builder.Verify(b => b.WithCollectionBuilder<ComponentCollectionBuilder>(), Times.Never);
        }

        [Test]
        public void Compose_WithUmbracoDeploy_RegistersHandlersAndComponent()
        {
            var (builder, services) = CreateBuilder(typeof(global::Umbraco.Deploy.OnPrem.UmbracoDeployOnPremComposer));

            new SeoToolkitDeployComposer().Compose(builder.Object);

            Assert.That(services.Any(IsNotificationHandler), Is.True);
            builder.Verify(b => b.WithCollectionBuilder<ComponentCollectionBuilder>(), Times.Once);
        }

        [TestCase(SeoToolkitDeployConstants.UdiEntityType.SeoSetting)]
        [TestCase(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsSetting)]
        [TestCase(SeoToolkitDeployConstants.UdiEntityType.SitemapPageType)]
        [TestCase(SeoToolkitDeployConstants.UdiEntityType.Script)]
        [TestCase(SeoToolkitDeployConstants.UdiEntityType.DomainCollection)]
        [TestCase(SeoToolkitDeployConstants.UdiEntityType.KeyValues)]
        [TestCase(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue)]
        [TestCase(SeoToolkitDeployConstants.UdiEntityType.SitemapContent)]
        public void Compose_WithUmbracoDeploy_RegistersUdiTypeBeforeComponentsInitialize(string entityType)
        {
            var (builder, _) = CreateBuilder(typeof(global::Umbraco.Deploy.OnPrem.UmbracoDeployOnPremComposer));

            new SeoToolkitDeployComposer().Compose(builder.Object);

            Assert.That(UdiParser.TryParse($"umb://{entityType}/5e0a7f2c9d4b4c6a8e1f3b2a6c9d0e51", out Udi? udi), Is.True);
            Assert.That(udi, Is.TypeOf<GuidUdi>());
        }

        private static bool IsNotificationHandler(ServiceDescriptor descriptor)
            => descriptor.ServiceType.IsGenericType
               && descriptor.ServiceType.GetGenericTypeDefinition() == typeof(INotificationAsyncHandler<>);

        private static (Mock<IUmbracoBuilder> Builder, IServiceCollection Services) CreateBuilder(params Type[] composerTypes)
        {
            var typeFinder = new Mock<ITypeFinder>();
            typeFinder.SetupGet(f => f.AssembliesToScan).Returns(Array.Empty<Assembly>());
            // IComposer is IDiscoverable, so the loader scans for IDiscoverable and filters from there.
            typeFinder
                .Setup(f => f.FindClassesOfType(typeof(IDiscoverable), It.IsAny<IEnumerable<Assembly>>(), It.IsAny<bool>()))
                .Returns(composerTypes);

            var services = new ServiceCollection();
            var builder = new Mock<IUmbracoBuilder>();
            builder.SetupGet(b => b.Services).Returns(services);
            builder.SetupGet(b => b.Config).Returns(new ConfigurationBuilder().Build());
            builder.SetupGet(b => b.TypeLoader).Returns(new TypeLoader(typeFinder.Object, NullLogger<TypeLoader>.Instance));
            builder.Setup(b => b.WithCollectionBuilder<ComponentCollectionBuilder>()).Returns(new ComponentCollectionBuilder());

            return (builder, services);
        }
    }
}

// Stand-in for Umbraco Deploy's OnPrem composer: the detector matches it by full type name.
namespace Umbraco.Deploy.OnPrem
{
    internal class UmbracoDeployOnPremComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
        }
    }
}
