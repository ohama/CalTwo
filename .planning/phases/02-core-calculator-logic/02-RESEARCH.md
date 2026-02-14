# Phase 2: Core Calculator Logic - Research

**Researched:** 2026-02-14
**Domain:** F# calculator state modeling, Elmish MVU architecture, left-to-right arithmetic evaluation, Fable testing
**Confidence:** HIGH

## Summary

Research focused on how to implement calculator logic using the Elmish MVU (Model-View-Update) pattern in F# with Fable. The standard approach uses discriminated unions for both calculator state and messages, with a pure update function handling state transitions. Calculator state includes display string and optional pending operation (operator + first operand).

Key findings:
- F# discriminated unions are the canonical way to model calculator state and messages
- F# for Fun and Profit calculator walkthrough provides authoritative design patterns
- Fable.Mocha (v2.17.0) is the established testing library for Fable apps, mirrors Expecto API
- Vitest is a modern alternative that works well with Vite-based Fable projects
- Left-to-right evaluation stores pending operations and evaluates when equals or next operator is pressed
- Edge cases (divide-by-zero, multiple decimals, leading zeros) are handled through validation in service functions

**Primary recommendation:** Use discriminated unions for state/messages, store pending operations as `(MathOp * Number) option`, test pure update functions with Fable.Mocha, handle edge cases in display update services.

## Standard Stack

The established libraries/tools for F# Fable calculator testing:

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Fable.Mocha | 2.17.0 | Unit testing for Fable | Mirrors Expecto API, runs in node/browser/dotnet, standard in SAFE stack |
| Fable.Core | >= 3.0.0 | Fable base library | Required dependency for Fable.Mocha |
| FSharp.Core | >= 4.7.0 | F# runtime | Required for F# code |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| Vitest | latest | Modern test runner | Alternative to Mocha, better Vite integration, 30-70% faster |
| Fable.Expect | (npm) | Assertion helpers for Fable | For Elmish-specific testing (Program.mountAndTest), DOM testing |
| Web Test Runner | latest | Browser test runner | For testing with ES modules in headless browser |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Fable.Mocha | Vitest + Fable.Expect | Vitest faster and better Vite integration, but Mocha more established in F# ecosystem |
| Fable.Mocha | Fable.Pyxpecto | Cross-platform (JS/Python/.NET), but less mature (v1.3.0 vs 2.17.0) |
| Browser testing | .NET unit tests with compiler directives | Faster CI, but requires separating Model from View code |

**Installation (Fable.Mocha approach):**
```bash
dotnet add package Fable.Mocha --version 2.17.0
npm install mocha --save-dev
```

**Installation (Vitest approach - RECOMMENDED for this project):**
```bash
npm install vitest --save-dev
npm install @web/test-runner --save-dev
```

## Architecture Patterns

### Recommended Project Structure
```
src/
├── App.fs               # Main calculator component
│   ├── Model types     # Calculator state, messages, config
│   ├── Init function   # Initial state
│   ├── Update function # Pure state transitions
│   └── View function   # React UI (Feliz)
├── CalculatorServices.fs  # Pure calculation logic (optional separate file)
└── Main.fs             # Elmish program + HMR

tests/
├── CalculatorTests.fs  # Unit tests for update function
└── index.html          # Test runner entry point (if using browser tests)
```

### Pattern 1: Calculator State with Discriminated Unions
**What:** Model calculator state as immutable record with optional pending operation
**When to use:** All calculator implementations in F#
**Example:**
```fsharp
// Source: https://fsharpforfunandprofit.com/posts/calculator-design/
type CalculatorState = {
    display: CalculatorDisplay
    pendingOp: (CalculatorMathOp * Number) option
}
and CalculatorDisplay = string
and Number = float

type CalculatorMathOp = Add | Subtract | Multiply | Divide

type CalculatorInput =
    | Digit of CalculatorDigit
    | Op of CalculatorMathOp
    | Action of CalculatorAction
and CalculatorDigit =
    | Zero | One | Two | Three | Four
    | Five | Six | Seven | Eight | Nine
    | DecimalSeparator
and CalculatorAction = Equals | Clear
```

