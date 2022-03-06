using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using uSeoToolkit.Umbraco.Redirects.Core.Interfaces;
using uSeoToolkit.Umbraco.Redirects.Core.Models.ViewModels;

namespace uSeoToolkit.Umbraco.Redirects.Core.Controllers
{
    [PluginController("uSeoToolkit")]
    public class RedirectsController : UmbracoAuthorizedApiController
    {
        private readonly IRedirectsService _redirectsService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public RedirectsController(IRedirectsService redirectsService, IUmbracoContextFactory umbracoContextFactory)
        {
            _redirectsService = redirectsService;
            _umbracoContextFactory = umbracoContextFactory;
        }

        public IActionResult GetAll()
        {
            return Ok(_redirectsService.GetAll().Select(it => new RedirectViewModel
            {
                Id = it.Id,
                OldUrl = it.OldUrl,
                NewUrl = it.GetNewUrl()
            }));
        }

        public IActionResult GetDomains()
        {
            using (var ctx = _umbracoContextFactory.EnsureUmbracoContext())
            {
                return Ok(ctx.UmbracoContext.Domains.GetAll(false));
            }
        }
    }
}
