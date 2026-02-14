# Technology Stack

**Project:** CalTwo - F# Elmish Web Calculator
**Researched:** 2026-02-14
**Confidence:** HIGH

## Recommended Stack

### Core Framework

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| .NET SDK | 10.0 | F# compiler and tooling | Latest stable .NET with F# 10 support. Required for Fable projects. |
| Fable | 4.28.0+ | F# to JavaScript compiler | Standard transpiler for F# web apps. Mature, actively maintained. |
| Elmish | 5.0.2 | MVU architecture library | Core abstraction for model-view-update pattern. De facto standard for F# MVU apps. |
| Fable.Elmish.React | 5.0.1 | React integration for Elmish | Connects Elmish MVU to React renderer. Well-maintained, version 5.x is current. |

**Confidence:** HIGH - All versions verified from NuGet/official sources as of Feb 2026.

### UI/Rendering Layer

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| Feliz | 2.9.0 | React bindings for F# | Modern React API for Fable. Better maintained than Fable.React. Type-safe CSS. |
| React | 18.x | DOM rendering | Industry standard. Feliz provides excellent F# bindings. |

**Why Feliz over Fable.React:**
- Fable.React is less maintained
- Feliz offers type-safe CSS (90%+ coverage)
- Better hooks support
- Official recommendation for new projects
- Discoverable API with proper docs

**Confidence:** HIGH - Feliz is explicitly recommended by Fable community for new projects.

### Build Tools

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| Vite | 7.3.x | Dev server & bundler | Modern, fast bundler. Replaces Webpack in 2026 ecosystem. |
| vite-plugin-fable | Latest | Vite integration for Fable | Enables Vite to compile .fs files. Keeps compiled JS in memory (no disk IO). |
| @vitejs/plugin-react | 5.1.4 | React support in Vite | Official Vite plugin for React. Required for JSX/React features. |
| npm/pnpm | Latest | JavaScript package manager | Standard for managing JS dependencies. |

**Why Vite over Webpack:**
- Faster dev server (ESM-based)
- Modern default in 2026
- vite-plugin-fable handles F# compilation transparently
- Better DX, no need to manually run Fable CLI
- Keeps transpiled output in memory (performance)

**Confidence:** HIGH - Vite is the current standard. vite-plugin-fable is documented in official Fable resources.

### Testing

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| Fable.Mocha | 2.17.0+ | Test framework | Inspired by Expecto. Runs in node, browser, or dotnet. Standard for Fable projects. |
| Vitest | 4.0.x (optional) | Modern test runner | Vite-native testing. Alternative if you want JS-first tooling. |

**Recommendation:** Use Fable.Mocha
- F#-first API (testList, testCase primitives)
- Can run same tests in browser or node.js
- Minimal human intervention (can run headless)
- Integrates with FABLE_COMPILER directive for cross-platform tests

**Confidence:** HIGH - Fable.Mocha is well-documented and widely used in Fable ecosystem.

### Deployment

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| GitHub Pages | N/A | Static hosting | Free, simple, user specified. Perfect for SPAs. |
| GitHub Actions | N/A | CI/CD pipeline | Automate build and deploy. Native GitHub integration. |

**Deployment approach:**
1. Vite builds static bundle (HTML + JS + CSS)
2. GitHub Actions runs build on push to main
3. Deploy action (peaceiris/actions-gh-pages or JamesIves/github-pages-deploy-action) publishes to gh-pages branch
4. GitHub Pages serves static site

**Confidence:** HIGH - Standard pattern for static SPA deployment.

### Optional/Supporting Libraries

| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| Fable.Elmish.HMR | 7.0.0 | Hot Module Replacement | Dev experience enhancement. Preserves state on code changes. Recommended. |
| Fable.Core | Latest | Core Fable library | Auto-installed with Fable. Provides JS interop primitives. |

## Alternatives Considered

| Category | Recommended | Alternative | Why Not |
|----------|-------------|-------------|---------|
| React Bindings | Feliz | Fable.React | Less maintained. Feliz officially recommended for new projects. |
| Build Tool | Vite | Webpack | Webpack is older, slower. Vite is modern standard in 2026. |
| UI Framework | Elmish + React | Elmish Land | Elmish Land is full-stack framework. Overkill for simple calculator. |
| Testing | Fable.Mocha | Expecto directly | Expecto is .NET-only. Fable.Mocha runs in browser/node (better for web apps). |
| Deployment | GitHub Pages | Netlify/Vercel | User specified GitHub Pages. Others would work but add complexity. |
| MVU Library | Elmish | SAFE Stack | SAFE is full-stack (includes backend). Not needed for calculator. |

