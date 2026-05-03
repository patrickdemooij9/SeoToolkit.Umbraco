# Umbraco SEO in a Headless Setup: A Complete Guide

Headless Umbraco gives you the freedom to build front-ends with any technology — Next.js, Nuxt, Astro, SvelteKit, or a native mobile app — while keeping content management in the familiar Umbraco back office. That freedom comes with a trade-off: none of the classic server-side rendering shortcuts for SEO apply anymore. You can't drop a tag helper into a Razor view and call it done.

This guide walks through what SEO means in a headless context, the challenges you'll face, the tools Umbraco provides out of the box, and how **SeoToolkit** fills the remaining gaps so your editors and developers can work without friction.

---

## What Is Headless Umbraco?

In a traditional ("coupled") Umbraco site, the CMS renders HTML on the server and sends a fully-formed page to the browser. Search engine crawlers see the same complete HTML your visitors do, including `<title>` tags, `<meta>` descriptions, canonical URLs, and Open Graph tags.

In a **headless** setup, Umbraco becomes a pure content API:

- Content is authored in the Umbraco back office as normal.
- A front-end application (React, Vue, Angular, a static site generator, etc.) fetches content via HTTP — typically through Umbraco's [Content Delivery API](https://docs.umbraco.com/umbraco-cms/reference/content-delivery-api).
- The front-end is responsible for rendering HTML, including everything search engines need.

This decoupled architecture is popular because it enables faster front-end development, better performance, and the ability to deliver the same content to multiple channels (web, mobile, kiosk, etc.).

---

## Why SEO Is More Complex in a Headless Setup

When the CMS no longer renders HTML, every SEO concern that was previously automatic becomes an explicit engineering decision.

### Meta tags must be injected by the front-end

Search engines rely on `<title>`, `<meta name="description">`, `<link rel="canonical">`, and Open Graph tags in the `<head>` of each page. In a headless app these tags must be set per-route, per-page — dynamically — using whatever head-management library your framework provides (e.g. `next/head`, `@vueuse/head`, `<svelte:head>`).

The data for those tags has to come from somewhere. Without a dedicated solution, developers either hard-code values or wire up custom logic to pull them from content properties.

### Rendering must be crawlable

Pure client-side JavaScript applications (single-page applications that only render in the browser) are problematic for SEO. While Google does execute JavaScript, it crawls JS-rendered content with a delay and lower priority than server-rendered HTML.

The safest approaches are:

- **Server-Side Rendering (SSR)** — the front-end framework renders HTML on the server per request.
- **Static Site Generation (SSG)** — HTML is pre-rendered at build time.
- **Incremental Static Regeneration (ISR)** — a hybrid: pages are statically generated but can be revalidated in the background.

All three produce fully-formed HTML that search engines can index immediately, without waiting for client-side JavaScript to run.

### Sitemaps, robots.txt, and structured data

A sitemap tells search engines which pages exist and when they were last updated. Robots.txt controls what crawlers are allowed to index. Structured data (JSON-LD schema markup) enhances search result appearance with rich snippets.

In a traditional Umbraco setup these are often handled by a package. In a headless setup they may live in the CMS, the front-end, or both — and keeping them in sync requires careful planning.

### Redirect handling

Content editors regularly rename pages, restructure sections, or retire old URLs. Each change risks breaking inbound links and losing accumulated search authority unless a 301 redirect exists. In a headless setup, redirects may need to be handled at the CDN layer, the front-end server, or the Umbraco back-end — sometimes all three.

---

## What Umbraco Provides Out of the Box

### Content Delivery API

Umbraco's built-in [Content Delivery API](https://docs.umbraco.com/umbraco-cms/reference/content-delivery-api) exposes published content as JSON. It supports:

- Fetching pages by URL path or content GUID
- Filtering and querying content trees
- Multi-language content via `Accept-Language` headers
- Preview mode for draft content

Enable it in `appsettings.json`:

```json
{
  "Umbraco": {
    "CMS": {
      "DeliveryApi": {
        "Enabled": true
      }
    }
  }
}
```

A typical response for a page looks like:

