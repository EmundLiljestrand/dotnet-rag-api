namespace RagApi.Services;

public class RagOptions
{
    public const string SectionName = "Rag";

    public int ChunkSize { get; set; } = 500;

    public int TopK { get; set; } = 3;

    public string EmbeddingProvider { get; set; } = "voyage";  // "voyage" or "local"

    public string EmbeddingModel { get; set; } = "voyage-3.5";

    public int LocalEmbeddingDimensions { get; set; } = 256;

    public string AnswerProvider { get; set; } = "claude";  // "claude" or "echo"

    public string ChatModel { get; set; } = "claude-opus-5";

    public int MaxAnswerTokens { get; set; } = 16000;
}
