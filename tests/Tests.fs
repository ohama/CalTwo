module Tests

open Fable.Mocha
open App

let smokeTests = testList "Smoke" [
    test "init returns a model" {
        let model, _ = init ()
        Expect.pass ()
    }
]

let allTests = testList "CalTwo" [
    smokeTests
]

[<EntryPoint>]
let main args =
    Mocha.runTests allTests
