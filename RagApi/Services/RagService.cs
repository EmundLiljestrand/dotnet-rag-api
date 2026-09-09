using Microsoft.Extensions.Options;
using RagApi.Models;

namespace RagApi.Services;

public class RagService(
    TextChunker chunker,
    IEmbeddingClient embeddings,
    IAnswerGenerator generator,
    IChunkStore store,
    IOptions<RagOptions> options)
{
    private readonly RagOptions _options = options.Value;

    public async Task<IndexResponse> IndexAsync(
        IndexRequest request,
        CancellationToken cancellationToken = default)
    {
        var documentId = string.IsNullOrWhiteSpace(request.DocumentId)
            ? Guid.NewGuid().ToString("n")
            : request.DocumentId.Trim();

        var chunks = chunker.Chunk(request.Text);
        if (chunks.Count == 0)
        {
            return new IndexResponse(documentId, 0);
        }

        var vectors = await embeddings.EmbedAsync(chunks, EmbeddingInputType.Document, cancellationToken);
        if (vectors.Count != chunks.Count)
        {
            throw new InvalidOperationException(
                $"Got {vectors.Count} vectors for {chunks.Count} chunks.");
        }

        store.AddRange(chunks.Select((text, ordinal) =>
            new StoredChunk(documentId, ordinal, text, vectors[ordinal])));

        return new IndexResponse(documentId, chunks.Count);
    }

    public async Task<QueryResponse> QueryAsync(
        QueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var topK = request.TopK is > 0 ? request.TopK.Value : _options.TopK;

        var queryVectors = await embeddings.EmbedAsync(
            [request.Question], EmbeddingInputType.Query, cancellationToken);
        var queryVector = queryVectors[0];

        // In part 2 this ranking moves down into Postgres.
        var hits = store.All()
            .Select(chunk => new
            {
                Chunk = chunk,
                Similarity = VectorMath.CosineSimilarity(chunk.Embedding, queryVector),
            })
            .OrderByDescending(hit => hit.Similarity)
            .Take(topK)
            .ToList();

        var sources = hits
            .Select(hit => new SourceChunk(
                hit.Chunk.DocumentId, hit.Chunk.Ordinal, hit.Chunk.Text, hit.Similarity))
            .ToList();

        var answer = await generator.GenerateAsync(
            request.Question, [.. hits.Select(hit => hit.Chunk.Text)], cancellationToken);

        return new QueryResponse(request.Question, answer, sources);
    }
}
