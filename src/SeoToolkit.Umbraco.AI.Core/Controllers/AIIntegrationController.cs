using Microsoft.AspNetCore.Mvc;
using SeoToolkit.Umbraco.AI.Core.Models;
using SeoToolkit.Umbraco.AI.Core.Services;
using SeoToolkit.Umbraco.Common.Core.Controllers;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Routing;

namespace SeoToolkit.Umbraco.AI.Core.Controllers
{
    [ApiExplorerSettings(GroupName = "Backoffice SeoToolkit AI Integration")]
    [BackOfficeRoute("seoToolkitAI")]
    public class AIIntegrationController : SeoToolkitAuthenticatedControllerBase
    {
        private readonly IMetaFieldsAIService _metaFieldsAIService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly IVariationContextAccessor _variationContextAccessor;
        private readonly ILanguageService _languageService;

        public AIIntegrationController(
            IMetaFieldsAIService metaFieldsAIService,
            IUmbracoContextFactory umbracoContextFactory,
            IVariationContextAccessor variationContextAccessor,
            ILanguageService languageService)
        {
            _metaFieldsAIService = metaFieldsAIService;
            _umbracoContextFactory = umbracoContextFactory;
            _variationContextAccessor = variationContextAccessor;
            _languageService = languageService;
        }

        [HttpPost("generate")]
        [ProducesResponseType(typeof(MetaFieldsAIGenerateResponseModel), 200)]
        public async Task<IActionResult> Generate(
            [FromBody] MetaFieldsAIGenerateRequestModel request,
            CancellationToken cancellationToken)
        {
            await EnsureLanguage(request.Culture);

            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var content = ctx.UmbracoContext.Content?.GetById(true, request.NodeId);
            if (content is null)
                return BadRequest($"Cannot find content with id: {request.NodeId}");

            var culture = string.IsNullOrWhiteSpace(request.Culture) || request.Culture == "invariant"
                ? await _languageService.GetDefaultIsoCodeAsync()
                : request.Culture;

            var result = await _metaFieldsAIService.GenerateAsync(content, culture, cancellationToken);
            return Ok(result);
        }

        private async Task EnsureLanguage(string? culture)
        {
            if (!string.IsNullOrWhiteSpace(culture) && culture != "invariant")
                _variationContextAccessor.VariationContext = new VariationContext(culture);
            else
                _variationContextAccessor.VariationContext = new VariationContext(await _languageService.GetDefaultIsoCodeAsync());
        }
    }
}
