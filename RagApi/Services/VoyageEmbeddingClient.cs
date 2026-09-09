using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace RagApi.Services;

// Anthropic has no embedding endpoint, so the vectors come from here.
// Requires VOYAGE_API_KEY.
public class VoyageEmbeddingClient(HttpClient httpClient, IOptions<RagOptions> options)
    : IEmbeddingClient
{
    private readonly RagOptions _options = options.Value;

    public async Task<IReadOnlyList<float[]>> EmbedAsync(
        IReadOnlyList<string> inputs,
        EmbeddingInputType inputType,
        CancellationToken cancellationToken = default)
    {
        if (inputs.Count == 0)
        {
            return [];
        }

        var request = new VoyageRequest(
            Input: [.. inputs],
            Model: _options.EmbeddingModel,
            InputType: inputType == EmbeddingInputType.Query ? "query" : "document");

        var response = await httpClient.PostAsJsonAsync("v1/embeddings", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Voyage returned {(int)response.StatusCode}: {body}");
        }

        var payload = await response.Content.ReadFromJsonAsync<VoyageResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Voyage returned an empty body.");

        // The order is not guaranteed, so sort on index before unpacking.
        return [.. payload.Data.OrderBy(d => d.Index).Select(d => d.Embedding)];
    }

    private sealed record VoyageRequest(
        [property: JsonPropertyName("input")] string[] Input,
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("input_type")] string InputType);

    private sealed record VoyageResponse(
        [property: JsonPropertyName("data")] VoyageData[] Data);

    private sealed record VoyageData(
        [property: JsonPropertyName("embedding")] float[] Embedding,
        [property: JsonPropertyName("index")] int Index);
}
