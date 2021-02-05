module Environment

open Elmish
open Shared
open Thoth.Fetch
open Fable.Core

type Model =
    {
        EnvironmentName: string
    }

type Msg =
    | Loaded of string


let init (environment: string): Model * Cmd<Msg> =
    let model =
        {
            EnvironmentName = environment
        }
    model, Cmd.none

let update (msg: Msg) (model: Model): Model * Cmd<Msg> =
    model, Cmd.none

open Fable.React
open Fable.React.Props
open Fulma


let view (model : Model) (dispatch : Msg -> unit) =
    div [] []