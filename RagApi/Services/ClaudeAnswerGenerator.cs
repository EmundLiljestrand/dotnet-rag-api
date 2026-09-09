using Anthropic;
using Anthropic.Models.Messages;
using Microsoft.Extensions.Options;

namespace RagApi.Services;

public class ClaudeAnswerGenerator(AnthropicClient client, IOptions<RagOptions> options)
    : IAnswerGenerator
{
    private readonly RagOptions _options = options.Value;

    public async Task<string> GenerateAsync(
        string question,
        IReadOnlyList<string> context,
        CancellationToken cancellationToken = default)
    {
        var response = await client.Messages.Create(new MessageCreateParams
        {
            Model = _options.ChatModel,
            MaxTokens = _options.MaxAnswerTokens,
            System = new List<TextBlockParam>
            {
                new() { Text = PromptBuilder.SystemPrompt },
            },
            Messages =
            [
                new()
                {
                    Role = Role.User,
                    Content = PromptBuilder.BuildUserPrompt(question, context),
                },
            ],
        }, cancellationToken);

        // A refusal arrives as HTTP 200 with empty content, not as an exception.
        if (response.StopReason == "refusal")
        {
            var reason = response.StopDetails?.Explanation ?? "no explanation given";
            throw new InvalidOperationException($"The model declined to answer: {reason}");
        }

        var text = string.Join(
            Environment.NewLine,
            response.Content.Select(block => block.Value).OfType<TextBlock>().Select(t => t.Text));

        return string.IsNullOrWhiteSpace(text) ? "The model returned no answer." : text;
    }
}
