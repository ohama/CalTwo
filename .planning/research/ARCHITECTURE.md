# Architecture Patterns

**Domain:** F# Elmish Web Applications
**Researched:** 2026-02-14
**Confidence:** HIGH

## Recommended Architecture

F# Elmish applications follow the **Model-View-Update (MVU)** pattern, also known as The Elm Architecture. This is a unidirectional data flow architecture where state is centralized and updates are pure functions.

```
┌─────────────────────────────────────────┐
│            Browser / DOM                │
└─────────────────┬───────────────────────┘
                  │ User Events
                  ▼
┌─────────────────────────────────────────┐
│              View (UI)                  │
│  - Renders current Model state          │
│  - Emits Msg on user interaction        │
└─────────────────┬───────────────────────┘
                  │ Msg
                  ▼
┌─────────────────────────────────────────┐
│           Update Function               │
│  - Pure function: Model -> Msg -> Model │
│  - Returns new Model + Cmd              │
└─────────────────┬───────────────────────┘
                  │ New Model
                  ▼
┌─────────────────────────────────────────┐
│         Model (State)                   │
│  - Immutable record type                │
│  - Single source of truth               │
└─────────────────────────────────────────┘
```

### Project File Structure

**Standard Fable + Elmish layout:**

```
CalTwo/
├── src/
│   ├── App.fs              # Main MVU wiring (init, update, view)
│   ├── Types.fs            # Model and Msg types
│   ├── State.fs            # Update logic (optional split)
│   └── View.fs             # UI rendering (optional split)
├── tests/
│   └── Tests.fs            # Unit tests for update logic
├── public/
│   └── index.html          # HTML shell
├── package.json            # npm dependencies
├── webpack.config.js       # Build configuration
├── CalTwo.fsproj           # F# project file
└── paket.dependencies      # .NET dependencies (or use NuGet)
```

**Alternative: Single file for simple apps**

For a basic calculator, all MVU components can live in a single `App.fs`:

```
CalTwo/
├── src/
│   └── App.fs              # Types, init, update, view all together
├── tests/
│   └── Tests.fs
├── public/
│   └── index.html
├── package.json
├── webpack.config.js
└── CalTwo.fsproj
```

**Recommendation for CalTwo:** Start with single-file `App.fs`. Split later if complexity grows.

### Component Boundaries

| Component | Responsibility | Communicates With |
|-----------|---------------|-------------------|
| **Model** | Application state (immutable record) | Updated by Update function, read by View |
| **Msg** | Discriminated union of all possible user actions | Dispatched by View, consumed by Update |
| **init** | Create initial Model + optional startup Cmd | Called once at app start, returns Model |
| **update** | Pure function applying Msg to Model | Receives Msg + current Model, returns new Model + Cmd |
| **view** | Renders Model as HTML using React bindings | Reads Model, dispatches Msg on events |
| **Program** | Elmish runtime orchestrating MVU loop | Ties init/update/view together, handles Cmd execution |

**Key architectural constraint:** All components are **pure functions** except the Program runtime.

### Data Flow

**1. Initialization**
```fsharp
// init : unit -> Model * Cmd<Msg>
let init () =
    { Display = "0"; CurrentValue = 0.0 }, Cmd.none
```

**2. User Interaction → Msg Dispatch**
```fsharp
// User clicks button "7"
button [ OnClick (fun _ -> dispatch (DigitPressed 7)) ] [ str "7" ]
```

**3. Update Function Processes Msg**
```fsharp
// update : Msg -> Model -> Model * Cmd<Msg>
let update msg model =
    match msg with
    | DigitPressed digit ->
        { model with Display = model.Display + string digit }, Cmd.none
```

**4. View Re-renders with New Model**
```fsharp
// view : Model -> Dispatch<Msg> -> ReactElement
let view model dispatch =
    div [] [
        input [ Value model.Display; ReadOnly true ]
        button [ OnClick (fun _ -> dispatch (DigitPressed 7)) ] [ str "7" ]
    ]
```

**5. Cycle Repeats**

The Elmish runtime ensures:
- View always reflects current Model
- Model updates are atomic (no partial states)
- All state changes flow through update function

### Module Organization

**For Calculator App (Simple):**

