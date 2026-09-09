using System.Text;

namespace RagApi.Services;

public static class PromptBuilder
{
    public const string SystemPrompt =
        """
        You answer questions using only the context you are given.

        Rules:
        - Use only information from the context. Do not add your own knowledge.
        - If the context is not enough, say so plainly. Never guess.
        - Answer concisely, in the same language as the question.
        """;

    public static string BuildUserPrompt(string question, IReadOnlyList<string> context)
    {
        var builder = new StringBuilder();

        builder.AppendLine("<context>");
        if (context.Count == 0)
        {
            builder.AppendLine("(no context found)");
        }
        else
        {
            for (var i = 0; i < context.Count; i++)
            {
                builder.AppendLine($"[{i + 1}] {context[i]}");
            }
        }
        builder.AppendLine("</context>");
        builder.AppendLine();
        builder.Append("Question: ").Append(question);

        return builder.ToString();
    }
}
