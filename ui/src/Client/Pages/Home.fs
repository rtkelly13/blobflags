module Home

open Elmish
open Shared
open Thoth.Fetch
open Fable.Core

type Model =
    { Todos: Todo list
      Input: string }

type TodoList = Todo list
type Msg =
    | GotTodos of Todo list
    | SetInput of string
    | AddTodo
    | AddedTodo of Todo

let getTodos() =
    async {
        return! Fetch.get<_, TodoList>("/api/todos") |> Async.AwaitPromise
    }

let postTodo todo =
    async{
        let! response =  Fetch.put<Todo, Result<unit,string>>("/api/todos", todo) |> Async.AwaitPromise
        return
            match response with
            | Ok _ -> todo
            | Error s -> failwith s
    }

let init(): Model * Cmd<Msg> =
    let model =
        { Todos = []
          Input = "" }
    let cmd = Cmd.OfAsync.perform getTodos () GotTodos
    model, cmd

let update (msg: Msg) (model: Model): Model * Cmd<Msg> =
    match msg with
    | GotTodos todos ->
        { model with Todos = todos }, Cmd.none
    | SetInput value ->
        { model with Input = value }, Cmd.none
    | AddTodo ->
        let todo = Todo.create model.Input
        let cmd = Cmd.OfAsync.perform postTodo todo AddedTodo
        { model with Input = "" }, cmd
    | AddedTodo todo ->
        { model with Todos = model.Todos @ [ todo ] }, Cmd.none

open Fable.React
open Fable.React.Props
open Fulma


let containerBox (model : Model) (dispatch : Msg -> unit) =
    Box.box' [ ] [
        Content.content [ ] [
            Content.Ol.ol [ ] [
                for todo in model.Todos do
                    li [ ] [ str todo.Description ]
            ]
        ]
        Field.div [ Field.IsGrouped ] [
            Control.p [ Control.IsExpanded ] [
                Input.text [
                  Input.Value model.Input
                  Input.Placeholder "What needs to be done?"
                  Input.OnChange (fun x -> SetInput x.Value |> dispatch) ]
            ]
            Control.p [ ] [
                Button.a [
                    Button.Color IsPrimary
                    Button.Disabled (Todo.isValid model.Input |> not)
                    Button.OnClick (fun _ -> dispatch AddTodo)
                ] [
                    str "Add"
                ]
            ]
        ]
    ]

let view (model : Model) (dispatch : Msg -> unit) =
    Hero.hero [
        Hero.Color IsPrimary
        Hero.IsFullHeight
        Hero.Props [
            Style [
                Background """linear-gradient(rgba(0, 0, 0, 0.5), rgba(0, 0, 0, 0.5)), url("https://unsplash.it/1200/900?random") no-repeat center center fixed"""
                BackgroundSize "cover"
            ]
        ]
    ] [
        Hero.body [ ] [
            Container.container [ ] [
                Column.column [
                    Column.Width (Screen.All, Column.Is6)
                    Column.Offset (Screen.All, Column.Is3)
                ] [
                    Heading.p [ Heading.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ] [ str "client" ]
                    containerBox model dispatch
                ]
            ]
        ]
    ]