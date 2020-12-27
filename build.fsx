#r "paket: groupref build //"
#load "./.fake/build.fsx/intellisense.fsx"
#r "netstandard"

open Fake.Core
open Fake.DotNet
open Fake.IO

Target.initEnvironment ()



let uiPath = Path.getFullName "./ui"
let sharedPath = Path.getFullName "./ui/src/Shared"
let serverPath = Path.getFullName "./ui/src/Server"
let deployDir = Path.getFullName "./ui/deploy"
let sharedTestsPath = Path.getFullName "./ui/tests/Shared"
let serverTestsPath = Path.getFullName "./ui/tests/Server"

let npm args workingDir =
    let npmPath =
        match ProcessUtils.tryFindFileOnPath "npm" with
        | Some path -> path
        | None ->
            "npm was not found in path. Please install it and make sure it's available from your path. " +
            "See https://safe-stack.github.io/docs/quickstart/#install-pre-requisites for more info"
            |> failwith

    let arguments = args |> String.split ' ' |> Arguments.OfArgs

    Command.RawCommand (npmPath, arguments)
    |> CreateProcess.fromCommand
    |> CreateProcess.withWorkingDirectory workingDir
    |> CreateProcess.ensureExitCode
    |> Proc.run
    |> ignore

let dotnet cmd workingDir =
    let result = DotNet.exec (DotNet.Options.withWorkingDirectory workingDir) cmd ""
    if result.ExitCode <> 0 then failwithf "'dotnet %s' failed in %s" cmd workingDir

Target.create "Clean" (fun _ -> Shell.cleanDir deployDir)

Target.create "InstallClient" (fun _ -> npm "install" uiPath)

Target.create "Bundle" (fun _ ->
    dotnet (sprintf "publish -c Release -o \"%s\"" deployDir) serverPath
    npm "run build" uiPath
)

Target.create "Run" (fun _ ->
    dotnet "build" sharedPath
    [ async { dotnet "watch run" serverPath }
      async { npm "run start" uiPath } ]
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore
)

Target.create "RunClient" (fun _ ->
    async { npm "run start" uiPath }
    |> Async.RunSynchronously
    |> ignore
)

Target.create "RunTests" (fun _ ->
    dotnet "build" sharedTestsPath
    [ async { dotnet "watch run" serverTestsPath }
      async { npm "run test:live" "." } ]
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore
)

open Fake.Core.TargetOperators

"Clean"
    ==> "InstallClient"
    ==> "Bundle"

"InstallClient"
    ==> "Run"

"InstallClient"
    ==> "RunClient"

"InstallClient"
    ==> "RunTests"

Target.runOrDefaultWithArguments "Bundle"