```json
{
  "id":          "4b5f2a1c-...",
  "name":        "About Us",
  "contentType": "standardPage",
  "properties": {
    "pageTitle":   "About Us",
    "bodyText":    "<p>We are...</p>"
  }
}
```

This gives your front-end the raw content. What it does **not** give you out of the box is computed SEO values — the resolved page title after applying your title template, the correct canonical URL, the Open Graph image URL, or the robots directive that an editor has set.

---

## Handling SEO in Your Front-End

Before reaching for a package, it helps to understand what a bare-metal headless SEO implementation looks like so you know exactly what you're trading away.

### Setting `<title>` and `<meta>` tags

In Next.js (App Router) you'd export a `generateMetadata` function from each page:

```tsx
// app/[slug]/page.tsx
export async function generateMetadata({ params }) {
  const content = await fetchUmbracoContent(params.slug);
  return {
    title:       content.properties.seoTitle ?? content.name,
    description: content.properties.seoDescription,
    openGraph: {
      title:       content.properties.openGraphTitle ?? content.name,
      description: content.properties.openGraphDescription,
      images:      [content.properties.openGraphImage?.url],
      url:         content.properties.canonicalUrl,
    },
    alternates: {
      canonical: content.properties.canonicalUrl,
    },
  };
}
```

This works, but it requires:

1. Editors to know and fill in the correct property aliases.
2. Developers to map every SEO property in every page type.
3. Fallback logic (e.g. fall back to the page title when `seoTitle` is empty) to be duplicated across every route.

### Canonical URLs

Canonical URLs prevent duplicate-content penalties when the same page is accessible at multiple URLs (e.g. with and without a trailing slash, or through a CDN alias). In a headless setup the canonical URL often needs to be computed from the public domain of the front-end, not the Umbraco back-end domain. Getting this right consistently across all pages requires careful logic.

### Structured Data (JSON-LD)

