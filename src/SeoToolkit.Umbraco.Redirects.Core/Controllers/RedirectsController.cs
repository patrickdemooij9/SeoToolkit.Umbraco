using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SeoToolkit.Umbraco.Common.Core.Controllers;
using SeoToolkit.Umbraco.Redirects.Core.Constants;
using SeoToolkit.Umbraco.Redirects.Core.Enumerators;
using SeoToolkit.Umbraco.Redirects.Core.Helpers;
using SeoToolkit.Umbraco.Redirects.Core.Interfaces;
using SeoToolkit.Umbraco.Redirects.Core.Models.Business;
using SeoToolkit.Umbraco.Redirects.Core.Models.PostModels;
using SeoToolkit.Umbraco.Redirects.Core.Models.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.Redirects.Core.Controllers
{
    [ApiExplorerSettings(GroupName = "Backoffice SeoToolkit Redirects")]
    [BackOfficeRoute("seoToolkitRedirects")]
    public class RedirectsController : SeoToolkitAuthenticatedControllerBase
    {
        private readonly IRedirectsService _redirectsService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly ILanguageService _languageService;
        private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
        private readonly RedirectsImportHelper _redirectsImportHelper;
        private readonly ITemporaryFileService _temporaryFileService;

        public RedirectsController(IRedirectsService redirectsService,
            IUmbracoContextFactory umbracoContextFactory,
            ILanguageService languageService,
            IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
            RedirectsImportHelper redirectsImportHelper,
            ITemporaryFileService temporaryFileService)
        {
            _redirectsService = redirectsService;
            _umbracoContextFactory = umbracoContextFactory;
            _languageService = languageService;
            _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
            _redirectsImportHelper = redirectsImportHelper;
            _temporaryFileService = temporaryFileService;
        }

        [HttpPost("redirect")]
        public async Task<IActionResult> Save(SaveRedirectPostModel postModel)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();

            var redirect = new Redirect
            {
                Id = postModel.Id,
                Key = postModel.Key ?? Guid.NewGuid(),
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

            if (!string.IsNullOrWhiteSpace(postModel.NewCultureId))
            {
                var languages = await _languageService.GetAllAsync();
                redirect.NewNodeCulture = languages.FirstOrDefault(it => it.IsoCode == postModel.NewCultureId);
                if (redirect.NewNodeCulture is null)
                    return new BadRequestResult();
            }

            if (!postModel.Key.HasValue)
            {
                redirect.CreatedBy = null;
                var userId = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.Key;
                if (userId.HasValue)
                {
                    redirect.CreatedBy = userId.Value;
                }
            }

            _redirectsService.Save(redirect);
            return Ok();
        }

        [HttpPost("updateStatusCodes")]
        public IActionResult UpdateStatusCodes(UpdateStatusCodesRedirectPostModel postModel)
        {
            _redirectsService.UpdateRedirectCodes(postModel.RedirectIds, postModel.RedirectCode);
            return Ok();
        }

        [HttpGet("redirects")]
        [ProducesResponseType(typeof(PagedViewModel<RedirectListViewModel>), 200)]
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
                    Key = it.Key,
                    IsEnabled = it.IsEnabled,
                    OldUrl = it.OldUrl.IfNullOrWhiteSpace("/"),
                    NewUrl = it.GetNewUrl(),
                    Domain = domain,
                    StatusCode = it.RedirectCode,
                    LastUpdated = it.LastUpdated.ToShortDateString()
                };
            });
            return Ok(new PagedViewModel<RedirectListViewModel>() { Total = redirectsPaged.TotalItems, Items = viewModels });
        }

        [HttpGet("redirect")]
        [ProducesResponseType(typeof(RedirectViewModel), 200)]
        public IActionResult Get(Guid id)
        {
            var redirect = _redirectsService.Get(id);
            if (redirect is null)
                return NotFound();
            return Ok(new RedirectViewModel(redirect));
        }

        [HttpGet("domains")]
        [ProducesResponseType(typeof(DomainViewModel[]), 200)]
        public IActionResult GetDomains()
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            return Ok(ctx.UmbracoContext.Domains.GetAll(false).Select(it => new DomainViewModel
            {
                Id = it.Id,
                Name = it.Name.StartsWith("/") ? $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{it.Name}" : it.Name
            }));
        }

        [HttpDelete("redirect")]
        public IActionResult Delete(DeleteRedirectsPostModel postModel)
        {
            _redirectsService.Delete(postModel.Ids);
            return Ok();
        }

        [HttpGet("export")]
        public IActionResult Export()
        {
            // Get all redirects (use a very large page size to ensure all records are returned)
            var redirectsPaged = _redirectsService.GetAll(1, int.MaxValue);
            var redirects = redirectsPaged.Items;

            using var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream, System.Text.Encoding.UTF8, 1024, leaveOpen: true))
            {
                writer.WriteLine("From,To,Domain,Status code,Enabled");
                foreach (var it in redirects)
                {
                    var domain = it.Domain?.Name ?? it.CustomDomain;
                    if (domain?.StartsWith("/") is true)
                        domain = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{domain}";

                    // Local helper to escape CSV values
                    static string EscapeCsv(string? value)
                    {
                        if (string.IsNullOrEmpty(value)) return string.Empty;
                        var escaped = value.Replace("\"", "\"\"").Replace("\r", " ").Replace("\n", " ");
                        if (escaped.Contains(',') || escaped.Contains('"') || escaped.Contains('\n') || escaped.Contains('\r'))
                            return $"\"{escaped}\"";
                        return escaped;
                    }

                    var from = EscapeCsv(it.OldUrl);
                    var to = EscapeCsv(it.GetNewUrl());
                    var domainEsc = EscapeCsv(domain);
                    var status = it.RedirectCode;
                    var enabled = it.IsEnabled ? "true" : "false";

                    writer.WriteLine($"{from},{to},{domainEsc},{status},{enabled}");
                }
            }

            memoryStream.Position = 0;
            return File(memoryStream.ToArray(), "text/csv; charset=utf-8", "redirects.csv");
        }

        [HttpPost("validate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Validate(ImportRedirectsFileExtension fileExtension, int domain, Guid tempFileId)
        {
            var file = await _temporaryFileService.GetAsync(tempFileId);
            if (file is null)
            {
                return BadRequest("Please select a file");
            }

            using var memoryStream = new MemoryStream();
            file.OpenReadStream().CopyTo(memoryStream);

            var result = _redirectsImportHelper.Validate(fileExtension, memoryStream, domain);
            if (result.Success)
            {
                // Storing the file contents in session for later import
                HttpContext.Session.Set(ImportConstants.SessionAlias, memoryStream.ToArray());
                HttpContext.Session.SetString(ImportConstants.SessionFileTypeAlias, fileExtension.ToString());
                HttpContext.Session.SetString(ImportConstants.SessionDomainId, domain.ToString());

                return Ok();
            }

            var problemDetailsBuilder = new ProblemDetailsBuilder();
            return UnprocessableEntity(problemDetailsBuilder.WithOperationStatus(ContentEditingOperationStatus.Unknown).WithTitle(!string.IsNullOrWhiteSpace(result.Status) ? result.Status : "Something went wrong during the validation").Build());
        }

        [HttpPost("import")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public IActionResult Import()
        {
            var fileContent = HttpContext.Session.Get(ImportConstants.SessionAlias);
            var fileExtensionString = HttpContext.Session.GetString(ImportConstants.SessionFileTypeAlias);
            var domain = int.Parse(HttpContext.Session.GetString(ImportConstants.SessionDomainId));

            if (fileContent == null || fileExtensionString == null)
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
