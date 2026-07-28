namespace BlobFlags;

/// <summary>
/// Write-side operations: updating flags and maintaining the timestamped checkpoint
/// history described in design.md.
/// </summary>
public sealed class BlobFlagsAdmin(IBlobStorage storage, BlobFlagsOptions? options = null, TimeProvider? timeProvider = null)
{
    private readonly BlobFlagsOptions _options = options ?? new BlobFlagsOptions();
    private readonly TimeProvider _time = timeProvider ?? TimeProvider.System;

    private string RootKey => $"{_options.Prefix}/checkpoint.json";
    private string HistoryKey(DateTimeOffset at)
        => $"{_options.Prefix}/checkpoints/{at.ToString("yyyy-MM-ddTHH.mm.ss.fffffffK", System.Globalization.CultureInfo.InvariantCulture).Replace(':', '.')}.json";
    private string GroupKey(string group) => $"{_options.Prefix}/{group}/checkpoint.json";

    public async Task<RootCheckpoint?> GetRootAsync(CancellationToken cancellationToken = default)
    {
        var json = await storage.GetAsync(RootKey, cancellationToken).ConfigureAwait(false);
        return json is null ? null : BlobFlagsJson.Deserialize<RootCheckpoint>(json);
    }

    /// <summary>Saves the root checkpoint and appends a timestamped copy to the history folder.</summary>
    public async Task SaveRootAsync(RootCheckpoint root, CancellationToken cancellationToken = default)
    {
        var json = BlobFlagsJson.Serialize(root);
        await storage.PutAsync(RootKey, json, cancellationToken).ConfigureAwait(false);
        await storage.PutAsync(HistoryKey(_time.GetUtcNow()), json, cancellationToken).ConfigureAwait(false);
    }

    public async Task<GroupData> GetGroupAsync(string group, CancellationToken cancellationToken = default)
    {
        var json = await storage.GetAsync(GroupKey(group), cancellationToken).ConfigureAwait(false);
        return (json is null ? null : BlobFlagsJson.Deserialize<GroupData>(json)) ?? new GroupData();
    }

    /// <summary>Creates or updates a single flag within a group's data file.</summary>
    public async Task<GroupData> SetFlagAsync(string group, string flag, bool value, CancellationToken cancellationToken = default)
    {
        var data = await GetGroupAsync(group, cancellationToken).ConfigureAwait(false);
        var features = new List<Flag>(data.Features);
        var index = features.FindIndex(f => string.Equals(f.FlagName, flag, StringComparison.OrdinalIgnoreCase));
        var updated = new Flag { FlagName = flag, Value = value };
        if (index >= 0)
            features[index] = updated;
        else
            features.Add(updated);

        var next = data with { Features = features };
        await storage.PutAsync(GroupKey(group), BlobFlagsJson.Serialize(next), cancellationToken).ConfigureAwait(false);
        return next;
    }
}
