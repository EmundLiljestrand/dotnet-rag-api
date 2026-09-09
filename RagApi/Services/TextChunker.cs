namespace RagApi.Services;

// Splits on a fixed character length. Knows nothing about words or sentences.
public class TextChunker
{
    private readonly int _chunkSize;

    public TextChunker(int chunkSize)
    {
        _chunkSize = chunkSize;
    }

    public List<string> Chunk(string text)
    {
        var chunks = new List<string>();

        if (string.IsNullOrWhiteSpace(text))
        {
            return chunks;
        }

        for (var start = 0; start < text.Length; start += _chunkSize)
        {
            var length = Math.Min(_chunkSize, text.Length - start);
            chunks.Add(text.Substring(start, length));
        }

        return chunks;
    }
}
