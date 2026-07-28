namespace BlobFlags;

/// <summary>
/// Minimal abstraction over a blob/object store (S3, Azure Blob, local files, ...).
/// Keys are '/'-separated paths relative to the store root.
/// </summary>
public interface IBlobStorage
{
    /// <summary>Returns the blob content as UTF-8 text, or null when the key does not exist.</summary>
    Task<string?> GetAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the blob at <paramref name="key"/> with UTF-8 text content.</summary>
    Task PutAsync(string key, string content, CancellationToken cancellationToken = default);
}
