namespace BlobFlags.Storage;

/// <summary>
/// Filesystem-backed store. Useful for local development and as the default
/// backing for the admin UI; swap in an S3/Azure implementation in production.
/// </summary>
public sealed class LocalFileBlobStorage(string rootDirectory) : IBlobStorage
{
    private string PathFor(string key)
    {
        var relative = key.Replace('/', Path.DirectorySeparatorChar);
        var full = Path.GetFullPath(Path.Combine(rootDirectory, relative));
        var root = Path.GetFullPath(rootDirectory);
        if (!full.StartsWith(root, StringComparison.Ordinal))
            throw new ArgumentException($"Key '{key}' escapes the storage root.", nameof(key));
        return full;
    }

    public async Task<string?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        var path = PathFor(key);
        if (!File.Exists(path))
            return null;
#if NET
        return await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
#else
        using var reader = new StreamReader(path);
        return await reader.ReadToEndAsync().ConfigureAwait(false);
#endif
    }

    public async Task PutAsync(string key, string content, CancellationToken cancellationToken = default)
    {
        var path = PathFor(key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
#if NET
        await File.WriteAllTextAsync(path, content, cancellationToken).ConfigureAwait(false);
#else
        using var writer = new StreamWriter(path);
        await writer.WriteAsync(content).ConfigureAwait(false);
#endif
    }
}
