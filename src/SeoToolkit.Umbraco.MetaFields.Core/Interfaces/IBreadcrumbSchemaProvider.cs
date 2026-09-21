using Schema.NET;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace SeoToolkit.Umbraco.MetaFields.Core.Interfaces
{
    public interface IBreadcrumbSchemaProvider
    {
        /// <summary>
        /// Generates the breadcrumb schema for the given content, or <c>null</c> when no breadcrumb should be rendered.
        /// </summary>
        IThing Get(IPublishedContent content);
    }
}
