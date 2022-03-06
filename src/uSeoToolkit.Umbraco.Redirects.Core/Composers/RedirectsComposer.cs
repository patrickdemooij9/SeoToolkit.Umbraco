using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;
using uSeoToolkit.Umbraco.Redirects.Core.Interfaces;
using uSeoToolkit.Umbraco.Redirects.Core.Repositories;
using uSeoToolkit.Umbraco.Redirects.Core.Services;

namespace uSeoToolkit.Umbraco.Redirects.Core.Composers
{
    public class RedirectsComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddUnique<IRedirectsRepository, RedirectsRepository>();
            builder.Services.AddUnique<IRedirectsService, RedirectsService>();
        }
    }
}
