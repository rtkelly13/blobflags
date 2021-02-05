module Page

open Elmish
open Elmish.UrlParser

type Route =
    | HomeRoute
    | EnvironmentRoute of string

type Model =
    | HomePageModel of Home.Model
    | EnvironmentPageModel of Environment.Model

type Msg =
    | HomeMsg of Home.Msg
    | EnvironmentMsg of Environment.Msg

let route : State<(Route -> Route)> -> State<Route> list =
    oneOf [
        map HomeRoute top
        map EnvironmentRoute (s "Env" </> str)
    ]

let init (initialRoute: Route) =
    match initialRoute with
    | HomeRoute ->
        Home.init() |> Extensions.mapUpdate HomePageModel HomeMsg
    | EnvironmentRoute env ->
        Environment.init env |> Extensions.mapUpdate EnvironmentPageModel EnvironmentMsg

let update (msg: Msg) (model: Model): Model * Cmd<Msg> =
    match msg, model with
    | HomeMsg homeMsg, HomePageModel homeModel ->
        Home.update homeMsg homeModel
            |> Extensions.mapUpdate HomePageModel HomeMsg
    | EnvironmentMsg envMsg, EnvironmentPageModel envModel ->
        Environment.update envMsg envModel
            |> Extensions.mapUpdate EnvironmentPageModel EnvironmentMsg
    | _, _ ->
        model, Cmd.none

let view (model : Model) (dispatch : Msg -> unit) =
    match model with
    | HomePageModel home ->
        Home.view home (HomeMsg >> dispatch)
    | EnvironmentPageModel environment ->
        Environment.view environment (EnvironmentMsg >> dispatch)