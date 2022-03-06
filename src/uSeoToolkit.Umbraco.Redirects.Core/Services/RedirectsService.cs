using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;
using uSeoToolkit.Umbraco.Redirects.Core.Interfaces;
using uSeoToolkit.Umbraco.Redirects.Core.Models.Business;

namespace uSeoToolkit.Umbraco.Redirects.Core.Services
{
    public class RedirectsService : IRedirectsService
    {
        private readonly IRedirectsRepository _redirectsRepository;
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public RedirectsService(IRedirectsRepository redirectsRepository,
            IUmbracoContextFactory umbracoContextFactory)
        {
            _redirectsRepository = redirectsRepository;
            _umbracoContextFactory = umbracoContextFactory;
        }

        public IEnumerable<Redirect> GetAll()
        {
            return _redirectsRepository.GetAll();
        }

        public void Save(Redirect redirect)
        {
            if (redirect is null) throw new ArgumentNullException(nameof(redirect));
            if (!redirect.RedirectCode.Equals(HttpStatusCode.Redirect) &&
                !redirect.RedirectCode.Equals(HttpStatusCode.PermanentRedirect))
                throw new ArgumentException("Only support for 301 & 302 redirects", nameof(redirect.RedirectCode));

            var oldUrl = CleanUrl(redirect.OldUrl);
            var newUrl = redirect.NewUrl;

            if (!Uri.IsWellFormedUriString(redirect.OldUrl, UriKind.Relative))
                oldUrl = redirect.OldUrl.EnsureStartsWith("/");

            newUrl = Uri.IsWellFormedUriString(newUrl, UriKind.Absolute) ?
                newUrl :
                newUrl.EnsureEndsWith("/").ToLower();

            var uri = new Uri(oldUrl);
            var existingRedirects =
                (redirect.Domain is null
                    ? _redirectsRepository.GetByUrls(uri.Host, oldUrl)
                    : _redirectsRepository.GetByUrls(redirect.CustomDomain, oldUrl)).ToArray();
            if (existingRedirects.Length > 0)
                throw new ArgumentException($"An redirect already exists for {oldUrl}");

            redirect.OldUrl = oldUrl;
            redirect.NewUrl = newUrl;
            _redirectsRepository.Save(redirect);
        }

        public Redirect GetByUrl(Uri uri)
        {
            using (var ctx = _umbracoContextFactory.EnsureUmbracoContext())
            {
                var domain = DomainUtilities.SelectDomain(ctx.UmbracoContext.Domains.GetAll(false), uri);

                var path = CleanUrl(uri.AbsolutePath);
                var pathAndQuery = CleanUrl(uri.PathAndQuery);

                //Because we are checking both the url with and without query, we might get two urls.
                var redirects = (domain is null ? _redirectsRepository.GetByUrls(uri.Host, path, pathAndQuery) : _redirectsRepository.GetByUrls(domain.Id, path, pathAndQuery)).ToArray();

                //TODO: Regex stuff
                return redirects.FirstOrDefault(it => it.OldUrl.Equals(pathAndQuery)) ?? redirects.FirstOrDefault();
            }
        }

        private string CleanUrl(string url)
        {
            var urlParts = url.ToLowerInvariant().Split('?');
            var baseUrl = urlParts[0].TrimEnd('/');
            return urlParts.Length == 1 ? baseUrl : $"{baseUrl}?{string.Join("?", urlParts.Skip(1))}";
        }
    }
}
