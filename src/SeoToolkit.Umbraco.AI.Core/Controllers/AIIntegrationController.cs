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
    // CSRF tokens are not required: all requests must carry a valid JWT Bearer token via
    // the BackOffice authorization policy inherited from SeoToolkitAuthenticatedControllerBase.
    [IgnoreAntiforgeryToken]
    public class AIIntegrationController : SeoToolkitAuthenticatedControllerBase
    {
        private readonly IMetaFieldsAIService _metaFieldsAIService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly IVariationContextAccessor _variationContextAccessor;
        private readonly ILocalizationService _localizationService;

        public AIIntegrationController(
            IMetaFieldsAIService metaFieldsAIService,
            IUmbracoContextFactory umbracoContextFactory,
            IVariationContextAccessor variationContextAccessor,
            ILocalizationService localizationService)
        {
            _metaFieldsAIService = metaFieldsAIService;
            _umbracoContextFactory = umbracoContextFactory;
            _variationContextAccessor = variationContextAccessor;
            _localizationService = localizationService;
        }

        [HttpPost("generate")]
        [ProducesResponseType(typeof(MetaFieldsAIGenerateResponseModel), 200)]
        public async Task<IActionResult> Generate(
            [FromBody] MetaFieldsAIGenerateRequestModel request,
            CancellationToken cancellationToken)
        {
            EnsureLanguage(request.Culture);

            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var content = ctx.UmbracoContext.Content?.GetById(true, request.NodeId);
            if (content is null)
                return BadRequest($"Cannot find content with id: {request.NodeId}");

            var culture = string.IsNullOrWhiteSpace(request.Culture) || request.Culture == "invariant"
                ? _localizationService.GetDefaultLanguageIsoCode()
                : request.Culture;

            var result = await _metaFieldsAIService.GenerateAsync(content, culture, cancellationToken);
            return Ok(result);
        }

        private void EnsureLanguage(string? culture)
        {
            if (!string.IsNullOrWhiteSpace(culture) && culture != "invariant")
                _variationContextAccessor.VariationContext = new VariationContext(culture);
            else
                _variationContextAccessor.VariationContext = new VariationContext(_localizationService.GetDefaultLanguageIsoCode());
        }
    }
}
