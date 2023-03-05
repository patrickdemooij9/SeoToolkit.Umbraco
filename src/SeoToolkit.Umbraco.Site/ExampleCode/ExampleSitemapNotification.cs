using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Services;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using SeoToolkit.Umbraco.Sitemap.Core.Notifications;
using System.Linq;
using Umbraco.Cms.Core.Events;

namespace SeoToolkit.Umbraco.Site.ExampleCode
{
    public class ExampleSitemapNotification : INotificationHandler<GenerateSitemapNotification>
    {
		private readonly IMetaFieldsValueService _metaFieldsValueService;

		public ExampleSitemapNotification(IMetaFieldsValueService metaFieldsValueService)
        {
			_metaFieldsValueService = metaFieldsValueService;
		}

        public void Handle(GenerateSitemapNotification notification)
        {
            notification.Nodes.Add(new SitemapNodeItem("https://google.nl"));
		}
    }
}
