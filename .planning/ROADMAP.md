# Roadmap: CalTwo

## Overview

CalTwo delivers a browser-based calculator with F# + Elmish, progressing from tooling foundation through pure MVU logic, UI implementation, automated testing, to GitHub Pages deployment. Each phase builds on the previous, mitigating critical pitfalls early (tool versioning, source maps, pure update functions) while ensuring complete test automation and continuous documentation in Korean.

## Phases

**Phase Numbering:**
- Integer phases (1, 2, 3): Planned milestone work
- Decimal phases (2.1, 2.2): Urgent insertions (marked with INSERTED)

Decimal phases appear between their surrounding integers in numeric order.

- [x] **Phase 1: Foundation & Tooling** - F# Elmish project with Vite, proper versioning, source maps
- [x] **Phase 2: Core Calculator Logic** - Pure MVU implementation with arithmetic operations and unit tests
- [x] **Phase 3: UI Implementation** - Responsive button grid, display, keyboard support
- [ ] **Phase 4: E2E Testing & CI** - Playwright browser automation running in GitHub Actions
- [ ] **Phase 5: Deployment & Polish** - GitHub Pages deployment with accessibility improvements

## Phase Details

### Phase 1: Foundation & Tooling
**Goal**: Developer can run F# Elmish app locally with HMR, proper versioning, and debuggable source maps
**Depends on**: Nothing (first phase)
**Requirements**: DEP-01, TUT-01, TUT-02, TUT-03
**Success Criteria** (what must be TRUE):
  1. Developer runs `npm run dev` and sees "Hello CalTwo" in browser at localhost
  2. Developer edits F# code and sees changes in browser without refresh (HMR)
  3. Developer opens browser DevTools and sees F# source files (not minified JS)
  4. Project has dotnet-tools.json, global.json with pinned versions
  5. Korean tutorial in tutorial/phase-01.md explains setup step-by-step for beginners
**Plans**: 3 plans

Plans:
- [x] 01-01-PLAN.md — Create project structure and configuration files
- [x] 01-02-PLAN.md — Install dependencies and verify dev server
- [x] 01-03-PLAN.md — Human verification of HMR and source maps

### Phase 2: Core Calculator Logic
**Goal**: Calculator arithmetic operations work correctly with pure, testable update function
**Depends on**: Phase 1
**Requirements**: INP-01, INP-02, INP-04, OPS-01, OPS-02, OPS-03, OPS-04, OPS-05, OPS-06, DSP-01, DSP-02, DSP-03, TST-01, TST-02, TUT-01, TUT-02, TUT-03
**Success Criteria** (what must be TRUE):
  1. User can input digits 0-9 and decimal point via Model state changes
  2. User can perform +, -, ×, ÷ operations with left-to-right evaluation
  3. User can press equals and see correct calculation result
  4. User sees "Error" when dividing by zero
  5. User can clear all input and start fresh calculation
  6. Update function is pure (unit tests pass without side effects)
  7. All edge cases tested (multiple decimals, divide-by-zero, negative results)
  8. Korean tutorial in tutorial/phase-02.md explains MVU pattern and arithmetic logic
**Plans**: 3 plans

Plans:
- [x] 02-01-PLAN.md — Test infrastructure (Fable.Mocha) and calculator type definitions
- [x] 02-02-PLAN.md — TDD calculator logic (all operations, edge cases)
- [x] 02-03-PLAN.md — Korean tutorial and minimal view wiring

### Phase 3: UI Implementation
**Goal**: Users interact with clean, responsive calculator UI via buttons and keyboard
**Depends on**: Phase 2
**Requirements**: UI-01, UI-02, UI-03, INP-03, TUT-01, TUT-02, TUT-03
**Success Criteria** (what must be TRUE):
  1. User sees button grid with 0-9, operators (+, -, ×, ÷), decimal, clear, equals
  2. User sees display area showing current input and calculation results
  3. User clicks any button and sees immediate visual feedback (hover/active states)
  4. User opens calculator on mobile device and all buttons are tappable (responsive)
  5. User presses keyboard keys (0-9, +, -, *, /, Enter, Escape) and calculator responds
  6. User presses Backspace and last input character is deleted
  7. Calculator layout works on desktop (1920px) and mobile (375px) screens
  8. Korean tutorial in tutorial/phase-03.md explains Feliz view functions and CSS styling
**Plans**: 3 plans

Plans:
- [x] 03-01-PLAN.md — BackspacePressed logic + CSS Grid UI + keyboard events
- [x] 03-02-PLAN.md — Korean tutorial for Phase 3
- [x] 03-03-PLAN.md — Human verification of UI layout and responsiveness

### Phase 4: E2E Testing & CI
**Goal**: Browser automation tests verify calculator behavior without human intervention in CI
**Depends on**: Phase 3
**Requirements**: TST-03, TST-04, TUT-01, TUT-02, TUT-03
**Success Criteria** (what must be TRUE):
  1. Playwright test clicks "2 + 3 =" in real browser and asserts display shows "5"
  2. Playwright test verifies divide-by-zero shows "Error"
  3. Playwright test runs headless (no visible browser window)
  4. GitHub Actions workflow runs all tests (unit + E2E) on every push
  5. CI passes without any manual steps (fully automated)
  6. Test failures show clear error messages with screenshots
  7. Korean tutorial in tutorial/phase-04.md explains Playwright setup and CI configuration
**Plans**: 3 plans

Plans:
- [ ] 04-01-PLAN.md — Playwright setup, data-testid, and E2E test suite
- [ ] 04-02-PLAN.md — GitHub Actions CI workflow
- [ ] 04-03-PLAN.md — Korean tutorial for Playwright and CI

### Phase 5: Deployment & Polish
**Goal**: CalTwo is publicly accessible at GitHub Pages URL with accessibility improvements
**Depends on**: Phase 4
**Requirements**: DEP-02, DEP-03, TUT-01, TUT-02, TUT-03
**Success Criteria** (what must be TRUE):
  1. Production build succeeds and generates static files (HTML + JS + CSS)
  2. GitHub Actions deploys to Pages on push to main (automated)
  3. User visits GitHub Pages URL and calculator loads correctly (no 404s)
  4. User navigates with keyboard (Tab, Enter) and sees visible focus states
  5. Screen reader announces button labels and display values (ARIA)
  6. Calculator works at both localhost:3000/ and github.io/CalTwo/ (base path configured)
  7. README.md explains setup, development, testing, and deployment for new contributors
  8. Korean tutorial in tutorial/phase-05.md explains Vite build config and GitHub Pages deployment
**Plans**: TBD

Plans:
- TBD (will be created during plan-phase)

## Progress

**Execution Order:**
Phases execute in numeric order: 1 → 2 → 3 → 4 → 5

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. Foundation & Tooling | 3/3 | Complete | 2026-02-14 |
| 2. Core Calculator Logic | 3/3 | Complete | 2026-02-14 |
| 3. UI Implementation | 3/3 | Complete | 2026-02-14 |
| 4. E2E Testing & CI | 0/3 | Not started | - |
| 5. Deployment & Polish | 0/TBD | Not started | - |

---

*Roadmap created: 2026-02-14*
*Last updated: 2026-02-14 after Phase 4 planning*
