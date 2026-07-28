module BlobFlags.Client.Api

open Fable.SimpleHttp
open Thoth.Json

type Flag = { FlagName: string; Value: bool }

type GroupDefinition =
    { Name: string
      RefreshInterval: string option }

type RootCheckpoint =
    { Name: string
      Colour: string
      RefreshInterval: string
      Groups: GroupDefinition list }

module Decoders =
    let flag: Decoder<Flag> =
        Decode.object (fun get ->
            { FlagName = get.Required.Field "FlagName" Decode.string
              Value = get.Required.Field "Value" Decode.bool })

    let groupData: Decoder<Flag list> =
        Decode.object (fun get -> get.Optional.Field "Features" (Decode.list flag) |> Option.defaultValue [])

    let groupDefinition: Decoder<GroupDefinition> =
        Decode.object (fun get ->
            { Name = get.Required.Field "Name" Decode.string
              RefreshInterval = get.Optional.Field "RefreshInterval" Decode.string })

    let root: Decoder<RootCheckpoint> =
        Decode.object (fun get ->
            { Name = get.Optional.Field "Name" Decode.string |> Option.defaultValue ""
              Colour = get.Optional.Field "Colour" Decode.string |> Option.defaultValue "#000000"
              RefreshInterval = get.Optional.Field "RefreshInterval" Decode.string |> Option.defaultValue "1m"
              Groups = get.Optional.Field "Groups" (Decode.list groupDefinition) |> Option.defaultValue [] })

let private get (url: string) (decoder: Decoder<'T>) : Async<Result<'T, string>> =
    async {
        let! response = Http.request url |> Http.method GET |> Http.send

        if response.statusCode = 200 then
            return Decode.fromString decoder response.responseText
        else
            return Error $"GET {url} failed with {response.statusCode}"
    }

let fetchRoot () = get "/api/root" Decoders.root

let fetchGroup (group: string) = get $"/api/groups/{group}" Decoders.groupData

let setFlag (group: string) (flag: string) (value: bool) : Async<Result<Flag list, string>> =
    async {
        let! response =
            Http.request $"/api/groups/{group}/flags/{flag}"
            |> Http.method PUT
            |> Http.content (BodyContent.Text(Encode.toString 0 (Encode.object [ "value", Encode.bool value ])))
            |> Http.header (Headers.contentType "application/json")
            |> Http.send

        if response.statusCode = 200 then
            return Decode.fromString Decoders.groupData response.responseText
        else
            return Error $"PUT flag failed with {response.statusCode}"
    }
