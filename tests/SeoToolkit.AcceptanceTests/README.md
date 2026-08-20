# SeoToolkit acceptance tests

Playwright browser tests that run against a **real, running Umbraco site** with SeoToolkit
installed. They cover the backoffice UI — the eight Lit/TypeScript bundles under
`src/SeoToolkit.Umbraco.<Feature>/assets/` — which unit tests cannot reach.

Current scope is a **smoke suite** (18 tests, ~1 minute): the section loads, every tree node
opens its workspace, the SEO tabs render on content and document types, the overridden
document-type Save still persists, and nothing throws in the browser console.

## Running locally

**1. Build the backoffice bundles.** They are gitignored (`**/entry/`) and .NET snapshots
static web assets at build time, so this must happen *before* you build or run the site.

```bash
cd src && npm ci && npm run build:all
```

**2. Start the test site.**

```bash
dotnet run --project src/SeoToolkit.Umbraco.Site
```

**3. Configure and run the tests.** Copy `.env.example` to `.env` and fill in the base URL and
a superadmin login for that site. **Use the HTTPS URL** (`https://localhost:44324`, not the HTTP
one) - backoffice auth does not work over plain HTTP, see below.

```bash
cd tests/SeoToolkit.AcceptanceTests && npm ci && npx playwright install chromium && npx playwright test
```

Use `npx playwright test --ui` while writing or debugging selectors — far faster than
re-running headless. `npx playwright show-report` opens the last HTML report.

## In CI

`.github/workflows/acceptance-tests.yml` runs the whole thing on `dev/main`: SQL Server 2022
service container, unattended Umbraco install, then this suite. It is a separate workflow from
`build-and-test.yml` so a slow browser job never blocks the fast unit-test signal.

Four things in that workflow are load-bearing and easy to break:

- **The site must be served over HTTPS.** Umbraco backoffice auth depends on the
  `__Host-umbPkceCode` cookie; the `__Host-` prefix and the `Secure` flag mean the browser
  never sends it back over plain HTTP, and the helper package dies in `extractPKCECookie`
  with a bare `Cannot read properties of undefined`. CI generates a throwaway dev cert for
  this; Playwright runs with `ignoreHTTPSErrors`, so the cert never needs to be trusted.
- **`npm run build:all` must precede `dotnet publish`.** Otherwise the site boots with no
  SeoToolkit UI. The "Verify backoffice bundles were published" step exists to turn that into
  a one-line failure instead of a wall of Playwright timeouts.
- **`Umbraco__CMS__Global__InstallMissingDatabase=true`.** Umbraco auto-creates the database
  only for SQLite and LocalDB; against a real SQL Server, boot fails without this.
- **The readiness probe waits on the backoffice, not on `/`.** Booting from `dotnet publish`
  output, the site front end returns Umbraco's "no published content" page even though content
  exists and `/sitemap.xml` renders correctly - the Clean package migration reports its
  templates as conflicting and leaves them without a usable physical file. The smoke suite
  never touches the front end, so the probe checks `/umbraco` plus a SeoToolkit bundle
  instead. **This will need solving before the frontend-output suite can be added.**

## Why the helper package is pinned to 17.3.x

The site targets **Umbraco 17.0.0** — deliberately, since that is the floor of the range the
feature packages support (`[17.0.0,17.999)`), and testing against the floor catches accidental
use of APIs added in later 17.x minors.

`@umbraco-cms/acceptance-test-helpers` is versioned in lockstep with Umbraco CMS minors, and its
**earliest published 17.x release is 17.3.0** — there is no 17.0.x build. So it is pinned to
`~17.3.0`, the closest available.

**Do not "upgrade" this to the latest 17.x or to 18.x** to silence a dependency warning. If a
helper turns out to depend on post-17.0 API surface (symptom: a 404 from a Management API call,
or a selector that never resolves), hand-roll that single call instead of moving the pin.

## Selector strategy

Assertions target SeoToolkit's **custom element tag names** (`seotoolkit-module-robotstxt`,
`st-content-view`, …) rather than DOM structure or visible label text. Tag names come from the
`@customElement` decorators and are already a stable public contract, since the manifests
reference those elements. Structure and labels are not.

`playwright.config.ts` sets `testIdAttribute: 'data-mark'` to match Umbraco's own convention.
SeoToolkit's elements do not currently carry `data-mark` attributes; adding them is worthwhile
when tests need to target *inner* controls (individual buttons, fields) where a tag name is no
longer unique — that is, when the CRUD suite lands.

## Known package quirks the tests work around

Found while building this suite. None are test bugs - they are package behaviour the specs
have to accommodate, and each is a candidate fix in its own right.

- **SEO sub-view order is nondeterministic.** `seoToolkitContentView.element.ts` and
  `seoToolkitDocumentView.element.ts` build their routes in whatever order
  `UmbExtensionsManifestInitializer` yields - the manifest `weight` is never applied - and
  then alias `routes[0]` to the empty path. So which sub-tab a user lands on (Meta Fields,
  Sitemap or Page checks) changes between page loads. The specs never assert a default
  sub-view; they click the one they want.
- **A dead branch in the tree mapper.** `seoToolkitTreeSource.ts` maps GUID
  `a9b6dec6-e045-476a-ba3f-742355e18e33` to the NotFound entity type, but no C# class
  implements `ISeoTreeSection` for NotFound, so that id is never served. NotFound surfaces
  through the Settings node instead.
- **Saving a document type gives no UI feedback.** No toast, no button state. The Save spec
  therefore asserts persistence through the API rather than looking for a notification.

## Layout

```
helpers/
  seoToolkitConstants.ts   Tree GUIDs, entity types, element tags, API routes.
                           Mirrors package source — update both together.
  test.ts                  Umbraco's test fixture + a console-error guard.
tests/
  auth.setup.ts            Logs in once; caches the session for every spec.
  smoke/                   The suite.
```

The console-error guard in `helpers/test.ts` is what gives these tests teeth: a Lit bundle that
fails to import, a missing custom element, or a broken API call all surface as console errors
rather than as visible failures. Its ignore list is deliberately short — every entry is a class
of regression the suite can no longer see, so add to it only with a comment explaining why.
