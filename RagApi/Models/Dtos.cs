namespace RagApi.Models;

public record IndexRequest(string Text, string? DocumentId = null);

public record IndexResponse(string DocumentId, int ChunkCount);

public record QueryRequest(string Question, int? TopK = null);

public record SourceChunk(string DocumentId, int Ordinal, string Text, double Similarity);

public record QueryResponse(string Question, string Answer, IReadOnlyList<SourceChunk> Sources);
