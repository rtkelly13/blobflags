module BlobFlags.Server.Program

open System
open System.IO
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open BlobFlags
open BlobFlags.Storage

type SetFlagRequest = { Value: bool }

let jsonResult (value: 'T) : IResult =
    Results.Text(BlobFlagsJson.Serialize value, "application/json")

/// Deserialize a request body, returning None for missing/invalid JSON.
let tryReadBody<'T> (request: HttpRequest) : Task<'T option> =
    task {
        use reader = new StreamReader(request.Body)
        let! body = reader.ReadToEndAsync()

        try
            return
                match box (BlobFlagsJson.Deserialize<'T> body) with
                | null -> None
                | value -> Some(unbox<'T> value)
        with :? Text.Json.JsonException ->
            return None
    }

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder args

    let dataRoot =
        match builder.Configuration["BlobFlags:Root"] with
        | null -> "./data"
        | path -> path

    builder.Services
        .AddSingleton<IBlobStorage>(LocalFileBlobStorage dataRoot)
        .AddSingleton<BlobFlagsAdmin>(fun sp -> BlobFlagsAdmin(sp.GetRequiredService<IBlobStorage>()))
    |> ignore

    let app = builder.Build()

    app.UseDefaultFiles() |> ignore
    app.UseStaticFiles() |> ignore

    app.MapGet(
        "/api/root",
        Func<BlobFlagsAdmin, Task<IResult>>(fun admin ->
            task {
                let! root = admin.GetRootAsync()

                return
                    match root with
                    | null -> jsonResult (RootCheckpoint())
                    | root -> jsonResult root
            })
    )
    |> ignore

    app.MapPut(
        "/api/root",
        Func<BlobFlagsAdmin, HttpRequest, Task<IResult>>(fun admin request ->
            task {
                match! tryReadBody<RootCheckpoint> request with
                | None -> return Results.BadRequest "Invalid root checkpoint"
                | Some root ->
                    do! admin.SaveRootAsync root
                    return jsonResult root
            })
    )
    |> ignore

    app.MapGet(
        "/api/groups/{group}",
        Func<BlobFlagsAdmin, string, Task<IResult>>(fun admin group ->
            task {
                let! data = admin.GetGroupAsync group
                return jsonResult data
            })
    )
    |> ignore

    app.MapPut(
        "/api/groups/{group}/flags/{flag}",
        Func<BlobFlagsAdmin, string, string, HttpRequest, Task<IResult>>(fun admin group flag request ->
            task {
                match! tryReadBody<SetFlagRequest> request with
                | None -> return Results.BadRequest "Expected body like {\"value\": true}"
                | Some payload ->
                    let! data = admin.SetFlagAsync(group, flag, payload.Value)
                    return jsonResult data
            })
    )
    |> ignore

    app.MapFallbackToFile "index.html" |> ignore

    app.Run()
    0
