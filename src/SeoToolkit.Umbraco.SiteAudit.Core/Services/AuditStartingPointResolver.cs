#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;
using SeoToolkit.Umbraco.Common.Core.Helpers;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Services
{
    /// <summary>Where a crawl begins, once the request has been made sense of.</summary>
    public sealed class AuditStartingPoint
    {
        public required Uri Url { get; init; }

        /// <summary>
        /// The decoupled frontend this run was retargeted to, when the domain the node belongs
        /// to declares one. Null when the site is served by Umbraco itself, or when the url was
        /// given directly.
        /// </summary>
        public string? BaseUrl { get; init; }

        /// <summary>The node the crawl started from, when it started from one.</summary>
        public Guid? ContentKey { get; init; }

        public string? Culture { get; init; }
    }

    /// <summary>
    /// The outcome of resolving a starting point. Carries the reason rather than throwing,
    /// because every failure here is something the person filling in the form can fix.
    /// </summary>
    public sealed class AuditStartingPointResult
    {
        public AuditStartingPoint? StartingPoint { get; private init; }
        public string? Error { get; private init; }

        /// <summary>Separated from a plain error so the controller can answer 404 rather than 400.</summary>
        public bool IsNotFound { get; private init; }

        public bool IsSuccess => StartingPoint is not null;

        public static AuditStartingPointResult Success(AuditStartingPoint startingPoint)
            => new() { StartingPoint = startingPoint };

        public static AuditStartingPointResult Invalid(string error) => new() { Error = error };

        public static AuditStartingPointResult NotFound(string error)
            => new() { Error = error, IsNotFound = true };
    }

    /// <summary>One language a node is published in, with the url the crawl would use for it.</summary>
    public sealed class AuditStartNodeCulture
    {
        public required string IsoCode { get; init; }
        public required string Name { get; init; }
        public required string Url { get; init; }
    }

    /// <summary>What the create screen needs to know about a chosen node.</summary>
    public sealed class AuditStartNode
    {
        public required Guid Key { get; init; }
        public required string Name { get; init; }

        /// <summary>True when the node is published in more than one language.</summary>
        public bool VariesByCulture { get; init; }

        /// <summary>The url for a node that does not vary. Null when it does.</summary>
        public string? Url { get; init; }

        public IReadOnlyList<AuditStartNodeCulture> Cultures { get; init; }
            = Array.Empty<AuditStartNodeCulture>();
    }

    public interface IAuditStartingPointResolver
    {
        /// <summary>Describes a node so the create screen can offer its languages.</summary>
        AuditStartNode? GetStartNode(Guid key);

        AuditStartingPointResult ResolveFromNode(Guid key, string? culture);

        AuditStartingPointResult ResolveFromUrl(string? url);
    }

    /// <summary>
    /// Works out what url an audit should start crawling.
    /// <para>
    /// A crawl can begin from an Umbraco node or from a url typed in directly. The node route
    /// exists because it is what an editor knows; the url route exists because a decoupled
    /// frontend can route however it likes, and there may be no node whose url resembles the
    /// page that is actually served.
    /// </para>
    /// <para>
    /// The node route applies the domain's configured BaseUrl, exactly as the sitemap and meta
    /// field generators do. Without it an audit of a decoupled site crawls the Umbraco backend
    /// rather than the site anyone visits - which is what it did before this existed.
    /// </para>
    /// </summary>
    public sealed class AuditStartingPointResolver : IAuditStartingPointResolver
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly ISeoDomainResolver _seoDomainResolver;

        public AuditStartingPointResolver(IUmbracoContextFactory umbracoContextFactory,
            ISeoDomainResolver seoDomainResolver)
        {
            _umbracoContextFactory = umbracoContextFactory;
            _seoDomainResolver = seoDomainResolver;
        }

        public AuditStartNode? GetStartNode(Guid key)
        {
            using var reference = _umbracoContextFactory.EnsureUmbracoContext();

            var content = reference.UmbracoContext.Content?.GetById(key);
            if (content is null) return null;

            var cultures = PublishedCultures(content)
                .Select(culture => new AuditStartNodeCulture
                {
                    IsoCode = culture,
                    Name = DescribeCulture(content, culture),
                    Url = Retarget(content.Url(culture, UrlMode.Absolute)).Url
                })
                .Where(it => !string.IsNullOrWhiteSpace(it.Url))
                .ToArray();

            return new AuditStartNode
            {
                Key = content.Key,
                Name = content.Name ?? content.Key.ToString(),
                VariesByCulture = cultures.Length > 1,
                // A node with a single language still has one url, and offering a choice of one
                // is just noise on the form.
                Url = cultures.Length > 1 ? null : Retarget(content.Url(mode: UrlMode.Absolute)).Url,
                Cultures = cultures
            };
        }

        public AuditStartingPointResult ResolveFromNode(Guid key, string? culture)
        {
            using var reference = _umbracoContextFactory.EnsureUmbracoContext();

            var content = reference.UmbracoContext.Content?.GetById(key);
            if (content is null) return AuditStartingPointResult.NotFound($"Content {key} was not found.");

            culture = string.IsNullOrWhiteSpace(culture) ? null : culture!.Trim();

            if (culture is not null)
            {
                var published = PublishedCultures(content);

                if (!published.Contains(culture, StringComparer.OrdinalIgnoreCase))
                    return AuditStartingPointResult.Invalid(
                        $"'{content.Name}' is not published in {culture}.");
            }

            // A null culture lets Umbraco pick, which is the right answer for a node that does
            // not vary at all.
            var url = content.Url(culture, UrlMode.Absolute);

            if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out _))
                return AuditStartingPointResult.Invalid(
                    $"'{content.Name}' has no absolute url to crawl. It may be unpublished, or have no template.");

            var (finalUrl, baseUrl) = Retarget(url);

            if (!Uri.TryCreate(finalUrl, UriKind.Absolute, out var parsed))
                return AuditStartingPointResult.Invalid($"'{finalUrl}' is not a url that can be crawled.");

            return AuditStartingPointResult.Success(new AuditStartingPoint
            {
                Url = parsed,
                BaseUrl = baseUrl,
                ContentKey = content.Key,
                Culture = culture
            });
        }

        /// <summary>
        /// A url typed in directly is crawled exactly as given. No BaseUrl is applied: the whole
        /// point of typing one is to aim the crawl somewhere the configuration does not describe.
        /// </summary>
        public AuditStartingPointResult ResolveFromUrl(string? url)
            => TryParseCrawlUrl(url, out var parsed, out var error)
                ? AuditStartingPointResult.Success(new AuditStartingPoint { Url = parsed! })
                : AuditStartingPointResult.Invalid(error!);

        /// <summary>
        /// Turns what someone typed into a url the crawler can start from, or explains why it
        /// cannot. Kept static and free of Umbraco so the rules can be tested on their own.
        /// </summary>
        public static bool TryParseCrawlUrl(string? value, out Uri? url, out string? error)
        {
            url = null;
            error = null;

            if (string.IsNullOrWhiteSpace(value))
            {
                error = "Enter the url the crawl should start from.";
                return false;
            }

            var candidate = value!.Trim();

            // Typing example.com is a reasonable thing to do, and refusing it would be pedantic.
            if (!candidate.Contains("://", StringComparison.Ordinal))
                candidate = "https://" + candidate;

            if (!Uri.TryCreate(candidate, UriKind.Absolute, out var parsed))
            {
                error = $"'{value}' is not a valid url.";
                return false;
            }

            if (parsed.Scheme != Uri.UriSchemeHttp && parsed.Scheme != Uri.UriSchemeHttps)
            {
                error = "The crawl can only start from an http or https url.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(parsed.Host))
            {
                error = $"'{value}' has no host to crawl.";
                return false;
            }

            // A fragment never identifies a different page to the server, and carrying one into
            // the crawl would only make the starting url look unlike every other url in the run.
            if (!string.IsNullOrEmpty(parsed.Fragment))
                parsed = new UriBuilder(parsed) { Fragment = string.Empty }.Uri;

            url = parsed;
            return true;
        }

        /// <summary>Applies the BaseUrl of whichever domain the url belongs to, if it declares one.</summary>
        private (string Url, string? BaseUrl) Retarget(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return (string.Empty, null);
            if (!Uri.TryCreate(url, UriKind.Absolute, out var parsed)) return (url!, null);

            var baseUrl = _seoDomainResolver.ResolveSeoDomain(parsed)?.BaseUrl;

            return (BaseUrlHelper.ApplyBaseUrl(url!, baseUrl), string.IsNullOrWhiteSpace(baseUrl) ? null : baseUrl);
        }

        private static string[] PublishedCultures(IPublishedContent content)
            => content.Cultures.Keys
                .Where(culture => !string.IsNullOrWhiteSpace(culture) && content.IsPublished(culture))
                .ToArray();

        /// <summary>
        /// A readable name for a language. Falls back to the iso code, which is still perfectly
        /// usable, rather than failing over a culture the machine does not recognise.
        /// </summary>
        private static string DescribeCulture(IPublishedContent content, string culture)
        {
            try
            {
                return CultureInfo.GetCultureInfo(culture).DisplayName;
            }
            catch (CultureNotFoundException)
            {
                return content.Cultures.TryGetValue(culture, out var info) && !string.IsNullOrWhiteSpace(info.Name)
                    ? info.Name
                    : culture;
            }
        }
    }
}
