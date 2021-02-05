module Shared.Tests

#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif

open Shared

let shared =
    testList
        "Shared"
        [ testCase "2 should equal 2"
          <| fun _ -> Expect.equal 2 2 "2 should equal 2" ]
