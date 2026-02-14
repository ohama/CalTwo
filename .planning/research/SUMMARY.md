# Project Research Summary

**Project:** CalTwo - F# Elmish Web Calculator
**Domain:** F# web application using Fable + Elmish + React
**Researched:** 2026-02-14
**Confidence:** HIGH

## Executive Summary

CalTwo is a basic web calculator built with F# using the Model-View-Update (MVU) pattern via Elmish. Expert practitioners build this type of application using a minimal, modern F# web stack: Fable 4.x for F#-to-JavaScript transpilation, Elmish 5.x for MVU architecture, Feliz 2.9 for React bindings, and Vite 7.x for build tooling. This stack represents the current standard as of 2026, replacing older approaches like SAFE Stack (too heavy for simple apps) and Fable.React (less maintained than Feliz).

The recommended approach prioritizes simplicity over frameworks. Start with core arithmetic operations and a clean button-grid UI, establish pure update functions from day one, and configure deployment tooling upfront to avoid the critical GitHub Pages base path pitfall. The MVU architecture provides excellent structure for a calculator: Model holds display/accumulator state, Msg represents user actions (digit presses, operations), and update functions remain pure and testable. Testing uses Fable.Mocha with headless browser support for CI automation.

Key risks include tool version mismatches (Fable/dotnet-fable/NuGet versions must align), source map configuration (essential for debugging production issues), and GitHub Pages deployment path issues (requires Vite base path configuration). Mitigation strategies are well-documented: use dotnet-tools.json for version pinning, configure source maps in both Fable and Vite, and test production builds locally before deployment. The MVU pattern's biggest pitfall is impure update functions - keeping update logic pure (no side effects) is non-negotiable and enforced through unit testing.

## Key Findings

### Recommended Stack

The minimal viable modern F# web stack for 2026 consists of .NET 10 with F# 10, Fable 4.28.0+ for transpilation, Elmish 5.0.2 for MVU architecture, Feliz 2.9.0 for React bindings, and Vite 7.3.x as the build tool. This stack avoids unnecessary complexity - no SAFE Stack (includes unneeded backend), no Elmish Land (overkill for simple apps), no Fable.React (superseded by Feliz).

**Core technologies:**
- **Fable 4.28.0+**: F# to JavaScript transpiler — mature, actively maintained, standard for F# web apps
- **Elmish 5.0.2**: MVU architecture library — de facto standard for F# MVU pattern, provides pure functional state management
- **Feliz 2.9.0**: React bindings for F# — officially recommended over Fable.React, type-safe CSS, better hooks support
- **Vite 7.3.x + vite-plugin-fable**: Build tool and dev server — modern replacement for Webpack, fast HMR, keeps transpiled output in memory
- **Fable.Mocha 2.17.0**: Testing framework — F#-first API, runs in browser or headless for CI
- **GitHub Pages + Actions**: Deployment — free static hosting with automated CI/CD pipeline

**Critical version dependencies:**
- .NET SDK 10.0 required for F# 10 support
- Femto tool for npm/NuGet dependency synchronization
- React 18.x as peer dependency for Feliz

### Expected Features

Research shows basic calculator users expect standard functionality as table stakes: four operations (+, -, ×, ÷), number input (0-9), decimal point, display area, clear/reset, equals button, and error handling for divide-by-zero. Mobile-responsive design is non-negotiable in 2026. Differentiators that add value include keyboard support (high value, medium effort), backspace/delete for editing, calculation history, copy-to-clipboard, and dark mode theming.

**Must have (table stakes):**
- Four basic operations (+, -, ×, ÷) — core calculator purpose
- Number input buttons (0-9) and decimal point — expected UI pattern
- Display area showing current input and result — required for usability
- Clear/reset button and equals execution — standard calculator functions
- Error handling for divide by zero — users will test this
- Responsive layout for mobile and desktop — mobile traffic dominates 2026

**Should have (competitive):**
- Keyboard support — power users value typing over clicking
- Backspace/delete key — more forgiving than full clear
- Visual feedback on button press — expected interaction polish
- Clean minimalist design — reduces cognitive load
- Accessibility (ARIA labels, focus states) — inclusive design

