module Index

open Elmish
open Fable.React
open Fable.React.Props

type Model =
    { Route: Page.Route
      Page: Page.Model
      Navigation: Navigation.Model }

type Msg =
    | PageMsg of Page.Msg
    | NavigationMsg of Navigation.Msg

let init (initialRoute: Option<Page.Route>) =
    let route =
        initialRoute |> Option.defaultValue Page.HomeRoute

    let navInit = Navigation.init ()

    let buildModel i =
        { Route = route
          Page = i
          Navigation = navInit }

    Page.init route
    |> Extensions.mapUpdate buildModel PageMsg

let update (msg: Msg) (model: Model): Model * Cmd<Msg> =
    match msg with
    | PageMsg pageMsg ->
        let mapModel pageModel = { model with Page = pageModel }

        Page.update pageMsg model.Page
        |> Extensions.mapUpdate mapModel PageMsg
    | NavigationMsg navMsg ->
        let mapModel navModel = { model with Navigation = navModel }

        Navigation.update navMsg model.Navigation
        |> Extensions.mapUpdate mapModel NavigationMsg

let updateUrl (updatedRoute: Option<Page.Route>) model =
    let route =
        updatedRoute
        |> Option.defaultWith (fun _ -> failwith "Route not updated properly")

    let buildModel i = { model with Route = route; Page = i }

    Page.init route
    |> Extensions.mapUpdate buildModel PageMsg

let view (model: Model) (dispatch: Msg -> unit) =
    div [] [
        Navigation.view model.Navigation (NavigationMsg >> dispatch)
        Page.view model.Page (PageMsg >> dispatch)
    ]