Rich results in Google Search (star ratings, FAQ dropdowns, breadcrumbs, event listings) are powered by structured data in the form of JSON-LD embedded in the page. Adding schema markup per page type is straightforward but repetitive, and errors are invisible unless you use the [Google Rich Results Test](https://search.google.com/test/rich-results).

### The Problem at Scale

All of the above is manageable for a small site. At scale — dozens of content types, multiple languages, multi-domain deployments — it becomes a maintenance burden. Content editors lose the immediate feedback that a traditional Umbraco SEO package provides (the live preview of what a search result will look like), and developers have to maintain SEO logic spread across both the CMS and the front-end.

---

## How SeoToolkit Solves Headless SEO

[SeoToolkit](https://useotoolkit.com/) is a comprehensive SEO package for Umbraco that was built to work in both traditional and headless architectures. Instead of pushing SEO logic to the front-end, SeoToolkit computes and exposes fully-resolved SEO values from Umbraco itself — so the front-end only needs to render what it receives.

### What SeoToolkit Provides

| Feature | Description |
|---------|-------------|
| **Meta Fields** | Editors set SEO title, description, Open Graph fields, canonical URL, robots directive, and more — all computed with fallback logic and title templates |
| **Script Manager** | Manage Google Tag Manager, GA4, Hotjar, and custom scripts from the back office; expose them via API |
| **Sitemap** | Automatically generated `sitemap.xml` per domain, with multi-language `hreflang` support |
| **Robots.txt** | Editable `robots.txt` with a back-office UI |
| **Redirects** | 301/302 redirect management with regex support |
| **Site Audit** | Crawl-based checks for broken links, missing titles, missing descriptions, and missing image alt text |
| **Not Found Handling** | Select a content node to serve as the 404 page — no custom code required |

### Installation

```bash
dotnet add package SeoToolkit.Umbraco
```

Or via NuGet Package Manager:

```
Install-Package SeoToolkit.Umbraco
```

---

## Integrating SeoToolkit with a Headless Front-End

SeoToolkit exposes its computed SEO data through two purpose-built API options. Both return exactly the same data; you choose based on your architecture.

### Option A: Public SEO API

A dedicated REST endpoint at `/api/seo` returns all SEO data for any content node. This is the right choice when:

- You are not using the Umbraco Delivery API.
- You want to fetch SEO data independently of content (e.g. in a middleware layer or at a different cache TTL).

**Enable it** in `appsettings.json`:

```json
{
  "SeoToolkit": {
    "Global": {
      "EnableApiEndpoints": true
    }
  }
}
```

**Fetch SEO data:**

```
GET /api/seo?contentGuid={pageGuid}&culture={culture}
```

**Example response:**

```json
{
  "metaFields": {
    "seoTitle":             "About Us | Acme Corp",
    "seoDescription":       "Learn about our mission and team.",
    "openGraphTitle":       "About Us | Acme Corp",
    "openGraphDescription": "Learn about our mission and team.",
    "openGraphImage":       "https://example.com/media/og-about.jpg",
    "openGraphUrl":         "https://example.com/about",
    "canonicalUrl":         "https://example.com/about",
    "robots":               "index, follow",
    "schema":               null,
    "twitterCardType":      "summary_large_image",
    "twitterSite":          "@acmecorp",
    "twitterCreator":       null,
    "facebookId":           null
  },
  "scripts": [
    {
      "definitionAlias": "googleTagManager",
      "config": { "containerId": "GTM-XXXXXXX" }
    }
  ]
}
```

Your Next.js `generateMetadata` function becomes trivial:

```tsx
export async function generateMetadata({ params }) {
  const page    = await fetchDeliveryApi(params.slug);
  const seo     = await fetch(`/api/seo?contentGuid=${page.id}`).then(r => r.json());
  const meta    = seo.metaFields;

  return {
    title:       meta.seoTitle,
    description: meta.seoDescription,
    robots:      meta.robots,
    openGraph: {
      title:       meta.openGraphTitle,
      description: meta.openGraphDescription,
      url:         meta.openGraphUrl,
      images:      meta.openGraphImage ? [meta.openGraphImage] : [],
    },
    alternates: {
      canonical: meta.canonicalUrl,
    },
  };
}
```

All fallback logic, title templates, and editor overrides are resolved server-side by SeoToolkit before your front-end ever sees the data.

### Option B: Delivery API Integration

If you're already using the Umbraco Delivery API, SeoToolkit can embed SEO data directly in every content response as a `seoToolkit` property. This eliminates a second round-trip.

**Enable it** in `appsettings.json` (requires an app restart):

```json
{
  "SeoToolkit": {
    "Global": {
      "EnableDeliveryApiSupport": true
    }
  }
}
```

Every Delivery API response is extended with:

```json
{
  "id":          "4b5f2a1c-...",
  "name":        "About Us",
  "contentType": "standardPage",
  "properties":  { ... },
  "seoToolkit": {
    "metaFields": {
      "seoTitle":             "About Us | Acme Corp",
      "seoDescription":       "Learn about our mission and team.",
      "canonicalUrl":         "https://example.com/about",
      "robots":               "index, follow",
      "openGraphTitle":       "About Us | Acme Corp",
      "openGraphDescription": "Learn about our mission and team.",
      "openGraphImage":       "https://example.com/media/og-about.jpg"
    },
    "scripts": [
      {
        "definitionAlias": "googleTagManager",
        "config": { "containerId": "GTM-XXXXXXX" }
      }
    ]
  }
}
```

Your front-end reads `seoToolkit.metaFields` directly from the same response it already fetches — no extra request needed.

---

## Script Management in a Headless Front-End

Analytics and tracking scripts (Google Tag Manager, GA4, Hotjar, etc.) are normally added directly to HTML templates. In a headless setup they need to be injected by the front-end framework. The challenge is giving content editors control over which scripts are active, and on which domains, without requiring a code deploy.

SeoToolkit's Script Manager solves this by storing script configuration in Umbraco and exposing it via the same API endpoints described above.

By default, the `scripts` response contains raw definitions — the script alias and its configuration values. Your front-end uses these to initialise the scripts at runtime.

If you'd prefer SeoToolkit to render the full `<script>` HTML for you, enable render mode:

```json
{
  "SeoToolkit": {
    "ScriptManager": {
      "RenderScriptsInApi": true
    }
  }
}
```

The response then contains rendered HTML strings bucketed by injection position:

```json
{
  "scripts": {
    "headBottom": ["<!-- Google Tag Manager --><script>...</script>"],
    "bodyTop":    ["<!-- Google Tag Manager (noscript) --><noscript>...</noscript>"],
    "bodyBottom": []
  }
}
```

Inject each array at the corresponding position in your layout. Editors can add, remove, or reconfigure scripts from the back office without any front-end changes.

---

## Sitemaps and Robots.txt Without a Back-End Rebuild

SeoToolkit serves `sitemap.xml` and `robots.txt` directly from Umbraco, so these critical files are always up-to-date as content is published. This is a significant advantage over static-generation approaches where sitemaps need to be rebuilt on every content change.

- `/sitemap.xml` — a sitemap index listing all per-domain sitemaps
- `/sitemap-{domain}.xml` — a sitemap per domain
- `/robots.txt` — managed from the back office with a UI, including automatic sitemap references

No extra configuration is required for headless setups. Because Umbraco serves these files directly, your front-end proxy simply needs to pass requests for these paths through to Umbraco rather than serving them from the front-end.

---

## Multi-Language and Multi-Domain Support

Both API endpoints support multi-language content. Pass the `culture` query parameter to the public SEO API to receive language-specific meta values:

```
GET /api/seo?contentGuid={guid}&culture=nl-NL
```

For the Delivery API, use the standard `Accept-Language` header:

```
Accept-Language: nl-NL
```

SeoToolkit resolves per-language SEO values consistently, including language-specific title templates and fallback chains.

Multi-domain setups are also supported — SeoToolkit associates domain-specific settings (such as Script Manager scripts) with the correct Umbraco domain automatically.

---

## Checklist: Headless Umbraco SEO

Use this checklist when setting up a headless Umbraco project:

- [ ] **Rendering strategy** — use SSR, SSG, or ISR; avoid pure client-side rendering for publicly indexed pages
- [ ] **Meta tags** — use SeoToolkit to provide resolved values; inject them in your framework's head management
- [ ] **Canonical URLs** — sourced from SeoToolkit `canonicalUrl`; ensure your front-end domain matches
- [ ] **Open Graph** — OpenGraph title, description, and image sourced from SeoToolkit `metaFields`
- [ ] **Robots** — per-page robots directive sourced from SeoToolkit `robots`
- [ ] **Sitemap** — served by SeoToolkit at `/sitemap.xml`; proxy or link to it from your front-end domain
- [ ] **Robots.txt** — managed in the SeoToolkit back office; proxy from your front-end domain
- [ ] **Redirects** — managed in SeoToolkit's Redirect module; handle at the Umbraco layer or mirror to your CDN/front-end
- [ ] **Scripts/tracking** — managed in SeoToolkit Script Manager; injected by the front-end from API data
- [ ] **Multi-language** — pass `culture` / `Accept-Language` to receive language-correct SEO values
- [ ] **Site audit** — run periodically in the SeoToolkit back office to catch broken links and missing meta fields

---

## Summary

Headless Umbraco opens up exciting architectural possibilities, but it shifts responsibility for SEO from the CMS to the front-end by default. Managed poorly, this leads to duplicated logic, editor friction, and hard-to-debug SEO regressions.

SeoToolkit closes this gap by:

1. Giving editors a rich back-office interface to manage all SEO fields with live previews and fallback logic.
2. Exposing fully-resolved SEO data via a purpose-built API or embedded in the Umbraco Delivery API response.
3. Keeping sitemaps, robots.txt, redirects, and tracking scripts managed centrally in Umbraco — regardless of how many front-ends consume the content.

The result is a headless setup where SEO is a first-class concern, not an afterthought — and where your front-end team receives clean, ready-to-use data rather than raw content properties to interpret.

**Get started:** [install SeoToolkit](https://www.nuget.org/packages/SeoToolkit.Umbraco) and follow the [headless integration guide](./headless.md) for step-by-step configuration.
