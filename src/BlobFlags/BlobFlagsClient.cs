namespace BlobFlags;

public sealed class BlobFlagsOptions
{
    /// <summary>Key prefix under which all blobflags files live, e.g. "features".</summary>
    public string Prefix { get; set; } = "features";

    /// <summary>When true, a missing root checkpoint or group file throws instead of returning defaults.</summary>
    public bool FailIfEmpty { get; set; }
}

/// <summary>
/// Read-side client. Fetches the root checkpoint and per-group data files from blob
/// storage and caches them for the refresh interval declared in the checkpoint.
/// </summary>
public sealed class BlobFlagsClient(IBlobStorage storage, BlobFlagsOptions? options = null, TimeProvider? timeProvider = null)
{
    private readonly BlobFlagsOptions _options = options ?? new BlobFlagsOptions();
    private readonly TimeProvider _time = timeProvider ?? TimeProvider.System;
    private readonly object _gate = new();

    private CacheEntry<RootCheckpoint>? _root;
    private readonly Dictionary<string, CacheEntry<GroupData>> _groups = new(StringComparer.OrdinalIgnoreCase);

    internal string RootKey => $"{_options.Prefix}/checkpoint.json";
    internal string GroupKey(string group) => $"{_options.Prefix}/{group}/checkpoint.json";

    /// <summary>Returns the flag value, or <paramref name="defaultValue"/> when the group or flag is unknown.</summary>
    public async Task<bool> GetFlagAsync(string group, string flag, bool defaultValue = false, CancellationToken cancellationToken = default)
    {
        var data = await GetGroupAsync(group, cancellationToken).ConfigureAwait(false);
        foreach (var feature in data.Features)
        {
            if (string.Equals(feature.FlagName, flag, StringComparison.OrdinalIgnoreCase))
                return feature.Value;
        }
        return defaultValue;
    }

    public async Task<RootCheckpoint> GetRootAsync(CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            if (_root is { } cached && cached.ExpiresAt > _time.GetUtcNow())
                return cached.Value;
        }

        var json = await storage.GetAsync(RootKey, cancellationToken).ConfigureAwait(false);
        var root = json is null ? null : BlobFlagsJson.Deserialize<RootCheckpoint>(json);
        if (root is null)
        {
            if (_options.FailIfEmpty)
                throw new InvalidOperationException($"No root checkpoint found at '{RootKey}'.");
            root = new RootCheckpoint();
        }

        lock (_gate)
        {
            _root = new CacheEntry<RootCheckpoint>(root, _time.GetUtcNow() + root.RefreshInterval);
        }
        return root;
    }

    public async Task<GroupData> GetGroupAsync(string group, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            if (_groups.TryGetValue(group, out var cached) && cached.ExpiresAt > _time.GetUtcNow())
                return cached.Value;
        }

        var root = await GetRootAsync(cancellationToken).ConfigureAwait(false);
        GroupDefinition? definition = null;
        foreach (var g in root.Groups)
        {
            if (string.Equals(g.Name, group, StringComparison.OrdinalIgnoreCase))
                definition = g;
        }

        var json = await storage.GetAsync(GroupKey(group), cancellationToken).ConfigureAwait(false);
        var data = json is null ? null : BlobFlagsJson.Deserialize<GroupData>(json);
        if (data is null)
        {
            if (_options.FailIfEmpty)
                throw new InvalidOperationException($"No data file found for group '{group}' at '{GroupKey(group)}'.");
            data = new GroupData();
        }

        var refresh = definition?.RefreshInterval ?? root.RefreshInterval;
        lock (_gate)
        {
            _groups[group] = new CacheEntry<GroupData>(data, _time.GetUtcNow() + refresh);
        }
        return data;
    }

    private readonly record struct CacheEntry<T>(T Value, DateTimeOffset ExpiresAt);
}
