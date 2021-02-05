module Server.Tests

open Expecto

open Shared
open Server

let server =
    testList
        "Server"
        [ testCase "1 should equal 1"
          <| fun _ -> Expect.equal 1 1 "1 should equal 1" ]

let all =
    testList "All" [ Shared.Tests.shared; server ]

[<EntryPoint>]
let main _ = runTests defaultConfig all
