using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using SeoToolkit.Umbraco.Redirects.Core.Interfaces;
using System.Diagnostics;
using System.Linq;

namespace SeoToolkit.Umbraco.Redirects.Core.Middleware
{
    public class RedirectMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IRuntimeState _runtimeState;
        private readonly IRedirectsService _redirectsService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly ILanguageService _languageService;

        public RedirectMiddleware(RequestDelegate next,
            IRuntimeState runtimeState,
            IRedirectsService redirectsService,
            IUmbracoContextFactory umbracoContextFactory,
            ILanguageService languageService)
        {
            _next = next;
            _runtimeState = runtimeState;
            _redirectsService = redirectsService;
            _umbracoContextFactory = umbracoContextFactory;
            _languageService = languageService;
        }

        public async Task Invoke(HttpContext context)
        {
            if (_runtimeState.Level != RuntimeLevel.Run)
            {
                await _next.Invoke(context);
                return;
            }

            var pathAndQuery = context.Request.GetEncodedPathAndQuery();

            if (pathAndQuery.IndexOf("/umbraco", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                await _next(context);
                return;
            }

            if (!await HandleRedirect(context))
            {
                await _next(context);
            }
        }

        private async Task<bool> HandleRedirect(HttpContext context)
        {
            var url = new Uri(context.Request.GetEncodedUrl());
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var matchedRedirectResult = _redirectsService.GetByUrl(url);
            stopwatch.Stop();
            if (matchedRedirectResult is null)
            {
                return false;
            }

            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var isPerm = matchedRedirectResult.Redirect.RedirectCode == (int)HttpStatusCode.MovedPermanently;
            var isoCode = matchedRedirectResult.Redirect.NewNodeCultureId.HasValue ? (await _languageService.GetAllAsync()).FirstOrDefault(l => l.Id == matchedRedirectResult.Redirect.NewNodeCultureId.Value)?.IsoCode : null;
            context.Response.Redirect(matchedRedirectResult.GetNewUrl(isoCode), isPerm);
            return true;
        }
    }
}
