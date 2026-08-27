#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Crawling
{
    public interface IPageFactsParser
    {
        /// <summary>
        /// Extracts every fact a check might want from a page, in one pass.
        /// </summary>
        /// <param name="html">The markup.</param>
        /// <param name="pageUrl">Used to resolve relative links.</param>
        /// <param name="isInternal">Decides whether a link counts as on-site.</param>
        /// <param name="xRobotsTag">The X-Robots-Tag response header, if it was captured.</param>
        PageFacts Parse(string html, Uri pageUrl, Func<Uri, bool> isInternal, string? xRobotsTag = null);
    }

    /// <summary>
    /// The single parse that the whole check catalogue reads from.
    /// <para>
    /// Everything is gathered here so that adding a check costs nothing at crawl time. The old
    /// design had each check run its own XPath over the document, so the cost of the catalogue
    /// grew with every check added; here it is one parse per page regardless.
    /// </para>
    /// <para>
    /// AngleSharp rather than HtmlAgilityPack: it parses to the HTML5 spec, so what it produces
    /// matches what a browser would build - which is what search engines see. It also gives real
    /// css selectors and proper text extraction.
    /// </para>
    /// </summary>
    public sealed class AngleSharpPageFactsParser : IPageFactsParser
    {
        // Stateless and thread safe, so one instance serves every crawl worker.
        private static readonly HtmlParser Parser = new(new HtmlParserOptions
        {
            IsScripting = false,
            IsEmbedded = false
        });

        public PageFacts Parse(string html, Uri pageUrl, Func<Uri, bool> isInternal, string? xRobotsTag = null)
        {
            if (html is null) throw new ArgumentNullException(nameof(html));
            if (pageUrl is null) throw new ArgumentNullException(nameof(pageUrl));
            isInternal ??= _ => false;

            using var document = Parser.ParseDocument(html);

            var titles = document.Head?.QuerySelectorAll("title") ?? document.QuerySelectorAll("title");
            var titleElement = titles.FirstOrDefault();

            var descriptions = document.QuerySelectorAll("meta[name='description' i]");
            var descriptionContent = descriptions.FirstOrDefault()?.GetAttribute("content");

            var textContent = ExtractVisibleText(document);
            var wordCount = CountWords(textContent);

            return new PageFacts
            {
                Title = Clean(titleElement?.TextContent),
                TitleCount = titles.Length,
                MetaDescription = Clean(descriptionContent),
                MetaDescriptionCount = descriptions.Length,
                MetaRobots = ParseRobots(document.QuerySelectorAll("meta[name='robots' i]")
                    .Select(it => it.GetAttribute("content"))),
                XRobotsTag = ParseRobots(new[] { xRobotsTag }),
                Canonicals = ResolveAll(document.QuerySelectorAll("link[rel='canonical' i]"), "href", pageUrl),
                H1s = document.QuerySelectorAll("h1").Select(it => Clean(it.TextContent) ?? string.Empty).ToArray(),
                Headings = ExtractHeadings(document),
                HreflangEntries = ExtractHreflang(document, pageUrl),
                OpenGraph = ExtractOpenGraph(document, pageUrl),
                TwitterCard = ExtractTwitterCard(document, pageUrl),
                JsonLdBlocks = ExtractJsonLd(document),
                Images = ExtractImages(document, pageUrl),
                Links = ExtractLinks(document, pageUrl, isInternal),
                Scripts = ResolveAll(document.QuerySelectorAll("script[src]"), "src", pageUrl),
                Stylesheets = ResolveAll(document.QuerySelectorAll("link[rel='stylesheet' i][href]"), "href", pageUrl),
                TextContent = textContent,
                WordCount = wordCount,
                HtmlSizeBytes = Encoding.UTF8.GetByteCount(html),
                TextToHtmlRatio = html.Length == 0 ? 0 : (double)(textContent?.Length ?? 0) / html.Length,
                ContentHash = ComputeContentHash(textContent),
                Lang = Clean(document.DocumentElement?.GetAttribute("lang")),
                Viewport = Clean(document.QuerySelector("meta[name='viewport' i]")?.GetAttribute("content")),
                FaviconHref = Resolve(document.QuerySelector("link[rel~='icon' i]")?.GetAttribute("href"), pageUrl),
                FormCount = document.QuerySelectorAll("form").Length,
                MetaRefresh = Clean(document.QuerySelector("meta[http-equiv='refresh' i]")?.GetAttribute("content"))
            };
        }

        /// <summary>
        /// Stable 64 bit FNV-1a hash of the page text. Retained on the index in place of the text
        /// itself, so exact duplicate detection survives long after the body has been released.
        /// </summary>
        public static long ComputeContentHash(string? text)
        {
            if (string.IsNullOrEmpty(text)) return 0;

            const ulong offsetBasis = 14695981039346656037;
            const ulong prime = 1099511628211;

            var hash = offsetBasis;
            foreach (var c in text!)
            {
                hash ^= c;
                hash *= prime;
            }

            // Zero is reserved to mean "no content", so nudge a genuine zero hash off it.
            var result = unchecked((long)hash);
            return result == 0 ? 1 : result;
        }

        /// <summary>
        /// Visible text only. Script and style bodies are markup, not content, and counting them
        /// would make a script-heavy page look content-rich to the thin-content check.
        /// </summary>
        private static string ExtractVisibleText(IHtmlDocument document)
        {
            var body = document.Body;
            if (body is null) return string.Empty;

            var builder = new StringBuilder(512);
            AppendText(body, builder);

            return CollapseWhitespace(builder.ToString());
        }

        private static void AppendText(INode node, StringBuilder builder)
        {
            foreach (var child in node.ChildNodes)
            {
                switch (child.NodeType)
                {
                    case NodeType.Text:
                        builder.Append(child.TextContent).Append(' ');
                        break;

                    case NodeType.Element:
                        var name = ((IElement)child).LocalName;
                        if (name is "script" or "style" or "noscript" or "template" or "svg")
                            continue;
                        AppendText(child, builder);
                        break;
                }
            }
        }

        private static IReadOnlyList<DiscoveredHeading> ExtractHeadings(IHtmlDocument document)
        {
            var elements = document.QuerySelectorAll("h1, h2, h3, h4, h5, h6");
            if (elements.Length == 0) return Array.Empty<DiscoveredHeading>();

            var result = new DiscoveredHeading[elements.Length];
            for (var i = 0; i < elements.Length; i++)
            {
                // QuerySelectorAll returns document order, which is what level-skip checks need.
                var level = elements[i].LocalName[1] - '0';
                result[i] = new DiscoveredHeading(level, Clean(elements[i].TextContent) ?? string.Empty);
            }
            return result;
        }

        private static IReadOnlyList<DiscoveredLink> ExtractLinks(IHtmlDocument document, Uri pageUrl, Func<Uri, bool> isInternal)
        {
            var anchors = document.QuerySelectorAll("a[href]");
            if (anchors.Length == 0) return Array.Empty<DiscoveredLink>();

            var result = new List<DiscoveredLink>(anchors.Length);
            foreach (var anchor in anchors)
            {
                var url = Resolve(anchor.GetAttribute("href"), pageUrl);
                if (url is null) continue;

                result.Add(new DiscoveredLink(
                    url,
                    Clean(anchor.TextContent),
                    anchor.GetAttribute("rel"),
                    isInternal(url),
                    DetermineRegion(anchor),
                    Clean(anchor.GetAttribute("title"))));
            }
            return result;
        }

        /// <summary>
        /// Where a link sits, from its nearest landmark ancestor. Approximate by nature, but
        /// enough to tell repeated navigation chrome from links that are really about the page.
        /// </summary>
        private static LinkRegion DetermineRegion(IElement anchor)
        {
            var current = anchor.ParentElement;
            while (current is not null)
            {
                switch (current.LocalName)
                {
                    case "nav": return LinkRegion.Navigation;
                    case "footer": return LinkRegion.Footer;
                    case "header": return LinkRegion.Header;
                    case "aside": return LinkRegion.Aside;
                    case "main":
                    case "article": return LinkRegion.Body;
                }
                current = current.ParentElement;
            }
            return LinkRegion.Unknown;
        }

        private static IReadOnlyList<DiscoveredImage> ExtractImages(IHtmlDocument document, Uri pageUrl)
        {
            var images = document.QuerySelectorAll("img");
            if (images.Length == 0) return Array.Empty<DiscoveredImage>();

            var result = new List<DiscoveredImage>(images.Length);
            foreach (var image in images)
            {
                var url = Resolve(image.GetAttribute("src"), pageUrl);
                if (url is null) continue;

                result.Add(new DiscoveredImage(
                    url,
                    image.GetAttribute("alt"),
                    // A missing alt and a deliberately empty one are different problems.
                    image.HasAttribute("alt"),
                    ParseDimension(image.GetAttribute("width")),
                    ParseDimension(image.GetAttribute("height")),
                    image.GetAttribute("loading"),
                    image.GetAttribute("srcset")));
            }
            return result;
        }

        private static IReadOnlyList<HreflangEntry> ExtractHreflang(IHtmlDocument document, Uri pageUrl)
        {
            var links = document.QuerySelectorAll("link[rel='alternate' i][hreflang]");
            if (links.Length == 0) return Array.Empty<HreflangEntry>();

            var result = new List<HreflangEntry>(links.Length);
            foreach (var link in links)
            {
                var language = link.GetAttribute("hreflang");
                var url = Resolve(link.GetAttribute("href"), pageUrl);
                if (string.IsNullOrWhiteSpace(language) || url is null) continue;

                result.Add(new HreflangEntry(language!, url));
            }
            return result;
        }

        private static IReadOnlyList<JsonLdBlock> ExtractJsonLd(IHtmlDocument document)
        {
            var scripts = document.QuerySelectorAll("script[type='application/ld+json' i]");
            if (scripts.Length == 0) return Array.Empty<JsonLdBlock>();

            var result = new List<JsonLdBlock>(scripts.Length);
            foreach (var script in scripts)
            {
                var raw = script.TextContent?.Trim() ?? string.Empty;
                string? type = null;
                string? error = null;

                try
                {
                    using var parsed = System.Text.Json.JsonDocument.Parse(raw);
                    var root = parsed.RootElement;
                    if (root.ValueKind == System.Text.Json.JsonValueKind.Object &&
                        root.TryGetProperty("@type", out var typeElement))
                    {
                        type = typeElement.ValueKind == System.Text.Json.JsonValueKind.String
                            ? typeElement.GetString()
                            : typeElement.ToString();
                    }
                }
                catch (System.Text.Json.JsonException ex)
                {
                    error = ex.Message;
                }

                result.Add(new JsonLdBlock(raw, type, error));
            }
            return result;
        }

        private static SocialCardFacts ExtractOpenGraph(IHtmlDocument document, Uri pageUrl)
        {
            var properties = ReadMeta(document, "meta[property^='og:' i]", "property", "og:");
            if (properties.Count == 0) return SocialCardFacts.Empty;

            return new SocialCardFacts
            {
                Title = Get(properties, "title"),
                Description = Get(properties, "description"),
                Type = Get(properties, "type"),
                Image = Resolve(Get(properties, "image"), pageUrl),
                ImageAlt = Get(properties, "image:alt"),
                Url = Resolve(Get(properties, "url"), pageUrl),
                SiteName = Get(properties, "site_name"),
                Additional = properties
            };
        }

        private static SocialCardFacts ExtractTwitterCard(IHtmlDocument document, Uri pageUrl)
        {
            var properties = ReadMeta(document, "meta[name^='twitter:' i]", "name", "twitter:");
            if (properties.Count == 0) return SocialCardFacts.Empty;

            return new SocialCardFacts
            {
                Card = Get(properties, "card"),
                Title = Get(properties, "title"),
                Description = Get(properties, "description"),
                Image = Resolve(Get(properties, "image"), pageUrl),
                ImageAlt = Get(properties, "image:alt"),
                Additional = properties
            };
        }

        private static Dictionary<string, string> ReadMeta(IHtmlDocument document, string selector, string keyAttribute, string prefix)
        {
            var elements = document.QuerySelectorAll(selector);
            if (elements.Length == 0) return new Dictionary<string, string>(0);

            var result = new Dictionary<string, string>(elements.Length, StringComparer.OrdinalIgnoreCase);
            foreach (var element in elements)
            {
                var key = element.GetAttribute(keyAttribute);
                var value = element.GetAttribute("content");
                if (string.IsNullOrEmpty(key) || value is null) continue;

                var trimmed = key!.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                    ? key[prefix.Length..]
                    : key;

                // First declaration wins, matching how consumers read these.
                if (!result.ContainsKey(trimmed)) result[trimmed] = value;
            }
            return result;
        }

        private static string? Get(IReadOnlyDictionary<string, string> values, string key)
            => values.TryGetValue(key, out var value) ? value : null;

        private static RobotsDirectives ParseRobots(IEnumerable<string?> values)
        {
            var directives = RobotsDirectives.None;

            foreach (var value in values)
            {
                if (string.IsNullOrWhiteSpace(value)) continue;

                foreach (var token in value!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    directives |= token.ToLowerInvariant() switch
                    {
                        "index" => RobotsDirectives.Index,
                        "noindex" => RobotsDirectives.NoIndex,
                        "follow" => RobotsDirectives.Follow,
                        "nofollow" => RobotsDirectives.NoFollow,
                        "noarchive" => RobotsDirectives.NoArchive,
                        "nosnippet" => RobotsDirectives.NoSnippet,
                        "noimageindex" => RobotsDirectives.NoImageIndex,
                        "notranslate" => RobotsDirectives.NoTranslate,
                        "none" => RobotsDirectives.None_Directive,
                        _ => RobotsDirectives.None
                    };
                }
            }

            return directives;
        }

        private static IReadOnlyList<Uri> ResolveAll(IEnumerable<IElement> elements, string attribute, Uri pageUrl)
        {
            List<Uri>? result = null;
            foreach (var element in elements)
            {
                var url = Resolve(element.GetAttribute(attribute), pageUrl);
                if (url is null) continue;

                result ??= new List<Uri>(2);
                result.Add(url);
            }
            return (IReadOnlyList<Uri>?)result ?? Array.Empty<Uri>();
        }

        private static Uri? Resolve(string? value, Uri pageUrl)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;

            return Uri.TryCreate(pageUrl, value.Trim(), out var resolved) ? resolved : null;
        }

        private static int? ParseDimension(string? value)
            => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;

        private static int CountWords(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;

            var count = 0;
            var inWord = false;
            foreach (var c in text!)
            {
                if (char.IsWhiteSpace(c)) { inWord = false; continue; }
                if (!inWord) { count++; inWord = true; }
            }
            return count;
        }

        private static string CollapseWhitespace(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;

            var builder = new StringBuilder(value.Length);
            var lastWasSpace = false;
            foreach (var c in value)
            {
                if (char.IsWhiteSpace(c))
                {
                    if (!lastWasSpace && builder.Length > 0) builder.Append(' ');
                    lastWasSpace = true;
                    continue;
                }
                builder.Append(c);
                lastWasSpace = false;
            }
            return builder.ToString().TrimEnd();
        }

        private static string? Clean(string? value)
        {
            if (value is null) return null;
            var collapsed = CollapseWhitespace(value);
            return collapsed.Length == 0 ? null : collapsed;
        }
    }
}