```fsharp
// src/App.fs
module App

// Types Section
type Model = { Display: string; Accumulator: float option; Operation: Op option }
and Op = Add | Subtract | Multiply | Divide

type Msg =
    | DigitPressed of int
    | OperationPressed of Op
    | EqualsPressed
    | Clear

// State Section
let init () = { Display = "0"; Accumulator = None; Operation = None }, Cmd.none

let update msg model =
    match msg with
    | DigitPressed d -> { model with Display = model.Display + string d }, Cmd.none
    | EqualsPressed -> (* calculate *) model, Cmd.none
    // ...

// View Section
open Fable.React
open Fable.React.Props

let view model dispatch =
    div [ ClassName "calculator" ] [
        input [ Value model.Display; ReadOnly true ]
        // buttons...
    ]

// Program Wiring
open Elmish
open Elmish.React

Program.mkSimple init update view
|> Program.withReactSynchronous "elmish-app"
|> Program.run
```

**For Larger Apps (Modular):**

Split concerns across files:

```fsharp
// Types.fs
module Types
type Model = { ... }
type Msg = ...

// State.fs
module State
open Types
let init () = ...
let update msg model = ...

// View.fs
module View
open Types
open Fable.React
let view model dispatch = ...

// App.fs (entry point)
module App
open Elmish
open Elmish.React

Program.mkSimple State.init State.update View.view
|> Program.withReactSynchronous "elmish-app"
|> Program.run
```

### Build Pipeline Architecture

**Fable Compilation Flow:**

```
F# Source (.fs)
    │
    ▼
Fable Compiler
    │ (F# → JavaScript transpilation)
    ▼
JavaScript Modules (.js)
    │
    ▼
Webpack / Vite
    │ (bundling, optimization)
    ▼
Static Bundle (bundle.js)
    │
    ▼
index.html (loads bundle)
```

**Standard Build Tools:**

| Tool | Purpose | Configuration File |
|------|---------|-------------------|
| Fable | F# to JS transpiler | `fable.config.js` or inline in webpack |
| Webpack | Module bundler (traditional) | `webpack.config.js` |
| Vite | Fast dev server (modern alternative) | `vite.config.js` |
| npm/yarn | JavaScript package manager | `package.json` |
| dotnet | .NET SDK for F# compilation | `CalTwo.fsproj` |

**Recommendation for CalTwo:**

Use **Webpack** for build simplicity and broad compatibility with GitHub Pages deployment.

```javascript
// webpack.config.js (minimal)
module.exports = {
    entry: './src/App.fs.js',
    output: {
        path: path.join(__dirname, './public'),
        filename: 'bundle.js',
    },
    devServer: {
        contentBase: './public',
    },
    module: {
        rules: [
            {
                test: /\.fs(x|proj)?$/,
                use: 'fable-loader'
            }
        ]
    }
}
```

### Component Dependencies & Build Order

**Dependency Graph:**

```
fsproj file
    │
    ├─> .NET packages (Fable.Core, Fable.Elmish, Fable.React)
    │
    └─> F# source files (in compilation order)
            │
            ▼
        Types.fs (no dependencies)
            │
            ▼
        State.fs (depends on Types)
            │
            ▼
        View.fs (depends on Types)
            │
            ▼
        App.fs (depends on State, View)

package.json
    │
    └─> npm packages (react, react-dom, webpack, fable-loader)
```

**Build Order:**

1. Install .NET dependencies: `dotnet restore`
2. Install npm dependencies: `npm install`
3. Build F# → JS: `npm run build` (runs Fable + Webpack)
4. Output: `public/bundle.js` + `public/index.html`

**Development workflow:**

```bash
npm start  # Starts webpack-dev-server with hot reload
```

