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
            if (!redirect.RedirectCode.Equals((int)HttpStatusCode.Redirect) &&
                !redirect.RedirectCode.Equals((int)HttpStatusCode.MovedPermanently))
                throw new ArgumentException("Only support for 301 & 302 redirects", nameof(redirect.RedirectCode));

            var oldUrl = CleanUrl(redirect.OldUrl);
            var newUrl = redirect.NewUrl;

            if (!Uri.IsWellFormedUriString(redirect.OldUrl, UriKind.Relative))
                oldUrl = redirect.OldUrl.EnsureStartsWith("/");

            if (redirect.NewNode is null)
            {
                newUrl = Uri.IsWellFormedUriString(newUrl, UriKind.Absolute) ?
                    newUrl :
                    newUrl.EnsureEndsWith("/").ToLower();
            }

            var existingRedirects = _redirectsRepository.GetByUrls(oldUrl).Where(it => it.Id != redirect.Id).ToArray();
            if (existingRedirects.Length > 0)
            {
                if (existingRedirects.Any(it => it.Domain is null && string.IsNullOrWhiteSpace(it.CustomDomain)))
                    throw new ArgumentException($"An redirect already exists for {oldUrl}");

                if (redirect.Domain != null && existingRedirects.Any(it => it.Domain?.Id == redirect.Domain.Id))
                    throw new ArgumentException($"An redirect already exists for {oldUrl}");

                if (!string.IsNullOrWhiteSpace(redirect.CustomDomain) && existingRedirects.Any(it => it.CustomDomain.Equals(redirect.CustomDomain)))
                    throw new ArgumentException($"An redirect already exists for {oldUrl}");
            }

            redirect.OldUrl = oldUrl;
            redirect.NewUrl = newUrl;
            redirect.LastUpdated = DateTime.Now;
            _redirectsRepository.Save(redirect);
        }

        public void Delete(int[] ids)
        {
            foreach (var id in ids)
            {
                var redirect = _redirectsRepository.Get(id);
                if (redirect is null) continue;

                _redirectsRepository.Delete(redirect);
            }
        }

        public Redirect GetByUrl(Uri uri)
        {
            using (var ctx = _umbracoContextFactory.EnsureUmbracoContext())
            {
                var domain = DomainUtilities.SelectDomain(ctx.UmbracoContext.Domains.GetAll(false), uri);

                var path = CleanUrl(uri.AbsolutePath);
                var pathAndQuery = CleanUrl(uri.PathAndQuery);

                //Because we are checking both the url with and without query, we might get two urls.
                var redirects = _redirectsRepository.GetByUrls(path, pathAndQuery).ToArray();
                if (redirects.Length == 0) return null;

                Redirect foundRedirect = null;
                if (domain != null)
                {
                    foundRedirect = redirects.FirstOrDefault(it => it.Domain == domain);
                    if (foundRedirect != null)
                        return foundRedirect;
                }

                foundRedirect = redirects.FirstOrDefault(it => it.CustomDomain != null && (it.CustomDomain.Equals(uri.Host, StringComparison.InvariantCultureIgnoreCase) || it.CustomDomain.Equals($"{uri.Scheme}://{uri.Host}")));
                if (foundRedirect != null) return foundRedirect;

                foundRedirect = redirects.FirstOrDefault(it =>
                    it.OldUrl.Equals(path, StringComparison.InvariantCultureIgnoreCase) ||
                    it.OldUrl.Equals(pathAndQuery, StringComparison.InvariantCultureIgnoreCase));
                if (foundRedirect != null) return foundRedirect;

                return null;
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
