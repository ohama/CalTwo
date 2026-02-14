# Domain Pitfalls: F# Fable/Elmish Web Calculator

**Domain:** F# + Fable + Elmish web applications
**Researched:** 2026-02-14
**Project context:** Basic calculator (+-×÷), automated testing, GitHub Pages deployment

---

## Critical Pitfalls

Mistakes that cause rewrites, deployment failures, or major issues.

### Pitfall 1: Tool Version Mismatch Between Fable and .NET SDK

**What goes wrong:** Build fails with cryptic errors about incompatible .NET SDK versions or Fable refusing to compile.

**Why it happens:** Fable has specific .NET SDK requirements that aren't always clearly documented. Global vs local tool installations can conflict. The `dotnet-fable` package version needs to match `Fable.Core` version, but this isn't enforced automatically.

**Consequences:**
- Build works locally but fails in CI
- Cannot compile F# to JavaScript
- Wastes hours debugging tool installation issues

**Prevention:**
- Pin exact versions in `.config/dotnet-tools.json` (local tool manifest)
- Use `dotnet tool restore` instead of global tool installation
- Verify Fable.Core NuGet version matches dotnet-fable tool version
- Document exact .NET SDK version in `global.json`

**Detection:**
- Errors mentioning "netcoreapp" or SDK version incompatibility
- Build works on one machine but not another
- "Could not execute because the specified command or file was not found" for fable

**Phase mapping:** Address in Phase 1 (Project Setup) - create proper tool manifest upfront.

---

### Pitfall 2: Missing or Broken Source Maps in Production

**What goes wrong:** JavaScript errors in production show minified code locations instead of F# source. Stack traces are useless for debugging.