### Pattern 2: Pure Update Function
**What:** Update function maps (Msg, Model) to (Model, Cmd)
**When to use:** All Elmish applications
**Example:**
```fsharp
// Source: Elmish architecture pattern
let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | DigitPressed digit ->
        let newDisplay = updateDisplayFromDigit digit model.display
        { model with display = newDisplay }, Cmd.none
    | OperatorPressed op ->
        let newModel =
            match model.pendingOp with
            | Some (pendingOp, firstNum) ->
                // Evaluate pending operation (left-to-right)
                let result = evaluateOperation pendingOp firstNum (parseDisplay model.display)
                { display = formatNumber result; pendingOp = Some (op, result) }
            | None ->
                let currentNum = parseDisplay model.display
                { display = "0"; pendingOp = Some (op, currentNum) }
        newModel, Cmd.none
    | EqualsPressed ->
        match model.pendingOp with
        | Some (op, firstNum) ->
            let secondNum = parseDisplay model.display
            let result = evaluateOperation op firstNum secondNum
            { display = formatResult result; pendingOp = None }, Cmd.none
        | None -> model, Cmd.none
    | ClearPressed ->
        init(), Cmd.none
```

### Pattern 3: Left-to-Right Evaluation with Pending Operation
**What:** Store first operand and operator when user presses +, -, ×, ÷; evaluate when next operator or equals pressed
**When to use:** Simple calculators without operator precedence
**Example:**
```fsharp
// Source: https://fsharpforfunandprofit.com/posts/calculator-complete-v2/
// When operator button pressed:
// 1. If there's already a pending operation, evaluate it first
// 2. Store the new operator and current display value as pending
// 3. Reset display for next input

// Example: User enters 2 + 3 +
// Step 1: Press 2 -> display="2", pendingOp=None
// Step 2: Press + -> display="2", pendingOp=Some(Add, 2.0)
// Step 3: Press 3 -> display="3", pendingOp=Some(Add, 2.0)
// Step 4: Press + -> evaluate 2+3=5, display="5", pendingOp=Some(Add, 5.0)
```

### Pattern 4: State Machine Design (Alternative, More Complex)
**What:** Model calculator as explicit states (ZeroState, AccumulatorState, ComputedState, ErrorState)
**When to use:** When you need very explicit state handling and type safety
**Example:**
```fsharp
// Source: https://fsharpforfunandprofit.com/posts/calculator-complete-v2/
type CalculatorState =
    | ZeroState of ZeroStateData
    | AccumulatorState of AccumulatorStateData
    | AccumulatorWithDecimalState of AccumulatorWithDecimalStateData
    | ComputedState of ComputedStateData
    | ErrorState of ErrorStateData

// Each state handles inputs differently
// More complex but provides compile-time guarantees
```

### Pattern 5: Testing Pure Functions
**What:** Test update function by passing messages and checking resulting state
**When to use:** All calculator logic testing
**Example:**
```fsharp
// Source: https://github.com/Zaid-Ajaj/Fable.Mocha
open Fable.Mocha

let calculatorTests = testList "Calculator" [
    test "Adding two numbers" {
        let initialState = init()
        let state1 = initialState |> update (DigitPressed Two) |> fst
        let state2 = state1 |> update (OperatorPressed Add) |> fst
        let state3 = state2 |> update (DigitPressed Three) |> fst
        let finalState = state3 |> update EqualsPressed |> fst

        Expect.equal finalState.display "5" "2 + 3 should equal 5"
    }

    test "Divide by zero shows error" {
        let initialState = init()
        let finalState =
            initialState
            |> update (DigitPressed Five)
            |> fst
            |> update (OperatorPressed Divide)
            |> fst
            |> update (DigitPressed Zero)
            |> fst
            |> update EqualsPressed
            |> fst

        Expect.equal finalState.display "Error" "Division by zero should show Error"
    }
]

Mocha.runTests calculatorTests |> ignore
```

### Anti-Patterns to Avoid
- **Mutable state in Model:** Never use mutable fields; always return new state
- **Side effects in update function:** Keep update pure; no console.log, no HTTP calls
- **Mixed view/logic code:** Separate React components from calculator logic for testability
- **Complex state machines for simple calculator:** Start with simple pending operation approach; only use state machine if needed

## Don't Hand-Roll

