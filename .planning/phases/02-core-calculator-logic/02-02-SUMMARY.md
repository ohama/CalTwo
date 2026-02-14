---
phase: 02-core-calculator-logic
plan: 02
subsystem: core-logic
tags: [fsharp, elmish, tdd, unit-testing, mocha, fable]

# Dependency graph
requires:
  - phase: 02-01
    provides: Test infrastructure and domain types
provides:
  - Complete calculator logic with all arithmetic operations
  - Comprehensive test suite (27 unit tests)
  - Pure update function with left-to-right evaluation
  - Division by zero error handling
  - Edge case coverage (decimals, negatives, large numbers)
affects: [02-03-ui-implementation]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "TDD RED-GREEN cycle: failing tests first, then implementation"
    - "Pure update function pattern: Model -> Msg -> Model"
    - "Service layer pattern: doMathOp, parseDisplay, formatResult"
    - "StartNew flag pattern: operator chaining without premature evaluation"

key-files:
  created: []
  modified:
    - tests/Tests.fs
    - src/Calculator.fs
    - src/App.fs

key-decisions:
  - "StartNew flag in Model tracks when next digit should replace (not append) display"
  - "OperatorPressed checks StartNew to distinguish operator chaining from left-to-right evaluation"
  - "Error state recovery on digit press (typing after 'Error' resets calculator)"
  - "formatResult uses simple string conversion (F#/Fable handles .0 trimming automatically)"

patterns-established:
  - "Test organization: group by feature (Digit Entry, Decimal Point, Basic Operations, etc.)"
  - "Model pipeline pattern: m1 |> App.update msg |> fst for sequential state transitions"
  - "Error recovery: Display = 'Error' triggers full reset on next digit"

# Metrics
duration: 3min
completed: 2026-02-14
---

# Phase 2 Plan 2: Calculator Logic Summary

**Pure calculator logic with 27 passing unit tests covering all operations, left-to-right evaluation, division by zero, and edge cases**

## Performance

- **Duration:** 3 min
- **Started:** 2026-02-14T08:36:30Z
- **Completed:** 2026-02-14T08:39:05Z
- **Tasks:** 2
- **Files modified:** 3

## Accomplishments
- Comprehensive test suite with 27 unit tests covering all calculator requirements
- Pure update function implementing all state transitions without side effects
- Left-to-right evaluation for chained operations (2+3+4 = 9)
- Division by zero error handling with automatic recovery
- Edge case coverage: decimals, negatives, large numbers, operator replacement

## Task Commits

Each task was committed atomically following TDD RED-GREEN cycle:

1. **Task 1: RED - Write comprehensive failing tests** - `520644b` (test)
2. **Task 2: GREEN - Implement calculator logic** - `1730772` (feat)

## Files Created/Modified
- `tests/Tests.fs` - 27 unit tests organized into 7 test groups (Digit Entry, Decimal Point, Basic Operations, Left-to-Right Evaluation, Division by Zero, Clear, Edge Cases)
- `src/Calculator.fs` - Added service functions (doMathOp, parseDisplay, formatResult) and StartNew flag to Model
- `src/App.fs` - Implemented complete update function handling all 5 message types

## Decisions Made

**StartNew flag for operator chaining:**
- When operator is pressed, StartNew=true signals next digit should replace display
- This enables operator replacement (5 + × 3 = 15) without premature evaluation
- Prevents left-to-right evaluation until second operand is actually entered

**Error recovery pattern:**
- Display = "Error" triggers on division by zero
- Any digit press after error resets calculator completely (Display, PendingOp, StartNew)
- User-friendly recovery without requiring explicit Clear

**Simple formatResult implementation:**
- F#/Fable's string conversion handles .0 trimming automatically in JS context
- No complex formatting logic needed for basic calculator display

## Deviations from Plan

None - plan executed exactly as written. TDD cycle followed precisely: RED (27 failing tests), GREEN (all tests passing).

## Issues Encountered

**One test initially failed (operator replacement):**
- Test expected: 5 + × 3 = 15 (second operator replaces first)
- Initial implementation computed: 5 + 5 × 3 = 30 (evaluated immediately)
- Root cause: OperatorPressed always evaluated pending operation
- Fix: Check StartNew flag - if true, just replace operator without evaluating
- Result: All 27 tests pass

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

**Ready for Phase 2 Plan 3 (UI Implementation):**
- Calculator logic fully tested and working
- All 13 requirements satisfied: INP-01, INP-02, INP-04, OPS-01-06, DSP-01-03
- Update function is pure (returns Model * Cmd, uses Cmd.none)
- No blockers or concerns

**What's available:**
- App.init() returns initial model
- App.update() handles all messages
- Model.Display contains string to show in UI
- Ready to wire up button click handlers in view function

---
*Phase: 02-core-calculator-logic*
*Completed: 2026-02-14*
