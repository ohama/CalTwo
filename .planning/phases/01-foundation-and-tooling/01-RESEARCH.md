# Phase 1: Foundation & Tooling - Research

**Researched:** 2026-02-14
**Domain:** Fable + Elmish + Feliz + Vite Development Stack
**Confidence:** HIGH

## Summary

This research covers the modern Fable 4 + Elmish + Feliz + Vite stack for building F# web applications with React. The key finding is that vite-plugin-fable has fundamentally changed the setup process - you no longer install Fable as a dotnet tool, and the plugin handles all Fable compiler interactions automatically.

The standard approach uses vite-plugin-fable to integrate F# files directly into the Vite dev server, leveraging HMR through Elmish.HMR for state preservation during hot reloads. Source maps work automatically when Vite is in dev mode, but require explicit configuration for production builds.

Version pinning is critical for reproducible builds. Use global.json for SDK version, dotnet-tools.json for any local tools (though Fable itself is now managed by the npm plugin), and explicit package versions in .fsproj. CSS is imported using Fable.Core.JsInterop.importSideEffects for proper bundling.

**Primary recommendation:** Use vite-plugin-fable 0.1.1+ with Fable 4.x packages, .NET 8+ (or .NET 10 for new projects), and follow the plugin-first architecture where Vite manages the build pipeline.

## Standard Stack

The established libraries/tools for this domain:

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| .NET SDK | 8.0+ (10.0 for new) | F# compiler and runtime | Required by Fable 4, .NET 10 is current LTS (Nov 2025-2028) |
| Fable.Core | 4.3.0+ | F# to JS transpiler core | Fable 4 is current major version, required for modern React |
| vite-plugin-fable | 0.1.1+ | Vite integration for Fable | Official plugin, eliminates manual Fable tool installation |
| Vite | 7.0+ | Build tool and dev server | Fast, modern, HMR-native, current major version |
| Feliz | 2.9.0 | F# DSL for React | Type-safe React bindings, community standard |
| Fable.Elmish | 5.0.2 | MVU architecture | Latest stable, required for Elmish.HMR 7.0 |
| Fable.Elmish.React | 5.0.1 | React integration | Bridges Elmish programs to React rendering |
| Fable.Elmish.HMR | 7.0.0 | Hot module replacement | Preserves state during reloads, DEBUG-only |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| React | 18.3.1 | UI library | Peer dependency of Feliz, React 19 has key warnings |
| react-dom | 18.3.1 | React DOM rendering | Required with React |
| @vitejs/plugin-react | 4.3.4+ | JSX transformation | When using Feliz.CompilerPlugins or Fable.Core.JSX |
| @fable-org/fable-library-js | 2.0.0-beta.3+ | Fable runtime library | Auto-installed by vite-plugin-fable |
| Femto | Latest | npm/NuGet sync | Optional, for libraries with npm dependencies |
| Fable.Browser.Dom | 2.14.0+ | Browser DOM APIs | When directly manipulating DOM |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Vite | Webpack 5 | Webpack is mature but slower, more config, older Fable tooling approach |
| vite-plugin-fable | Manual Fable CLI | Plugin approach is simpler, fewer moving parts, auto-manages versions |
| Feliz | Fable.React | Feliz provides type-safe DSL, Fable.React is lower-level React.createElement |
| Elmish.HMR | Manual HMR | Elmish.HMR preserves MVU state automatically, manual is complex |
| React 18 | React 19 | React 19 has key warnings with Feliz 2.x, stick with 18 until Feliz updates |

**Installation:**
```bash
# Node packages
npm install react@18.3.1 react-dom@18.3.1
npm install -D vite@^7.0.0 vite-plugin-fable@^0.1.1 @vitejs/plugin-react@^4.3.4

# F# packages (in .fsproj)
dotnet add package Fable.Core --version 4.3.0
dotnet add package Feliz --version 2.9.0
dotnet add package Fable.Elmish --version 5.0.2
dotnet add package Fable.Elmish.React --version 5.0.1
dotnet add package Fable.Elmish.HMR --version 7.0.0
dotnet add package Fable.Browser.Dom --version 2.14.0
```

