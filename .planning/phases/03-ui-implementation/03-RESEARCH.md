# Phase 3: UI Implementation - Research

**Researched:** 2026-02-14
**Domain:** Feliz UI styling, responsive calculator layout, keyboard event handling, F# string manipulation
**Confidence:** HIGH

## Summary

This research covers implementing a responsive calculator UI using Feliz (F# React DSL) with CSS Grid for button layout, inline styling for component-specific styles, keyboard event handling via React props, and string manipulation for Backspace functionality. The standard approach uses Feliz's type-safe inline styles via `prop.style` for component styling, CSS Grid with `repeat(4, 1fr)` for button layout, React's `onKeyDown` event handler via `prop.onKeyDown` for keyboard support, and F# string slicing for character deletion.

Key findings:
- Feliz provides type-safe inline styling through `prop.style` with full support for flexbox, grid, and responsive units
- CSS Grid with 4-column layout (`grid-template-columns: repeat(4, 1fr)`) is the standard pattern for calculator button grids
- Keyboard events are handled via `prop.onKeyDown` using React's `KeyboardEvent` with pattern matching on `e.key`
- Touch target size must be minimum 44x44px for accessibility (WCAG 2.1 AAA), with buttons 42-72px optimal for mobile
- Backspace functionality uses F# string slicing: `display.[0..display.Length-2]` to remove last character
- Responsive design uses CSS Grid with `fr` units (fluid) and media queries for mobile-first breakpoints
- Hover/active states are handled via CSS classes, not inline styles (pseudo-classes not supported inline)

**Primary recommendation:** Use Feliz inline styles for layout/spacing, external CSS file for hover/active states, CSS Grid for button layout, `prop.onKeyDown` for keyboard events, and F# string slicing for Backspace.

## Standard Stack

The established libraries/tools for Feliz UI implementation:

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Feliz | 2.9.0 | Type-safe React DSL | Already in project, provides `prop.style` for inline styling |
| React | 18.3.1 | UI library | Already in project, handles event system and rendering |
| CSS Grid | Native | Button layout | Browser-native, no dependencies, perfect for calculator grids |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| External CSS | N/A | Hover/active states | For pseudo-classes (`:hover`, `:active`) not supported inline |
| TypedCssClasses | Latest | Type-safe CSS classes | Optional: provides compile-time verification of CSS class names |
| CSS modules | N/A | Scoped styling | Optional: prevents class name conflicts in larger apps |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Feliz inline styles | External CSS only | Inline styles offer type safety and co-location, CSS offers pseudo-classes |
| CSS Grid | Flexbox | Grid is 2D (rows+columns), Flexbox is 1D (single axis), Grid better for calculator |
| prop.onKeyDown | Elmish subscriptions | Subscriptions add complexity, onKeyDown simpler for single-component keyboard handling |
| External CSS | Tailwind CSS | Tailwind requires build setup, plain CSS simpler for small project |
| String slicing | String.Remove() | Slicing is more idiomatic F#, Remove() is .NET-style |

**Installation:**
```bash
# No new packages needed - Feliz 2.9.0 already installed
# For optional TypedCssClasses:
dotnet add package Zanaptak.TypedCssClasses --version 1.0.0
```

## Architecture Patterns

### Recommended Project Structure
```
src/
├── Calculator.fs       # Domain types + service functions (existing)
├── App.fs              # init, update, view with UI layout (existing)
├── Main.fs             # Elmish program + HMR (existing)
└── styles.css          # Hover/active states (NEW - Phase 3)

index.html              # HTML entry point
```

### Pattern 1: Feliz Inline Styles for Layout
**What:** Use `prop.style` for component-specific layout, spacing, and colors
**When to use:** Always for non-pseudo-class styles (layout, colors, sizing)
**Example:**
```fsharp
// Source: Feliz documentation + Phase 1/2 existing code
Html.div [
    prop.style [
        style.display.grid
        style.gridTemplateColumns [
            length.fr 1; length.fr 1; length.fr 1; length.fr 1
        ]
        style.gap 10
        style.padding 20
    ]
    prop.children [ (* buttons *) ]
]
```

### Pattern 2: CSS Grid for Calculator Button Layout
**What:** 4-column grid using `fr` units for equal-width columns
**When to use:** Always for calculator button grids (standard pattern)
**Example:**
```fsharp
// Source: https://freshman.tech/css-grid-calculator/
// Buttons container
Html.div [
    prop.style [
        style.display.grid
        style.gridTemplateColumns (repeat(4, fr 1))  // 4 equal columns
        style.gap 10
    ]
    prop.children [
        // Buttons fill grid cells automatically left-to-right, top-to-bottom
        for d in 0..9 do
            Html.button [
                prop.text (string d)
                prop.onClick (fun _ -> dispatch (DigitPressed d))
            ]
    ]
]
```

### Pattern 3: External CSS for Hover/Active States
**What:** Use CSS file with class names for pseudo-classes (`:hover`, `:active`)
**When to use:** Always for hover/active/focus states (not supported by inline styles)
**Example:**
```css
/* Source: React button best practices */
.calc-button {
    border: 1px solid #ccc;
    background: #f0f0f0;
    transition: all 0.1s ease;
}

.calc-button:hover {
    background: #e0e0e0;
    transform: scale(1.05);
}

.calc-button:active {
    background: #d0d0d0;
    transform: scale(0.95);
}
```

```fsharp
// In App.fs
Html.button [
    prop.className "calc-button"
    prop.text "7"
    prop.onClick (fun _ -> dispatch (DigitPressed 7))
]
```

### Pattern 4: Keyboard Events via prop.onKeyDown
**What:** Handle keyboard input using React's `KeyboardEvent` with pattern matching
**When to use:** For global keyboard shortcuts (digits, operators, Enter, Escape, Backspace)
**Example:**
```fsharp
// Source: https://github.com/fable-compiler/samples-browser/blob/master/src/react-todomvc/React.TodoMVC.fs
let handleKeyDown (e: Browser.Types.KeyboardEvent) =
    match e.key with
    | "0" -> dispatch (DigitPressed 0)
    | "1" -> dispatch (DigitPressed 1)
    | "2" -> dispatch (DigitPressed 2)
    | "3" -> dispatch (DigitPressed 3)
    | "4" -> dispatch (DigitPressed 4)
    | "5" -> dispatch (DigitPressed 5)
    | "6" -> dispatch (DigitPressed 6)
    | "7" -> dispatch (DigitPressed 7)
    | "8" -> dispatch (DigitPressed 8)
    | "9" -> dispatch (DigitPressed 9)
    | "+" -> dispatch (OperatorPressed Add)
    | "-" -> dispatch (OperatorPressed Subtract)
    | "*" -> dispatch (OperatorPressed Multiply)
    | "/" -> dispatch (OperatorPressed Divide)
    | "Enter" -> dispatch EqualsPressed
    | "Escape" -> dispatch ClearPressed
    | "Backspace" -> dispatch BackspacePressed
    | "." -> dispatch DecimalPressed
    | _ -> ()

// Attach to document or container
Html.div [
    prop.onKeyDown handleKeyDown
    prop.tabIndex 0  // Make div focusable to receive keyboard events
    prop.children [ (* calculator UI *) ]
]
```

### Pattern 5: Backspace String Manipulation
**What:** Remove last character from display string using F# slicing
**When to use:** For Backspace functionality in calculator
**Example:**
```fsharp
// Source: F# string manipulation patterns
| BackspacePressed ->
    if model.Display.Length > 1 then
        { model with Display = model.Display.[0..model.Display.Length-2] }, Cmd.none
    else
        // If only one character, reset to "0"
        { model with Display = "0" }, Cmd.none
```

### Pattern 6: Responsive Grid with Media Queries
**What:** Use CSS media queries to adjust grid columns for mobile vs desktop
**When to use:** When calculator needs to adapt to different screen sizes
**Example:**
```css
/* Source: https://freshman.tech/css-grid-calculator/ + responsive design best practices */
/* Mobile-first approach */
.calculator-keys {
    display: grid;
    grid-template-columns: repeat(4, 1fr);  /* 4 columns on mobile */
    gap: 8px;
    padding: 16px;
}

/* Tablet and up - increase gap for better touch targets */
@media (min-width: 768px) {
    .calculator-keys {
        gap: 12px;
        padding: 20px;
    }
}

/* Desktop - can use smaller buttons with more spacing */
@media (min-width: 1920px) {
    .calculator-keys {
        gap: 16px;
        padding: 24px;
    }
}
```

**Alternative: Inline responsive styles using Feliz (less common)**
```fsharp
// Responsive units work inline, but media queries require CSS
Html.div [
    prop.style [
        style.display.grid
        style.gridTemplateColumns (repeat(4, fr 1))
        style.gap (length.vw 2)  // 2% of viewport width - scales automatically
        style.padding (length.vw 4)
    ]
]
```

### Pattern 7: Touch Target Sizing for Mobile
**What:** Ensure buttons meet 44x44px minimum for accessibility
**When to use:** Always for mobile-responsive calculator
**Example:**
```fsharp
// Source: WCAG 2.1 AAA guidelines + mobile button research
Html.button [
    prop.className "calc-button"
    prop.style [
        style.minHeight 44  // Minimum touch target
        style.minWidth 44
        style.fontSize 20
        style.padding 10
    ]
    prop.text "7"
]
```

### Anti-Patterns to Avoid
- **Inline styles for hover/active states:** CSS pseudo-classes don't work inline; use external CSS + className
- **Flexbox for 2D button grid:** Use CSS Grid instead; Flexbox is for 1D layouts
- **Hardcoded pixel values for responsive design:** Use `fr`, `%`, `vw`, `rem` units instead
- **String.Remove() for backspace:** Use F# slicing `str.[0..len-2]` for more idiomatic code
- **Global keyboard event subscriptions:** Use `prop.onKeyDown` on container instead (simpler, no cleanup needed)
- **Touch targets smaller than 44px:** Violates accessibility guidelines, causes mobile usability issues

## Don't Hand-Roll

Problems that look simple but have existing solutions:

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| CSS Grid layout | Manual positioning with absolute/relative | `grid-template-columns: repeat(4, 1fr)` | Grid handles responsiveness, alignment, spacing automatically |
| Hover/active states | JavaScript event listeners | CSS `:hover` and `:active` pseudo-classes | CSS is declarative, GPU-accelerated, no JS overhead |
| Keyboard event mapping | Manual keyCode checks | Pattern matching on `e.key` string | `keyCode` deprecated, `e.key` is standard and readable |
| Responsive design | JavaScript window.innerWidth checks | CSS media queries | Declarative, browser-optimized, no JS performance cost |
| Touch target sizing | Visual button size only | Separate visual size + padding for touch target | Accessibility compliance requires 44px minimum regardless of visual size |
| String character removal | Loop through chars and rebuild | F# string slicing `str.[0..len-2]` | Built-in, performant, idiomatic F# |

**Key insight:** CSS Grid was designed for 2D layouts like calculator grids. Using Flexbox or manual positioning adds complexity without benefit. Let the browser do the work.

## Common Pitfalls

### Pitfall 1: Inline Styles for Hover States
**What goes wrong:** Hover styles don't apply; buttons have no visual feedback on hover
**Why it happens:** CSS pseudo-classes (`:hover`, `:active`) cannot be defined in inline styles
**How to avoid:** Use external CSS file with class names for hover/active states; keep inline styles for layout/colors
**Warning signs:** `prop.style [ style.hover.backgroundColor "red" ]` doesn't exist in Feliz API

### Pitfall 2: Keyboard Events Don't Fire
**What goes wrong:** User presses keys but calculator doesn't respond
**Why it happens:** Container div not focusable; keyboard events only fire on focused elements
**How to avoid:** Add `prop.tabIndex 0` to container div or attach `onKeyDown` to body/document
**Warning signs:** Clicking calculator works but keyboard doesn't; no focus outline visible

### Pitfall 3: Backspace Deletes Entire Display
**What goes wrong:** Pressing Backspace clears all input instead of last character
**Why it happens:** Confused Backspace with Clear; or browser default Backspace (navigate back) triggered
**How to avoid:**
- Implement `BackspacePressed` message separate from `ClearPressed`
- Call `e.preventDefault()` in keyboard handler to prevent browser navigation
- Handle edge case: if `display.Length = 1`, reset to "0" instead of empty string
**Warning signs:** Display becomes empty string ""; browser navigates to previous page

### Pitfall 4: Buttons Too Small on Mobile
**What goes wrong:** Users miss buttons on touchscreen; low accuracy
**Why it happens:** Buttons designed for desktop mouse (24px) don't meet mobile touch target size (44px)
**How to avoid:** Use `minHeight 44` and `minWidth 44` in button styles; test on real mobile device (375px viewport)
**Warning signs:** Buttons < 42px; users report "hard to tap"; accessibility audit fails

### Pitfall 5: Grid Columns Don't Fit Mobile Screen
**What goes wrong:** Buttons overflow screen or are tiny; horizontal scrollbar appears
**Why it happens:** Container width not constrained; grid uses fixed pixel widths instead of `fr` units
**How to avoid:**
- Use `fr` units for fluid columns: `repeat(4, 1fr)` not `repeat(4, 60px)`
- Set max-width on calculator container: `style.maxWidth 400`
- Use viewport units: `style.width (length.vw 90)` for 90% of viewport
**Warning signs:** Horizontal scroll on mobile; buttons squeezed or cropped

### Pitfall 6: CSS File Not Imported
**What goes wrong:** Hover states don't work; classes not applied
**Why it happens:** Created `styles.css` but didn't import it in `Main.fs` using `importSideEffects`
**How to avoid:** Add `importSideEffects "./styles.css"` at top of `Main.fs` (before Elmish program initialization)
**Warning signs:** Browser DevTools shows no CSS file loaded; classes exist but have no styles

### Pitfall 7: String Index Out of Bounds on Backspace
**What goes wrong:** Backspace on empty display crashes app with index error
**Why it happens:** Attempting `display.[0..display.Length-2]` when `display.Length = 0` or `display.Length = 1`
**How to avoid:** Guard clause: `if model.Display.Length > 1 then ...`; handle empty/single-char cases separately
**Warning signs:** Runtime error "Index out of range"; crash when pressing Backspace on "0"

### Pitfall 8: Keyboard Shortcuts Conflict with Browser
**What goes wrong:** Pressing "/" focuses browser search bar instead of calculator
**Why it happens:** Browser default keyboard shortcuts not prevented
**How to avoid:** Call `e.preventDefault()` in `handleKeyDown` for keys you handle (/, +, -, *, Enter, Backspace)
**Warning signs:** "/" opens browser find; Backspace navigates back; keyboard input "stolen" by browser

## Code Examples

Verified patterns from official sources:

### Complete Calculator UI with Grid Layout
```fsharp
// Source: Feliz + CSS Grid calculator patterns
let view (model: Model) (dispatch: Msg -> unit) =
    Html.div [
        prop.style [ style.padding 20; style.maxWidth 400; style.margin.auto ]
        prop.onKeyDown (fun (e: Browser.Types.KeyboardEvent) ->
            e.preventDefault()  // Prevent browser defaults
            match e.key with
            | "0" -> dispatch (DigitPressed 0)
            | "1" -> dispatch (DigitPressed 1)
            | "2" -> dispatch (DigitPressed 2)
            | "3" -> dispatch (DigitPressed 3)
            | "4" -> dispatch (DigitPressed 4)
            | "5" -> dispatch (DigitPressed 5)
            | "6" -> dispatch (DigitPressed 6)
            | "7" -> dispatch (DigitPressed 7)
            | "8" -> dispatch (DigitPressed 8)
            | "9" -> dispatch (DigitPressed 9)
            | "+" -> dispatch (OperatorPressed Add)
            | "-" -> dispatch (OperatorPressed Subtract)
            | "*" -> dispatch (OperatorPressed Multiply)
            | "/" -> dispatch (OperatorPressed Divide)
            | "Enter" -> dispatch EqualsPressed
            | "Escape" -> dispatch ClearPressed
            | "Backspace" -> dispatch BackspacePressed
            | "." -> dispatch DecimalPressed
            | _ -> ()
        )
        prop.tabIndex 0  // Make focusable for keyboard events
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
                ]
                prop.text model.Display
            ]

            // Button grid
            Html.div [
                prop.style [
                    style.display.grid
                    style.gridTemplateColumns [
                        length.fr 1; length.fr 1; length.fr 1; length.fr 1
                    ]
                    style.gap 10
                ]
                prop.children [
                    // Numbers 7-9
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
                        prop.text "÷"
                        prop.onClick (fun _ -> dispatch (OperatorPressed Divide))
                    ]

                    // Numbers 4-6
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
                        prop.text "×"
                        prop.onClick (fun _ -> dispatch (OperatorPressed Multiply))
                    ]

                    // Numbers 1-3
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
                        prop.text "-"
                        prop.onClick (fun _ -> dispatch (OperatorPressed Subtract))
                    ]

                    // Bottom row: 0, decimal, equals, add
                    Html.button [
                        prop.className "calc-button"
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
                    Html.button [
                        prop.className "calc-button calc-operator"
                        prop.text "+"
                        prop.onClick (fun _ -> dispatch (OperatorPressed Add))
                    ]

                    // Clear button (full width)
                    Html.button [
                        prop.className "calc-button calc-clear"
                        prop.style [
                            style.gridColumn (4, 5)  // Span column 4
                        ]
                        prop.text "C"
                        prop.onClick (fun _ -> dispatch ClearPressed)
                    ]
                ]
            ]
        ]
    ]
```

### External CSS for Hover/Active States
```css
/* Source: React button best practices + accessibility guidelines */
/* styles.css */

.calc-button {
    min-height: 44px;
    min-width: 44px;
    padding: 12px;
    font-size: 18px;
    border: 1px solid #ccc;
    background: #f0f0f0;
    cursor: pointer;
    border-radius: 4px;
    transition: all 0.1s ease;
}

.calc-button:hover {
    background: #e0e0e0;
    transform: scale(1.05);
}

.calc-button:active {
    background: #d0d0d0;
    transform: scale(0.95);
}

.calc-operator {
    background: #ff9500;
    color: white;
}

.calc-operator:hover {
    background: #ff8000;
}

.calc-equals {
    background: #4cd964;
    color: white;
}

.calc-equals:hover {
    background: #3cc954;
}

.calc-clear {
    background: #ff3b30;
    color: white;
}

.calc-clear:hover {
    background: #ff2b20;
}

/* Mobile responsiveness */
@media (max-width: 375px) {
    .calc-button {
        min-height: 48px;  /* Larger touch targets on mobile */
        min-width: 48px;
        font-size: 16px;
    }
}
```

### Backspace Implementation
```fsharp
// Source: F# string manipulation + calculator backspace patterns
// In Calculator.fs - add new message
type Msg =
    | DigitPressed of int
    | DecimalPressed
    | OperatorPressed of MathOp
    | EqualsPressed
    | ClearPressed
    | BackspacePressed  // NEW

// In App.fs - update function
let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    // ... existing cases ...

    | BackspacePressed ->
        // Don't delete from "Error" or "0"
        if model.Display = "Error" || model.Display = "0" then
            model, Cmd.none
        // If only 1 character, reset to "0"
        elif model.Display.Length = 1 then
            { model with Display = "0" }, Cmd.none
        // Remove last character
        else
            { model with Display = model.Display.[0..model.Display.Length-2] }, Cmd.none
```

### Responsive Grid with Viewport Units
```fsharp
// Source: Responsive design best practices
// Alternative to media queries: use viewport units for fluid scaling
Html.div [
    prop.style [
        style.display.grid
        style.gridTemplateColumns [
            length.fr 1; length.fr 1; length.fr 1; length.fr 1
        ]
        style.gap (length.vw 2)  // 2% of viewport width
        style.padding (length.vw 4)
        style.width (length.vw 90)  // 90% of viewport width
        style.maxWidth 400  // Cap at 400px for large screens
    ]
]
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Flexbox for calculator grid | CSS Grid | 2017+ | 2D grid native support, simpler markup, auto-placement |
| keyCode for keyboard events | e.key string | 2019+ | keyCode deprecated, e.key more readable ("Enter" vs 13) |
| Fixed pixel button sizes | Min-height + padding for touch targets | 2018+ (WCAG 2.1) | Accessibility compliance, mobile usability |
| JavaScript for hover states | CSS :hover pseudo-class | Always standard | GPU-accelerated, declarative, no JS overhead |
| className string concatenation | classNames utility or Feliz prop.className | 2015+ React | Type-safe, conditional classes, no string manipulation |
| Inline styles for everything | Hybrid: inline for layout, CSS for pseudo-classes | 2020+ | Balance type safety with CSS features |
| window.innerWidth JS checks | CSS media queries | Always standard | Declarative, browser-optimized, SSR-compatible |
| String.Remove() | F# string slicing | F# idiomatic | More functional, concise syntax |

**Deprecated/outdated:**
- **keyCode property:** Use `e.key` instead (keyCode deprecated in DOM Level 3)
- **Fixed pixel grids:** Use `fr` units and viewport units for fluid layouts
- **Touch targets < 44px:** Fails WCAG 2.1 AAA accessibility (updated 2018)
- **Flexbox for 2D layouts:** CSS Grid is now standard for grids (2017+)
- **onKeyPress:** Deprecated in React, use onKeyDown instead

## Open Questions

Things that couldn't be fully resolved:

1. **TypedCssClasses necessity**
   - What we know: Provides compile-time verification of CSS class names from external stylesheets
   - What's unclear: Whether benefits justify setup complexity for small project with ~10 CSS classes
   - Recommendation: Skip for Phase 3; reconsider if CSS grows beyond 50 classes or team size increases

2. **Elmish subscriptions vs prop.onKeyDown for keyboard**
   - What we know: Subscriptions handle global events, prop.onKeyDown is component-local
   - What's unclear: Performance difference at scale; whether subscriptions needed for document-level keyboard events
   - Recommendation: Start with `prop.onKeyDown` on container div (simpler); only use subscriptions if need document-level events without focus

3. **CSS Grid browser support for target browsers**
   - What we know: CSS Grid supported in all modern browsers since 2017; IE11 requires -ms- prefixes
   - What's unclear: Whether CalTwo needs IE11 support (not specified in requirements)
   - Recommendation: Assume modern browsers only (Chrome, Firefox, Safari, Edge); no IE11 support needed

4. **Responsive breakpoints for tablet**
   - What we know: Requirements specify 375px (mobile) and 1920px (desktop)
   - What's unclear: Behavior at intermediate sizes (tablet 768px, laptop 1366px)
   - Recommendation: Use fluid grid with `fr` units - works at all sizes without explicit breakpoints

5. **Korean tutorial depth for CSS Grid**
   - What we know: Tutorial must explain Feliz view functions and CSS styling for beginners
   - What's unclear: How much CSS Grid theory to include vs practical examples only
   - Recommendation: Focus on practical calculator example with brief CSS Grid explanation; link to MDN for deeper learning

## Sources

### Primary (HIGH confidence)
- [Feliz Styles.fs](https://github.com/Zaid-Ajaj/Feliz/blob/master/Feliz/Styles.fs) - Type-safe inline styling API
- [CSS Grid Calculator Tutorial](https://freshman.tech/css-grid-calculator/) - Standard grid layout pattern
- [Fable TodoMVC Sample](https://github.com/fable-compiler/samples-browser/blob/master/src/react-todomvc/React.TodoMVC.fs) - Keyboard event handling pattern
- [WCAG 2.1 Target Size](https://www.w3.org/WAI/WCAG21/Understanding/target-size.html) - Accessibility requirements (44x44px minimum)
- [F# Strings Documentation](https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/strings) - String slicing syntax
- [Elmish Subscriptions](https://elmish.github.io/elmish/docs/subscription.html) - Event subscription patterns

### Secondary (MEDIUM confidence)
- [Modern CSS Layout Techniques 2025-2026](https://www.frontendtools.tech/blog/modern-css-layout-techniques-flexbox-grid-subgrid-2025) - Current CSS Grid best practices
- [Accessible Touch Target Sizes](https://www.smashingmagazine.com/2023/04/accessible-tap-target-sizes-rage-taps-clicks/) - Mobile button sizing research
- [Optimal Mobile Button Size](https://www.designmonks.co/blog/perfect-mobile-button-size) - 42-72px optimal range
- [TypedCssClasses](https://github.com/zanaptak/TypedCssClasses) - F# CSS type provider
- [React Keyboard Event Handling](https://www.kindacode.com/article/react-typescript-handling-keyboard-events) - onKeyDown patterns
- [CSS Grid Responsive Layouts](https://blog.shubhra.dev/css-grid-responsive-layouts-guide/) - Media query strategies

### Tertiary (LOW confidence)
- WebSearch: "Feliz prop.className documentation" - No official docs found, inferred from Fable.React patterns
- WebSearch: "calculator backspace implementation" - General patterns, not F#-specific
- WebSearch: "React button hover states" - Generic React, not Feliz-specific

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - Feliz 2.9.0 already in project, CSS Grid is browser-native standard
- Architecture: HIGH - CSS Grid calculator pattern well-documented, keyboard events standard React pattern
- Pitfalls: HIGH - Touch target sizes from WCAG spec, keyboard conflicts documented, string slicing is F# core
- Feliz hover states: MEDIUM - Inferred from React patterns + Feliz inline style limitations (no pseudo-classes)
- Responsive design: HIGH - CSS Grid + media queries are established standards

**Research date:** 2026-02-14
**Valid until:** 2026-04-14 (60 days - CSS Grid and React keyboard events are stable standards)

**Notes:**
- Feliz 2.9.0 inline styles do NOT support pseudo-classes (`:hover`, `:active`) - must use external CSS
- CSS Grid `repeat(4, 1fr)` is the canonical pattern for calculator button grids (4 equal columns)
- WCAG 2.1 AAA requires 44x44px minimum touch targets for accessibility compliance
- F# string slicing `str.[0..len-2]` is more idiomatic than `String.Remove(str, str.Length-1)`
- `e.key` (string) is modern standard, replacing deprecated `e.keyCode` (number)
