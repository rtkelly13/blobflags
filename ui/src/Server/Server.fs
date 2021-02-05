module Server

open FSharp.Control.Tasks.V2
open Saturn

open Shared
open Giraffe.ResponseWriters
open Giraffe.ModelBinding
open SixLabors.ImageSharp.Web
open SixLabors.ImageSharp.Web.DependencyInjection

type Storage() =
    let todos = ResizeArray<_>()

    member __.GetTodos() = List.ofSeq todos

    member __.AddTodo(todo: Todo) =
        if Todo.isValid todo.Description then
            todos.Add todo
            Ok()
        else
            Error "Invalid todo"

let storage = Storage()

storage.AddTodo(Todo.create "Create new SAFE project")
|> ignore

storage.AddTodo(Todo.create "Write your app")
|> ignore

storage.AddTodo(Todo.create "Ship it !!!")
|> ignore

let webApp =
    router {
        get
            "/api/todos"
            (fun next ctx ->
                task {
                    let result = storage.GetTodos()
                    return! json result next ctx
                })

        put
            "/api/todos"
            (fun next ctx ->
                task {
                    let! todo = ctx.BindJsonAsync<Todo>()
                    let result = storage.AddTodo todo
                    return! json result next ctx
                })
    }

let app =
    application {
        url "http://0.0.0.0:8085"
        use_router webApp
        memory_cache
        use_json_serializer (Thoth.Json.Giraffe.ThothSerializer())
        use_static "public"
        use_gzip

        service_config
            (fun services ->
                services.AddImageSharp() |> ignore
                services)

        app_config
            (fun app ->
                app.UseImageSharp() |> ignore
                app)
    }

run app
