module App

open Elmish
open Elmish.React
open Elmish.Navigation
open Elmish.UrlParser
open Page

#if DEBUG
open Elmish.Debug
open Elmish.HMR

#endif

Program.mkProgram Index.init Index.update Index.view
|> Program.toNavigable (parsePath Page.route) Index.updateUrl
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactSynchronous "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
