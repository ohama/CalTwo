module Tests

open Fable.Mocha
open App
open Calculator

// ============================================================================
// 1. DIGIT ENTRY TESTS
// ============================================================================

let digitEntryTests = testList "Digit Entry" [
    test "Pressing 1 shows '1' (replaces initial '0')" {
        let model, _ = App.init ()
        let m1 = model |> App.update (DigitPressed 1) |> fst
        Expect.equal m1.Display "1" "Digit 1 should replace initial 0"
    }

    test "Pressing 1 then 2 shows '12'" {
        let model, _ = App.init ()
        let m1 = model |> App.update (DigitPressed 1) |> fst
        let m2 = m1 |> App.update (DigitPressed 2) |> fst
        Expect.equal m2.Display "12" "Should append digits"
    }

    test "Pressing 1, 2, 3 shows '123'" {
        let model, _ = App.init ()
        let m1 = model |> App.update (DigitPressed 1) |> fst
        let m2 = m1 |> App.update (DigitPressed 2) |> fst
        let m3 = m2 |> App.update (DigitPressed 3) |> fst
        Expect.equal m3.Display "123" "Should build multi-digit number"
    }

    test "Pressing 0 when display is '0' keeps '0' (no leading zeros)" {
        let model, _ = App.init ()
        let m1 = model |> App.update (DigitPressed 0) |> fst
        Expect.equal m1.Display "0" "Should not add leading zeros"
    }

    test "Pressing 5 then 0 shows '50' (trailing zero OK)" {
        let model, _ = App.init ()
        let m1 = model |> App.update (DigitPressed 5) |> fst
        let m2 = m1 |> App.update (DigitPressed 0) |> fst
        Expect.equal m2.Display "50" "Trailing zero should work"
    }
]

// ============================================================================
// 2. DECIMAL POINT TESTS
// ============================================================================

let decimalPointTests = testList "Decimal Point" [
    test "Pressing decimal shows '0.' (appends to zero)" {
        let model, _ = App.init ()
        let m1 = model |> App.update DecimalPressed |> fst
        Expect.equal m1.Display "0." "Decimal should append to initial zero"
    }

    test "Pressing 3 then decimal shows '3.'" {
        let model, _ = App.init ()
        let m1 = model |> App.update (DigitPressed 3) |> fst
        let m2 = m1 |> App.update DecimalPressed |> fst
        Expect.equal m2.Display "3." "Decimal should append to digit"
    }

    test "Pressing 3, decimal, 1 shows '3.1'" {
        let model, _ = App.init ()
        let m1 = model |> App.update (DigitPressed 3) |> fst
        let m2 = m1 |> App.update DecimalPressed |> fst
        let m3 = m2 |> App.update (DigitPressed 1) |> fst
        Expect.equal m3.Display "3.1" "Should build decimal number"
    }

    test "Pressing 3, decimal, decimal — second decimal ignored, still '3.'" {
        let model, _ = App.init ()
        let m1 = model |> App.update (DigitPressed 3) |> fst
        let m2 = m1 |> App.update DecimalPressed |> fst
        let m3 = m2 |> App.update DecimalPressed |> fst
        Expect.equal m3.Display "3." "Second decimal should be ignored"
    }

    test "Pressing 3, decimal, 1, decimal — second decimal ignored, still '3.1'" {
        let model, _ = App.init ()
        let m1 = model |> App.update (DigitPressed 3) |> fst
        let m2 = m1 |> App.update DecimalPressed |> fst
        let m3 = m2 |> App.update (DigitPressed 1) |> fst
        let m4 = m3 |> App.update DecimalPressed |> fst
        Expect.equal m4.Display "3.1" "Second decimal should be ignored"
    }
]

// ============================================================================
// 3. BASIC OPERATIONS TESTS
// ============================================================================

