using BlobFlags.Storage;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace BlobFlags.Tests;

public class BlobFlagsClientTests
{
    private static async Task<InMemoryBlobStorage> SeedAsync()
    {
        var storage = new InMemoryBlobStorage();
        await storage.PutAsync("features/checkpoint.json", """
            {
              "Name": "Debug",
              "Colour": "#336699",
              "RefreshInterval": "1s",
              "Groups": [ { "Name": "ServerlessFlags", "RefreshInterval": "1m" } ]
            }
            """);
        await storage.PutAsync("features/ServerlessFlags/checkpoint.json", """
            {
              "Features": [
                { "FlagName": "Feature-F", "Value": false },
                { "FlagName": "my-product-1234", "Value": true }
              ]
            }
            """);
        return storage;
    }

    [Fact]
    public async Task Reads_flags_from_design_doc_layout()
    {
        var client = new BlobFlagsClient(await SeedAsync());

        Assert.True(await client.GetFlagAsync("ServerlessFlags", "my-product-1234"));
        Assert.False(await client.GetFlagAsync("ServerlessFlags", "Feature-F"));
    }

    [Fact]
    public async Task Unknown_flag_returns_default()
    {
        var client = new BlobFlagsClient(await SeedAsync());

        Assert.False(await client.GetFlagAsync("ServerlessFlags", "nope"));
        Assert.True(await client.GetFlagAsync("ServerlessFlags", "nope", defaultValue: true));
    }

    [Fact]
    public async Task Missing_files_throw_when_FailIfEmpty()
    {
        var client = new BlobFlagsClient(new InMemoryBlobStorage(), new BlobFlagsOptions { FailIfEmpty = true });

        await Assert.ThrowsAsync<InvalidOperationException>(() => client.GetFlagAsync("g", "f"));
    }

    [Fact]
    public async Task Group_data_is_cached_until_refresh_interval_elapses()
    {
        var storage = await SeedAsync();
        var time = new FakeTimeProvider();
        var client = new BlobFlagsClient(storage, timeProvider: time);

        Assert.False(await client.GetFlagAsync("ServerlessFlags", "Feature-F"));

        var admin = new BlobFlagsAdmin(storage, timeProvider: time);
        await admin.SetFlagAsync("ServerlessFlags", "Feature-F", true);

        // Still cached: group refresh interval is 1m.
        Assert.False(await client.GetFlagAsync("ServerlessFlags", "Feature-F"));

        time.Advance(TimeSpan.FromMinutes(2));
        Assert.True(await client.GetFlagAsync("ServerlessFlags", "Feature-F"));
    }

    [Fact]
    public async Task Admin_writes_history_checkpoint_on_root_save()
    {
        var storage = new InMemoryBlobStorage();
        var time = new FakeTimeProvider(DateTimeOffset.Parse("2026-01-08T09:00:00Z"));
        var admin = new BlobFlagsAdmin(storage, timeProvider: time);

        await admin.SaveRootAsync(new RootCheckpoint { Name = "Debug" });

        Assert.NotNull(await storage.GetAsync("features/checkpoint.json"));
        Assert.NotNull(await storage.GetAsync("features/checkpoints/2026-01-08T09.00.00.0000000+00.00.json"));
    }
}
