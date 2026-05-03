# Using SeoToolkit in a Headless Setup

SeoToolkit supports headless Umbraco setups through two approaches: a dedicated public SEO API endpoint and native Umbraco Delivery API integration. Both expose the same SEO data (meta fields, scripts, etc.) so you can choose whichever fits your architecture.

---

## Option 1: Public SEO API

SeoToolkit exposes a lightweight public REST endpoint that returns SEO data for any published content node. This is framework-agnostic and works with any front-end that can make HTTP requests.

### Configuration

Enable the endpoint in your `appsettings.json`:

```json
{
  "SeoToolkit": {
    "Global": {
      "EnableApiEndpoints": true
    }
  }
}
```

The endpoint returns `404 Not Found` when `EnableApiEndpoints` is `false` (the default).

### Endpoint

```
GET /api/seo?contentGuid={guid}&culture={culture}
```

| Parameter     | Required | Description |
|---------------|----------|-------------|
| `contentGuid` | Yes      | The `Key` (GUID) of the published content node |
| `culture`     | No       | Language/culture code (e.g. `en-US`). Omit for invariant content or the default culture |

### Example Request

```
GET /api/seo?contentGuid=4b5f2a1c-9d3e-4a7b-8f6c-1e2d3a4b5c6d&culture=en-US
```

### Example Response

```json
{
  "metaFields": {
    "seoTitle": "My Page Title",
    "seoDescription": "A short description of this page.",
    "openGraphTitle": "My Page Title",
    "openGraphDescription": "A short description of this page.",
    "openGraphImage": "https://example.com/media/image.jpg",
    "openGraphUrl": "https://example.com/my-page",
    "canonicalUrl": "https://example.com/my-page",
    "robots": "index, follow",
    "schema": null,
    "twitterCardType": "summary_large_image",
    "twitterSite": "@mysite",
    "twitterCreator": "@author",
    "facebookId": null
  },
  "scripts": [
    {
      "definitionAlias": "googleTagManager",
      "config": {
        "containerId": "GTM-XXXXXXX"
      }
    }
  ]
}
```

> **Note:** The `metaFields` key is present when the MetaFields module is installed and not disabled for the API. The `scripts` key is present when the Script Manager module is installed and not disabled for the API.

### Script Rendering Mode

By default, the `scripts` array returns raw script definitions (alias + configuration) for your front-end to interpret. If you want SeoToolkit to render the full script HTML for you instead, enable render mode:

```json
{
  "SeoToolkit": {
    "ScriptManager": {
      "RenderScriptsInApi": true
    }
  }
}
```

With `RenderScriptsInApi: true` the `scripts` value becomes:

```json
{
  "scripts": {
    "headBottom": ["<script>...</script>"],
    "bodyTop":    ["<noscript>...</noscript>"],
    "bodyBottom": ["<script>...</script>"]
  }
}
```

Each array contains the rendered HTML strings to inject at the named position in your page.

---

## Option 2: Umbraco Delivery API

If your project already uses [Umbraco's Content Delivery API](https://docs.umbraco.com/umbraco-cms/reference/content-delivery-api), SeoToolkit can extend every content response with a `seoToolkit` property automatically.

### Configuration

First, make sure the Umbraco Delivery API itself is enabled in `appsettings.json`:

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

Then enable SeoToolkit's Delivery API support:

```json
{
  "SeoToolkit": {
    "Global": {
      "EnableDeliveryApiSupport": true
    }
  }
}
```

> **Important:** `EnableDeliveryApiSupport` is read at startup and registers SeoToolkit's custom `IApiContentResponseBuilder`. Changing this setting requires an application restart.

### Response Shape

All Delivery API content responses gain a top-level `seoToolkit` property alongside the standard `id`, `name`, `properties`, etc.:

```json
{
  "id":          "4b5f2a1c-9d3e-4a7b-8f6c-1e2d3a4b5c6d",
  "name":        "My Page",
  "contentType": "blogPost",
  "properties":  { ... },
  "seoToolkit": {
    "metaFields": {
      "seoTitle":           "My Page Title",
      "seoDescription":     "A short description of this page.",
      "openGraphTitle":     "My Page Title",
      "openGraphDescription": "A short description of this page.",
      "openGraphImage":     "https://example.com/media/image.jpg",
      "openGraphUrl":       "https://example.com/my-page",
      "canonicalUrl":       "https://example.com/my-page",
      "robots":             "index, follow",
      "schema":             null,
      "twitterCardType":    "summary_large_image",
      "twitterSite":        "@mysite",
      "twitterCreator":     "@author",
      "facebookId":         null
    },
    "scripts": [
      {
        "definitionAlias": "googleTagManager",
        "config": {
          "containerId": "GTM-XXXXXXX"
        }
      }
    ]
  }
}
```

The `seoToolkit` object follows the same structure as the public SEO API response, including respecting the `RenderScriptsInApi` setting for scripts.

---

## Disabling API Data per Module

Individual modules can be excluded from both API approaches by adding `"Api"` to their `DisabledModules` list.

**Exclude meta fields from the API:**

```json
{
  "SeoToolkit": {
    "MetaFields": {
      "DisabledModules": ["Api"]
    }
  }
}
```

**Exclude scripts from the API:**

```json
{
  "SeoToolkit": {
    "ScriptManager": {
      "DisabledModules": ["Api"]
    }
  }
}
```

---

## Choosing Between the Two Approaches

| | Public SEO API | Delivery API |
|-|----------------|--------------|
| **Requires Umbraco Delivery API** | No | Yes |
| **Config key to enable** | `SeoToolkit:Global:EnableApiEndpoints` | `SeoToolkit:Global:EnableDeliveryApiSupport` |
| **Endpoint** | `GET /api/seo?contentGuid=...` | Standard Delivery API endpoints |
| **Good for** | Dedicated SEO fetches, non-Delivery API setups | Already using the Delivery API; receive SEO data in one request |

Both approaches expose the same underlying data — choose whichever reduces the number of HTTP round-trips for your front-end.
