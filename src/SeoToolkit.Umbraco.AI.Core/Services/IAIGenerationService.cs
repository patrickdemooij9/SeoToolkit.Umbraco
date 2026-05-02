namespace SeoToolkit.Umbraco.AI.Core.Services
{
    public interface IAIGenerationService
    {
        Task<string> GenerateRawResponseAsync(
            string systemPrompt,
            string userPrompt,
            CancellationToken cancellationToken = default);
    }
}
