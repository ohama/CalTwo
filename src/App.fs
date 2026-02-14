module App

open Feliz
open Elmish
open Calculator

let init () : Model * Cmd<Msg> =
    { Display = "0"; PendingOp = None; StartNew = false }, Cmd.none

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | DigitPressed digit ->
        // Handle error state - reset on digit press
        if model.Display = "Error" then
            { Display = string digit; PendingOp = None; StartNew = false }, Cmd.none
        // Handle StartNew flag - replace display instead of append
        elif model.StartNew then
            { model with Display = string digit; StartNew = false }, Cmd.none
        // Handle leading zeros - don't append 0 to "0"
        elif model.Display = "0" && digit = 0 then
            model, Cmd.none
        // Replace initial "0" with digit
        elif model.Display = "0" then
            { model with Display = string digit }, Cmd.none
        // Normal append
        else
            { model with Display = model.Display + string digit }, Cmd.none

    | DecimalPressed ->
        // Handle StartNew - start with "0."
        if model.StartNew then
            { model with Display = "0."; StartNew = false }, Cmd.none
        // Only add decimal if not already present
        elif not (model.Display.Contains(".")) then
            { model with Display = model.Display + "." }, Cmd.none
        else
            model, Cmd.none

    | OperatorPressed op ->
        let currentValue = parseDisplay model.Display
        match model.PendingOp with
        | None ->
            // No pending operation - just store this one
            { model with PendingOp = Some (op, currentValue); StartNew = true }, Cmd.none
        | Some (pendingOp, firstOperand) ->
            // If StartNew is true, user pressed operator right after another operator
            // In this case, just replace the operator (don't evaluate yet)
            if model.StartNew then
                { model with PendingOp = Some (op, firstOperand) }, Cmd.none
            else
                // User entered a second operand - evaluate pending operation first (left-to-right)
                match doMathOp pendingOp firstOperand currentValue with
                | Success result ->
                    let resultStr = formatResult result
                    { Display = resultStr; PendingOp = Some (op, result); StartNew = true }, Cmd.none
                | DivideByZeroError ->
                    { Display = "Error"; PendingOp = None; StartNew = true }, Cmd.none

    | EqualsPressed ->
        match model.PendingOp with
        | None ->
            // No pending operation - do nothing
            model, Cmd.none
        | Some (op, firstOperand) ->
            let secondOperand = parseDisplay model.Display
            match doMathOp op firstOperand secondOperand with
            | Success result ->
                let resultStr = formatResult result
                { Display = resultStr; PendingOp = None; StartNew = true }, Cmd.none
            | DivideByZeroError ->
                { Display = "Error"; PendingOp = None; StartNew = true }, Cmd.none

    | ClearPressed ->
        init () |> fst, Cmd.none

    | BackspacePressed ->
        if model.Display = "Error" || model.Display = "0" then
            model, Cmd.none
        elif model.Display.Length = 1 then
            { model with Display = "0" }, Cmd.none
        else
            { model with Display = model.Display.[0..model.Display.Length-2] }, Cmd.none

let view (model: Model) (dispatch: Msg -> unit) =
    Html.div [
        prop.style [ style.padding 20; style.fontFamily "monospace"; style.maxWidth 400 ]
        prop.children [
            Html.h2 "CalTwo Calculator"
            // Display
            Html.div [
                prop.style [
                    style.fontSize 32
                    style.padding 15
                    style.backgroundColor "#222"
                    style.color "#0f0"
                    style.textAlign.right
                    style.marginBottom 10
                    style.borderRadius 5
                    style.minHeight 50
                    style.lineHeight 50
                ]
                prop.text model.Display
            ]
            // Temporary test buttons (Phase 3 will replace with proper UI)
            Html.div [
                prop.style [ style.display.flex; style.flexWrap.wrap; style.gap 5 ]
                prop.children [
                    for d in 0..9 do
                        Html.button [
                            prop.text (string d)
                            prop.onClick (fun _ -> dispatch (DigitPressed d))
                            prop.style [ style.padding(10, 15); style.fontSize 16 ]
                        ]
                    Html.button [
                        prop.text "."
                        prop.onClick (fun _ -> dispatch DecimalPressed)
                        prop.style [ style.padding(10, 15); style.fontSize 16 ]
                    ]
                    Html.button [
                        prop.text "+"
                        prop.onClick (fun _ -> dispatch (OperatorPressed Add))
                        prop.style [ style.padding(10, 15); style.fontSize 16 ]
                    ]
                    Html.button [
                        prop.text "-"
                        prop.onClick (fun _ -> dispatch (OperatorPressed Subtract))
                        prop.style [ style.padding(10, 15); style.fontSize 16 ]
                    ]
                    Html.button [
                        prop.text "×"
                        prop.onClick (fun _ -> dispatch (OperatorPressed Multiply))
                        prop.style [ style.padding(10, 15); style.fontSize 16 ]
                    ]
                    Html.button [
                        prop.text "÷"
                        prop.onClick (fun _ -> dispatch (OperatorPressed Divide))
                        prop.style [ style.padding(10, 15); style.fontSize 16 ]
                    ]
                    Html.button [
                        prop.text "="
                        prop.onClick (fun _ -> dispatch EqualsPressed)
                        prop.style [ style.padding(10, 15); style.fontSize 16 ]
                    ]
                    Html.button [
                        prop.text "C"
                        prop.onClick (fun _ -> dispatch ClearPressed)
                        prop.style [ style.padding(10, 15); style.fontSize 16 ]
                    ]
                ]
            ]
        ]
    ]