**Defer (v2+):**
- Calculation history — nice-to-have but not essential for v1
- Memory functions (M+, M-, MR, MC) — traditional but rarely used in web calculators
- Dynamic calculation preview — requires complex state management
- Dark mode theme toggle — polish feature after core UX validated
- Copy result to clipboard — easy enhancement after core works

**Anti-features (explicitly avoid):**
- Scientific functions (sin, cos, log) — scope creep beyond "basic" calculator
- Currency/unit conversion — different domain entirely
- Multiple calculator modes — confusing for basic use case
- Over-engineering state management — simple state needs, keep flat Model

### Architecture Approach

F# Elmish applications follow the Model-View-Update (MVU) pattern with unidirectional data flow. The architecture centers on a single immutable Model record containing all application state, a discriminated union Msg type representing all possible user actions, a pure update function transforming (Msg, Model) to (Model, Cmd), and a stateless view function rendering Model as React elements. The Elmish runtime orchestrates the MVU loop: user interactions dispatch Msg values, update function produces new Model, view re-renders with updated state.

**Major components:**
1. **Model (State)** — Single immutable record with Display, Accumulator, and PendingOperation fields. No mutable state, single source of truth.
2. **Msg (Actions)** — Discriminated union cases like DigitPressed, OperationPressed, EqualsPressed, Clear. Exhaustive pattern matching enforced by compiler.
3. **Update (Logic)** — Pure function applying messages to model. No side effects, fully testable. Returns new Model + Cmd for async operations (not needed for basic calculator).
4. **View (UI)** — Stateless rendering using Feliz React bindings. Reads Model, dispatches Msg on events. No local component state.
5. **Build Pipeline** — F# source → Fable compiler → JavaScript modules → Vite bundler → static bundle (HTML + JS + CSS) → GitHub Pages

**Key architectural patterns:**
- Pure update functions (no side effects, same input produces same output)
- Single Model type with nested records for organization
- Exhaustive Msg matching (compiler enforces handling all cases)
- View as function of Model (no component-level state)
- Cmd for side effects (not needed for calculator - all synchronous)

**File structure recommendation:**
For a simple calculator, start with single `App.fs` containing Types, init, update, and view. Split into separate files (Types.fs, State.fs, View.fs) only if complexity grows. Avoid premature modularization.

### Critical Pitfalls

Research identified 14 pitfalls across critical/moderate/minor severity. The top 5 critical pitfalls that can cause rewrites or deployment failures:

1. **Tool Version Mismatch (Critical)** — Fable, .NET SDK, and NuGet package versions must align precisely. Build works locally but fails in CI if versions drift. Prevention: use dotnet-tools.json for local tool manifest, global.json for SDK version, verify Fable.Core NuGet matches dotnet-fable tool version. Address in Phase 1 (Project Setup).

2. **Missing Source Maps in Production (Critical)** — JavaScript errors show minified code instead of F# source, making debugging impossible. Source maps require explicit configuration in both Fable and Vite, with pipeline (F# → Fable → Babel → Vite) providing multiple breakage points. Prevention: enable sourceMaps in fableconfig.json, set build.sourcemap in vite.config.js, verify F# files appear in browser DevTools. Address in Phase 1, verify in Phase 3 (Testing).

3. **Impure Update Functions (Critical)** — Side effects (localStorage, random, I/O) placed directly in update function instead of Cmd pattern. Breaks testability and violates MVU architectural guarantees. Prevention: keep update pure (msg, model) → (model, Cmd), move side effects to Cmd.OfFunc/OfAsync, unit test update with simple assertions. Address in Phase 2 (Core Logic) with architecture review checkpoint.

4. **npm/NuGet Dependency Mismatch (Critical)** — Install Fable binding via NuGet but forget corresponding npm package (e.g., install Fable.React but not react/react-dom). Runtime errors: "Cannot find module". Prevention: use Femto to auto-install npm packages matching NuGet bindings, verify package.json after adding NuGet packages. Address in Phase 1.

5. **Headless Testing Misconfiguration for CI (Critical)** — Tests pass locally in browser but fail in GitHub Actions because headless browser not configured. Required for "automated testing" goal. Prevention: choose test runner upfront (Vitest or Fable.Mocha + Puppeteer), add setup-chrome action to GitHub Actions, test CI early. Address in Phase 3 (Testing Setup).

