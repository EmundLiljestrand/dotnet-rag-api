namespace RagApi.Services;

// Stand-in for running without an API key. Returns the retrieved context
// instead of a generated answer, so the search can still be judged.
public class EchoAnswerGenerator : IAnswerGenerator
{
    public Task<string> GenerateAsync(
        string question,
        IReadOnlyList<string> context,
        CancellationToken cancellationToken = default)
    {
        if (context.Count == 0)
        {
            return Task.FromResult(
                "[echo] No context found for the question. Index some text via POST /index first.");
        }

        var answer = $"[echo] No language model is configured. "
                   + $"The search found {context.Count} relevant passages:"
                   + Environment.NewLine
                   + string.Join(
                        Environment.NewLine,
                        context.Select((text, i) => $"  [{i + 1}] {Preview(text)}"));

        return Task.FromResult(answer);
    }

    private static string Preview(string text) =>
        text.Length <= 160 ? text : string.Concat(text.AsSpan(0, 160), "...");
}
