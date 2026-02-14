# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-02-14)

**Core value:** 사칙연산이 정확하게 동작하는 계산기를 브라우저에서 바로 사용할 수 있어야 한다.
**Current focus:** Phase 5 - Deployment & Polish (In Progress)

## Current Position

Phase: 5 of 5 (Deployment & Polish)
Plan: 3 of 3 in current phase
Status: Phase complete
Last activity: 2026-02-14 — Completed 05-03-PLAN.md

Progress: [██████████████] 100%

## Performance Metrics

**Velocity:**
- Total plans completed: 15
- Average duration: 2.9 min
- Total execution time: 0.79 hours

**By Phase:**

| Phase | Plans | Total | Avg/Plan |
|-------|-------|-------|----------|
| 01-foundation-and-tooling | 3 | 11.5min | 3.8min |
| 02-core-calculator-logic | 3 | 9.8min | 3.3min |
| 03-ui-implementation | 3 | 8min | 2.7min |
| 04-e2e-testing-and-ci | 3 | 8min | 2.7min |
| 05-deployment-and-polish | 3 | 8.3min | 2.8min |

**Recent Trend:**
- Last 5 plans: 4min, 4min, 2.3min, 4min, 2min
- Trend: Consistently fast execution (under 4min per plan)

*Updated after each plan completion*

## Accumulated Context

### Decisions

Decisions are logged in PROJECT.md Key Decisions table.
Recent decisions affecting current work:

- Phase 1: F# + Elmish selected per user specification
- Phase 1: GitHub Pages chosen for deployment (free, static hosting)
- 01-01: React 18.3.1 instead of React 19 (Feliz 2.x compatibility)
- 01-01: Empty dotnet-tools.json (vite-plugin-fable manages Fable internally)
- 01-01: Plugin order fable() before react() (transpilation order critical)
- 01-01: Elmish.HMR opened last (shadows Program.run for HMR)
- 01-02: vite-plugin-fable 0.2.0 (stable version with Vite 7.x compatibility)
- 01-02: Use 'fsproj' config key not 'project' (plugin recognizes fsproj only)
- 01-02: Accept non-fatal plugin warnings (all functionality works despite warnings)
- 02-01: DigitPressed of int (0-9) instead of individual DU cases (simpler, less boilerplate)
- 02-01: PendingOp stores (MathOp * float) option for left-to-right evaluation
- 02-01: MathResult discriminated union handles divide-by-zero gracefully
- 02-01: Fable CLI as dotnet tool (consistent with .NET workflow)
- 02-02: StartNew flag in Model tracks when next digit should replace display (enables operator chaining)
- 02-02: OperatorPressed checks StartNew to distinguish operator replacement from evaluation
- 02-02: Error state recovery on digit press (typing after 'Error' resets calculator)
- 02-02: formatResult uses simple string conversion (F#/Fable handles .0 trimming)
- 02-03: Tutorial follows phase-01.md style with 851 lines of comprehensive Korean content
- 02-03: Temporary inline buttons for testing (Phase 3 will replace with grid layout)
- 02-03: Terminal-style display (green-on-black) for clear state visualization
- 03-01: Backspace on single character resets to '0' (never empty display)
- 03-01: Backspace on Error state is no-op (user must type digit to recover)
- 03-01: Use style.custom() for CSS Grid properties not in Feliz typed API
- 03-01: Prevent default for handled keys only (don't block Tab, etc.)
- 03-02: Follow same tutorial style and depth as phase-01.md and phase-02.md for consistency
- 03-02: Include actual code from completed source files (not hypothetical examples)
- 03-03: Visual verification deferred (user on remote terminal)
- 04-01: Playwright chosen for E2E testing (robust, web-first assertions, screenshot/trace on failure)
- 04-01: data-testid='display' for stable display locator (survives style changes)
- 04-01: strictPort: true in Vite config prevents port auto-increment (consistent test environment)
- 04-01: webServer auto-start in Playwright config (tests manage dev server lifecycle)
- 04-01: Use getByRole for button locators (accessibility-first, survives refactors)
- 04-02: GitHub Actions for CI (native GitHub integration, free for public repos)
- 04-02: .NET SDK 10.0.x in CI to match global.json (ensures consistent Fable compilation)
- 04-02: Cache node_modules and Playwright browsers (reduces CI time from ~5min to ~1min)
- 04-02: Install only Chromium browser in CI (matches local test environment, faster installs)
- 04-02: Upload artifacts with if: !cancelled() (uploads on success/failure, skips on cancellation)
- 04-03: Tutorial combines Playwright and GitHub Actions in single guide (better narrative flow)
- 04-03: Include actual project code snippets instead of hypothetical examples (authenticity)
- 04-03: Troubleshooting section covers 5 common issues with solutions (practical helpfulness)
- 05-01: base: '/CalTwo/' in vite.config.js for GitHub Pages subdirectory deployment
- 05-01: :focus-visible instead of :focus for keyboard-only focus indicators (avoids mouse click rings)
- 05-01: @supports fallback ensures older browsers get focus indicators (progressive enhancement)
- 05-01: aria-live='polite' on display for non-interrupting screen reader announcements
- 05-01: role='status' on display for semantic ARIA landmark
- 05-02: README structure includes demo URL, prerequisites, setup, testing, building, deployment, accessibility, tech stack, contributing
- 05-02: Korean tutorial targets beginner web developers (knows F# basics, new to Vite/deployment)
- 05-02: Tutorial uses actual project code instead of hypothetical examples for authenticity
- 05-03: actions/checkout@v4 and actions/setup-node@v4 for consistency with ci.yml (avoid version drift)
- 05-03: workflow_dispatch trigger allows manual deployment without pushing to main
- 05-03: Concurrency group 'pages' with cancel-in-progress prevents conflicting deployments
- 05-03: Three-step Pages deployment pattern using official GitHub actions

### Pending Todos

None yet.

### Blockers/Concerns

None yet.

## Session Continuity

Last session: 2026-02-14T12:00:56Z
Stopped at: Completed 05-03-PLAN.md — GitHub Pages deployment automation
Resume file: None

**Phase 5 Complete:** All deployment and polish tasks finished. Project ready for public use.
