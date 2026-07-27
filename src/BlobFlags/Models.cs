namespace BlobFlags;

/// <summary>The root checkpoint file describing an environment and its flag groups.</summary>
public sealed record RootCheckpoint
{
    public string Name { get; init; } = string.Empty;
    public string Colour { get; init; } = "#000000";
    public TimeSpan RefreshInterval { get; init; } = TimeSpan.FromMinutes(1);
    public IReadOnlyList<GroupDefinition> Groups { get; init; } = [];
}

/// <summary>A flag group entry within the root checkpoint.</summary>
public sealed record GroupDefinition
{
    public string Name { get; init; } = string.Empty;
    public TimeSpan? RefreshInterval { get; init; }
}

/// <summary>The per-group data file holding the actual flag values.</summary>
public sealed record GroupData
{
    public IReadOnlyList<Flag> Features { get; init; } = [];
}

public sealed record Flag
{
    public string FlagName { get; init; } = string.Empty;
    public bool Value { get; init; }
}
