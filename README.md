<img src="package/SeoToolkitIcon.png?raw=true" alt="Umbraco Friendly Sitemap" width="250" align="right" />

# SeoToolkit

SeoToolkit is a SEO package for Umbraco 9 to 17. This package features most functionalities needed for your SEO needs like meta fields, sitemap, robots.txt and much more.

SeoToolkit is also an award winning Umbraco package, winning the Umbraco Package Award 2025!

[Official Site](https://useotoolkit.com/) - [Documentation](https://seotoolkit.gitbook.io/useotoolkit/)

## Installation

Installation of that package can be done through the NuGet command: 

`Install-Package SeoToolkit.Umbraco`

If you are installing SeoToolkit for Umbraco 9, use the 1.x versions. 
If you are installing for Umbraco 10, use the 2.x versions.
If you are installing for Umbraco 11-13, use the 3.x versions.
If you are installing for Umbraco 15, use the 4.x versions.
If you are installing for Umbraco 16, use the 5.x versions.
If you are installing for Umbraco 17, use the 6.x versions.

After installing, you'll want to add the following lines to the _ViewImports.cshtml file:

`@addTagHelper *, SeoToolkit.Umbraco.ScriptManager.Core`

`@addTagHelper *, SeoToolkit.Umbraco.MetaFields.Core`

After that, add the following tag helpers in your master template

At the bottom of the head tag
`<render-script position="HeadBottom"></render-script>`
  
At the top of the body tag
`<render-script position="BodyTop"></render-script>`
  
At the bottom of the body tag
`<render-script position="BodyBottom"></render-script>`
  
Wherever you want to render your meta fields
`<meta-fields></meta-fields>`
  
After that, your installation is complete and you can get started with the package!

## Features

SeoToolkit has many features that are very important for your SEO needs. This package also supports multi-language and multi-domain websites out of the box. The SeoToolkit has these features:

- Meta fields
- Sitemap
- Robots.txt
- Script manager
- Redirects
- Site audit
- Not found handling

Each of these functionalities can also be found in separate packages. So if you only want to use the sitemap functionality and the robots.txt functionality then you can do that!

### Meta Fields
Meta Fields allow you to easily set your meta fields like Title, Description, Open Graph Title/Description/Image and canonical URL based on already existing fields on your content node. This allows your users to see where the values are coming from and also what their values will be. At the same time your users can also change these values themselves, so not code is required.

### Meta Fields AI (Add-on)
Two packages power the AI integration, following the same layered add-on pattern as `SeoToolkit.Umbraco.uSync`:

| Package | Purpose |
|---|---|
| `SeoToolkit.Umbraco.AI.Core` | Prompt-building logic and the `IMetaFieldsAIService` abstraction. No AI provider dependency — install your own integration on top. |
| `SeoToolkit.Umbraco.AI.Integration` | Wires `SeoToolkit.Umbraco.AI.Core` to [Umbraco.AI](https://marketplace.umbraco.com/packages/umbraco.ai). Install this when using the official Umbraco AI package. |

**Prerequisites**: Umbraco.AI (v1.x) must be installed and configured with at least one chat profile.

**Installation** (typical setup):

```
Install-Package SeoToolkit.Umbraco.AI.Integration
```

This also pulls in `SeoToolkit.Umbraco.AI.Core` automatically.

Once installed, a **✨ Generate with AI** button appears in the **document workspace action bar** (bottom bar, next to the Save button) for any SEO-enabled content node. Clicking it:

1. Calls the configured Umbraco.AI chat profile to generate suggestions.
2. Opens a sidebar modal showing the generated values for:
   - **Page Title** (50–60 characters)
   - **Meta Description** (150–160 characters)
   - **OG Title**
   - **OG Description**
3. Editors can toggle individual suggestions on or off before clicking **Apply**.
4. Applied values are written into the fields. No values are auto-saved — the normal save flow still applies.

**Availability detection**: The button is shown only when `SeoToolkit.Umbraco.AI.Integration` is installed (the `SeoToolkit.AI.IsAvailable` condition checks the endpoint). When the package is absent the Meta Fields workspace behaves exactly as before.

**Custom AI provider**: If you want to use a different AI backend, install only `SeoToolkit.Umbraco.AI.Core` and register your own implementation of `IAIGenerationService`.

### Sitemap
Sitemap gives you an sitemap.xml where all your pages are listed. This package works with multiple domains and languages. It creates a /sitemap.xml for each domain and also a sitemap index with all your sitemaps listed within.

### Robots.txt
Robots.txt gives you an easy interface to edit your robots.txt. After installing the package, you'll get the /robots.txt path that'll display your configured robots.txt.

### Script Manager
Script manager easily allows your users to add new scripts to the website. They are able to quickly add Google Tag Manager or Hotjar with just a few clicks. It also gives them the ability for adding custom scripts if their script definition is not yet available.

### Redirects
Redirects allow you with an easy interface to create redirects from content/media to other nodes. You are also able to use regex redirects to handle a lot of redirects at once.

#### Importing redirects

Redirects can be imported in bulk via the **Import** action in the Redirects backoffice section. The import flow has two steps:

1. **Validate** — upload the file and choose a domain. The file is checked for errors before anything is saved.
2. **Import** — if validation passes, click **Submit** to write the redirects to the database.

**Supported file formats**

| Format | Accepted extensions |
|--------|---------------------|
| CSV    | `.csv`              |
| Excel  | `.xls`, `.xlsx`     |

**CSV format**

The file must be comma-delimited (`,`) and contain **2 to 4 columns** in this order:

| Column | Required | Description |
|--------|----------|-------------|
| `From` | ✅ | The old (source) URL. Must be a relative path, e.g. `/old-page`. Query strings are preserved. |
| `To`   | ✅ | The new (destination) URL. Can be a relative path or an absolute URL. |
| `StatusCode` | ❌ | The HTTP redirect code. Accepted values: `301` (permanent, default) or `302` (temporary). |
| `Enabled` | ❌ | Whether the redirect is active. Accepted values: `true` (default) or `false`. |

An optional header row (`From,To,StatusCode,Enabled`) is recognised and skipped automatically.

Example CSV with all four columns:

```csv
From,To,StatusCode,Enabled
/old-page,/new-page,301,true
/another-old-page,https://example.com/,302,true
/disabled-redirect,/target-page,301,false
```

Minimal example (only required columns):

```csv
/old-page,/new-page
/another-old-page,https://example.com/
```

**Excel format**

The same column structure applies as for CSV. The first sheet of the workbook is read. Values follow the same rules (relative or absolute URLs, `301`/`302` status codes, `true`/`false` enabled flag).

**Domain scoping**

Before importing you are asked to select a domain. Choose **All Sites** to create redirects that apply across all domains, or pick a specific domain to scope the redirects to that domain only.

**Validation rules**

- Every row must have at least a `From` and a `To` value.
- The `From` URL must be a valid relative URL.
- A redirect for the same `From` URL must not already exist in the database for the chosen domain.
- The same `From` URL must not appear more than once in the import file.
- Only status codes `301` and `302` are accepted.

#### Exporting redirects

All existing redirects can be exported via the **Export** action in the Redirects backoffice section. This downloads a file called `redirects.csv` that contains every redirect in the following format:

```csv
From,To,StatusCode,Enabled
/old-page,/new-page,301,true
/another-old-page,https://example.com/,302,false
```

The exported file is fully compatible with the import format, making it easy to migrate redirects between environments or use the export as a starting point for bulk edits.

### Site audit
Site audits crawl your website and find issues that could impact the user experience.

### Not found handling
Easily select a page which is used for your 404 pages. No need to develop anything custom, just select your content node and everything will be working out of the box.

## Documentation
All documentation about the package can be found here: https://seotoolkit.gitbook.io/useotoolkit/

## Credits
The logo used for SeoToolkit can be found here: https://thenounproject.com/icon/toolkit-2311174/
