<img src="package/SeoToolkitIcon.png?raw=true" alt="Umbraco Friendly Sitemap" width="250" align="right" />

# SeoToolkit

SeoToolkit is a SEO package for Umbraco 9. This package features most functionalities needed for your SEO needs like meta fields, sitemap, robots.txt and much more.

## Installation

Installation of that package can be done through the NuGet command: 

`Install-Package uSeoToolkit.Umbraco`

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

SeoToolkit has many features that are very important for your SEO needs. The SeoToolkit has these features:

- Meta Fields
- Sitemap
- Robots.txt
- Script Manager
- Redirects

Each of these functionalities can also be found in separate packages. So if you only want to use the sitemap functionality and the robots.txt functionality then you can do that!

### Meta Fields
Meta Fields allow you to easily set your meta fields like Title, Description, Open Graph Title/Description/Image and canonical URL based on already existing fields on your content node. This allows your users to see where the values are coming from and also what their values will be. At the same time your users can also change these values themselves, so not code is required.

### Sitemap
Sitemap gives you an sitemap.xml where all your pages are listed. This package works with multiple domains and languages. It creates a /sitemap.xml for each domain and also a sitemap index with all your sitemaps listed within.

### Robots.txt
Robots.txt gives you an easy interface to edit your robots.txt. After installing the package, you'll get the /robots.txt path that'll display your configured robots.txt.

### Script Manager
Script manager easily allows your users to add new scripts to the website. They are able to quickly add Google Tag Manager or Hotjar with just a few clicks. It also gives them the ability for adding custom scripts if their script definition is not yet available.

### Redirects
Redirects allow you with an easy interface to create redirects from content/media to other nodes. You are also able to use regex redirects to handle a lot of redirects at once.

## Documentation
All documentation about the package can be found here: https://seotoolkit.gitbook.io/useotoolkit/

## Credits
The logo used for SeoToolkit can be found here: https://thenounproject.com/icon/toolkit-2311174/
