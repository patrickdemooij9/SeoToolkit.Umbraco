namespace SeoToolkit.Umbraco.AI.Core.Services
{
    /// <summary>
    /// Abstraction for any AI provider that can generate a text completion response.
    /// Implement this interface in an integration package (e.g. SeoToolkit.Umbraco.AI.Integration)
    /// to connect to an actual AI provider such as Umbraco.AI.
    /// </summary>
    public interface IAIGenerationService
    {
        Task<string> GenerateRawResponseAsync(
            string systemPrompt,
            string userPrompt,
            CancellationToken cancellationToken = default);
    }
}
