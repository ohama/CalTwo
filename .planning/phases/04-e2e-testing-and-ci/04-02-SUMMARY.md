---
phase: 04-e2e-testing-and-ci
plan: 02
subsystem: ci/cd
tags: [github-actions, ci, automation, playwright, testing]
wave: 2
requires: [04-01]
provides:
  - ".github/workflows/ci.yml: automated CI pipeline"
  - "npm run test:all: local pre-push verification"
affects: []
tech-stack:
  added: [github-actions]
  patterns: [ci-pipeline, test-automation, artifact-upload, dependency-caching]
key-files:
  created:
    - .github/workflows/ci.yml
  modified:
    - package.json
decisions:
  - id: ci-01
    choice: "GitHub Actions for CI (native GitHub integration)"
    context: "Repository hosted on GitHub, native Actions integration simplest"
    rationale: "Zero external services, free for public repos, excellent Playwright support"
  - id: ci-02
    choice: ".NET SDK 10.0.x in CI to match global.json"
    context: "Project uses .NET SDK 10.0.100 for Fable compilation"
    rationale: "CI must match local environment to ensure consistent test results"
  - id: ci-03
    choice: "Cache both node_modules and Playwright browsers"
    context: "CI runs can be slow without caching"
    rationale: "Reduces CI time from ~5min to ~1min, improves developer experience"
  - id: ci-04
    choice: "Install only Chromium browser (not all browsers)"
    context: "playwright.config.ts only uses Chromium"
    rationale: "Faster installs, less disk space, matches local test environment"
  - id: ci-05
    choice: "Upload artifacts with if: !cancelled() not if: always()"
    context: "Need artifacts on failure but not on cancellation by user"
    rationale: "!cancelled() uploads on success and failure, skips on manual cancel"
metrics:
  duration: "1 minute"
  completed: "2026-02-14"
---

# Phase 04 Plan 02: CI Pipeline Summary

**One-liner:** GitHub Actions CI running unit + E2E tests with Playwright browser caching and artifact uploads on failure.

## Overview

Created a fully automated CI pipeline using GitHub Actions that runs on every push and pull request to main. The workflow installs .NET SDK 10.0.x, Node.js LTS, npm dependencies, and dotnet tools, then runs both unit tests (Mocha) and E2E tests (Playwright), uploading test reports and screenshots as artifacts on failure.

**What was delivered:**
- `.github/workflows/ci.yml` with complete test automation
- `npm run test:all` script for local pre-push verification
- Dependency caching for faster CI runs (node_modules, Playwright browsers)
- Artifact uploads for debugging test failures

**Key achievement:** Zero-manual-step CI. Developers push code, CI runs all tests automatically, failures provide downloadable reports and screenshots.

## Tasks Completed

| Task | Description | Commit | Files |
|------|-------------|--------|-------|
| 1 | Create GitHub Actions CI workflow | 5c9a025 | .github/workflows/ci.yml |
| 2 | Add CI-related npm script | aa5bc4c | package.json |

## Technical Details

### CI Workflow Architecture

**Trigger events:**
- Push to main branch
- Pull requests targeting main branch

**Job steps:**
1. Checkout code (actions/checkout@v4)
2. Install .NET SDK 10.0.x (actions/setup-dotnet@v4)
3. Install Node.js LTS (actions/setup-node@v4)
4. Cache node_modules using package-lock.json hash
5. Install npm dependencies with `npm ci`
6. Restore dotnet tools (Fable compiler)
7. Run unit tests with `npm test`
8. Cache Playwright browsers using package-lock.json hash
9. Install Chromium browser (with or without deps based on cache hit)
10. Run E2E tests with `npx playwright test`
11. Upload playwright-report artifact (if tests ran, even on failure)
12. Upload test-results artifact (screenshots, traces)

**Caching strategy:**
- Node modules cached by `package-lock.json` hash
- Playwright browsers cached by `package-lock.json` hash
- First install uses `--with-deps` (installs OS dependencies)
- Cache hit uses `install-deps` (refreshes OS deps without re-downloading browser)

**Artifact strategy:**
- `if: ${{ !cancelled() }}` ensures uploads happen on test failure
- playwright-report contains HTML test report
- test-results contains screenshots and traces for debugging
- 30-day retention for debugging failed tests

### Local Pre-push Verification

Added `test:all` script to package.json:
```json
"test:all": "npm test && npm run test:e2e"
```

