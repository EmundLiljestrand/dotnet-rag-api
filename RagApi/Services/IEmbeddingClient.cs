namespace RagApi.Services;

// Providers treat stored text and search text differently. The wrong type gives worse hits.
public enum EmbeddingInputType
{
    Document,
    Query,
}

public interface IEmbeddingClient
{
    Task<IReadOnlyList<float[]>> EmbedAsync(
        IReadOnlyList<string> inputs,
        EmbeddingInputType inputType,
        CancellationToken cancellationToken = default);
}
