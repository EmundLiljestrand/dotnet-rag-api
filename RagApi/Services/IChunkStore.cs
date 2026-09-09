namespace RagApi.Services;

public record StoredChunk(string DocumentId, int Ordinal, string Text, float[] Embedding);

public interface IChunkStore
{
    void AddRange(IEnumerable<StoredChunk> chunks);

    IReadOnlyList<StoredChunk> All();

    int Count { get; }
}
