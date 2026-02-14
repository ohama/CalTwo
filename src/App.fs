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
        prop.style [ style.width (length.percent 90); style.maxWidth 400; style.margin.auto; style.fontFamily "monospace" ]
        prop.children [
            Html.h2 [
                prop.style [ style.textAlign.center ]
                prop.text "CalTwo Calculator"
            ]
            // Keyboard-enabled container
            Html.div [
                prop.tabIndex 0
                prop.onKeyDown (fun e ->
                    match e.key with
                    | "0" -> dispatch (DigitPressed 0); e.preventDefault()
                    | "1" -> dispatch (DigitPressed 1); e.preventDefault()
                    | "2" -> dispatch (DigitPressed 2); e.preventDefault()
                    | "3" -> dispatch (DigitPressed 3); e.preventDefault()
                    | "4" -> dispatch (DigitPressed 4); e.preventDefault()
                    | "5" -> dispatch (DigitPressed 5); e.preventDefault()
                    | "6" -> dispatch (DigitPressed 6); e.preventDefault()
                    | "7" -> dispatch (DigitPressed 7); e.preventDefault()
                    | "8" -> dispatch (DigitPressed 8); e.preventDefault()
                    | "9" -> dispatch (DigitPressed 9); e.preventDefault()
                    | "." -> dispatch DecimalPressed; e.preventDefault()
                    | "+" -> dispatch (OperatorPressed Add); e.preventDefault()
                    | "-" -> dispatch (OperatorPressed Subtract); e.preventDefault()
                    | "*" -> dispatch (OperatorPressed Multiply); e.preventDefault()
                    | "/" -> dispatch (OperatorPressed Divide); e.preventDefault()
                    | "Enter" -> dispatch EqualsPressed; e.preventDefault()
                    | "Escape" -> dispatch ClearPressed; e.preventDefault()
                    | "Backspace" -> dispatch BackspacePressed; e.preventDefault()
                    | _ -> ()  // Don't prevent default for unhandled keys
                )
                prop.children [
                    // Display
                    Html.div [
                        prop.testId "display"
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
                    // Button grid (4 columns)
                    Html.div [
                        prop.style [
                            style.display.grid
                            style.custom ("gridTemplateColumns", "repeat(4, 1fr)")
                            style.gap 8
                        ]
                        prop.children [
                            // Row 1: C, BS, (empty), ÷
                            Html.button [
                                prop.className "calc-button calc-clear"
                                prop.text "C"
                                prop.onClick (fun _ -> dispatch ClearPressed)
                            ]
                            Html.button [
                                prop.className "calc-button calc-backspace"
                                prop.text "←"
                                prop.onClick (fun _ -> dispatch BackspacePressed)
                            ]
                            Html.div []  // Empty cell
                            Html.button [
                                prop.className "calc-button calc-operator"
                                prop.text "÷"
                                prop.onClick (fun _ -> dispatch (OperatorPressed Divide))
                            ]
                            // Row 2: 7, 8, 9, ×
                            Html.button [
                                prop.className "calc-button"
                                prop.text "7"
                                prop.onClick (fun _ -> dispatch (DigitPressed 7))
                            ]
                            Html.button [
                                prop.className "calc-button"
                                prop.text "8"
                                prop.onClick (fun _ -> dispatch (DigitPressed 8))
                            ]
                            Html.button [
                                prop.className "calc-button"
                                prop.text "9"
                                prop.onClick (fun _ -> dispatch (DigitPressed 9))
                            ]
                            Html.button [
                                prop.className "calc-button calc-operator"
                                prop.text "×"
                                prop.onClick (fun _ -> dispatch (OperatorPressed Multiply))
                            ]
                            // Row 3: 4, 5, 6, -
                            Html.button [
                                prop.className "calc-button"
                                prop.text "4"
                                prop.onClick (fun _ -> dispatch (DigitPressed 4))
                            ]
                            Html.button [
                                prop.className "calc-button"
                                prop.text "5"
                                prop.onClick (fun _ -> dispatch (DigitPressed 5))
                            ]
                            Html.button [
                                prop.className "calc-button"
                                prop.text "6"
                                prop.onClick (fun _ -> dispatch (DigitPressed 6))
                            ]
                            Html.button [
                                prop.className "calc-button calc-operator"
                                prop.text "-"
                                prop.onClick (fun _ -> dispatch (OperatorPressed Subtract))
                            ]
                            // Row 4: 1, 2, 3, +
                            Html.button [
                                prop.className "calc-button"
                                prop.text "1"
                                prop.onClick (fun _ -> dispatch (DigitPressed 1))
                            ]
                            Html.button [
                                prop.className "calc-button"
                                prop.text "2"
                                prop.onClick (fun _ -> dispatch (DigitPressed 2))
                            ]
                            Html.button [
                                prop.className "calc-button"
                                prop.text "3"
                                prop.onClick (fun _ -> dispatch (DigitPressed 3))
                            ]
                            Html.button [
                                prop.className "calc-button calc-operator"
                                prop.text "+"
                                prop.onClick (fun _ -> dispatch (OperatorPressed Add))
                            ]
                            // Row 5: 0 (span 2), ., =
                            Html.button [
                                prop.className "calc-button"
                                prop.style [ style.custom ("gridColumn", "1 / 3") ]
                                prop.text "0"
                                prop.onClick (fun _ -> dispatch (DigitPressed 0))
                            ]
                            Html.button [
                                prop.className "calc-button"
                                prop.text "."
                                prop.onClick (fun _ -> dispatch DecimalPressed)
                            ]
                            Html.button [
                                prop.className "calc-button calc-equals"
                                prop.text "="
                                prop.onClick (fun _ -> dispatch EqualsPressed)
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]
