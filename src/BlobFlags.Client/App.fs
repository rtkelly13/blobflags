module BlobFlags.Client.App

open Browser.Dom
open Fable.Core.JsInterop
open Feliz
open BlobFlags.Client.Api

importSideEffects "./index.css"

[<ReactComponent>]
let FlagRow (flag: Flag) (onToggle: bool -> unit) =
    Html.div [
        prop.className "flex items-center justify-between px-4 py-3"
        prop.children [
            Html.span [
                prop.className "font-mono text-sm text-slate-700 dark:text-slate-200"
                prop.text flag.FlagName
            ]
            Html.button [
                prop.className (
                    "relative inline-flex h-6 w-11 items-center rounded-full transition-colors "
                    + if flag.Value then "bg-emerald-500" else "bg-slate-300 dark:bg-slate-600"
                )
                prop.ariaLabel $"Toggle {flag.FlagName}"
                prop.onClick (fun _ -> onToggle (not flag.Value))
                prop.children [
                    Html.span [
                        prop.className (
                            "inline-block h-4 w-4 transform rounded-full bg-white transition-transform "
                            + if flag.Value then "translate-x-6" else "translate-x-1"
                        )
                    ]
                ]
            ]
        ]
    ]

[<ReactComponent>]
let GroupCard (group: GroupDefinition) =
    let flags, setFlags = React.useState List.empty
    let error, setError = React.useState Option.None

    let load () =
        async {
            let! result = fetchGroup group.Name

            match result with
            | Ok fs -> setFlags fs
            | Error e -> setError (Some e)
        }

    React.useEffect ((fun () -> Async.StartImmediate(load ())), [| box group.Name |])

    let toggle flagName value =
        async {
            let! result = setFlag group.Name flagName value

            match result with
            | Ok fs -> setFlags fs
            | Error e -> setError (Some e)
        }
        |> Async.StartImmediate

    Html.section [
        prop.className "rounded-xl border border-slate-200 bg-white shadow-sm dark:border-slate-700 dark:bg-slate-800"
        prop.children [
            Html.header [
                prop.className "flex items-center justify-between border-b border-slate-200 px-4 py-3 dark:border-slate-700"
                prop.children [
                    Html.h2 [
                        prop.className "font-semibold text-slate-900 dark:text-white"
                        prop.text group.Name
                    ]
                    Html.span [
                        prop.className "text-xs text-slate-400"
                        prop.text (group.RefreshInterval |> Option.map (sprintf "refresh %s") |> Option.defaultValue "")
                    ]
                ]
            ]
            match error with
            | Some e ->
                Html.p [
                    prop.className "px-4 py-3 text-sm text-red-500"
                    prop.text e
                ]
            | None ->
                Html.div [
                    prop.className "divide-y divide-slate-100 dark:divide-slate-700"
                    prop.children [
                        if List.isEmpty flags then
                            Html.p [
                                prop.className "px-4 py-3 text-sm text-slate-400"
                                prop.text "No flags in this group yet."
                            ]
                        for flag in flags do
                            FlagRow flag (toggle flag.FlagName)
                    ]
                ]
        ]
    ]

[<ReactComponent>]
let App () =
    let root, setRoot = React.useState Option.None
    let error, setError = React.useState Option.None

    React.useEffect (
        (fun () ->
            async {
                let! result = fetchRoot ()

                match result with
                | Ok r -> setRoot (Some r)
                | Error e -> setError (Some e)
            }
            |> Async.StartImmediate),
        [||]
    )

    Html.div [
        prop.className "min-h-screen bg-slate-50 dark:bg-slate-900"
        prop.children [
            Html.header [
                prop.className "border-b border-slate-200 bg-white px-6 py-4 dark:border-slate-700 dark:bg-slate-800"
                prop.children [
                    Html.div [
                        prop.className "mx-auto flex max-w-3xl items-center gap-3"
                        prop.children [
                            Html.span [
                                prop.className "inline-block h-3 w-3 rounded-full"
                                prop.style [
                                    style.backgroundColor (root |> Option.map _.Colour |> Option.defaultValue "#94a3b8")
                                ]
                            ]
                            Html.h1 [
                                prop.className "text-lg font-bold text-slate-900 dark:text-white"
                                prop.text "blobflags"
                            ]
                            Html.span [
                                prop.className "text-sm text-slate-400"
                                prop.text (root |> Option.map _.Name |> Option.defaultValue "loading…")
                            ]
                        ]
                    ]
                ]
            ]
            Html.main [
                prop.className "mx-auto max-w-3xl space-y-4 px-6 py-8"
                prop.children [
                    match error with
                    | Some e ->
                        Html.p [
                            prop.className "text-sm text-red-500"
                            prop.text e
                        ]
                    | None -> ()
                    match root with
                    | Some r when List.isEmpty r.Groups ->
                        Html.p [
                            prop.className "text-sm text-slate-400"
                            prop.text "No flag groups configured. PUT /api/root to create some."
                        ]
                    | Some r ->
                        for group in r.Groups do
                            GroupCard group
                    | None -> ()
                ]
            ]
        ]
    ]

let reactRoot = ReactDOM.createRoot (document.getElementById "root")
reactRoot.render (App())
