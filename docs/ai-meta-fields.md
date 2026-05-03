# AI Meta Fields

SeoToolkit can use AI to automatically generate suggestions for your meta fields. The following fields can be generated:

- **Page Title** (target: up to 55 characters)
- **Meta Description** (target: up to 150 characters)
- **Open Graph Title**
- **Open Graph Description** (target: up to 200 characters)

The AI reads the content of the page — its name, URL segment, content type, and any text properties — and returns tailored suggestions. Editors review and selectively apply each suggestion before saving; nothing is written automatically.

---

## Packages

The feature is split into two packages following the same layered add-on pattern used by `SeoToolkit.Umbraco.uSync`:

| Package | Purpose |
|---|---|
| `SeoToolkit.Umbraco.AI.Core` | Prompt-building logic and the `IMetaFieldsAIService`/`IAIGenerationService` abstractions. No AI-provider dependency. |
| `SeoToolkit.Umbraco.AI.Integration` | Wires `SeoToolkit.Umbraco.AI.Core` to the [Umbraco.AI](https://marketplace.umbraco.com/packages/umbraco.ai) package. |

Install `SeoToolkit.Umbraco.AI.Integration` for the standard Umbraco.AI-backed setup. Install only `SeoToolkit.Umbraco.AI.Core` if you want to connect a different AI backend.

---

## Prerequisites

- **SeoToolkit** installed and Meta Fields enabled on at least one document type.
- **Umbraco.AI** (v1.x) installed and configured with at least one chat profile.

See the [Umbraco.AI documentation](https://marketplace.umbraco.com/packages/umbraco.ai) for details on setting up a chat profile (Azure OpenAI, OpenAI, etc.).

---

## Installation

### 1. Install the NuGet packages

Install `SeoToolkit.Umbraco.AI.Integration`. This pulls in `SeoToolkit.Umbraco.AI.Core` automatically:

```
dotnet add package SeoToolkit.Umbraco.AI.Integration
```

Or using the Package Manager Console:

```
Install-Package SeoToolkit.Umbraco.AI.Integration
```

### 2. Configure Umbraco.AI

Umbraco.AI requires an AI provider to be configured in `appsettings.json`. The exact configuration depends on the provider. For example, using Azure OpenAI:

```json
{
  "Umbraco": {
    "AI": {
      "ChatProfiles": [
        {
          "Name": "default",
          "Provider": "AzureOpenAI",
          "Endpoint": "https://YOUR_RESOURCE.openai.azure.com/",
          "ApiKey": "YOUR_API_KEY",
          "DeploymentName": "YOUR_DEPLOYMENT_NAME"
        }
      ]
    }
  }
}
```

Refer to the [Umbraco.AI documentation](https://marketplace.umbraco.com/packages/umbraco.ai) for all available providers and configuration options.

### 3. Rebuild and run

No code changes are required. The SeoToolkit composers automatically detect the integration package and enable the AI button in the backoffice.

---

## Using AI suggestions in the backoffice

Once the packages are installed and Umbraco.AI is configured, a **✨ Generate with AI** button appears in the document workspace action bar (the bottom bar next to the **Save** button) for any content node that has Meta Fields enabled.

**Step-by-step:**

1. Open a content node in the Umbraco backoffice.
2. Click **✨ Generate with AI** in the action bar.
3. SeoToolkit sends the page context to the configured AI chat profile and generates suggestions for:
   - Page Title
   - Meta Description
   - OG Title
   - OG Description
4. A modal opens showing all four suggestions. Each suggestion has a toggle.
5. Deselect any suggestions you do not want to apply.
6. Click **Apply** to write the selected values into the Meta Fields editor.
7. The values are populated in the editor but **not saved yet**. Use the normal **Save** or **Save and publish** button to persist the changes.

> **Note:** The button is only visible when `SeoToolkit.Umbraco.AI.Integration` is installed. Removing or not installing the package leaves the Meta Fields workspace unchanged.

---

## How it works

When the **Generate with AI** button is clicked, SeoToolkit builds a context string from the content node:

- Page name
- URL segment
- Content type alias
- Existing page title and meta description (if any)
- Plain-text extracted from all string properties on the node (up to 500 characters)

This context is sent to the AI as a structured prompt. The AI returns a JSON object with the four field values, which SeoToolkit parses and presents in the suggestions modal.

---

## Custom AI provider

If you want to use an AI backend other than Umbraco.AI, install only `SeoToolkit.Umbraco.AI.Core` and register your own implementation of `IAIGenerationService`:

```csharp
public class MyCustomAIService : IAIGenerationService
{
    public async Task<string> GenerateRawResponseAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        // Call your AI provider here and return the raw text response.
        // SeoToolkit expects a JSON string like:
        // {"title":"...","metaDescription":"...","openGraphTitle":"...","openGraphDescription":"..."}
        throw new NotImplementedException();
    }
}
```

Register it in a composer and set the `IsAIEnabled` flag so the backoffice button is shown:

```csharp
using SeoToolkit.Umbraco.AI.Core.Services;
using SeoToolkit.Umbraco.Common.Core.Helpers;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

public class MyAIComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IAIGenerationService, MyCustomAIService>();
        AIHelper.IsAIEnabled = true;
    }
}
```

> **Note:** `SeoToolkit.Umbraco.AI.Core` is registered automatically via its own composer (`AIComposer`) and wires up `IMetaFieldsAIService`. You only need to provide the `IAIGenerationService` implementation.
