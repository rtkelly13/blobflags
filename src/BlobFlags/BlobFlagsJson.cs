using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlobFlags;

/// <summary>Serialization settings for all blobflags checkpoint and data files.</summary>
public static class BlobFlagsJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new RefreshIntervalConverter() },
    };

    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);

    public static T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, Options);
}

/// <summary>
/// Reads/writes refresh intervals in the compact form used by the design doc:
/// "500ms", "1s", "5m", "1h". Plain numbers are treated as seconds.
/// </summary>
public sealed class RefreshIntervalConverter : JsonConverter<TimeSpan>
{
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
            return TimeSpan.FromSeconds(reader.GetDouble());

        var text = reader.GetString();
        if (string.IsNullOrWhiteSpace(text))
            throw new JsonException("Refresh interval cannot be empty.");

        return Parse(text!);
    }

    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        => writer.WriteStringValue(Format(value));

    public static TimeSpan Parse(string text)
    {
        text = text.Trim();
        var (suffix, factor) = text switch
        {
            _ when text.EndsWith("ms", StringComparison.OrdinalIgnoreCase) => ("ms", TimeSpan.TicksPerMillisecond),
            _ when text.EndsWith("s", StringComparison.OrdinalIgnoreCase) => ("s", TimeSpan.TicksPerSecond),
            _ when text.EndsWith("m", StringComparison.OrdinalIgnoreCase) => ("m", TimeSpan.TicksPerMinute),
            _ when text.EndsWith("h", StringComparison.OrdinalIgnoreCase) => ("h", TimeSpan.TicksPerHour),
            _ => throw new JsonException($"Unrecognised refresh interval '{text}'. Use e.g. \"500ms\", \"30s\", \"5m\", \"1h\"."),
        };
        var number = text.Substring(0, text.Length - suffix.Length);
        if (!double.TryParse(number, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var value))
            throw new JsonException($"Unrecognised refresh interval '{text}'.");
        return TimeSpan.FromTicks((long)(value * factor));
    }

    public static string Format(TimeSpan value) => value switch
    {
        { TotalHours: >= 1 } when value.Ticks % TimeSpan.TicksPerHour == 0 => $"{(long)value.TotalHours}h",
        { TotalMinutes: >= 1 } when value.Ticks % TimeSpan.TicksPerMinute == 0 => $"{(long)value.TotalMinutes}m",
        { TotalSeconds: >= 1 } when value.Ticks % TimeSpan.TicksPerSecond == 0 => $"{(long)value.TotalSeconds}s",
        _ => $"{(long)value.TotalMilliseconds}ms",
    };
}
