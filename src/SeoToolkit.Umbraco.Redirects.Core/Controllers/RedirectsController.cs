using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SeoToolkit.Umbraco.Redirects.Core.Constants;
using SeoToolkit.Umbraco.Redirects.Core.Enumerators;
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

        public RedirectsController(IRedirectsService redirectsService,
            IUmbracoContextFactory umbracoContextFactory,
            ILocalizationService localizationService,
            IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        {
            _redirectsService = redirectsService;
            _umbracoContextFactory = umbracoContextFactory;
            _localizationService = localizationService;
            _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
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
        public IActionResult ValidateRedirects(ImportRedirectsFileExtension fileExtension, string domain)
        {
            var files = HttpContext.Request.Form.Files;
            if (files.Count != 1 || files[0].Length == 0)
            {
                return BadRequest(new { isValid = false, Error = "Please select a file"});
            }
        
            var file = HttpContext.Request.Form.Files[0];
            using var memoryStream = new MemoryStream();
            file.CopyTo(memoryStream);

            return Ok(Validate(fileExtension, memoryStream, domain, false));
        }

        private IActionResult Validate(ImportRedirectsFileExtension fileExtension, MemoryStream memoryStream,
            string domain, bool importFile)
        {
            HttpResponseMessage validationResult;
            switch (fileExtension)
            {
                case ImportRedirectsFileExtension.Csv:
                    validationResult = redirectsImportHelper.ImportCsv(memoryStream, importFile, domain);
                    break;
                case ImportRedirectsFileExtension.Excel:
                    validationResult = redirectsImportHelper.ImportExcel(memoryStream, importFile, domain);
                    break;
                default:
                    return BadRequest("Invalid filetype, you may only use .csv or .xls");
            }
        }

        public IActionResult ImportRedirects()
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
                return BadRequest("Invalid file extension.");
            }
        
            using var memoryStream = new MemoryStream(fileContent);
            return Ok(Validate(fileExtension, memoryStream, domain,true));
        }
    }
}
