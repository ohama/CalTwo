---
phase: 01-foundation-and-tooling
plan: 01
subsystem: infra
tags: [fsharp, fable, elmish, vite, react, hmr, dotnet]

# Dependency graph
requires:
  - phase: 00-project-init
    provides: Project directory structure and planning framework
provides:
  - Complete F# + Elmish + Vite project structure with version pinning
  - Package manifests with exact versions (React 18.3.1, Vite 7, Fable 4.3.0)
  - Minimal MVU application with HMR enabled
  - Build configuration with sourcemap support
affects: [02-calculator-core, 03-ui-components, 04-integration, 05-deployment]

# Tech tracking
tech-stack:
  added: [Fable 4.3.0, Feliz 2.9.0, Fable.Elmish 5.0.2, Vite 7, vite-plugin-fable 0.1.1, React 18.3.1]
  patterns: [MVU architecture, Elmish program pattern, HMR integration, Version pinning for reproducibility]

key-files:
  created:
    - global.json
    - .config/dotnet-tools.json
    - .gitignore
    - package.json
    - src/App.fsproj
    - src/App.fs
    - src/Main.fs
    - vite.config.js
    - index.html
  modified: []

key-decisions:
  - "React 18.3.1 (NOT React 19) - Feliz 2.x has compatibility warnings with React 19"
  - "Empty dotnet-tools.json manifest - vite-plugin-fable manages Fable internally, no CLI tool needed"
  - "Plugin order: fable() before react() - ensures proper F# transpilation before React processing"
  - "Elmish.HMR opened last in Main.fs - shadows Program.run to enable hot module replacement"

patterns-established:
  - "Version pinning: .NET SDK via global.json, exact npm versions in package.json"
  - "MVU architecture: Model-View-Update pattern with Elmish"
  - "HMR workflow: Elmish.HMR must be last open statement"
  - "Module import: index.html uses import statement (NOT src attribute) for .fs files"

# Metrics
duration: 1min
completed: 2026-02-14
---

# Phase 01 Plan 01: Project Structure Setup Summary

**F# + Elmish + Vite project with React 18.3.1, Fable 4.3.0, HMR enabled, and complete version pinning for reproducible builds**

## Performance

- **Duration:** 1 min 28 sec
- **Started:** 2026-02-14T06:26:55Z
- **Completed:** 2026-02-14T06:28:24Z
- **Tasks:** 3
- **Files modified:** 9

## Accomplishments
- Complete project structure with all configuration files for F# + Vite workflow
- Version pinning established (.NET 10.0.100, React 18.3.1, Fable 4.3.0)
- Minimal "Hello CalTwo" MVU application with HMR enabled
- Build configuration ready for development and production with source maps

## Task Commits

Each task was committed atomically:

1. **Task 1: Create version pinning and package manifest files** - `6804e1d` (chore)
   - global.json, .config/dotnet-tools.json, .gitignore, package.json

2. **Task 2: Create F# project structure with Elmish packages** - `71524b4` (feat)
   - src/App.fsproj, src/App.fs, src/Main.fs

3. **Task 3: Create Vite configuration and HTML entry point** - `663c2f7` (chore)
   - vite.config.js, index.html

## Files Created/Modified

- `global.json` - Pins .NET SDK to 10.0.100 with latestFeature rollForward
- `.config/dotnet-tools.json` - Empty dotnet tools manifest (follows .NET convention)
- `.gitignore` - Ignores .NET, Node, Vite, IDE, and OS artifacts
- `package.json` - npm dependencies with exact versions (React 18.3.1, Vite 7)
- `src/App.fsproj` - F# project with 6 Fable packages (Core, Feliz, Elmish, React, HMR, Browser.Dom)
- `src/App.fs` - Minimal MVU structure with "Hello CalTwo" message
- `src/Main.fs` - Elmish program entry point with HMR enabled
- `vite.config.js` - Vite configuration with fable + react plugins, sourcemap enabled
- `index.html` - HTML entry point with module import for Main.fs

## Decisions Made

1. **React 18.3.1 instead of React 19** - RESEARCH.md documented Feliz 2.x compatibility warnings with React 19, so pinned to 18.3.1 for stability

2. **Empty dotnet-tools.json** - vite-plugin-fable manages Fable internally, so Fable CLI not needed as dotnet tool. Manifest follows .NET best practices and allows future tool additions.

3. **Plugin order: fable() before react()** - Per RESEARCH.md, Fable must transpile F# to JS before React plugin processes JSX. Incorrect order causes build failures.

4. **Elmish.HMR opened last** - Per RESEARCH.md Common Pitfalls #3, Elmish.HMR must be last open statement to shadow Program.run and enable HMR.

5. **Module import statement** - index.html uses `import "/src/Main.fs"` (NOT `<script src="/src/Main.fs">`) per RESEARCH.md Pattern 4, which allows Vite to recognize .fs extension.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None - all tasks completed without errors or unexpected problems.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

**Ready for Phase 01 Plan 02:**
- Project structure complete and ready for npm install and dotnet restore
- All version pinning in place for reproducible builds
- Minimal MVU app in place as baseline for feature development
- Build configuration ready for development (HMR) and production (sourcemaps)

**Blockers/Concerns:**
None - foundation is solid and follows all RESEARCH.md best practices.

---
*Phase: 01-foundation-and-tooling*
*Completed: 2026-02-14*
