using Microsoft.AspNetCore.Mvc;
using SeoToolkit.Umbraco.Common.Core.Collections;
using SeoToolkit.Umbraco.Common.Core.Models.Config;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;

namespace SeoToolkit.Umbraco.Common.Core.Controllers
{
    [ApiController]
    [Route("/api/seo")]
    public class SeoApiController : Controller
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly IVariationContextAccessor _variationContextAccessor;
        private readonly ISettingsService<GlobalConfig> _config;
        private readonly IApiDataHandler[] _dataHandlers;

        public SeoApiController(IUmbracoContextFactory umbracoContextFactory, IVariationContextAccessor variationContextAccessor, ISettingsService<GlobalConfig> config, IEnumerable<IApiDataHandler> dataHandlers)
        {
            _umbracoContextFactory = umbracoContextFactory;
            _variationContextAccessor = variationContextAccessor;
            _config = config;
            _dataHandlers = dataHandlers?.ToArray() ?? Array.Empty<IApiDataHandler>();
        }

        [HttpGet]
        [ProducesResponseType(typeof(Dictionary<string, object>), 200)]
        public IActionResult Get(Guid contentGuid, string? culture = null)
        {
            if (!_config.GetSettings().EnableApiEndpoints)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(culture))
            {
                _variationContextAccessor.VariationContext = new VariationContext(culture);
            }

            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var content = ctx.UmbracoContext.Content?.GetById(contentGuid);
            if (content is null)
            {
                return NotFound();
            }

            var data = new Dictionary<string, object>();
            foreach (var handler in _dataHandlers)
            {
                data.Add(handler.Name, handler.GetData(content));
            }
            return Ok(data);
        }
    }
}
