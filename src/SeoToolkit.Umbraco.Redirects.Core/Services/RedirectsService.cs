using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;
using SeoToolkit.Umbraco.Redirects.Core.Extensions;
using SeoToolkit.Umbraco.Redirects.Core.Interfaces;
using SeoToolkit.Umbraco.Redirects.Core.Models.Business;

namespace SeoToolkit.Umbraco.Redirects.Core.Services
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

        public PagedResult<Redirect> GetAll(int pageNumber, int pageSize, string orderBy = null, string orderDirection = null, string search = "")
        {
            var result = _redirectsRepository.GetAll(pageNumber, pageSize, out var totalRecords, orderBy, orderDirection, search);
            return new PagedResult<Redirect>(totalRecords, pageNumber, pageSize) { Items = result };
        }

        public Redirect Get(int id)
        {
            return _redirectsRepository.Get(id);
        }

        public void Save(Redirect redirect)
        {
            if (redirect is null) throw new ArgumentNullException(nameof(redirect));
            if (!redirect.RedirectCode.Equals((int)HttpStatusCode.Redirect) &&
                !redirect.RedirectCode.Equals((int)HttpStatusCode.MovedPermanently))
                throw new ArgumentException("Only support for 301 & 302 redirects", nameof(redirect.RedirectCode));

            var oldUrl = redirect.IsRegex ? redirect.OldUrl : redirect.OldUrl.CleanUrl();
            var newUrl = redirect.NewUrl;

            if (!redirect.IsRegex && !Uri.IsWellFormedUriString(redirect.OldUrl, UriKind.Relative))
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

        public RedirectFindResult GetByUrl(Uri uri)
        {
            using (var ctx = _umbracoContextFactory.EnsureUmbracoContext())
            {
                var domain = DomainUtilities.SelectDomain(ctx.UmbracoContext.Domains.GetAll(false), uri);
                if (domain != null)
                {
                    //We do this to ensure that we support subdirectories. So if you have domain domain.com/en and relative path /test123, you don't also need to include the subdirectory /en/test123.
                    uri = new Uri(domain.Uri, uri.AbsolutePath.TrimStart(domain.Uri.LocalPath));
                }

                var path = uri.AbsolutePath.CleanUrl();
                var pathAndQuery = uri.PathAndQuery.CleanUrl();
                var customDomainWithoutScheme = uri.Host;
                var customDomainWithScheme = $"{uri.Scheme}://{uri.Host}";

                //Because we are checking both the url with and without query, we might get two urls.
                var redirects = _redirectsRepository.GetByUrls(path, pathAndQuery).ToArray();
                if (redirects.Length > 0)
                {
                    Redirect foundRedirect = null;
                    if (domain != null)
                    {
                        //See if we can find a redirect with the same domain
                        foundRedirect = redirects.FirstOrDefault(it => it.Domain?.Id == domain.Id);
                        if (foundRedirect != null)
                            return new RedirectFindResult(uri, foundRedirect);
                    }
                    else
                    {
                        //Else check if we can find a redirect on the custom domain
                        foundRedirect = redirects.FirstOrDefault(it => it.CustomDomain != null && (it.CustomDomain.Equals(customDomainWithoutScheme, StringComparison.InvariantCultureIgnoreCase) || it.CustomDomain.Equals(customDomainWithScheme)));
                        if (foundRedirect != null) return new RedirectFindResult(uri, foundRedirect);
                    }                    

                    //Else check if we can find a redirect on the global level
                    foundRedirect = redirects.FirstOrDefault(it =>
                        it.Domain is null &&
                        string.IsNullOrWhiteSpace(it.CustomDomain) &&
                        (it.OldUrl.Equals(path, StringComparison.InvariantCultureIgnoreCase) ||
                        it.OldUrl.Equals(pathAndQuery, StringComparison.InvariantCultureIgnoreCase)));
                    if (foundRedirect != null) return new RedirectFindResult(uri, foundRedirect);
                }

                var regexRedirects = _redirectsRepository.GetAllRegexRedirects().Where(it =>
                {
                    if (it.Domain?.Id == 0) return true;
                    if (domain != null && it.Domain?.Id == domain.Id) return true;
                    if (domain is null &&
                        !string.IsNullOrWhiteSpace(it.CustomDomain) &&
                        (it.CustomDomain.Equals(customDomainWithoutScheme,
                             StringComparison.InvariantCultureIgnoreCase) ||
                         it.CustomDomain.Equals(customDomainWithScheme))) return true;
                    return false;
                });

                foreach (var regexRedirect in regexRedirects)
                {
                    if (Regex.IsMatch(pathAndQuery, regexRedirect.OldUrl)) return new RedirectFindResult(uri, regexRedirect);
                }

                return null;
            }
        }
    }
}
