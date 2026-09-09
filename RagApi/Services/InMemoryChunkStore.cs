namespace RagApi.Services;

// Everything is lost on restart. Postgres comes in part 2.
// Registered as a singleton, so every request shares the list - hence the lock.
public class InMemoryChunkStore : IChunkStore
{
    private readonly List<StoredChunk> _chunks = [];
    private readonly Lock _gate = new();

    public void AddRange(IEnumerable<StoredChunk> chunks)
    {
        lock (_gate)
        {
            _chunks.AddRange(chunks);
        }
    }

    public IReadOnlyList<StoredChunk> All()
    {
        lock (_gate)
        {
            return _chunks.ToArray();
        }
    }

    public int Count
    {
        get
        {
            lock (_gate)
            {
                return _chunks.Count;
            }
        }
    }
}
