---
phase: 02-core-calculator-logic
plan: 01
subsystem: testing
tags: [fable, mocha, fsharp, tdd, elmish]

# Dependency graph
requires:
  - phase: 01-foundation-and-tooling
    provides: F# + Elmish + Vite tooling, development environment
provides:
  - Test infrastructure with Fable CLI and Fable.Mocha
  - Calculator domain types (Model, Msg, MathOp, MathResult)
  - Working npm test pipeline (F# to JS compilation + mocha execution)
affects: [02-core-calculator-logic, testing, calculator-logic]

# Tech tracking
tech-stack:
  added: [Fable CLI 4.29.0, Fable.Mocha 2.17.0, mocha 11.7.5]
  patterns: [Elmish Model-View-Update, Discriminated Unions for domain modeling]

key-files:
  created: [src/Calculator.fs, tests/Tests.fsproj, tests/Tests.fs]
  modified: [.config/dotnet-tools.json, package.json, src/App.fsproj, src/App.fs]

key-decisions:
  - "DigitPressed takes int (0-9) instead of individual DU cases per digit (simpler, less boilerplate)"
  - "PendingOp stores (MathOp * float) option for left-to-right evaluation pattern"
  - "MathResult discriminated union handles divide-by-zero gracefully"
  - "Fable CLI installed as dotnet tool instead of npm package (consistent with .NET workflow)"

patterns-established:
  - "Pattern 1: Discriminated unions for type-safe domain modeling (MathOp, MathResult, Msg)"
  - "Pattern 2: Test project references main App.fsproj via ProjectReference"
  - "Pattern 3: npm test pipeline: compile F# to JS with Fable, then run with mocha"

# Metrics
duration: 2.8min
completed: 2026-02-14
---

# Phase 2 Plan 01: Test Infrastructure & Domain Types Summary

**Test infrastructure with Fable CLI and Fable.Mocha, calculator domain types using discriminated unions, working npm test pipeline**

## Performance

- **Duration:** 2.8 min
- **Started:** 2026-02-14T17:30:52Z
- **Completed:** 2026-02-14T17:33:39Z
- **Tasks:** 2
- **Files modified:** 8

## Accomplishments
- Fable CLI installed as dotnet tool, test project compiles F# to JS successfully
- Calculator domain types defined with type-safe discriminated unions
- End-to-end test pipeline working: npm test compiles and runs tests with mocha

## Task Commits

Each task was committed atomically:

1. **Task 1: Create test infrastructure with Fable.Mocha** - `4d64f48` (chore)
2. **Task 2: Define calculator domain types and wire into project** - `a1ee8b4` (feat)

## Files Created/Modified
- `.config/dotnet-tools.json` - Added Fable 4.29.0 as dotnet tool
- `tests/Tests.fsproj` - F# test project with Fable.Mocha 2.17.0 and ProjectReference to App.fsproj
- `tests/Tests.fs` - Skeleton test file with smoke test for init function
- `package.json` - Added test:compile, test:run, and test scripts; mocha 11.7.5 dev dependency
- `src/Calculator.fs` - Calculator domain types (MathOp, MathResult, Msg, Model)
- `src/App.fsproj` - Include Calculator.fs before App.fs in compilation order
- `src/App.fs` - Use Calculator types, stub update function, display calculator UI

## Decisions Made

**Key architectural decisions:**

1. **DigitPressed takes int parameter (0-9)**: Chose `DigitPressed of int` instead of individual discriminated union cases like `Digit0 | Digit1 | ... | Digit9`. This reduces boilerplate while maintaining type safety (validation happens in update function). Aligns with RESEARCH.md Pattern 1.

2. **PendingOp uses (MathOp * float) option**: Stores the pending operation and first operand together. This enables left-to-right evaluation pattern from RESEARCH.md Pattern 3. None when no operation pending, Some when waiting for second operand.

3. **MathResult handles division by zero**: Discriminated union with `Success of float | DivideByZeroError` allows graceful error handling without exceptions. Aligns with F# functional error handling patterns.

4. **Fable CLI as dotnet tool**: Installed Fable 4.29.0 via dotnet tool manifest instead of npm package. This keeps .NET tooling consistent and allows version locking in dotnet-tools.json.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None - all tasks completed without issues. Test infrastructure compiled and ran successfully on first attempt.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

**Ready for TDD implementation (Plan 02-02):**
- Test infrastructure functional: `npm test` compiles F# to JS and runs with mocha
- Calculator types available for import in tests (Model, Msg, MathOp, MathResult)
- Stub update function in place, ready to be replaced with TDD implementation

**TDD can now begin:**
- Tests can reference Calculator types
- Red-Green-Refactor cycle ready
- Test compilation and execution pipeline verified

**No blockers or concerns.**

---
*Phase: 02-core-calculator-logic*
*Completed: 2026-02-14*