## Architecture Patterns

### Recommended Project Structure
```
CalTwo/
├── .config/                  # .NET local tools manifest
│   └── dotnet-tools.json     # (if using any local tools)
├── public/                   # Static assets (favicon, etc)
├── src/
│   ├── Main.fs               # Entry point - Elmish program initialization
│   ├── App.fs                # Root component - Model/Update/View
│   ├── Components/           # (future phases)
│   └── App.fsproj            # F# project file
├── .gitignore
├── global.json               # Pin .NET SDK version
├── index.html                # HTML entry point
├── package.json              # Node dependencies
├── vite.config.js            # Vite + plugin config
└── tutorial/
    └── phase-01.md           # Korean tutorial
```

### Pattern 1: Vite Plugin Configuration
**What:** Configure vite-plugin-fable to transpile F# files in Vite pipeline
**When to use:** Always - this is the entry point for Fable integration
**Example:**
```javascript
// Source: https://fable.io/vite-plugin-fable/getting-started.html
import { defineConfig } from "vite";
import fable from "vite-plugin-fable";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [
    fable({
      jsx: "automatic"  // Use React automatic JSX runtime
    }),
    react({
      include: /\.(fs|js|jsx|ts|tsx)$/  // Process F# files for fast refresh
    })
  ],
  build: {
    sourcemap: true  // Enable source maps for production
  }
});
```

### Pattern 2: F# Entry Point with HMR
**What:** Initialize Elmish program with HMR support in Main.fs
**When to use:** Always - this is how you start the app
**Example:**
```fsharp
// Source: https://elmish.github.io/hmr/
module Main

open Elmish
open Elmish.React
open Elmish.HMR  // MUST be last - shadows Program.run

Program.mkProgram App.init App.update App.view
|> Program.withReactSynchronous "elmish-app"
|> Program.run  // HMR-enabled version when DEBUG defined
```

### Pattern 3: CSS Import via JsInterop
**What:** Import CSS files for bundling using importSideEffects
**When to use:** Always - for any CSS to be included in bundle
**Example:**
```fsharp
// Source: https://medium.com/@zaid.naom/f-interop-with-javascript-in-fable-the-complete-guide-ccc5b896a59f
module Main

open Fable.Core.JsInterop

// Import CSS - generates: import "./styles.css"
importSideEffects "./styles.css"

// Rest of program initialization...
```

### Pattern 4: HTML Entry Point
**What:** Import F# entry file directly in HTML using module script
**When to use:** Always - index.html is the Vite entry point
**Example:**
```html
<!-- Source: https://fable.io/vite-plugin-fable/getting-started.html -->
<!DOCTYPE html>
<html>
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>CalTwo</title>
</head>
<body>
  <div id="elmish-app"></div>
  <script type="module">
    import "/src/Main.fs";
  </script>
</body>
</html>
```
**CRITICAL:** Do NOT use `<script type="module" src="/src/Main.fs">` - Vite cannot recognize .fs extension that way.

### Pattern 5: .fsproj Structure
**What:** Minimal F# project file with correct target framework and packages
**When to use:** Always - defines project compilation
**Example:**
```xml
<!-- Source: https://raw.githubusercontent.com/jkone27/feliz-vite/main/src/App.fsproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <Compile Include="App.fs" />
    <Compile Include="Main.fs" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Fable.Core" Version="4.3.0" />
    <PackageReference Include="Feliz" Version="2.9.0" />
    <PackageReference Include="Fable.Elmish" Version="5.0.2" />
    <PackageReference Include="Fable.Elmish.React" Version="5.0.1" />
    <PackageReference Include="Fable.Elmish.HMR" Version="7.0.0" />
    <PackageReference Include="Fable.Browser.Dom" Version="2.14.0" />
  </ItemGroup>
</Project>
```