Problems that look simple but have existing solutions:

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Decimal validation | Custom string parsing | String.Contains check + config for separator | Locale-specific decimal separators (. vs ,) |
| Leading zero prevention | Complex regex | Simple conditional: `if display="0" then "" else "0"` | Edge cases like "0.5" need special handling |
| Divide by zero | Manual checks | try/catch with DivideByZero exception | F# raises exception; catch and return error state |
| Test runner setup | Custom Vite config | Fable.Mocha or Vitest (drop-in with Vite) | Mocha/Vitest handle ES modules, HMR, browser testing |
| Assertion library | Custom expect functions | Fable.Mocha Expect module | Provides equal, isTrue, throws, etc. with proper diff output |

**Key insight:** Calculator edge cases (multiple decimals, leading zeros, div-by-zero) are well-documented in F# for Fun and Profit. Don't reinvent; follow proven patterns.

## Common Pitfalls

### Pitfall 1: Forgetting to Evaluate Pending Operation on Next Operator
**What goes wrong:** User presses 2 + 3 +, expects 5 but sees 3
**Why it happens:** Only storing new operator without evaluating previous one
**How to avoid:** Always check for pendingOp when new operator pressed; if exists, evaluate it first
**Warning signs:** Chained operations (2+3+4) don't accumulate correctly

### Pitfall 2: Multiple Decimal Points
**What goes wrong:** User can input "3.14.15" instead of "3.14"
**Why it happens:** Not validating that display already contains decimal separator
**How to avoid:** Check `display.Contains(config.decimalSeparator)` before appending
**Warning signs:** Display shows invalid number formats