## NOT Recommended

**SAFE Stack (Suave/Saturn + Azure + Fable + Elmish):**
- Too heavy for a simple calculator
- Includes backend (ASP.NET/Giraffe/Saturn) - not needed
- Azure deployment adds complexity
- Use plain Elmish + Fable instead

**Elmish Land:**
- Modern full-stack F# framework
- Great for complex apps with routing, backend
- Overkill for single-page calculator
- Adds learning curve

**Fable.React:**
- Older React bindings
- Less maintained than Feliz
- Missing modern React features
- Official recommendation is to use Feliz

**Why keep it simple:**
This project is a basic calculator. The recommended stack (Fable + Elmish + Feliz + Vite) is the minimal viable stack for F# web apps in 2026. Anything more adds unnecessary complexity.

## Project Structure

```
CalTwo/
├── src/
│   ├── App.fs              # Main Elmish app (Model, Msg, update, view)
│   ├── Calculator.fs       # Calculator logic (pure functions)
│   └── Main.fs             # Entry point (Program.mkProgram)
├── tests/
│   └── Calculator.Tests.fs # Fable.Mocha tests
├── public/
│   └── index.html          # HTML shell
├── package.json            # npm dependencies
├── CalTwo.fsproj           # F# project file
├── vite.config.js          # Vite configuration
└── .github/
    └── workflows/
        └── deploy.yml      # GitHub Actions deployment
```

## Installation

### Prerequisites

```bash
# Install .NET SDK 10
# Download from: https://dotnet.microsoft.com/download/dotnet/10.0

# Verify installation
dotnet --version  # Should be 10.x
```

### F# Project Setup

```bash
# Create F# library project (netstandard2.0 for Fable compatibility)
dotnet new classlib -lang F# -n CalTwo -f netstandard2.0

# Add Fable packages
dotnet add package Fable.Core
dotnet add package Fable.Elmish --version 5.0.2
dotnet add package Fable.Elmish.React --version 5.0.1
dotnet add package Feliz --version 2.9.0

# Add testing (optional)
dotnet add package Fable.Mocha --version 2.17.0

# Add HMR for dev experience (optional but recommended)
dotnet add package Fable.Elmish.HMR --version 7.0.0
```

### JavaScript/Build Tools Setup

```bash
# Initialize npm project
npm init -y

# Install Vite and plugins
npm install --save-dev vite vite-plugin-fable @vitejs/plugin-react

# Install React (peer dependencies for Feliz)
npm install react react-dom

# Install testing (if using Vitest instead of Fable.Mocha)
npm install --save-dev vitest
```

### vite.config.js

```javascript
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import fable from 'vite-plugin-fable'

export default defineConfig({
  plugins: [
    fable({
      // Entry point is your F# project file
      fsproj: './CalTwo.fsproj'
    }),
    react()
  ],
  base: '/CalTwo/', // GitHub Pages deployment path (replace with repo name)
  build: {
    outDir: 'dist'
  }
})
```

### package.json scripts

```json
{
  "scripts": {
    "dev": "vite",
    "build": "vite build",
    "preview": "vite preview",
    "test": "dotnet fable tests --run mocha"
  }
}
```

## GitHub Actions Deployment

Create `.github/workflows/deploy.yml`:

```yaml
name: Deploy to GitHub Pages

on:
  push:
    branches: [main]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Setup Node
        uses: actions/setup-node@v4
        with:
          node-version: '20'

      - name: Install dependencies
        run: npm ci

      - name: Build
        run: npm run build

      - name: Deploy to GitHub Pages
        uses: peaceiris/actions-gh-pages@v4
        with:
          github_token: ${{ secrets.GITHUB_TOKEN }}
          publish_dir: ./dist
```

## Confidence Assessment

| Component | Confidence | Rationale |
|-----------|------------|-----------|
| Fable 4.x | HIGH | Stable, mature. Version 4.28.0 on NuGet verified. |
| Elmish 5.x | HIGH | Version 5.0.2 verified on NuGet. Current stable release. |
| Feliz 2.9.0 | HIGH | Latest stable on NuGet. Version 3.0 in RC, but 2.9 is production-ready. |
| Vite 7.x | HIGH | Latest stable version 7.3.x verified from npm. Current standard. |
| Fable.Mocha | HIGH | Version 2.17.0 on NuGet. Standard testing library for Fable. |
| .NET 10 with F# 10 | HIGH | Officially released with Visual Studio 2026. |
| Deployment approach | HIGH | Standard GitHub Pages pattern. Well-documented. |

