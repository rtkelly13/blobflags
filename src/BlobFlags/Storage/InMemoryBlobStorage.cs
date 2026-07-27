using System.Collections.Concurrent;

namespace BlobFlags.Storage;

/// <summary>Dictionary-backed store for tests and local development.</summary>
public sealed class InMemoryBlobStorage : IBlobStorage
{
    private readonly ConcurrentDictionary<string, string> _blobs = new(StringComparer.Ordinal);

    public Task<string?> GetAsync(string key, CancellationToken cancellationToken = default)
        => Task.FromResult(_blobs.TryGetValue(key, out var value) ? value : null);

    public Task PutAsync(string key, string content, CancellationToken cancellationToken = default)
    {
        _blobs[key] = content;
        return Task.CompletedTask;
    }
}