### Pitfall 3: Leading Zeros
**What goes wrong:** Display shows "000" or "0123" instead of "0" or "123"
**Why it happens:** Appending to "0" display without replacement logic
**How to avoid:** When Zero digit pressed and display="0", return empty string (don't append)
**Warning signs:** Numbers start with multiple zeros

### Pitfall 4: Division by Zero Crashes App
**What goes wrong:** Unhandled exception when dividing by zero
**Why it happens:** F# raises `System.DivideByZeroException` for float division by zero
**How to avoid:** Wrap division in try/catch, return `Failure DivideByZero` result type
**Warning signs:** App crashes when equals pressed after "÷ 0"

### Pitfall 5: Testing with Side Effects
**What goes wrong:** Tests fail in .NET runner or CI environment
**Why it happens:** Mixing browser-specific code (DOM, window) with calculator logic
**How to avoid:** Keep Model and update logic in separate file from View; use compiler directives for browser APIs
**Warning signs:** Tests work in browser but fail in node/dotnet

### Pitfall 6: Negative Results Not Handled
**What goes wrong:** Display shows "-5" but digit input doesn't work correctly
**Why it happens:** Assuming display always starts with digit, not minus sign
**How to avoid:** Handle negative results as normal numbers; parse with `float` function which handles "-"
**Warning signs:** Calculator works for positive results but breaks after subtraction yielding negative

### Pitfall 7: Commands in Update Function Make Testing Hard
**What goes wrong:** Can't easily test update function because it returns `Cmd<Msg>`
**Why it happens:** Trying to do side effects (toasts, logging) inside update
**How to avoid:** Use `Cmd.ofEffect` for occasional impure operations, or better yet, keep update returning `Cmd.none` for pure calculators
**Warning signs:** Can't test update function without mocking infrastructure

## Code Examples

Verified patterns from official sources:

### Edge Case: Multiple Decimal Separators
```fsharp
// Source: https://fsharpforfunandprofit.com/posts/calculator-complete-v1/
type CalculatorConfiguration = {
    decimalSeparator: string
    maxDisplayLength: int
}

let updateDisplayFromDigit (config: CalculatorConfiguration) (digit: CalculatorDigit) (display: string) : string =
    match digit with
    | DecimalSeparator ->
        if display.Contains(config.decimalSeparator) then
            "" // Already has decimal, ignore this input
        else
            config.decimalSeparator
    | Zero ->
        if display = "0" then "" else "0"
    | One -> "1"
    | Two -> "2"
    // ... etc
    |> fun digitStr ->
        if display.Length > config.maxDisplayLength then
            display // Silently ignore if too long
        else
            display + digitStr
```

### Edge Case: Division by Zero
```fsharp
// Source: https://fsharpforfunandprofit.com/posts/calculator-design/
type MathOperationResult =
    | Success of Number
    | Failure of MathOperationError
and MathOperationError = DivideByZero

let doMathOperation (op: CalculatorMathOp) (f1: float) (f2: float) : MathOperationResult =
    match op with
    | Add -> Success (f1 + f2)
    | Subtract -> Success (f1 - f2)
    | Multiply -> Success (f1 * f2)
    | Divide ->
        try
            Success (f1 / f2)
        with
        | :? System.DivideByZeroException ->
            Failure DivideByZero
```

### Edge Case: Locale-Specific Decimal Separator
```fsharp
// Source: https://fsharpforfunandprofit.com/posts/calculator-complete-v1/
let config = {
    decimalSeparator =
        System.Globalization.CultureInfo.CurrentCulture
            .NumberFormat.CurrencyDecimalSeparator
    maxDisplayLength = 10
}
```

### Testing: Chained Operations (Left-to-Right)
```fsharp
// Source: Fable.Mocha documentation
open Fable.Mocha

let chainedOperationsTest = test "2 + 3 + 4 equals 9" {
    let initialState = init()
    let finalState =
        initialState
        |> update (DigitPressed Two) |> fst
        |> update (OperatorPressed Add) |> fst
        |> update (DigitPressed Three) |> fst
        |> update (OperatorPressed Add) |> fst  // Should evaluate 2+3=5 first
        |> update (DigitPressed Four) |> fst
        |> update EqualsPressed |> fst           // Then 5+4=9

    Expect.equal finalState.display "9" "Left-to-right evaluation"
}
```

### Testing: Pure Function (No Mocking Needed)
```fsharp
// Source: https://jordanmarr.github.io/fsharp/unit-testing-fable-dotnet/
// Calculator update function is pure - takes (Msg, Model) returns (Model, Cmd)
// No need for mocks or stubs

let testDigitEntry = testList "Digit Entry" [
    test "Entering 1-2-3" {
        let state0 = init()
        let state1 = state0 |> update (DigitPressed One) |> fst
        Expect.equal state1.display "1" "First digit"

        let state2 = state1 |> update (DigitPressed Two) |> fst
        Expect.equal state2.display "12" "Second digit"

        let state3 = state2 |> update (DigitPressed Three) |> fst
        Expect.equal state3.display "123" "Third digit"
    }

    test "Cannot enter multiple decimals" {
        let state =
            init()
            |> update (DigitPressed Three) |> fst
            |> update (DigitPressed DecimalSeparator) |> fst
            |> update (DigitPressed One) |> fst
            |> update (DigitPressed DecimalSeparator) |> fst  // Should be ignored
            |> update (DigitPressed Four) |> fst

        Expect.equal state.display "3.14" "Only one decimal point"
    }
]
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Jest for Fable testing | Vitest | 2024-2025 | 30-70% faster, better Vite integration, ES modules native |
| Webpack bundler | Vite | 2021+ | Faster dev server, HMR, simpler config |
| Fable.Mocha with Webpack | Fable.Mocha with Vite or Vitest | 2024+ | Simpler setup, but Mocha still widely used |
| Manual test HTML | Web Test Runner | 2023+ | Headless browser testing, ES modules, better CI |
| Fable.Import.Mocha | Fable.Mocha | 2018+ | Mirrors Expecto API, better F# integration |

**Deprecated/outdated:**
- Fable.Import.Mocha (v0.1.0): Use Fable.Mocha (v2.17.0) instead
- Fable.Import.MochaJS (v1.0.1): Use Fable.Mocha instead
- Jest with F#: Vitest now preferred for Vite projects

**Current best practice (2025-2026):**
- For new Vite-based Fable projects: Vitest is recommended
- For established SAFE stack projects: Fable.Mocha is standard
- This project uses Vite, so Vitest is recommended, but Fable.Mocha also works fine

## Open Questions

Things that couldn't be fully resolved:

1. **Fable.Expect npm package installation**
   - What we know: GitHub repo exists, used with Web Test Runner, provides Elmish testing utilities
   - What's unclear: No NuGet package, no npm installation docs in README, package.json shows it's an npm package but no published version found
   - Recommendation: Use Fable.Mocha (established) or Vitest (modern) instead; Fable.Expect appears to be for more advanced Elmish component testing

2. **Vitest configuration for F# Fable**
   - What we know: FableStarter template uses Vitest, works with Vite, 30-70% faster than Jest
   - What's unclear: Exact vitest.config.js for Fable projects not documented in search results
   - Recommendation: Start with Fable.Mocha (proven), consider Vitest if build performance becomes issue

3. **Operator precedence vs left-to-right**
   - What we know: Simple calculators use left-to-right (2+3*4 = 20), scientific calculators use precedence (= 14)
   - What's unclear: User expectations - requirements say "left-to-right" but users might expect precedence
   - Recommendation: Requirements specify left-to-right (OPS-02), implement that; document behavior in tutorial

4. **Negative number input**
   - What we know: Results can be negative, display shows minus sign
   - What's unclear: Can user input negative numbers directly (like "-5")? No minus button in typical calculator
   - Recommendation: Support negative results but not direct negative input; user must subtract from zero

## Sources

### Primary (HIGH confidence)
- [Calculator Walkthrough: Part 1 - F# for Fun and Profit](https://fsharpforfunandprofit.com/posts/calculator-design/) - Type-first design, state modeling
- [Calculator Walkthrough: Part 3 - F# for Fun and Profit](https://fsharpforfunandprofit.com/posts/calculator-complete-v1/) - Edge case handling (decimals, zeros, div-by-zero)
- [Calculator Walkthrough: Part 4 - F# for Fun and Profit](https://fsharpforfunandprofit.com/posts/calculator-complete-v2/) - State machine design patterns
- [Fable.Mocha GitHub](https://github.com/Zaid-Ajaj/Fable.Mocha) - Testing framework documentation
- [Fable.Mocha NuGet v2.17.0](https://www.nuget.org/packages/Fable.Mocha) - Latest version, compatibility

### Secondary (MEDIUM confidence)
- [Elmish Official Documentation](https://elmish.github.io/elmish/) - MVU architecture patterns
- [SAFE Stack: Testing the Client](https://safe-stack.github.io/docs/recipes/developing-and-testing/testing-the-client/) - Fable.Mocha setup
- [FableStarter GitHub](https://github.com/rastreus/FableStarter) - Vitest with Fable template
- [Tips for Unit Testing Fable Apps using .NET - Jordan Marr](https://jordanmarr.github.io/fsharp/unit-testing-fable-dotnet/) - Pure function testing patterns
- [Fable.Expect GitHub](https://github.com/fable-compiler/Fable.Expect) - Elmish testing utilities
- [Discriminated Unions - F# for Fun and Profit](https://fsharpforfunandprofit.com/posts/discriminated-unions/) - State modeling patterns

### Tertiary (LOW confidence)
- [Jest vs Vitest 2025 comparison](https://medium.com/@ruverd/jest-vs-vitest-which-test-runner-should-you-use-in-2025-5c85e4f2bda9) - WebSearch, verified Vitest performance claims
- [Calculator State Machine Blog](https://mubaraqwahab.com/blog/calculator/) - Pending operation patterns
- [Fable.io Resources](https://fable.io/resources.html) - Testing library listings

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - Fable.Mocha v2.17.0 is established, well-documented, used in SAFE stack
- Architecture: HIGH - F# for Fun and Profit calculator walkthrough is authoritative source, multiple parts covering all aspects
- Pitfalls: HIGH - All edge cases documented with code examples from official sources
- Testing setup: MEDIUM - Fable.Mocha well-documented, but Vitest+Fable config not fully documented (FableStarter template exists but config not detailed)
- Left-to-right evaluation: HIGH - Clear pattern from F# for Fun and Profit (pending operation + evaluate on next op/equals)

**Research date:** 2026-02-14
**Valid until:** 2026-04-14 (60 days - F# ecosystem stable, testing tools mature)

**Notes:**
- F# for Fun and Profit calculator walkthrough (4 parts) is the definitive guide for this domain
- Fable.Mocha is proven and recommended; Vitest is newer but promising alternative
- Calculator logic patterns are well-established and stable (not rapidly changing)
- Edge case handling is thoroughly documented with code examples
- Testing pure functions is straightforward - no mocking infrastructure needed
