module Main

open Elmish
open Elmish.React
open Elmish.HMR  // MUST be last - shadows Program.run
open Fable.Core.JsInterop

importSideEffects "./styles.css"

Program.mkProgram App.init App.update App.view
|> Program.withReactSynchronous "elmish-app"
|> Program.run
