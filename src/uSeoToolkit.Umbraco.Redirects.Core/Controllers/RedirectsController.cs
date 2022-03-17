using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using uSeoToolkit.Umbraco.Redirects.Core.Interfaces;
using uSeoToolkit.Umbraco.Redirects.Core.Models.Business;
using uSeoToolkit.Umbraco.Redirects.Core.Models.PostModels;
using uSeoToolkit.Umbraco.Redirects.Core.Models.ViewModels;

namespace uSeoToolkit.Umbraco.Redirects.Core.Controllers
{
    [PluginController("uSeoToolkit")]
    public class RedirectsController : UmbracoAuthorizedApiController
    {
        private readonly IRedirectsService _redirectsService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly ILocalizationService _localizationService;

        public RedirectsController(IRedirectsService redirectsService,
            IUmbracoContextFactory umbracoContextFactory,
            ILocalizationService localizationService)
        {
            _redirectsService = redirectsService;
            _umbracoContextFactory = umbracoContextFactory;
            _localizationService = localizationService;
        }

        [HttpPost]
        public IActionResult Save(SaveRedirectPostModel postModel)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();

            var redirect = new Redirect
            {
                Id = postModel.Id,
                CustomDomain = postModel.CustomDomain,
                IsRegex = postModel.IsRegex,
                OldUrl = postModel.OldUrl,
                NewUrl = postModel.NewUrl,
                RedirectCode = postModel.RedirectCode
            };
            
            if (postModel.Domain != null)
            {
                var foundDomain = ctx.UmbracoContext.Domains.GetAll(false).FirstOrDefault(it => it.Id == postModel.Domain);
                if (foundDomain is null)
                    return new BadRequestResult();
                redirect.Domain = foundDomain;
            }

            if (postModel.NewNodeId != null)
            {
                redirect.NewNode = postModel.NewCultureId != null
                    ? ctx.UmbracoContext.Content.GetById(postModel.NewNodeId.Value)
                    : ctx.UmbracoContext.Media.GetById(postModel.NewNodeId.Value);
                if (redirect.NewNode is null)
                    return new BadRequestResult();
            }

            if (postModel.NewCultureId != null)
            {
                redirect.NewNodeCulture = _localizationService.GetLanguageById(postModel.NewCultureId.Value);
                if (redirect.NewNodeCulture is null)
                    return new BadRequestResult();
            }

            _redirectsService.Save(redirect);
            return Ok();
        }

        public IActionResult GetAll()
        {
            return Ok(_redirectsService.GetAll().Select(it =>
            {
                var domain = it.Domain?.Name ?? it.CustomDomain;
                if (domain?.StartsWith("/") is true)
                    domain = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{domain}";
                return new RedirectListViewModel
                {
                    Id = it.Id,
                    OldUrl = it.OldUrl,
                    NewUrl = it.GetNewUrl(),
                    Domain = domain,
                    StatusCode = it.RedirectCode
                };
            }));
        }

        public IActionResult Get(int id)
        {
            var redirect = _redirectsService.Get(id);
            if (redirect is null)
                return NotFound();
            return Ok(new RedirectViewModel(redirect));
        }

        public IActionResult GetDomains()
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            return Ok(ctx.UmbracoContext.Domains.GetAll(false).Select(it => new
            {
                Id = it.Id,
                Name = it.Name.StartsWith("/") ? $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{it.Name}" : it.Name
            }));
        }

        [HttpPost]
        public IActionResult Delete(DeleteRedirectsPostModel postModel)
        {
            _redirectsService.Delete(postModel.Ids);
            return GetAll();
        }
    }
}
