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