### Pattern 6: Version Pinning with global.json
**What:** Pin .NET SDK version for reproducible builds
**When to use:** Always - prevents unexpected SDK upgrades breaking builds
**Example:**
```json
// Source: https://learn.microsoft.com/en-us/dotnet/core/tools/global-json
{
  "sdk": {
    "version": "10.0.100",
    "rollForward": "latestFeature"
  }
}
```
**Note:** `rollForward: "latestFeature"` allows patch and feature band updates within the same major version (e.g., 10.0.x or 10.1.x, but not 11.0.0).

### Anti-Patterns to Avoid
- **Installing Fable as dotnet tool when using vite-plugin-fable:** The plugin manages Fable internally, manual installation causes version conflicts
- **Importing F# files with `src=` attribute:** Vite cannot process .fs extensions in src attributes, use `<script type="module">import</script>` instead
- **Opening Elmish.HMR before other Elmish modules:** HMR shadows Program.run, must be last to work correctly
- **Using React 19 with Feliz 2.x:** Causes missing key warnings, stick with React 18 until Feliz compatibility confirmed
- **Omitting sourcemap configuration in production:** Source maps are dev-only by default in Vite, must explicitly enable for production debugging
- **Plugin order: React before Fable:** vite-plugin-fable must be listed before @vitejs/plugin-react in plugins array

## Don't Hand-Roll

Problems that look simple but have existing solutions:

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| HMR state preservation | Custom module.hot.accept() | Fable.Elmish.HMR | Understands MVU model/message types, preserves state correctly, DEBUG-only |
| F# to React binding | Manual React.createElement calls | Feliz | Type-safe DSL, compile-time checking, community standard |
| npm/NuGet sync | Manual package.json edits | Femto | Auto-detects npm requirements from F# bindings, prevents runtime errors |
| CSS bundling | Manual link tags | importSideEffects | Vite tree-shaking, cache busting, minification |
| Build pipeline | Custom webpack config | vite-plugin-fable | Maintained by community, handles edge cases, auto-updates Fable |
| Source map generation | Manual sourceRoot config | Vite build.sourcemap | Correct paths, browser DevTools compatibility |
| React fast refresh | Manual HMR setup | @vitejs/plugin-react | Official plugin, handles React-specific HMR edge cases |

**Key insight:** The Fable ecosystem has matured significantly - vite-plugin-fable eliminates entire categories of manual configuration that plagued earlier Webpack-based setups. Trust the plugin architecture.

## Common Pitfalls

### Pitfall 1: Tool Version Mismatch
**What goes wrong:** Build fails with cryptic errors like "Unsupported format of the sourcemap" or silent failures during compilation.
**Why it happens:** Fable 4 requires .NET 8+, vite-plugin-fable 0.1.1+ requires .NET 8+ runtime, mixing Fable 3 and Fable 4 packages causes incompatibilities.
**How to avoid:**
- Create global.json with explicit SDK version (e.g., "version": "10.0.100")
- Use PackageReference with explicit versions in .fsproj (not wildcard *)
- Verify vite-plugin-fable postinstall script ran: `npm ls vite-plugin-fable`
- Use consistent versions across all Fable.* packages (all 4.x or all 3.x, never mixed)
**Warning signs:**
- Error: "This project is up for adoption" message (outdated vite-plugin-fable)
- Fable compilation succeeds but Vite shows .fs files as 404
- Source maps show node_modules paths instead of F# source

### Pitfall 2: Source Maps Not Working
**What goes wrong:** Browser DevTools show minified JS instead of F# source files, breakpoints don't hit.
**Why it happens:** Vite only generates source maps in dev mode by default, production builds omit them unless explicitly configured.
**How to avoid:**
- Add `build: { sourcemap: true }` to vite.config.js for production source maps
- Ensure Vite is in dev mode: `npm run dev` not `npm run build`
- Check browser DevTools settings: "Enable JavaScript source maps" must be checked
- Verify .fs files appear in DevTools Sources tab under webpack:// or similar
**Warning signs:**
- Only see bundle.js or similar in Sources tab
- Setting breakpoints shows "Breakpoint ignored" message
- Stack traces show line numbers in generated JS, not F# lines