## Version Strategy

**Use stable versions, not pre-release:**
- Feliz 2.9.0 (stable) NOT 3.0.0-rc.2 (pre-release)
- Fable.Elmish 5.0.2 (stable)
- Vite 7.3.x (stable)

**Rationale:** Simple calculator doesn't need bleeding edge. Stable = fewer surprises.

## Sources

### Primary Sources (HIGH confidence)

- [Fable Official Site](https://fable.io/) - Build tools and ecosystem
- [Elmish Official Docs](https://elmish.github.io/elmish/) - MVU architecture
- [Fable.Elmish NuGet](https://www.nuget.org/packages/Fable.Elmish/) - Version 5.0.2 verified
- [Fable.Elmish.React NuGet](https://www.nuget.org/packages/Fable.Elmish.React) - Version 5.0.1 verified
- [Feliz NuGet](https://www.nuget.org/packages/Feliz/) - Version 2.9.0 verified
- [Feliz GitHub](https://github.com/Zaid-Ajaj/Feliz) - React bindings docs
- [Vite Official Releases](https://vite.dev/releases) - Version 7.3.x verified
- [Fable.Mocha GitHub](https://github.com/Zaid-Ajaj/Fable.Mocha) - Testing library
- [vite-plugin-fable Docs](https://fable.io/vite-plugin-fable/) - Vite integration

### Secondary Sources (MEDIUM confidence)

- [SAFE Stack Docs](https://safe-stack.github.io/) - Alternative full-stack approach (not recommended here)
- [Elmish Land](https://elmish.land/) - Modern full-stack F# (overkill for this project)
- [GitHub Actions for GitHub Pages](https://github.com/peaceiris/actions-gh-pages) - Deployment
- [.NET 10 Download](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) - SDK installation
- [F# 10 Announcement](https://devblogs.microsoft.com/dotnet/introducing-fsharp-10/) - Language features

### Community Insights

- [Compositional IT: Working with React Components in F#](https://www.compositional-it.com/news-blog/working-with-react-components-in-fsharp/) - Practical guidance
- [Amplifying F#: Vite Plugin Reveal](https://amplifyingfsharp.io/sessions/2024/02/23/) - Vite integration background
- [Medium: Feliz Navidad (Feliz + Vite + Fable)](https://jkone27-3876.medium.com/feliz-navidad-fd1869b31044) - Real-world setup example

## Next Steps

After stack is implemented:

1. **Verify build works:** `npm run dev` should start Vite dev server
2. **Verify Fable compilation:** Check browser console for F# compilation errors
3. **Verify HMR:** Edit .fs file, check if app updates without full refresh
4. **Verify tests run:** `npm run test` should execute Fable.Mocha tests
5. **Verify deployment:** Push to main, check GitHub Actions, verify GitHub Pages site

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| vite-plugin-fable issues | LOW | HIGH | Well-maintained plugin. Fallback: use fable CLI + webpack |
| GitHub Pages routing | MEDIUM | LOW | SPA on GitHub Pages works if no HTML5 routing (calculator doesn't need routes) |
| Feliz v3 breaking changes | LOW | MEDIUM | Stay on Feliz 2.9.0 (stable). Upgrade later if needed. |
| .NET 10 breaking changes | LOW | LOW | F# 10 is refinement release. Minimal breaking changes from F# 9. |
| Fable compatibility | LOW | HIGH | Fable 4.x is mature and stable. Wide ecosystem adoption. |

## Summary

The recommended stack is the **minimal viable modern F# web stack** for 2026:

- **F#:** .NET 10 + F# 10
- **Transpiler:** Fable 4.x
- **Architecture:** Elmish 5.x (MVU pattern)
- **UI:** Feliz 2.9 (React bindings) + React 18
- **Build:** Vite 7.x + vite-plugin-fable
- **Testing:** Fable.Mocha 2.x
- **Deployment:** GitHub Pages via GitHub Actions

This stack is:
- **Modern:** All tools current as of 2026
- **Simple:** No unnecessary frameworks (SAFE, Elmish Land)
- **Proven:** Well-documented, actively maintained
- **Type-safe:** End-to-end F# with type-safe CSS
- **Fast:** Vite provides excellent dev experience
- **Free:** GitHub Pages hosting costs nothing

**Do NOT add:** SAFE Stack, Elmish Land, Fable.React, Webpack, or backend frameworks. Keep it simple.