let basicOperationsTests = testList "Basic Operations" [
    test "2 + 3 = shows '5'" {
        let model, _ = App.init ()
        let m1 = model |> App.update (DigitPressed 2) |> fst
        let m2 = m1 |> App.update (OperatorPressed Add) |> fst
        let m3 = m2 |> App.update (DigitPressed 3) |> fst
        let m4 = m3 |> App.update EqualsPressed |> fst
        Expect.equal m4.Display "5" "2 + 3 should equal 5"
    }

    test "9 - 4 = shows '5'" {
        let model, _ = App.init ()
        let m1 = model |> App.update (DigitPressed 9) |> fst
        let m2 = m1 |> App.update (OperatorPressed Subtract) |> fst
        let m3 = m2 |> App.update (DigitPressed 4) |> fst
        let m4 = m3 |> App.update EqualsPressed |> fst
        Expect.equal m4.Display "5" "9 - 4 should equal 5"
    }

    test "3 × 4 = shows '12'" {
        let model, _ = App.init ()
        let m1 = model |> App.update (DigitPressed 3) |> fst
        let m2 = m1 |> App.update (OperatorPressed Multiply) |> fst
        let m3 = m2 |> App.update (DigitPressed 4) |> fst
        let m4 = m3 |> App.update EqualsPressed |> fst
        Expect.equal m4.Display "12" "3 × 4 should equal 12"
    }

    test "8 ÷ 2 = shows '4'" {
        let model, _ = App.init ()
        let m1 = model |> App.update (DigitPressed 8) |> fst
        let m2 = m1 |> App.update (OperatorPressed Divide) |> fst
        let m3 = m2 |> App.update (DigitPressed 2) |> fst
        let m4 = m3 |> App.update EqualsPressed |> fst
        Expect.equal m4.Display "4" "8 ÷ 2 should equal 4"
    }
]

// ============================================================================
// 4. LEFT-TO-RIGHT EVALUATION TESTS
// ============================================================================

let leftToRightTests = testList "Left-to-Right Evaluation" [
    test "2 + 3 + 4 = shows '9' (evaluates 2+3=5 when second + pressed, then 5+4=9)" {
        let model, _ = App.init ()
        let m1 = model |> App.update (DigitPressed 2) |> fst
        let m2 = m1 |> App.update (OperatorPressed Add) |> fst
        let m3 = m2 |> App.update (DigitPressed 3) |> fst
        let m4 = m3 |> App.update (OperatorPressed Add) |> fst  // Should evaluate 2+3=5
        let m5 = m4 |> App.update (DigitPressed 4) |> fst
        let m6 = m5 |> App.update EqualsPressed |> fst
        Expect.equal m6.Display "9" "2 + 3 + 4 should equal 9"
    }

    test "10 - 3 + 2 = shows '9'" {
        let model, _ = App.init ()
        let m1 = model |> App.update (DigitPressed 1) |> fst
        let m2 = m1 |> App.update (DigitPressed 0) |> fst
        let m3 = m2 |> App.update (OperatorPressed Subtract) |> fst
        let m4 = m3 |> App.update (DigitPressed 3) |> fst
        let m5 = m4 |> App.update (OperatorPressed Add) |> fst  // Should evaluate 10-3=7
        let m6 = m5 |> App.update (DigitPressed 2) |> fst
        let m7 = m6 |> App.update EqualsPressed |> fst
        Expect.equal m7.Display "9" "10 - 3 + 2 should equal 9"
    }

    test "2 × 3 + 1 = shows '7'" {
        let model, _ = App.init ()
        let m1 = model |> App.update (DigitPressed 2) |> fst
        let m2 = m1 |> App.update (OperatorPressed Multiply) |> fst
        let m3 = m2 |> App.update (DigitPressed 3) |> fst
        let m4 = m3 |> App.update (OperatorPressed Add) |> fst  // Should evaluate 2×3=6
        let m5 = m4 |> App.update (DigitPressed 1) |> fst
        let m6 = m5 |> App.update EqualsPressed |> fst
        Expect.equal m6.Display "7" "2 × 3 + 1 should equal 7"
    }
]

// ============================================================================
// 5. DIVISION BY ZERO TESTS
// ============================================================================

