module App

open Feliz
open Elmish
open Calculator

let init () : Model * Cmd<Msg> =
    { Display = "0"; PendingOp = None }, Cmd.none

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    // Placeholder — will be implemented via TDD in Plan 02-02
    model, Cmd.none

let view (model: Model) (dispatch: Msg -> unit) =
    Html.div [
        prop.style [ style.padding 20; style.fontFamily "sans-serif" ]
        prop.children [
            Html.h1 "CalTwo Calculator"
            Html.div [
                prop.style [
                    style.fontSize 24
                    style.padding 10
                    style.backgroundColor "#f0f0f0"
                    style.textAlign.right
                    style.minHeight 40
                ]
                prop.text model.Display
            ]
            Html.p "Phase 2: Calculator logic in progress..."
        ]
    ]
