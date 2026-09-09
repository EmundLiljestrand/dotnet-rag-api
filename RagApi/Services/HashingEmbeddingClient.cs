using Microsoft.Extensions.Options;

namespace RagApi.Services;

// Embeddings without an API key. Hashes every word to a position in the vector,
// so texts sharing words come out similar. It matches words, not meaning -
// "car" and "vehicle" end up far apart.
public class HashingEmbeddingClient(IOptions<RagOptions> options) : IEmbeddingClient
{
    private readonly int _dimensions = options.Value.LocalEmbeddingDimensions;

    // Without this filter, short words like "and" and "the" weigh as much as
    // meaningful ones, which ranks the wrong documents first.
    private const int MinimumTokenLength = 3;

    public Task<IReadOnlyList<float[]>> EmbedAsync(
        IReadOnlyList<string> inputs,
        EmbeddingInputType inputType,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<float[]> vectors = [.. inputs.Select(Embed)];
        return Task.FromResult(vectors);
    }

    private float[] Embed(string text)
    {
        var vector = new float[_dimensions];

        foreach (var token in Tokenize(text))
        {
            var bucket = (int)(StableHash(token) % (uint)_dimensions);
            vector[bucket] += 1f;
        }

        // Normalise to length 1 so long and short texts stay comparable.
        var magnitude = Math.Sqrt(vector.Sum(v => (double)v * v));
        if (magnitude > 0)
        {
            for (var i = 0; i < vector.Length; i++)
            {
                vector[i] = (float)(vector[i] / magnitude);
            }
        }

        return vector;
    }

    private static IEnumerable<string> Tokenize(string text) =>
        text.Split(
                (char[]?)null,
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(word => new string([.. word.Where(char.IsLetterOrDigit)]).ToLowerInvariant())
            .Where(word => word.Length > MinimumTokenLength);

    // Own hash instead of GetHashCode, which is randomised per process in .NET.
    // The same text has to produce the same vector after a restart.
    private static uint StableHash(string value)
    {
        unchecked
        {
            var hash = 2166136261u;
            foreach (var c in value)
            {
                hash ^= c;
                hash *= 16777619u;
            }
            return hash;
        }
    }
}
