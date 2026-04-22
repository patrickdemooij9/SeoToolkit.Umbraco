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
The `SeoToolkit.Umbraco.MetaFields.AI` add-on integrates with the [Umbraco.AI](https://marketplace.umbraco.com/packages/umbraco.ai) package to let editors automatically generate SEO meta field suggestions for any content node with a single click.

**Prerequisites**: Umbraco.AI (v1.x) must be installed and configured with at least one chat profile.

**Installation**:

```
Install-Package SeoToolkit.Umbraco.MetaFields.AI
```

Once installed, a **✨ Generate with AI** button appears in the Meta Fields workspace for each content node. Clicking it calls your configured AI chat profile and fills in suggested values for:

- **Title** (50–60 characters)
- **Meta Description** (150–160 characters)
- **Open Graph Title**
- **Open Graph Description**

The generated suggestions are applied directly to the fields so you can review and adjust them before saving. No values are automatically persisted — the normal save flow in Umbraco still applies.

**Availability detection**: The button is only shown when the `SeoToolkit.Umbraco.MetaFields.AI` package is installed and reachable. When the package is absent the Meta Fields workspace behaves exactly as before.

### Sitemap
Sitemap gives you an sitemap.xml where all your pages are listed. This package works with multiple domains and languages. It creates a /sitemap.xml for each domain and also a sitemap index with all your sitemaps listed within.

### Robots.txt
Robots.txt gives you an easy interface to edit your robots.txt. After installing the package, you'll get the /robots.txt path that'll display your configured robots.txt.

### Script Manager
Script manager easily allows your users to add new scripts to the website. They are able to quickly add Google Tag Manager or Hotjar with just a few clicks. It also gives them the ability for adding custom scripts if their script definition is not yet available.

### Redirects
Redirects allow you with an easy interface to create redirects from content/media to other nodes. You are also able to use regex redirects to handle a lot of redirects at once.

### Site audit
Site audits crawl your website and find issues that could impact the user experience.

### Not found handling
Easily select a page which is used for your 404 pages. No need to develop anything custom, just select your content node and everything will be working out of the box.

## Documentation
All documentation about the package can be found here: https://seotoolkit.gitbook.io/useotoolkit/

## Credits
The logo used for SeoToolkit can be found here: https://thenounproject.com/icon/toolkit-2311174/