Runs both unit tests (33 tests) and E2E tests (10 tests) sequentially. CI runs them separately for better error reporting, but this script is useful for local verification before pushing.

### .NET SDK Version Alignment

CI uses `dotnet-version: '10.0.x'` to match global.json which specifies SDK 10.0.100. This ensures Fable compilation in CI uses the same SDK as local development.

## Decisions Made

### 1. GitHub Actions for CI (native GitHub integration)
**Context:** Repository hosted on GitHub, need automated testing on every push/PR.
**Decision:** Use GitHub Actions instead of external CI services (Travis, CircleCI, Jenkins).
**Rationale:** Zero external services, free for public repos, excellent Playwright support, native GitHub integration.

### 2. .NET SDK 10.0.x in CI to match global.json
**Context:** Project uses .NET SDK 10.0.100 for Fable compilation.
**Decision:** Specify `dotnet-version: '10.0.x'` in workflow.
**Rationale:** CI must match local environment to ensure consistent test results. Fable compiler behavior can vary between SDK versions.

### 3. Cache both node_modules and Playwright browsers
**Context:** CI runs can be slow without caching, especially Playwright browser downloads.
**Decision:** Cache `~/.npm`, `node_modules`, and `~/.cache/ms-playwright` using package-lock.json hash.
**Rationale:** Reduces CI time from ~5min to ~1min, improves developer experience, saves GitHub Actions minutes.

### 4. Install only Chromium browser (not all browsers)
**Context:** playwright.config.ts only uses Chromium project.
**Decision:** `npx playwright install chromium` instead of `npx playwright install`.
**Rationale:** Faster installs (1 browser vs 3), less disk space (400MB vs 1.2GB), matches local test environment.

### 5. Upload artifacts with if: !cancelled() not if: always()
**Context:** Need test reports and screenshots when tests fail, but not when user cancels workflow.
**Decision:** Use `if: ${{ !cancelled() }}` instead of `if: always()`.
**Rationale:** `!cancelled()` uploads on success and failure (for debugging), but skips on manual cancellation (saves storage).

## Integration Points

**Upstream dependencies:**
- 04-01: Playwright setup and E2E test suite (uses `npx playwright test`)
- package.json scripts (npm test, npm run test:e2e)
- global.json (.NET SDK version requirement)

**Files modified:**
- `.github/workflows/ci.yml` (created): CI pipeline definition
- `package.json` (modified): Added test:all script

**External services:**
- GitHub Actions (CI runner)
- npm registry (dependency installation)
- Playwright CDN (browser downloads)

## Verification Results

All verification criteria passed:

1. `.github/workflows/ci.yml` exists and is valid YAML
2. Workflow triggers on push and PR to main branch (2 trigger events configured)
3. Workflow installs .NET SDK, Node.js, npm deps, dotnet tools, Playwright (6 installation steps)
4. Workflow runs `npm test` (unit tests) and `npx playwright test` (E2E tests)
5. Workflow uploads playwright-report and test-results artifacts (2 upload-artifact steps)
6. `npm run test:all` passes locally (33 unit tests + 10 E2E tests all passing)

**Local test run:**
- Unit tests: 33 passing (8ms)
- E2E tests: 10 passing (3.0s)
- Total: 43 tests passing

## Deviations from Plan

None. Plan executed exactly as written.

## Next Phase Readiness

**Phase 04 status:** Plan 04-02 complete (2 of 3 plans in phase)

**Blockers:** None

**Risks:** None

**Next steps:**
- Plan 04-03: Visual regression testing (optional, if specified in phase plan)
- OR Phase 05: Deployment and production readiness

**What downstream phases can now do:**
- Rely on automated CI to catch regressions
- View test artifacts (reports, screenshots) on CI failures
- Use CI as quality gate for merges to main

## Lessons Learned

**What worked well:**
- Caching strategy significantly reduces CI time
- Installing only Chromium browser keeps CI fast and lean
- `if: !cancelled()` provides artifacts when needed, skips when not
- Matching .NET SDK version between local and CI prevents surprises

**What could be improved:**
- Could add parallel test execution (Playwright supports sharding)
- Could add test result commenting on PRs (GitHub Actions supports PR comments)
- Could add Slack/Discord notifications on CI failure (if team collaboration needed)

**Technical debt:**
- None. CI is clean, well-cached, and maintainable.

**For next phase:**
- CI is ready for deployment workflows (Phase 05)
- Can add deployment steps to same workflow or separate workflow
- Artifacts (playwright-report, test-results) provide debugging for production issues
