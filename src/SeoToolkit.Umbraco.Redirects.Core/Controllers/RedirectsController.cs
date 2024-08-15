using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SeoToolkit.Umbraco.Redirects.Core.Constants;
using SeoToolkit.Umbraco.Redirects.Core.Enumerators;
using SeoToolkit.Umbraco.Redirects.Core.Helpers;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using SeoToolkit.Umbraco.Redirects.Core.Interfaces;
using SeoToolkit.Umbraco.Redirects.Core.Models.Business;
using SeoToolkit.Umbraco.Redirects.Core.Models.PostModels;
using SeoToolkit.Umbraco.Redirects.Core.Models.ViewModels;
using Umbraco.Cms.Core.Security;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.Redirects.Core.Controllers
{
    [PluginController("SeoToolkit")]
    public class RedirectsController : UmbracoAuthorizedApiController
    {
        private readonly IRedirectsService _redirectsService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly ILocalizationService _localizationService;
        private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
        private readonly RedirectsImportHelper _redirectsImportHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        

        public RedirectsController(IRedirectsService redirectsService,
            IUmbracoContextFactory umbracoContextFactory,
            ILocalizationService localizationService,
            IBackOfficeSecurityAccessor backOfficeSecurityAccessor, RedirectsImportHelper redirectsImportHelper, IHttpContextAccessor httpContextAccessor)
        {
            _redirectsService = redirectsService;
            _umbracoContextFactory = umbracoContextFactory;
            _localizationService = localizationService;
            _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
            _redirectsImportHelper = redirectsImportHelper;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost]
        public IActionResult Save(SaveRedirectPostModel postModel)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();

            var redirect = new Redirect
            {
                Id = postModel.Id,
                CustomDomain = postModel.CustomDomain,
                IsEnabled = postModel.IsEnabled,
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

            if (postModel.Id == 0)
            {
                redirect.CreatedBy = -1;
                var getUserAttempt = _backOfficeSecurityAccessor.BackOfficeSecurity?.GetUserId();
                if (getUserAttempt?.Success is true)
                {
                    redirect.CreatedBy = getUserAttempt.Value.Result;
                }
            }

            _redirectsService.Save(redirect);
            return Ok();
        }

        public IActionResult GetAll(int pageNumber, int pageSize, string orderBy = null, string orderDirection = null, string search = "")
        {
            var redirectsPaged = _redirectsService.GetAll(pageNumber, pageSize, orderBy, orderDirection, search);
            var viewModels = redirectsPaged.Items.Select(it =>
            {
                var domain = it.Domain?.Name ?? it.CustomDomain;
                if (domain?.StartsWith("/") is true)
                    domain = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{domain}";
                return new RedirectListViewModel
                {
                    Id = it.Id,
                    IsEnabled = it.IsEnabled,
                    OldUrl = it.OldUrl.IfNullOrWhiteSpace("/"),
                    NewUrl = it.GetNewUrl(),
                    Domain = domain,
                    StatusCode = it.RedirectCode,
                    LastUpdated = it.LastUpdated.ToShortDateString()
                };
            });
            return Ok(new PagedResult<RedirectListViewModel>(redirectsPaged.TotalItems, pageNumber, pageSize) { Items = viewModels });
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
            return GetAll(1, 20);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public IActionResult Validate(ImportRedirectsFileExtension fileExtension, string domain, IFormFile file)
        {
            if (file.Length == 0)
            {
                return BadRequest("Please select a file");
            }
        
            using var memoryStream = new MemoryStream();
            file.CopyTo(memoryStream);

            var result = _redirectsImportHelper.Validate(fileExtension, memoryStream, domain);
            if (result.Success)
            {
                if (_httpContextAccessor.HttpContext is null)
                {
                    return BadRequest("Could not get context, please try again");
                }

                // Storing the file contents in session for later import
                _httpContextAccessor.HttpContext.Session.Set(ImportConstants.SessionAlias, memoryStream.ToArray());
                _httpContextAccessor.HttpContext.Session.SetString(ImportConstants.SessionFileTypeAlias, fileExtension.ToString());
                _httpContextAccessor.HttpContext.Session.SetString(ImportConstants.SessionDomainId, domain);

                return Ok();
            }

            return UnprocessableEntity(!string.IsNullOrWhiteSpace(result.Status) ? result.Status : "Something went wrong during the validation");
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public IActionResult Import()
        {
            var fileContent = HttpContext.Session.Get(ImportConstants.SessionAlias);
            var fileExtensionString = HttpContext.Session.GetString(ImportConstants.SessionFileTypeAlias);
            var domain= HttpContext.Session.GetString(ImportConstants.SessionDomainId);

            if (fileContent == null || fileExtensionString == null || domain == null)
            {
                return BadRequest("Something went wrong during import, please try again");
            }
        
            if (!Enum.TryParse(fileExtensionString, out ImportRedirectsFileExtension fileExtension))
            {
                return UnprocessableEntity("Invalid file extension.");
            }
        
            using var memoryStream = new MemoryStream(fileContent);
            var result = _redirectsImportHelper.Import(fileExtension, memoryStream, domain);
            if (result.Success)
            {
                return Ok();
            }
        
            return UnprocessableEntity(!string.IsNullOrWhiteSpace(result.Status) ? result.Status : "Something went wrong during the import");
        }
    }
}
