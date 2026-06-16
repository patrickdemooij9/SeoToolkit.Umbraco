using Microsoft.Extensions.AI;
using SeoToolkit.Umbraco.AI.Core.Services;
using Umbraco.AI.Core.Chat;

namespace SeoToolkit.Umbraco.AI.Integration.Services
{
    /// <summary>
    /// Implements <see cref="IAIGenerationService"/> using Umbraco.AI's <see cref="IAIChatService"/>.
    /// This is the integration layer between SeoToolkit and the Umbraco.AI package.
    /// </summary>
    public class UmbracoAIGenerationService : IAIGenerationService
    {
        private readonly IAIChatService _chatService;

        public UmbracoAIGenerationService(IAIChatService chatService)
        {
            _chatService = chatService;
        }

        public async Task<string> GenerateRawResponseAsync(
            string systemPrompt,
            string userPrompt,
            CancellationToken cancellationToken = default)
        {
            var messages = new List<ChatMessage>
            {
                new(ChatRole.System, systemPrompt),
                new(ChatRole.User, userPrompt),
            };

            var response = await _chatService.GetChatResponseAsync(messages, cancellationToken: cancellationToken);
            return response.Text ?? string.Empty;
        }
    }
}