let divisionByZeroTests = testList "Division by Zero" [
    test "5 ÷ 0 = shows 'Error'" {
        let model, _ = App.init ()
        let m1 = model |> App.update (DigitPressed 5) |> fst
        let m2 = m1 |> App.update (OperatorPressed Divide) |> fst
        let m3 = m2 |> App.update (DigitPressed 0) |> fst
        let m4 = m3 |> App.update EqualsPressed |> fst
        Expect.equal m4.Display "Error" "Division by zero should show Error"
    }

    test "After error, pressing digit resets (e.g., press 3 → shows '3')" {
        let model, _ = App.init ()
        let m1 = model |> App.update (DigitPressed 5) |> fst
        let m2 = m1 |> App.update (OperatorPressed Divide) |> fst
        let m3 = m2 |> App.update (DigitPressed 0) |> fst
        let m4 = m3 |> App.update EqualsPressed |> fst  // Now Error
        let m5 = m4 |> App.update (DigitPressed 3) |> fst
        Expect.equal m5.Display "3" "After error, digit should reset calculator"
    }
]

// ============================================================================
// 6. CLEAR TESTS
// ============================================================================

let clearTests = testList "Clear" [
    test "Press some digits, then Clear → display='0', pendingOp=None" {
        let model, _ = App.init ()
        let m1 = model |> App.update (DigitPressed 1) |> fst
        let m2 = m1 |> App.update (DigitPressed 2) |> fst
        let m3 = m2 |> App.update (DigitPressed 3) |> fst
        let m4 = m3 |> App.update ClearPressed |> fst
        Expect.equal m4.Display "0" "Clear should reset display to 0"
        Expect.equal m4.PendingOp None "Clear should clear pending operation"
    }

    test "After operation, Clear resets everything" {
        let model, _ = App.init ()
        let m1 = model |> App.update (DigitPressed 2) |> fst
        let m2 = m1 |> App.update (OperatorPressed Add) |> fst
        let m3 = m2 |> App.update (DigitPressed 3) |> fst
        let m4 = m3 |> App.update ClearPressed |> fst
        Expect.equal m4.Display "0" "Clear should reset display"
        Expect.equal m4.PendingOp None "Clear should clear pending operation"
    }
]

// ============================================================================
// 7. BACKSPACE TESTS
// ============================================================================

let backspaceTests = testList "Backspace" [
    test "Backspace on '123' shows '12'" {
        let model, _ = App.init ()
        let m1 = model |> App.update (DigitPressed 1) |> fst
        let m2 = m1 |> App.update (DigitPressed 2) |> fst
        let m3 = m2 |> App.update (DigitPressed 3) |> fst
        let m4 = m3 |> App.update BackspacePressed |> fst
        Expect.equal m4.Display "12" "Backspace should remove last character"
    }

    test "Backspace on '5' (single digit) shows '0'" {
        let model, _ = App.init ()
        let m1 = model |> App.update (DigitPressed 5) |> fst
        let m2 = m1 |> App.update BackspacePressed |> fst
        Expect.equal m2.Display "0" "Backspace on single digit should reset to 0"
    }

    test "Backspace on '0' shows '0' (no-op)" {
        let model, _ = App.init ()
        let m1 = model |> App.update BackspacePressed |> fst
        Expect.equal m1.Display "0" "Backspace on 0 should do nothing"
    }

    test "Backspace on 'Error' shows 'Error' (no-op)" {
        let model, _ = App.init ()
        let m1 = model |> App.update (DigitPressed 5) |> fst
        let m2 = m1 |> App.update (OperatorPressed Divide) |> fst
        let m3 = m2 |> App.update (DigitPressed 0) |> fst
        let m4 = m3 |> App.update EqualsPressed |> fst  // Error state
        let m5 = m4 |> App.update BackspacePressed |> fst
        Expect.equal m5.Display "Error" "Backspace on Error should do nothing"
    }

    test "Backspace on '3.14' shows '3.1'" {
        let model, _ = App.init ()
        let m1 = model |> App.update (DigitPressed 3) |> fst
        let m2 = m1 |> App.update DecimalPressed |> fst
        let m3 = m2 |> App.update (DigitPressed 1) |> fst
        let m4 = m3 |> App.update (DigitPressed 4) |> fst
        let m5 = m4 |> App.update BackspacePressed |> fst
        Expect.equal m5.Display "3.1" "Backspace should work on decimal numbers"
    }

    test "Backspace on '3.' shows '3'" {
        let model, _ = App.init ()
        let m1 = model |> App.update (DigitPressed 3) |> fst
        let m2 = m1 |> App.update DecimalPressed |> fst
        let m3 = m2 |> App.update BackspacePressed |> fst
        Expect.equal m3.Display "3" "Backspace should remove decimal point"
    }
]

