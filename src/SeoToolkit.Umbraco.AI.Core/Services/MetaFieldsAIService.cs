using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using SeoToolkit.Umbraco.AI.Core.Models;
using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Services;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;

namespace SeoToolkit.Umbraco.AI.Core.Services
{
    public class MetaFieldsAIService : IMetaFieldsAIService
    {
        private static readonly string[] GeneratableFieldAliases =
        [
            SeoFieldAliasConstants.Title,
            SeoFieldAliasConstants.MetaDescription,
            SeoFieldAliasConstants.OpenGraphTitle,
            SeoFieldAliasConstants.OpenGraphDescription,
        ];

        private static readonly Regex HtmlTagRegex = new("<[^>]+>", RegexOptions.Compiled);
        private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);

        private readonly IAIGenerationService _generationService;
        private readonly IMetaFieldsService _metaFieldsService;
        private readonly IDocumentUrlService _documentUrlService;
        private readonly ILogger<MetaFieldsAIService> _logger;

        public MetaFieldsAIService(
            IAIGenerationService generationService,
            IMetaFieldsService metaFieldsService,
            IDocumentUrlService documentUrlService,
            ILogger<MetaFieldsAIService> logger)
        {
            _generationService = generationService;
            _metaFieldsService = metaFieldsService;
            _documentUrlService = documentUrlService;
            _logger = logger;
        }

        public async Task<MetaFieldsAIGenerateResponseModel> GenerateAsync(
            IPublishedContent content,
            string culture,
            CancellationToken cancellationToken = default)
        {
            var existingMeta = _metaFieldsService.Get(content, includeUserValues: false);

            var contextBuilder = new StringBuilder();
            contextBuilder.AppendLine($"Page name: {content.Name}");
            contextBuilder.AppendLine($"URL segment: {_documentUrlService.GetUrlSegment(content.Key, culture, true)}");
            contextBuilder.AppendLine($"Content type: {content.ContentType.Alias}");

            var currentTitle = existingMeta?.Title;
            var currentDescription = existingMeta?.MetaDescription;
            if (!string.IsNullOrWhiteSpace(currentTitle))
                contextBuilder.AppendLine($"Existing page title: {currentTitle}");
            if (!string.IsNullOrWhiteSpace(currentDescription))
                contextBuilder.AppendLine($"Existing meta description: {currentDescription}");

            var textSummary = BuildTextSummary(content, culture);
            if (!string.IsNullOrWhiteSpace(textSummary))
                contextBuilder.AppendLine($"Page text content: {textSummary}");

            var systemPrompt =
                "You are an SEO expert. Generate optimized meta field values for an Umbraco CMS page. " +
                "Return ONLY a valid JSON object with no additional text, markdown, or explanation. " +
                "The JSON must contain exactly these fields: title, metaDescription, openGraphTitle, openGraphDescription. " +
                "Follow these guidelines:\n" +
                "- title: SEO-optimized page title, 55 characters max\n" +
                "- metaDescription: Compelling meta description, 150 characters max\n" +
                "- openGraphTitle: Open Graph title for social sharing, can match or vary from title\n" +
                "- openGraphDescription: Open Graph description for social sharing, 200 characters max";

            var userPrompt =
                $"Generate SEO meta fields for this page:\n{contextBuilder}\n\n" +
                "Respond with a JSON object only, for example:\n" +
                "{\"title\":\"...\",\"metaDescription\":\"...\",\"openGraphTitle\":\"...\",\"openGraphDescription\":\"...\"}";

            var responseText = await _generationService.GenerateRawResponseAsync(
                systemPrompt, userPrompt, cancellationToken);

            return ParseResponse(responseText);
        }

        private static string BuildTextSummary(IPublishedContent content, string culture)
        {
            var textParts = new List<string>();

            foreach (var property in content.Properties)
            {
                var value = property.GetValue(culture: culture);
                if (value is string stringValue && !string.IsNullOrWhiteSpace(stringValue) && stringValue.Length > 10)
                {
                    var plainText = HtmlTagRegex.Replace(stringValue, " ");
                    plainText = WhitespaceRegex.Replace(plainText, " ").Trim();
                    if (!string.IsNullOrWhiteSpace(plainText))
                        textParts.Add(plainText);
                }
            }

            var combined = string.Join(" ", textParts);
            return combined.Length > 500 ? combined[..500] + "..." : combined;
        }

        private MetaFieldsAIGenerateResponseModel ParseResponse(string responseText)
        {
            var result = new MetaFieldsAIGenerateResponseModel();

            var json = responseText.Trim();
            if (json.StartsWith("```"))
            {
                var firstNewline = json.IndexOf('\n');
                var lastFence = json.LastIndexOf("```");
                if (firstNewline >= 0 && lastFence > firstNewline)
                    json = json[(firstNewline + 1)..lastFence].Trim();
            }

            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                foreach (var alias in GeneratableFieldAliases)
                {
                    if (root.TryGetProperty(alias, out var element))
                    {
                        var value = element.GetString();
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            result.Suggestions.Add(new MetaFieldsAIFieldSuggestion
                            {
                                Alias = alias,
                                Value = value,
                            });
                        }
                    }
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse AI response as JSON. Response: {Response}", responseText);
            }

            return result;
        }
    }
}
