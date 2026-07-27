# blobflags

Feature flags backed by blob storage (S3, Azure Blob, or any key/value object store).
See [design.md](design.md) for the checkpoint/data-file layout.

## Layout

| Project | Description |
| --- | --- |
| `src/BlobFlags` | C# SDK (`net10.0`/`netstandard2.0`): `BlobFlagsClient` (read + cache), `BlobFlagsAdmin` (write + checkpoint history), `IBlobStorage` abstraction with in-memory and local-file implementations. |
| `src/BlobFlags.Server` | F# ASP.NET Core minimal API exposing the flags over HTTP and serving the admin UI. |
| `src/BlobFlags.Client` | F# Fable 5 + Feliz + React 19 admin UI, built with Vite and Tailwind CSS 4. |
| `tests/BlobFlags.Tests` | xunit tests for the SDK. |

## Prerequisites

- .NET SDK 10.0 (see `global.json`)
- Node.js 22+

## Develop

```sh
dotnet tool restore

# API server on http://localhost:8085
dotnet run --project src/BlobFlags.Server

# Client dev server on http://localhost:8080 (proxies /api to 8085)
cd src/BlobFlags.Client
npm install
npm start
```

## Build & test

```sh
dotnet build blobflags.slnx
dotnet test tests/BlobFlags.Tests

# Production client build (outputs to src/BlobFlags.Server/wwwroot)
cd src/BlobFlags.Client && npm run build
```

## SDK usage

```csharp
using BlobFlags;
using BlobFlags.Storage;

IBlobStorage storage = new LocalFileBlobStorage("./data"); // or an S3/Azure adapter
var client = new BlobFlagsClient(storage, new BlobFlagsOptions { Prefix = "features", FailIfEmpty = true });

bool enabled = await client.GetFlagAsync("ServerlessFlags", "Feature-F");
```

Flag data is cached per group according to the `RefreshInterval` declared in the
root checkpoint (compact durations: `500ms`, `30s`, `5m`, `1h`).

## HTTP API

| Method | Route | Description |
| --- | --- | --- |
| GET | `/api/root` | Root checkpoint (environment + groups) |
| PUT | `/api/root` | Replace root checkpoint (also writes a timestamped history copy) |
| GET | `/api/groups/{group}` | Flag values for a group |
| PUT | `/api/groups/{group}/flags/{flag}` | Set a flag: `{"value": true}` |

The server stores files under `./data` by default; configure with `BlobFlags:Root`.