Changes to `.fs` files trigger:
1. Fable recompile (F# → JS)
2. Webpack rebundle
3. Browser hot reload (preserves state if using HMR)

## Patterns to Follow

### Pattern 1: Pure Update Functions
**What:** Update function must be pure (no side effects, same input → same output)

**When:** Always. Core MVU principle.

**Example:**
```fsharp
// GOOD: Pure function
let update msg model =
    match msg with
    | DigitPressed d ->
        let newDisplay = model.Display + string d
        { model with Display = newDisplay }, Cmd.none

// BAD: Side effect in update
let update msg model =
    match msg with
    | DigitPressed d ->
        printfn "User pressed %d" d  // SIDE EFFECT!
        { model with Display = model.Display + string d }, Cmd.none
```

### Pattern 2: Single Model Type
**What:** Entire application state in one immutable record

**When:** Always for simple apps. Use nested records for complex state.

**Example:**
```fsharp
// Simple calculator - single flat record
type Model = {
    Display: string
    Accumulator: float option
    PendingOp: Operation option
}

// More complex - nested records for organization
type Model = {
    Calculator: CalculatorState
    UI: UIState
    History: HistoryState
}
```

### Pattern 3: Exhaustive Msg Matching
**What:** Update function must handle all Msg cases

**When:** Always. Compiler enforces this with discriminated unions.

**Example:**
```fsharp
type Msg =
    | Digit of int
    | Operation of Op
    | Clear

let update msg model =
    match msg with
    | Digit d -> (* handle *) model, Cmd.none
    | Operation op -> (* handle *) model, Cmd.none
    | Clear -> (* handle *) model, Cmd.none
    // Compiler error if any case missing!
```

### Pattern 4: Cmd for Side Effects
**What:** Return Cmd<Msg> from update for async operations (API calls, timers)

**When:** When update needs to trigger side effects

**Example:**
```fsharp
// For calculator, mostly Cmd.none (no async)
let update msg model =
    match msg with
    | DigitPressed d ->
        { model with Display = model.Display + string d }, Cmd.none

// If we needed async (e.g., save to server):
    | SaveCalculation ->
        model, Cmd.OfAsync.perform saveToApi model.Display CalculationSaved
```

**For CalTwo:** Likely won't need Cmd patterns (pure client-side calculator).

### Pattern 5: View as Function of Model
**What:** View renders Model, doesn't maintain own state

**When:** Always. View is stateless representation of Model.

**Example:**
```fsharp
// GOOD: View derived from model
let view model dispatch =
    div [] [
        input [ Value model.Display ]  // Display comes from model
        button [ OnClick (fun _ -> dispatch Clear) ] [ str "C" ]
    ]

// BAD: View with local state (anti-pattern in Elmish)
// Don't use React.useState in Elmish views!
```

## Anti-Patterns to Avoid

### Anti-Pattern 1: Mutable State in Model
**What:** Using mutable fields or reference types that can change

**Why bad:** Breaks time-travel debugging, causes race conditions, defeats MVU predictability

**Instead:** Use immutable F# records and discriminated unions

```fsharp
// BAD
type Model = {
    mutable Display: string  // MUTABLE!
}

// GOOD
type Model = {
    Display: string  // Immutable
}
```

### Anti-Pattern 2: Side Effects in View
**What:** Dispatching messages during render or mutating external state

**Why bad:** Makes view impure, causes infinite render loops

**Instead:** Dispatch only in event handlers

```fsharp
// BAD
let view model dispatch =
    dispatch (LogRender DateTime.Now)  // Dispatching during render!
    div [] [ str model.Display ]

// GOOD
let view model dispatch =
    div [] [
        str model.Display
        button [ OnClick (fun _ -> dispatch Clear) ] [ str "C" ]  // Dispatch in handler
    ]
```

### Anti-Pattern 3: Direct DOM Manipulation
**What:** Using JS interop to modify DOM outside React/Elmish

**Why bad:** React won't know about changes, causes state desync

**Instead:** Let view function handle all rendering

```fsharp
// BAD
let update msg model =
    match msg with
    | Clear ->
        // Direct DOM manipulation!
        Browser.Dom.document.getElementById("display").innerText <- "0"
        model, Cmd.none

// GOOD
let update msg model =
    match msg with
    | Clear ->
        { model with Display = "0" }, Cmd.none  // Update model, view re-renders
```

### Anti-Pattern 4: Business Logic in View
**What:** Calculations or validation in view function

**Why bad:** Not testable, violates separation of concerns

**Instead:** Keep view as pure rendering, logic in update

```fsharp
// BAD
let view model dispatch =
    let result = calculate model.Accumulator model.Operation model.Display  // Logic in view!
    div [] [ str (string result) ]

// GOOD
// Logic in update function
let update msg model =
    match msg with
    | EqualsPressed ->
        let result = calculate model.Accumulator model.Operation model.Display
        { model with Display = string result }, Cmd.none

// View just renders
let view model dispatch =
    div [] [ str model.Display ]
```

### Anti-Pattern 5: Overly Nested Components
**What:** Breaking view into many sub-components too early

**Why bad:** Adds complexity for simple apps, makes dispatch threading cumbersome

**Instead:** Start with single view function, refactor when complexity demands

```fsharp
// OVERKILL for calculator
let digitButton digit dispatch = button [ OnClick (fun _ -> dispatch (Digit digit)) ] [ str (string digit) ]
let operationButton op dispatch = button [ OnClick (fun _ -> dispatch (Operation op)) ] [ str (opToString op) ]
let displayPanel model = div [] [ input [ Value model.Display ] ]
let buttonGrid dispatch = div [] [ (* 20 lines of grid layout *) ]

// BETTER for simple calculator
let view model dispatch =
    div [ ClassName "calculator" ] [
        input [ ClassName "display"; Value model.Display; ReadOnly true ]
        div [ ClassName "buttons" ] [
            button [ OnClick (fun _ -> dispatch (Digit 7)) ] [ str "7" ]
            button [ OnClick (fun _ -> dispatch (Digit 8)) ] [ str "8" ]
            // ...inline is fine for ~20 buttons
        ]
    ]
```

## Build Order Implications

**Recommended Development Sequence:**

1. **Define Types First** (`Model` and `Msg`)
   - Establishes contracts for all other components
   - Compiler guides implementation

2. **Implement Init** (initial Model)
   - Simple, no dependencies

3. **Implement Update Logic**
   - Core business logic
   - Fully testable in isolation (pure functions)
   - Can write tests before view exists

4. **Implement View**
   - Depends on Model type (reads state)
   - Depends on Msg type (dispatches messages)
   - Can develop with mock/hardcoded Model

5. **Wire Program**
   - Connect init/update/view to Elmish runtime
   - Minimal boilerplate

6. **Add Styling**
   - CSS classes on view elements
   - Independent of MVU logic

**Why this order:**

- Types → Update → View follows data flow
- Update can be tested before view exists
- View can be developed with hardcoded Models
- Each step validates previous step's design

**Parallel work possible:**

- Update logic and View can be developed concurrently (both depend on Types)
- Tests can be written alongside Update
- CSS can be developed alongside View

## Testing Architecture

**Unit Testing Strategy:**

```fsharp
// tests/Tests.fs
module Tests

open Expecto
open Types
open State

[<Tests>]
let tests =
    testList "Calculator" [
        test "Clear resets display" {
            let model = { Display = "123"; Accumulator = None; Operation = None }
            let newModel, _ = update Clear model
            Expect.equal newModel.Display "0" "Display should be 0"
        }

        test "Digit pressed appends to display" {
            let model = { Display = "0"; Accumulator = None; Operation = None }
            let newModel, _ = update (DigitPressed 7) model
            Expect.equal newModel.Display "07" "Should append digit"
        }
    ]
```

**What to test:**

- Update function logic (pure functions, easy to test)
- Edge cases (divide by zero, overflow, etc.)
- State transitions (operation sequences)

**What NOT to test:**

- View rendering (tested manually or with browser tests)
- Elmish framework internals
- React bindings

## Deployment Architecture

**GitHub Pages Deployment:**

```
Build Process (CI/GitHub Actions)
    │
    ├─> dotnet restore
    ├─> npm install
    ├─> npm run build
    │
    ▼
public/ directory
    ├─> index.html
    ├─> bundle.js
    ├─> styles.css
    └─> (any assets)
    │
    ▼
gh-pages branch (or docs/ folder)
    │
    ▼
GitHub Pages serves static files
```

**Key architectural decision:**

Output must be **fully static** (no server-side code). This works perfectly for Fable+Elmish:
- Fable compiles F# → JavaScript
- Webpack bundles into `bundle.js`
- HTML loads bundle client-side
- No .NET runtime needed in browser

**Build artifacts:**

```
public/
├── index.html       # Entry point
├── bundle.js        # Compiled F# + dependencies
└── bundle.js.map    # Source maps (optional)
```

## Scalability Considerations

For a calculator app, scalability is not a concern. However, understanding MVU scaling patterns:

| Concern | Simple (Calculator) | Medium (Multi-page) | Large (Enterprise) |
|---------|---------------------|---------------------|-------------------|
| **State management** | Single Model record | Nested Model with sub-records | Separate Models per feature |
| **Msg handling** | Single Msg DU (10-20 cases) | Nested Msg DUs | Msg per feature module |
| **View composition** | Single view function | View functions per page | Component hierarchy |
| **Code splitting** | Single bundle | Route-based chunks | Feature-based lazy loading |
| **Testing** | Update function unit tests | Integration tests per feature | E2E + visual regression |

**CalTwo stays firmly in "Simple" category.**

## Technology Compatibility Notes

**Browser requirements:**

- Modern evergreen browsers (Chrome, Firefox, Safari, Edge)
- ES6+ JavaScript support (Babel can transpile if needed)
- React 16.8+ (for hooks, though Elmish doesn't use them)

**F# version:**

- F# 5.0+ recommended (better type inference, string interpolation)
- F# 7.0+ for latest language features

**Fable version:**

- Fable 3.x (current stable as of early 2025)
- Fable 4.x if available (check for breaking changes)

## Sources

Based on authoritative Elmish documentation and community best practices:

- Elmish official docs (elmish.github.io) - MVU pattern definition
- Fable documentation (fable.io) - F# to JS compilation
- Elm Architecture guide (guide.elm-lang.org) - Original MVU inspiration
- F# for Fun and Profit (fsharpforfunandprofit.com) - F# patterns
- Community sample apps (Fulma demos, SAFE stack examples)

**Confidence level:** HIGH - This architecture is well-established and documented. MVU pattern has been stable since 2016.