**Why it happens:** Source maps require explicit configuration in both Fable and the bundler (Vite). Production builds often disable source maps by default for performance/security. The transformation pipeline (F# → Fable → Babel → Vite) has many steps where source map chaining can break.

**Consequences:**
- Cannot debug production errors
- Stack traces point to wrong locations
- User-reported bugs are nearly impossible to diagnose

**Prevention:**
- Enable source maps in development: `"sourceMaps": true` in fableconfig.json
- Configure Vite to generate source maps: `build.sourcemap: true` in vite.config.js
- Test that F# files appear in browser DevTools source panel
- For GitHub Pages: deploy source maps alongside bundle (security tradeoff decision)

**Detection:**
- Browser DevTools shows only JavaScript files, no F# files
- Error stack traces show bundle.js:1:2345 instead of Calculator.fs:23
- Cannot set breakpoints in F# code

**Phase mapping:** Address in Phase 1 (Project Setup) - configure before writing code. Verify in Phase 3 (Testing) - include DevTools debugging test.

---

### Pitfall 3: Impure Update Functions (Side Effects in Update)

**What goes wrong:** Side effects (HTTP calls, localStorage, random numbers) placed directly in the update function instead of using Cmd. This breaks testability and Elmish architectural guarantees.

**Why it happens:** Developers coming from imperative JavaScript/C# backgrounds don't understand the Elm architecture's pure update / command pattern. It "works" at first, making the mistake invisible.

**Consequences:**
- Update function becomes untestable (requires mocking, DOM, timers)
- Race conditions and timing bugs
- State updates can be lost or interleaved incorrectly
- Violates Elmish's core architectural principle

**Prevention:**
- Keep update function pure: `(msg, model) -> (model, Cmd<Msg>)`
- Move all side effects to Cmd.OfFunc, Cmd.OfAsync, or Cmd.OfPromise
- Unit test update functions with simple assertions (no mocking needed)
- Review: if update function needs DateTime.Now, random, or I/O → refactor to Cmd

**Detection:**
- Update function contains: async/await, Promise, localStorage, fetch, Math.random()
- Update function has side effects you can "see" (network tab shows requests during state change)
- Cannot test update function without browser/node environment

**Phase mapping:** Address in Phase 2 (Core Logic) - establish pattern early. Include architecture review checkpoint.

---

### Pitfall 4: npm/NuGet Package Dependency Mismatch

**What goes wrong:** Install a Fable binding (e.g., Fable.React) via NuGet but forget to install the corresponding npm package (react, react-dom). Or version mismatch between the binding and the npm package it expects.

**Why it happens:** Fable bridges F# and JavaScript ecosystems. A Fable binding is just F# type definitions - it needs the actual JavaScript library at runtime. Manual package.json management doesn't automatically track NuGet dependencies.

**Consequences:**
- Runtime errors: "Cannot find module 'react'"
- Type mismatches if npm package version doesn't match binding expectations
- Build succeeds but runtime fails

**Prevention:**
- Use Femto to auto-install npm packages: `dotnet femto install` or `femto --resolve`
- Femto detects Fable bindings in .fsproj and adds corresponding npm packages
- Verify package.json after adding NuGet packages
- Document dependency sync step in README

**Detection:**
- Webpack/Vite errors: "Module not found: Error: Can't resolve 'react'"
- Runtime errors in browser console about missing modules
- NuGet package installed but npm package missing from package.json

**Phase mapping:** Address in Phase 1 (Project Setup) - install Femto as dotnet tool, document workflow.

---

### Pitfall 5: Headless Testing Misconfiguration for CI

**What goes wrong:** Tests run fine locally in browser but fail in GitHub Actions CI because headless browser isn't configured or Puppeteer/Playwright setup is missing.

**Why it happens:** Fable.Mocha supports multiple test runners (node, browser, headless). Local dev usually uses browser, but CI needs headless. The Fable.MochaPuppeteerRunner package and Chrome/Chromium installation isn't obvious.

**Consequences:**
- Tests pass locally but CI always fails
- "No browser found" or timeout errors in GitHub Actions
- Cannot achieve automated testing goal

**Prevention:**
- Choose test runner upfront: Vitest (modern, Vite-native) or Fable.Mocha with headless
- If using Fable.Mocha: install Fable.MochaPuppeteerRunner NuGet package
- Add GitHub Actions step to install Chrome: `setup-chrome` action
- Test CI configuration early (Phase 3) - don't wait until deployment phase
- Alternative: use Web Test Runner (runs in actual headless browser, no node)

**Detection:**
- GitHub Actions logs: "Error: Failed to launch the browser process"
- Tests work with `npm test` locally but not in CI
- Timeout errors waiting for browser

**Phase mapping:** Address in Phase 3 (Testing Setup) - configure CI runner alongside test writing.

---

### Pitfall 6: GitHub Pages Base Path Issues

**What goes wrong:** App works locally at `localhost:3000/` but on GitHub Pages at `username.github.io/repo-name/` all asset paths break (404s for CSS, JS).

**Why it happens:** Vite defaults to root path `/`. GitHub Pages serves from `/repo-name/`. Asset paths become `/bundle.js` instead of `/repo-name/bundle.js`. This is a standard SPA deployment issue but critical for the project's deployment requirement.

**Consequences:**
- Blank white page on GitHub Pages
- 404 errors for all assets
- Deployment appears successful but app doesn't load

**Prevention:**
- Configure Vite base path in vite.config.js: `base: process.env.NODE_ENV === 'production' ? '/CalTwo/' : '/'`
- Use relative paths for assets or Vite's asset import mechanism
- Test production build locally: `npm run build && npm run preview`
- Document base path configuration prominently

**Detection:**
- Browser console on GitHub Pages: "Failed to load resource: 404" for JS/CSS
- Network tab shows requests to wrong URLs
- Works in dev server but not in production deployment

**Phase mapping:** Address in Phase 4 (Deployment) - configure before first deploy, but test locally earlier.

---

## Moderate Pitfalls

Mistakes that cause delays or technical debt.

### Pitfall 7: Parent-Child Component State Coupling

**What goes wrong:** Following pure Elm architecture, parent components must know about all child component state and messages, even when unnecessary. This creates tight coupling and makes components hard to reuse.

**Why it happens:** Elm architecture prescribes a single model tree with all state. This works for small apps but creates friction when you want isolated reusable components (like a modal or dropdown).

**Consequences:**
- Excessive boilerplate (every child message must be wrapped by parent)
- Cannot easily extract and reuse components
- Model grows with UI details that parent doesn't need
- Violations of separation of concerns

**Prevention:**
- For a simple calculator: accept the coupling (only 4-5 components max)
- For future projects: consider Feliz.UseElmish (component-level Elmish)
- Use discriminated unions to isolate page/component state when appropriate
- Ask: "Does parent actually need to know this state?" If no, reconsider architecture

**Detection:**
- Parent model contains UI-specific state (e.g., `isModalOpen: bool`)
- Messages like `ChildMsg of Child.Msg` wrapper types everywhere
- Changing a child component requires touching parent's model/update

**Phase mapping:** Accept in Phase 2 (Core Logic) - simple calculator doesn't need complex component isolation.

---

### Pitfall 8: Forgetting Fable.Core Library Limitations

**What goes wrong:** Use a .NET library (like FsToolkit.ErrorHandling) that doesn't compile with Fable because it uses unsupported .NET APIs or lacks Fable-specific source exposure.

**Why it happens:** Fable supports "some" BCL and FSharp.Core but not all. Not every F# NuGet package works with Fable. Documentation doesn't always clearly mark Fable compatibility.

**Consequences:**
- Compiler errors about unsupported .NET features
- Wasted time trying to use incompatible libraries
- Need to find Fable-specific alternatives

**Prevention:**
- Check Fable compatibility before adding NuGet packages
- Prefer libraries marked "Fable-compatible" or in awesome-fable list
- Consult Fable .NET compatibility docs: https://fable.io/docs/dotnet/compatibility.html
- For simple calculator: minimize dependencies, use built-in FSharp.Core

**Detection:**
- Fable compiler errors: "Cannot resolve member", "Not supported by Fable"
- Library works in F# console app but fails in Fable project

**Phase mapping:** Prevent in Phase 1 (Project Setup) - review dependency choices. For calculator, shouldn't need external libraries.

---

### Pitfall 9: Subscription Cleanup Not Implemented

**What goes wrong:** Subscriptions (timers, WebSocket listeners, event handlers) aren't properly disposed when component unmounts or Elmish program terminates. Causes memory leaks and "zombie" event handlers.

**Why it happens:** Earlier Elmish versions didn't have termination/cleanup APIs. Developers forget to return cleanup functions from subscriptions. Elmish 4 + React 18 improved this, but it's still easy to forget.

**Consequences:**
- Memory leaks (subscriptions never unsubscribe)
- Event handlers fire after component unmounts (errors, stale state updates)
- Performance degradation over time

**Prevention:**
- Use Elmish 4+ with proper subscription cleanup
- Return unsubscribe/dispose functions from Cmd.ofSub
- For simple calculator: unlikely to need subscriptions (no timers, no WebSockets)
- If adding keyboard shortcuts later: ensure cleanup

**Detection:**
- Browser memory usage grows over time
- Console errors after navigating away from component
- Event handlers fire when component isn't visible

**Phase mapping:** Not applicable for basic calculator. Flag for future feature phases (keyboard shortcuts, etc.).

---

### Pitfall 10: CSS Not Bundled Correctly

**What goes wrong:** CSS files aren't imported/bundled by Vite. Styles missing in production build even though they work in dev.

**Why it happens:** Vite requires explicit CSS imports in JavaScript/F# entry point. Putting CSS in `public/` folder doesn't automatically bundle it. Developers expect Webpack-style auto-discovery.

**Consequences:**
- Unstyled app in production
- CSS works in dev but missing in dist/ output
- Confusing because HTML might reference CSS that doesn't exist in build

**Prevention:**
- Import CSS in F# entry file: `importSideEffects "./style.css"` (Fable.Core.JsInterop)
- Or place CSS in `public/` and reference with absolute path (not bundled, just copied)
- Verify CSS appears in Vite build output (dist/ folder)
- Test production build locally before deploying

**Detection:**
- `npm run build` output doesn't list CSS files
- dist/ folder missing CSS file
- Production app has no styling

**Phase mapping:** Address in Phase 1 (Project Setup) - configure CSS handling upfront.

---

## Minor Pitfalls

Mistakes that cause annoyance but are easily fixable.

### Pitfall 11: Fable Watch Mode Not Recompiling

**What goes wrong:** Change F# code but browser doesn't update. Fable watch mode seems stuck.

**Why it happens:** Fable watch process crashed silently. File watcher hit OS limits. Terminal running `npm run dev` was backgrounded and paused.

**Consequences:**
- Developer confusion ("my changes aren't working")
- Manual restart required

**Prevention:**
- Use `dotnet fable watch` + Vite HMR together (both must be running)
- Configure Elmish.HMR for state preservation: `open Elmish.HMR`
- Check Fable watch terminal for errors
- Restart both Fable and Vite if changes stop appearing

**Detection:**
- Change F# code, save, no browser update
- Fable watch terminal shows no new compilation output
- Need to manually refresh or restart dev server

**Phase mapping:** Document in Phase 1 (Project Setup) - dev workflow instructions.

---

### Pitfall 12: Cmd Exceptions Swallowed Silently

**What goes wrong:** Exception in a Cmd side effect (e.g., async HTTP call) doesn't produce an error message. Command just fails silently.

**Why it happens:** Cmd.OfAsync and similar helpers don't automatically handle exceptions. If you don't provide an error handler, the exception disappears.

**Consequences:**
- Feature appears broken with no error message
- Debugging is extremely difficult
- Users see nothing happen (no feedback)

**Prevention:**
- Always provide error handlers in Cmd.OfAsync/OfFunc/OfPromise
- Use Result types for operations that can fail
- Log errors to console in error handler: `Error e -> console.error(e); model, Cmd.none`
- For calculator: not applicable (no async operations)

**Detection:**
- Button click does nothing, no error in console
- Async operation fails silently
- Need to add try/catch to find the exception

**Phase mapping:** Not applicable for basic calculator (no async ops). Flag for future API integration.

---

### Pitfall 13: .fsproj Not Included in Version Control

**What goes wrong:** Forgot to commit .fsproj file. Other developers can't build project.

**Why it happens:** Overly aggressive .gitignore or assumption that project files are "generated".

**Consequences:**
- Clone repository, cannot build
- Missing file list, package references

**Prevention:**
- Commit .fsproj, .fsx, and all F# source files
- Commit .config/dotnet-tools.json (tool manifest)
- Gitignore bin/, obj/, dist/, node_modules/ but NOT project files

**Detection:**
- Fresh clone fails with "could not find project file"
- Package references missing

**Phase mapping:** Address in Phase 1 (Project Setup) - initial git commit checklist.

---

### Pitfall 14: Hot Module Replacement State Loss

**What goes wrong:** Make a code change, HMR refreshes, but component state resets (counters, form inputs).

**Why it happens:** HMR reloads the component, re-initializing state. Without Elmish.HMR, state isn't preserved across updates.

**Consequences:**
- Annoying during development (lose test state after each change)
- Slows down iteration speed

**Prevention:**
- Use Elmish.HMR: `open Elmish.HMR` (only in DEBUG mode)
- Available by default in Vite + Fable setups
- State preserved across code changes

**Detection:**
- Edit code, save, calculator display resets to 0
- Form inputs cleared after HMR refresh

**Phase mapping:** Address in Phase 1 (Project Setup) - enable HMR immediately for better DX.

---

## Phase-Specific Warnings

| Phase Topic | Likely Pitfall | Mitigation |
|-------------|---------------|------------|
| **Phase 1: Project Setup** | Tool version mismatches (Pitfall 1) | Create dotnet-tools.json, global.json, document versions |
| **Phase 1: Project Setup** | npm/NuGet dependency sync (Pitfall 4) | Install Femto, document sync workflow |
| **Phase 1: Project Setup** | Source maps misconfigured (Pitfall 2) | Configure fableconfig.json + vite.config.js upfront |
| **Phase 2: Core Logic** | Impure update functions (Pitfall 3) | Code review checkpoint, unit test update functions |
| **Phase 2: Core Logic** | Parent-child coupling (Pitfall 7) | Accept for simple calculator, document for future |
| **Phase 3: Testing** | Headless browser missing (Pitfall 5) | Configure CI test runner alongside writing tests |
| **Phase 3: Testing** | Source maps verification (Pitfall 2) | Include DevTools debugging test case |
| **Phase 4: Deployment** | GitHub Pages base path (Pitfall 6) | Configure Vite base before first deploy |
| **Phase 4: Deployment** | CSS not bundled (Pitfall 10) | Test production build locally first |

---

## Calculator-Specific Notes

**Low-risk pitfalls for this project:**
- Pitfall 7 (Parent-child coupling): Not a problem - calculator is small
- Pitfall 9 (Subscription cleanup): Not applicable - no subscriptions needed
- Pitfall 12 (Cmd exceptions): Not applicable - no async operations

**High-risk pitfalls for this project:**
- Pitfall 1 (Tool versions): CRITICAL - will block initial setup
- Pitfall 5 (Headless testing): CRITICAL - required for "automated testing" goal
- Pitfall 6 (GitHub Pages base path): CRITICAL - required for deployment goal

**Recommended phase structure based on pitfalls:**

1. **Phase 1 (Foundation):** Mitigate Pitfalls 1, 2, 4, 10, 13, 14
   - Lock down tooling versions
   - Configure source maps
   - Set up dependency sync with Femto
   - Enable HMR

2. **Phase 2 (Core Logic):** Mitigate Pitfall 3
   - Establish pure update pattern
   - Write unit tests to enforce purity

3. **Phase 3 (Testing):** Mitigate Pitfall 5
   - Configure headless test runner
   - Validate in CI early

4. **Phase 4 (Deployment):** Mitigate Pitfall 6
   - Configure base path
   - Test production build locally
   - Deploy to GitHub Pages

---

## Sources

- [Maxime Mangel's Elmish Tips (Medium)](https://medium.com/@MangelMaxime/my-tips-for-working-with-elmish-ab8d193d52fd)
- [Elmish GitHub Repository](https://github.com/elmish/elmish)
- [Elmish Components with Elmish 4 and UseElmish](https://fable.io/blog/2022/2022-10-13-use-elmish.html)
- [Fable F# Notes (GitHub Gist)](https://gist.github.com/enerqi/b941b953f89f3ac91dddff81b932f2ea)
- [Tips for Unit Testing Fable Apps using .NET](https://jordanmarr.github.io/fsharp/unit-testing-fable-dotnet/)
- [Testing F# Elmish React Components with Fable](https://www.nathanfox.net/p/testing-f-elmish-react-components)
- [Fable.Mocha GitHub Repository](https://github.com/Zaid-Ajaj/Fable.Mocha)
- [Fable.Expect GitHub Repository](https://github.com/fable-compiler/Fable.Expect)
- [FableStarter Template with Vitest](https://github.com/rastreus/FableStarter)
- [Fable Getting Started Documentation](https://fable.io/docs/getting-started/your-first-fable-project.html)
- [Fable Build and Run Documentation](https://fable.io/docs/javascript/build-and-run.html)
- [Femto GitHub Repository](https://github.com/Zaid-Ajaj/Femto)
- [Introducing Femto Blog Post](https://fable.io/blog/2019/2019-06-29-Introducing-Femto.html)
- [Fable .NET Compatibility Documentation](https://fable.io/docs/dotnet/compatibility.html)
- [Elmish Parent-Child Composition Documentation](https://elmish.github.io/elmish/docs/parent-child.html)
- [Elmish Subscriptions Documentation](https://elmish.github.io/elmish/docs/subscription.html)
- [Elmish.HMR Documentation](https://elmish.github.io/hmr/)
- [Fable Source Maps Issues on GitHub](https://github.com/fable-compiler/Fable/issues/2166)
- [Fable Debugging Repository](https://github.com/CompositionalIT/fable-debugging)
- [Vite Build Documentation](https://vite.dev/guide/build)
- [Elmish Cmd Side Effects Discussion](https://github.com/elmish/elmish/issues/123)