6. **GitHub Pages Base Path Issues (Critical)** — App works at localhost:3000/ but fails on GitHub Pages at username.github.io/repo-name/ due to incorrect asset paths. Blank white page with 404s for all assets. Prevention: configure Vite base path in vite.config.js (base: '/CalTwo/'), use relative paths, test production build locally (npm run build && npm run preview). Address in Phase 4 (Deployment).

**Moderate pitfalls:**
- Parent-child component state coupling (acceptable for simple calculator)
- Forgetting Fable.Core limitations (avoid .NET libraries without Fable support)
- Subscription cleanup (not applicable - no timers/WebSockets in calculator)
- CSS not bundled correctly (import CSS in F# entry point or place in public/)

## Implications for Roadmap

Based on research, recommended 4-phase structure that addresses dependencies, mitigates critical pitfalls early, and delivers incrementally:

### Phase 1: Foundation & Tooling Setup
**Rationale:** All 6 critical pitfalls require upfront configuration. Establishing tooling, versions, and build pipeline before writing code prevents rework. Tool mismatches (Pitfall 1) block all development. Source maps (Pitfall 2) must be configured before debugging issues arise. npm/NuGet sync (Pitfall 4) needed before adding packages.

**Delivers:**
- Working dev environment with Vite dev server
- Proper tool versioning (dotnet-tools.json, global.json)
- Source maps configured and verified
- Femto installed for dependency sync
- HMR enabled for fast iteration
- Git repository initialized with correct .gitignore
- Basic project structure (src/, tests/, public/)

**Addresses (from STACK.md):**
- .NET 10 SDK installation
- Fable 4.28.0+ setup via local tools
- Vite 7.x + vite-plugin-fable configuration
- Elmish 5.0.2 + Feliz 2.9.0 packages
- CSS bundling strategy

**Avoids (from PITFALLS.md):**
- Pitfall 1: Tool Version Mismatch
- Pitfall 2: Source Maps configuration
- Pitfall 4: npm/NuGet sync
- Pitfall 10: CSS bundling
- Pitfall 13: .fsproj not committed
- Pitfall 14: HMR state loss

**Research flag:** Standard patterns - Fable/Elmish setup well-documented, skip deeper research.

---

### Phase 2: Core Calculator Logic (MVU)
**Rationale:** Establish pure MVU pattern early before complexity grows. Update function must be pure from the start - retrofitting purity later is painful (Pitfall 3). This phase implements table-stakes features identified in FEATURES.md without UI polish. Pure functions enable testing before view exists.

**Delivers:**
- Model type (Display, Accumulator, PendingOperation)
- Msg discriminated union (DigitPressed, OperationPressed, EqualsPressed, Clear)
- Pure update function with exhaustive pattern matching
- init function returning initial state
- Basic arithmetic logic (left-to-right evaluation)
- Error handling for divide by zero
- Unit tests for update function

**Addresses (from FEATURES.md):**
- Four basic operations (+, -, ×, ÷)
- Number input logic (0-9, decimal point)
- Clear/reset functionality
- Equals execution
- Error handling (divide by zero)

**Implements (from ARCHITECTURE.md):**
- Single Model record (immutable state)
- Discriminated union Msg type
- Pure update function pattern
- No side effects - all logic synchronous

**Avoids (from PITFALLS.md):**
- Pitfall 3: Impure update functions (enforce with architecture review + unit tests)
- Pitfall 7: Parent-child coupling (accept for simple calculator)

**Research flag:** Standard patterns - MVU calculator is textbook example, skip deeper research.

---

### Phase 3: UI & Testing
**Rationale:** View depends on Model/Msg types from Phase 2. Testing configured now to catch issues before deployment phase. Headless testing (Pitfall 5) must work before CI/CD pipeline. Responsive design is table stakes (FEATURES.md) but independent of core logic.

**Delivers:**
- View function using Feliz React bindings
- Button grid layout (0-9, operators, decimal, clear, equals)
- Display area showing current input/result
- Responsive CSS for mobile and desktop
- Visual feedback (hover states, button press)
- Fable.Mocha test suite with headless runner
- GitHub Actions workflow running tests in CI
- Keyboard support (differentiator from FEATURES.md)

**Addresses (from FEATURES.md):**
- Number input buttons (0-9)
- Operator buttons (+, -, ×, ÷)
- Display area
- Clear and equals buttons
- Responsive layout
- Visual button feedback
- Keyboard support (high-value differentiator)

**Implements (from ARCHITECTURE.md):**
- View as function of Model (stateless rendering)
- Feliz React bindings (type-safe elements)
- Event handlers dispatching Msg
- Program.mkSimple wiring (init, update, view)

**Avoids (from PITFALLS.md):**
- Pitfall 5: Headless testing for CI (configure Fable.Mocha + Puppeteer or Vitest)
- Pitfall 2: Verify source maps work in DevTools
- Pitfall 11: Document Fable watch mode workflow

**Research flag:** Standard patterns - calculator UI and Fable.Mocha testing well-documented, skip deeper research.

---

### Phase 4: Deployment & Polish
**Rationale:** GitHub Pages base path (Pitfall 6) is deployment-critical and must be configured before first deploy. CSS bundling (Pitfall 10) verified in production build. Polish features (dark mode, copy-to-clipboard) defer to v2 per FEATURES.md recommendations.

**Delivers:**
- Vite base path configured for GitHub Pages
- GitHub Actions deployment workflow
- Production build tested locally
- Deployed to GitHub Pages
- Documentation (README with setup/dev/deploy instructions)
- Accessibility improvements (ARIA labels, focus states)
- Error message polish

**Addresses (from FEATURES.md):**
- Deployment to GitHub Pages (project requirement)
- Clean minimalist design
- Accessibility (ARIA labels, keyboard navigation)
- Better error messages

**Implements (from STACK.md):**
- GitHub Actions CI/CD pipeline
- peaceiris/actions-gh-pages deployment action
- Vite production build optimization

**Avoids (from PITFALLS.md):**
- Pitfall 6: GitHub Pages base path (configure Vite base before deploy)
- Pitfall 10: CSS bundling verification
- Test production build locally (npm run build && npm run preview)

**Research flag:** Standard patterns - GitHub Pages SPA deployment well-documented, skip deeper research.

---

### Phase Ordering Rationale

**Dependency-driven ordering:**
- Phase 1 before all others: Tooling enables development
- Phase 2 before Phase 3: View depends on Model/Msg types
- Phase 3 before Phase 4: Test suite must pass before deployment
- Phase 4 last: Deployment validates entire stack

**Pitfall mitigation alignment:**
- Critical pitfalls 1, 2, 4 addressed in Phase 1 (foundation)
- Critical pitfall 3 addressed in Phase 2 (core logic)
- Critical pitfall 5 addressed in Phase 3 (testing)
- Critical pitfall 6 addressed in Phase 4 (deployment)

**Incremental value delivery:**
- Phase 1: Developer can run app locally
- Phase 2: Calculator logic works (testable without UI)
- Phase 3: Functional UI + automated tests
- Phase 4: Public deployment + documentation

**Architectural pattern enforcement:**
- Pure update functions established in Phase 2 (before view complexity)
- Single Model type prevents premature component splitting
- Testing validates MVU purity guarantees
- Build/deploy verifies entire transpilation pipeline

### Research Flags

**Phases with standard patterns (skip research-phase):**
- **Phase 1:** Fable + Elmish + Vite setup is well-documented in official docs and community templates
- **Phase 2:** MVU pattern for calculator is textbook example, straightforward implementation
- **Phase 3:** Fable.Mocha testing and calculator UI patterns well-established
- **Phase 4:** GitHub Pages SPA deployment is standard practice with clear documentation

**No phases require deeper research.** All components use mature, well-documented technologies with official guides and community examples. Calculator domain is simple with no niche integrations or experimental patterns.

**Future phase considerations (post-v1):**
- If adding calculation history: research localStorage persistence patterns in Elmish (Cmd side effects)
- If adding advanced keyboard shortcuts: research subscription cleanup patterns (Elmish subscriptions)
- If adding copy-to-clipboard: research Clipboard API interop with Fable

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Stack | HIGH | All versions verified from official sources (NuGet, npm). Vite 7.x + Fable 4.x is current 2026 standard. Feliz 2.9.0 explicitly recommended over Fable.React. |
| Features | HIGH | Calculator UX patterns well-established from NN/G, UXPin research. Table stakes vs differentiators clearly defined from 2026 best practices. |
| Architecture | HIGH | MVU pattern is mature (since 2016), well-documented in Elmish official docs. Single-file App.fs approach validated for simple apps. |
| Pitfalls | HIGH | Sourced from Maxime Mangel's Elmish tips, Fable GitHub issues, community blog posts. All pitfalls have documented prevention strategies. |

**Overall confidence:** HIGH

All four research areas based on authoritative sources: official documentation (Elmish, Fable, Vite), verified NuGet/npm package versions, established community best practices, and real-world project experience documented in blogs/GitHub. No reliance on speculation or single-source claims.

### Gaps to Address

**Minimal gaps identified due to simple domain:**

1. **Exact order of operations model:** Research didn't definitively specify whether basic calculator should use strict left-to-right evaluation vs PEMDAS. Community examples show both approaches.
   - **Resolution:** Default to left-to-right (simpler, matches most basic calculators). Document clearly in update function. User can refine if needed.

2. **Decimal point handling edge cases:** How to handle multiple decimal points in same number, leading zeros, etc.
   - **Resolution:** Standard validation patterns exist. Implement in Phase 2, unit test edge cases. Not a research gap - just implementation detail.

3. **Keyboard shortcut mappings:** Which keys for which operations (e.g., * vs x for multiply, / vs ÷ for divide).
   - **Resolution:** Use standard mappings (+, -, *, /, Enter for equals, Escape for clear). Test in Phase 3. Minor UX decision, not architectural.

**No architectural or tooling gaps.** Research provided clear recommendations for stack, patterns, and pitfall avoidance. Simple calculator domain has well-established conventions.

## Sources

### Primary (HIGH confidence)

**Stack:**
- Fable Official Docs (fable.io) — transpiler configuration, version compatibility
- Elmish Official Docs (elmish.github.io) — MVU pattern, architecture guidelines
- NuGet package pages — verified versions for Fable.Elmish 5.0.2, Feliz 2.9.0, Fable.Mocha 2.17.0
- Vite Official Docs (vite.dev) — build configuration, deployment
- vite-plugin-fable Docs (fable.io/vite-plugin-fable) — Vite integration

**Features:**
- Nielsen Norman Group (nngroup.com) — calculator UI best practices, design recommendations
- UXPin blog — calculator design patterns
- WCAG 2.1 (w3.org) — accessibility standards for keyboard support, ARIA labels

**Architecture:**
- Elmish GitHub repository — MVU pattern examples, subscription/Cmd documentation
- Elm Architecture Guide (guide.elm-lang.org) — original MVU pattern inspiration
- F# for Fun and Profit — F# patterns, discriminated unions, record types

**Pitfalls:**
- Maxime Mangel's Elmish Tips (Medium) — practical pitfalls from experienced Elmish developer
- Fable GitHub issues (#2166 source maps, #123 Cmd side effects) — known issues and resolutions
- Femto GitHub (Zaid-Ajaj/Femto) — npm/NuGet dependency sync tool
- Elmish.HMR docs — hot module replacement setup

### Secondary (MEDIUM confidence)

**Stack:**
- SAFE Stack Docs — alternative full-stack approach (evaluated and rejected as too heavy)
- Compositional IT blog — React component patterns in F#, practical guidance
- Amplifying F# podcast — Vite plugin context, ecosystem trends

**Features:**
- Muzli design inspiration — 60+ calculator UI patterns for 2026
- Web calculator best practices (Calconic blog) — feature prioritization
- Medium calculator design articles — UX insights, user expectations

**Pitfalls:**
- Jordan Marr blog — unit testing Fable apps with .NET
- Nathan Fox blog — testing Elmish React components
- FableStarter template (GitHub) — Vitest integration example
- CompositionalIT fable-debugging repository — debugging workflows

### Tertiary (LOW confidence, not relied upon)

None. Research avoided speculation and single-source claims. All findings verified across multiple authoritative sources or official documentation.

---

*Research completed: 2026-02-14*
*Ready for roadmap: yes*