// ============================================================================
// 8. EDGE CASE TESTS
// ============================================================================

let edgeCaseTests = testList "Edge Cases" [
    test "Pressing equals with no pending operation does nothing (display unchanged)" {
        let model, _ = App.init ()
        let m1 = model |> App.update (DigitPressed 5) |> fst
        let m2 = m1 |> App.update EqualsPressed |> fst
        Expect.equal m2.Display "5" "Equals without pending op should not change display"
    }

    test "Negative result: 3 - 8 = shows '-5'" {
        let model, _ = App.init ()
        let m1 = model |> App.update (DigitPressed 3) |> fst
        let m2 = m1 |> App.update (OperatorPressed Subtract) |> fst
        let m3 = m2 |> App.update (DigitPressed 8) |> fst
        let m4 = m3 |> App.update EqualsPressed |> fst
        Expect.equal m4.Display "-5" "Should handle negative results"
    }

    test "Decimal arithmetic: 1.5 + 2.5 = shows '4'" {
        let model, _ = App.init ()
        let m1 = model |> App.update (DigitPressed 1) |> fst
        let m2 = m1 |> App.update DecimalPressed |> fst
        let m3 = m2 |> App.update (DigitPressed 5) |> fst
        let m4 = m3 |> App.update (OperatorPressed Add) |> fst
        let m5 = m4 |> App.update (DigitPressed 2) |> fst
        let m6 = m5 |> App.update DecimalPressed |> fst
        let m7 = m6 |> App.update (DigitPressed 5) |> fst
        let m8 = m7 |> App.update EqualsPressed |> fst
        Expect.equal m8.Display "4" "Should handle decimal arithmetic"
    }

    test "Large number: 999999999 + 1 = shows '1000000000'" {
        let model, _ = App.init ()
        // Build 999999999
        let m1 = model |> App.update (DigitPressed 9) |> fst
        let m2 = m1 |> App.update (DigitPressed 9) |> fst
        let m3 = m2 |> App.update (DigitPressed 9) |> fst
        let m4 = m3 |> App.update (DigitPressed 9) |> fst
        let m5 = m4 |> App.update (DigitPressed 9) |> fst
        let m6 = m5 |> App.update (DigitPressed 9) |> fst
        let m7 = m6 |> App.update (DigitPressed 9) |> fst
        let m8 = m7 |> App.update (DigitPressed 9) |> fst
        let m9 = m8 |> App.update (DigitPressed 9) |> fst
        let m10 = m9 |> App.update (OperatorPressed Add) |> fst
        let m11 = m10 |> App.update (DigitPressed 1) |> fst
        let m12 = m11 |> App.update EqualsPressed |> fst
        Expect.equal m12.Display "1000000000" "Should handle large numbers"
    }

    test "Pressing operator right after init: should store 0 as first operand" {
        let model, _ = App.init ()
        let m1 = model |> App.update (OperatorPressed Add) |> fst
        let m2 = m1 |> App.update (DigitPressed 5) |> fst
        let m3 = m2 |> App.update EqualsPressed |> fst
        Expect.equal m3.Display "5" "0 + 5 should equal 5"
    }

    test "Pressing operator twice: second operator replaces first" {
        let model, _ = App.init ()
        let m1 = model |> App.update (DigitPressed 5) |> fst
        let m2 = m1 |> App.update (OperatorPressed Add) |> fst
        let m3 = m2 |> App.update (OperatorPressed Multiply) |> fst  // Replace Add with Multiply
        let m4 = m3 |> App.update (DigitPressed 3) |> fst
        let m5 = m4 |> App.update EqualsPressed |> fst
        Expect.equal m5.Display "15" "5 × 3 should equal 15 (not 5 + 3)"
    }
]

// ============================================================================
// ALL TESTS
// ============================================================================

let allTests = testList "CalTwo" [
    digitEntryTests
    decimalPointTests
    basicOperationsTests
    leftToRightTests
    divisionByZeroTests
    clearTests
    backspaceTests
    edgeCaseTests
]

[<EntryPoint>]
let main args =
    Mocha.runTests allTests
