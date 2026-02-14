module App

open Feliz
open Elmish

type Model = { Message: string }

type Msg = NoOp

let init () = { Message = "Hello CalTwo" }, Cmd.none

let update msg model =
    match msg with
    | NoOp -> model, Cmd.none

let view model dispatch =
    Html.div [
        prop.style [ style.padding 20; style.fontFamily "sans-serif" ]
        prop.children [
            Html.h1 model.Message
            Html.p "F# + Elmish + Vite + HMR is working!"
        ]
    ]