### Pitfall 3: HMR State Loss
**What goes wrong:** Editing F# code causes full page reload, losing application state (form inputs, navigation position, etc).
**Why it happens:** Elmish.HMR only works when opened AFTER other Elmish modules, or not opened at all in production builds.
**How to avoid:**
- Always `open Elmish.HMR` as the LAST open statement in Main.fs
- Verify DEBUG compilation symbol is defined (Fable adds it automatically in watch mode)
- Check Program.run is called from Elmish.HMR namespace, not Elmish directly
- Use single Elmish instance (Elmish.HMR doesn't support multiple simultaneous programs)
**Warning signs:**
- HMR works for JS/CSS but not F# files
- Console shows "HMR update failed" or similar
- State resets to initial model on every F# edit

### Pitfall 4: npm/NuGet Dependency Mismatch
**What goes wrong:** Build succeeds but runtime error: "Cannot find module 'some-npm-package'" when using F# bindings.
**Why it happens:** F# NuGet packages can reference npm packages via Import("pkg") but NuGet doesn't auto-install them.
**How to avoid:**
- Use Femto to scan project: `dotnet tool install femto && dotnet femto src/App.fsproj`
- Manually check F# binding docs for required npm packages
- Install missing packages: `npm install <package>@<version>`
- Verify package.json includes all required npm deps before committing
**Warning signs:**
- F# compiles successfully but browser console shows module not found
- Fable bindings (Fable.* packages) installed but corresponding npm package missing
- Error messages reference npm package names that aren't in package.json

### Pitfall 5: Plugin Order Wrong
**What goes wrong:** Fast refresh doesn't work for F# files, or Fable compilation errors are ignored.
**Why it happens:** @vitejs/plugin-react must process files AFTER vite-plugin-fable transforms them.
**How to avoid:**
- Always list fable() before react() in plugins array
- Ensure react plugin includes .fs files: `include: /\.(fs|js|jsx|ts|tsx)$/`
- Pass `jsx: "automatic"` to fable plugin when using Feliz or Fable.Core.JSX
**Warning signs:**
- Fast refresh works for .jsx but not .fs files
- Console shows "Fast refresh only works when a file only exports components"
- Vite doesn't detect F# file changes

### Pitfall 6: CSS Not Bundled
**What goes wrong:** Styles don't apply, browser shows 404 for CSS file, or styles load in dev but not production.
**Why it happens:** CSS files aren't imported via Fable.Core.JsInterop, only referenced in HTML link tags.
**How to avoid:**
- Import CSS in Main.fs: `importSideEffects "./styles.css"`
- Place CSS files in src/ directory (not public/), so Vite processes them
- Never use `<link rel="stylesheet">` for CSS that should be bundled
- Use relative paths from .fs file location (e.g., "./App.css" from App.fs)
**Warning signs:**
- Styles work in dev but disappear in production build
- Vite build output doesn't mention CSS files
- Browser DevTools Network tab shows 404 for CSS

### Pitfall 7: Wrong HTML Import Pattern
**What goes wrong:** Vite shows "Failed to resolve import" or .fs file returns 404.
**Why it happens:** Using `<script type="module" src="/Main.fs">` instead of import statement.
**How to avoid:**
- Always use `<script type="module">import "/src/Main.fs";</script>` pattern
- Import path is relative to project root (where vite.config.js lives)
- Include .fs extension in import path
**Warning signs:**
- Vite dev server shows 404 for .fs file
- Error: "Failed to resolve entry for package" mentioning .fs file
- Browser console: "Failed to fetch dynamically imported module"

## Code Examples

Verified patterns from official sources:

### Minimal Elmish Program
```fsharp
// Source: https://elmish.github.io/hmr/
module App

open Elmish
open Feliz

type Model = { Count: int }
type Msg = Increment | Decrement

let init () = { Count = 0 }, Cmd.none

let update msg model =
    match msg with
    | Increment -> { model with Count = model.Count + 1 }, Cmd.none
    | Decrement -> { model with Count = model.Count - 1 }, Cmd.none

let view model dispatch =
    Html.div [
        Html.h1 "Hello CalTwo"
        Html.p $"Count: {model.Count}"
        Html.button [
            prop.onClick (fun _ -> dispatch Increment)
            prop.text "+"
        ]
        Html.button [
            prop.onClick (fun _ -> dispatch Decrement)
            prop.text "-"
        ]
    ]
```

### Entry Point with HMR
```fsharp
// Source: https://elmish.github.io/hmr/
module Main

open Fable.Core.JsInterop
open Elmish
open Elmish.React
open Elmish.HMR  // MUST be last

// Import CSS for bundling
importSideEffects "./styles.css"

// Initialize Elmish program
Program.mkProgram App.init App.update App.view
|> Program.withReactSynchronous "elmish-app"
|> Program.run
```

### package.json Scripts
```json
// Source: https://raw.githubusercontent.com/jkone27/feliz-vite/main/package.json
{
  "scripts": {
    "dev": "vite",
    "build": "vite build",
    "preview": "vite preview"
  }
}
```

### Complete vite.config.js
```javascript
// Source: https://raw.githubusercontent.com/fable-compiler/vite-plugin-fable/main/sample-project/vite.config.js
import { defineConfig } from "vite";
import fable from "vite-plugin-fable";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [
    fable({
      jsx: "automatic",
      fsproj: "./src/App.fsproj"  // Optional if .fsproj is in same dir
    }),
    react({
      include: /\.(fs|js|jsx|ts|tsx)$/  // Enable fast refresh for F# files
    })
  ],
  server: {
    port: 4000  // Optional: customize dev server port
  },
  build: {
    sourcemap: true  // Enable source maps in production
  }
});
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Webpack + fable-loader | Vite + vite-plugin-fable | Feb 2024 | 10x faster builds, simpler config, native HMR |
| Manual Fable dotnet tool | Plugin-managed Fable | v0.1.0 (2024) | No version conflicts, automatic updates |
| Fable 3.x | Fable 4.x | 2023 | React 18 support, better JSX, breaking changes |
| Feliz 1.x | Feliz 2.x | 2024 | Fable 4 compatibility, React 18/19 support |
| Program.withReact | Program.withReactSynchronous | Elmish 4.0 (2022) | Better React 18 concurrent mode support |
| Webpack devtool | Vite build.sourcemap | Vite adoption | Simpler config, automatic in dev |
| React 17 | React 18 | 2022 | Concurrent features, automatic batching |
| .NET 6 | .NET 8/10 | Nov 2023/2025 | F# 8/10 features, better performance |

**Deprecated/outdated:**
- **Fable.Template.Elmish.React:** Outdated template using Webpack, use vite-plugin-fable approach instead
- **dotnet fable watch:** Replaced by vite-plugin-fable, no longer needed
- **fable-compiler npm package:** CLI tool, not needed with vite-plugin-fable
- **Paket for F# package management:** Still works but PackageReference in .fsproj is now standard
- **Program.withReact (async):** Use withReactSynchronous for React 18 compatibility
- **React.createElement bindings:** Use Feliz DSL or Fable.Core.JSX instead
- **importAll for CSS:** Use importSideEffects for side-effect-only imports like CSS

## Open Questions

Things that couldn't be fully resolved:

1. **Femto necessity in 2026**
   - What we know: Femto syncs npm packages for F# bindings, useful for libraries with npm dependencies
   - What's unclear: Whether vite-plugin-fable auto-detects npm deps from Import() calls, making Femto optional
   - Recommendation: Document Femto as optional safety check, not required for basic Feliz/Elmish

2. **React 19 compatibility timeline**
   - What we know: Feliz 2.9.0 shows key warnings with React 19, works with React 18.3.1
   - What's unclear: When Feliz 3.x will release with full React 19 support
   - Recommendation: Pin React to 18.3.1, monitor Feliz GitHub for React 19 compatibility announcements

3. **.NET 10 vs .NET 8 for new projects**
   - What we know: .NET 10 is current LTS (Nov 2025-2028), sample projects use .NET 8
   - What's unclear: Whether all Fable packages are tested with .NET 10, potential compatibility issues
   - Recommendation: Use .NET 10 for new projects (forward-compatible), .NET 8 is safe fallback if issues arise

4. **CSS approach: plain CSS vs Tailwind vs CSS modules**
   - What we know: importSideEffects works for plain CSS, Tailwind used in some templates
   - What's unclear: Best practice for CalTwo given no styling requirements specified
   - Recommendation: Start with plain CSS (simplest), defer Tailwind decision to later phase

5. **Tutorial language level for Korean phase-01.md**
   - What we know: Tutorial must explain setup for beginners in Korean
   - What's unclear: Assumed knowledge level (command line experience, npm familiarity, etc)
   - Recommendation: Assume basic command line knowledge, explain each step with screenshots

## Sources

### Primary (HIGH confidence)
- [vite-plugin-fable Getting Started](https://fable.io/vite-plugin-fable/getting-started.html) - Official setup guide
- [vite-plugin-fable Recipes](https://fable.io/vite-plugin-fable/recipes.html) - React integration patterns
- [Elmish.HMR Documentation](https://elmish.github.io/hmr/) - HMR setup and module ordering
- [Fable Build and Run](https://fable.io/docs/javascript/build-and-run.html) - Build tooling overview
- [Microsoft .NET global.json](https://learn.microsoft.com/en-us/dotnet/core/tools/global-json) - SDK version pinning
- [Microsoft .NET 10 Overview](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10/sdk) - Current .NET version
- [Microsoft F# 10 Introduction](https://devblogs.microsoft.com/dotnet/introducing-fsharp-10/) - F# language features
- [Sample project .fsproj](https://raw.githubusercontent.com/jkone27/feliz-vite/main/src/App.fsproj) - Modern Fable 4 project
- [Sample project vite.config.ts](https://raw.githubusercontent.com/jkone27/feliz-vite/main/vite.config.ts) - Working Vite config

### Secondary (MEDIUM confidence)
- [Fable GitHub vite-plugin-fable](https://github.com/fable-compiler/vite-plugin-fable) - Plugin repository
- [Vite Build Options](https://v3.vitejs.dev/config/build-options) - Source map configuration
- [F# Interop Guide](https://medium.com/@zaid.naom/f-interop-with-javascript-in-fable-the-complete-guide-ccc5b896a59f) - importSideEffects pattern
- [Femto Introduction](https://fable.io/blog/2019/2019-06-29-Introducing-Femto.html) - npm/NuGet sync
- [Femto GitHub](https://github.com/Zaid-Ajaj/Femto) - Tool repository
- [Feliz GitHub](https://github.com/fable-hub/Feliz) - React 18/19 compatibility discussion
- [Elmish.React Docs](https://elmish.github.io/react/) - Program.withReactSynchronous

### Tertiary (LOW confidence)
- [Amplifying F# Vite plugin reveal](https://amplifyingfsharp.io/sessions/2024/02/23/) - Community announcement
- [Elmish-Feliz-DaisyUI Template](https://github.com/jasiozet/elmish-feliz-daisyui-template) - Outdated but structure reference
- WebSearch results for "Fable source maps" - Historical context, not current best practices

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - Verified from official docs, sample repos, and package registries
- Architecture: HIGH - Official vite-plugin-fable docs prescriptive, confirmed in working samples
- Pitfalls: HIGH - Directly from GitHub issues, official docs warnings, and sample project comments
- React 19 compatibility: MEDIUM - Based on GitHub issue discussions, not official support statements
- Femto necessity: MEDIUM - Tool exists and works, but unclear if vite-plugin-fable supersedes it

**Research date:** 2026-02-14
**Valid until:** 2026-03-14 (30 days - stable ecosystem, but Feliz React 19 support may arrive sooner)
