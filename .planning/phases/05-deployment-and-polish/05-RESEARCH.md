# Phase 5: Deployment & Polish - Research

**Researched:** 2026-02-14
**Domain:** Vite production build, GitHub Pages deployment, web accessibility (WCAG 2.1)
**Confidence:** HIGH

## Summary

Phase 5 focuses on deploying the CalTwo calculator to GitHub Pages and implementing accessibility improvements. The research covers three primary areas: (1) Vite build configuration and GitHub Pages deployment via GitHub Actions, (2) web accessibility standards for keyboard navigation and screen readers, and (3) documentation best practices for open source projects.

The standard approach for deploying Vite apps to GitHub Pages involves configuring a `base` path in vite.config.js, creating a GitHub Actions workflow that builds the app and uses GitHub's official deployment actions, and ensuring proper repository settings. For CalTwo (deployed to `https://ohama.github.io/CalTwo/`), the base path must be set to `'/CalTwo/'`.

Accessibility implementation requires three key elements: visible focus indicators using the `:focus-visible` CSS pseudo-class (WCAG 2.1 SC 2.4.7), proper ARIA labels for screen reader users (`aria-label` attribute), and keyboard navigation support (Tab, Enter, Space keys). Documentation should include a comprehensive README.md and Korean tutorials that explain concepts step-by-step with working code examples.

**Primary recommendation:** Use GitHub's official actions (configure-pages, upload-pages-artifact, deploy-pages) for deployment, implement :focus-visible for keyboard focus states, add aria-label to all interactive elements, and structure tutorials with clear audience targeting and digestible code snippets.

## Standard Stack

The established libraries/tools for this domain:

### Core

| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| vite | 7.x | Production build tool | Modern, fast bundler with optimized static asset handling |
| GitHub Actions | N/A (platform) | CI/CD deployment | Native GitHub Pages integration, free for public repos |
| actions/configure-pages | v5 | GitHub Pages setup | Official action for Pages metadata and environment prep |
| actions/upload-pages-artifact | v4 | Artifact packaging | Official action for gzip-compressed tar file upload (<10GB) |
| actions/deploy-pages | v4 | Pages deployment | Official action for live environment deployment |

### Supporting

| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| actions/setup-node | v6 | Node.js environment | Required for npm install and build steps |
| actions/checkout | v5 | Repository checkout | Required to access source code in workflow |
| peaceiris/actions-gh-pages | N/A | Alternative deployment | Community-maintained alternative (simpler config, less control) |

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| GitHub Actions deploy | peaceiris/actions-gh-pages | Simpler setup but less official, uses GITHUB_TOKEN differently |
| :focus-visible | :focus only | Works everywhere but shows focus ring on mouse clicks (poor UX) |
| aria-label | aria-labelledby | More flexible for complex labels but requires separate label elements |

**Installation:**

No additional packages needed. GitHub Actions uses built-in platform actions. CSS and ARIA are web standards.

## Architecture Patterns

### Recommended Project Structure

```
.github/
├── workflows/
│   ├── ci.yml           # Existing: unit + E2E tests
│   └── deploy.yml       # New: build + deploy to Pages
dist/                     # Vite build output (not committed)
tutorial/
└── phase-05.md          # Korean tutorial for this phase
README.md                # Project documentation
vite.config.js           # Must include base: '/CalTwo/'
```

### Pattern 1: GitHub Actions Deployment Workflow

**What:** A GitHub Actions workflow that builds the Vite app and deploys to GitHub Pages using official actions.

**When to use:** Required for any Vite app deploying to GitHub Pages (Vite requires build step, unlike Jekyll).

**Example:**

```yaml
# Source: https://vite.dev/guide/static-deploy
name: Deploy static content to Pages

on:
  push:
    branches: ['main']
  workflow_dispatch:

permissions:
  contents: read
  pages: write
  id-token: write

concurrency:
  group: 'pages'
  cancel-in-progress: true

jobs:
  deploy:
    environment:
      name: github-pages
      url: ${{ steps.deployment.outputs.page_url }}
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v5
      - name: Set up Node
        uses: actions/setup-node@v6
        with:
          node-version: lts/*
          cache: 'npm'
      - name: Install dependencies
        run: npm ci
      - name: Build
        run: npm run build
      - name: Setup Pages
        uses: actions/configure-pages@v5
      - name: Upload artifact
        uses: actions/upload-pages-artifact@v4
        with:
          path: './dist'
      - name: Deploy to GitHub Pages
        id: deployment
        uses: actions/deploy-pages@v4
```

### Pattern 2: Vite Base Path Configuration

**What:** Setting the `base` option in vite.config.js to ensure assets load correctly when deployed to a subdirectory.

**When to use:** Always when deploying to `https://<USERNAME>.github.io/<REPO>/` (project repositories).

**Example:**

```javascript
// Source: https://vite.dev/guide/static-deploy
import { defineConfig } from "vite";
import fable from "vite-plugin-fable";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [
    fable({
      fsproj: "src/App.fsproj",
      jsx: "automatic"
    }),
    react({
      include: /\.(fs|js|jsx|ts|tsx)$/
    })
  ],
  base: '/CalTwo/',  // CRITICAL: Must match repository name
  server: {
    port: 5173,
    strictPort: true,
  },
  build: {
    sourcemap: true
  }
});
```

**Note:** For user/org sites (username.github.io), use `base: '/'` or omit entirely.

### Pattern 3: Accessible Focus States with :focus-visible

**What:** CSS pseudo-class that shows focus indicators only when needed (keyboard navigation), not on mouse clicks.

**When to use:** All interactive elements (buttons, inputs) for WCAG 2.1 SC 2.4.7 compliance.

**Example:**

```css
/* Source: https://developer.mozilla.org/en-US/docs/Web/CSS/Reference/Selectors/:focus-visible */

/* Default focus ring removed for mouse users */
button:focus {
  outline: none;
}

/* Visible focus ring for keyboard users */
button:focus-visible {
  outline: 2px solid #4A90E2;
  outline-offset: 2px;
}

/* Fallback for older browsers */
@supports not selector(:focus-visible) {
  button:focus {
    outline: 2px solid #4A90E2;
    outline-offset: 2px;
  }
}
```

**Browser support:** Baseline widely available (Chrome 86+, Firefox 85+, Safari 15.1+, Edge 86+).

### Pattern 4: ARIA Labels for Screen Readers

**What:** Using `aria-label` attribute to provide text descriptions that screen readers announce.

**When to use:** Buttons with symbols/icons, elements where visible text doesn't fully describe purpose.

**Example:**

```fsharp
// Source: https://www.w3.org/WAI/WCAG21/Techniques/aria/ARIA6.html

// For calculator buttons
Html.button [
  prop.className "calc-button"
  prop.onClick (fun _ -> dispatch (NumberPressed "7"))
  prop.ariaLabel "Number 7"  // Screen reader announces this
  prop.text "7"
]

// For operation buttons
Html.button [
  prop.className "calc-button operator"
  prop.onClick (fun _ -> dispatch (OperatorPressed Add))
  prop.ariaLabel "Add"  // More descriptive than "+"
  prop.text "+"
]

// For display (read-only)
Html.div [
  prop.className "calc-display"
  prop.role "status"  // ARIA live region
  prop.ariaLabel "Calculator display"
  prop.text state.Display
]
```

### Pattern 5: Keyboard Navigation Support

**What:** Ensuring all functionality accessible via keyboard (Tab, Enter, Space).

**When to use:** All interactive elements for WCAG 2.1 SC 2.1.1 compliance.

**Example:**

```fsharp
// Source: https://legacy.reactjs.org/docs/accessibility.html

// Buttons are keyboard-accessible by default (use <button>, not <div>)
Html.button [
  prop.className "calc-button"
  prop.onClick (fun _ -> dispatch action)
  prop.onKeyDown (fun e ->
    // Enter and Space trigger button
    if e.key = "Enter" || e.key = " " then
      e.preventDefault()
      dispatch action
  )
  prop.text label
]

// Tab order is natural (top to bottom, left to right)
// No custom tabIndex needed for calculator buttons
```

### Anti-Patterns to Avoid

- **Hardcoding base path in HTML/CSS:** Use Vite's `base` config instead; it rewrites all asset paths automatically
- **Using :focus without :focus-visible:** Shows ugly focus rings on mouse clicks, poor UX for mouse users
- **div/span as buttons:** Not keyboard-accessible, requires custom event handlers, violates semantic HTML
- **Missing ARIA labels on icon-only buttons:** Screen readers announce nothing or generic "button"
- **Removing focus indicators entirely:** Violates WCAG 2.1 SC 2.4.7 (Level A), makes keyboard navigation impossible
- **Deploying without testing locally:** Use `npm run build && npm run preview` to test production build
- **Committing dist/ folder:** GitHub Actions builds fresh on every deploy; committing causes merge conflicts

## Don't Hand-Roll

Problems that look simple but have existing solutions:

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| GitHub Pages deployment | Custom shell scripts, FTP | GitHub Actions with official actions | Handles permissions, OIDC auth, artifact upload (10GB limit), deployment gates automatically |
| Focus state detection | JavaScript focus tracking | :focus-visible CSS | Browser heuristics detect input modality (keyboard vs mouse) better than JS |
| Keyboard navigation | Custom keydown handlers | Semantic HTML (button, input) | Built-in keyboard support, screen reader announcements, focus management |
| Screen reader labels | JavaScript text injection | aria-label, aria-labelledby | Standard, tested across AT, backward compatible |
| Base path rewriting | String concatenation in code | Vite base config | Rewrites all asset paths (CSS, JS, images) automatically at build time |
| Production build optimization | Custom webpack config | Vite build defaults | Tree-shaking, minification, code-splitting, asset hashing built-in |

**Key insight:** Web standards (ARIA, semantic HTML, CSS pseudo-classes) are extensively tested across browsers and assistive technologies. Custom solutions introduce bugs and maintenance burden.

## Common Pitfalls

### Pitfall 1: Incorrect Base Path Configuration

**What goes wrong:** After deploying, page loads but all assets (CSS, JS) return 404. App shows blank screen.

**Why it happens:** Vite defaults to `base: '/'`, which generates asset paths like `/assets/index-abc123.js`. GitHub Pages serves from `/CalTwo/`, so correct path is `/CalTwo/assets/index-abc123.js`.

**How to avoid:**
- Set `base: '/CalTwo/'` in vite.config.js (must match repository name exactly)
- Test locally with `npm run build && npm run preview` to verify paths
- Check browser DevTools Network tab for 404s before deploying

**Warning signs:**
- `npm run preview` works but GitHub Pages shows blank screen
- Browser console errors: "Failed to load resource: the server responded with a status of 404"
- Network tab shows requests to `/assets/...` instead of `/CalTwo/assets/...`

### Pitfall 2: Missing Workflow Permissions

**What goes wrong:** GitHub Actions workflow succeeds at building but fails at deploying with "Resource not accessible by integration" error.

**Why it happens:** Deployment job needs `pages: write` and `id-token: write` permissions for OIDC authentication and Pages API access. Default GITHUB_TOKEN has read-only permissions.

**How to avoid:**
```yaml
permissions:
  contents: read      # Read repository
  pages: write        # Deploy to Pages
  id-token: write     # OIDC token for auth
```

**Warning signs:**
- Workflow fails at "Deploy to GitHub Pages" step
- Error message: "Resource not accessible by integration"
- Actions log shows 403 Forbidden

### Pitfall 3: Repository Settings Not Configured

**What goes wrong:** Workflow completes successfully but site doesn't update. Old version still live.

**Why it happens:** GitHub Pages Source must be set to "GitHub Actions" in repository settings. Default is "Deploy from branch" which ignores workflow deployments.

**How to avoid:**
- Navigate to repository Settings → Pages
- Under "Build and deployment", Source dropdown: select "GitHub Actions"
- No branch selection needed (workflow handles deployment)

**Warning signs:**
- Workflow shows green checkmark but site unchanged
- Pages settings show "Deploy from branch: gh-pages"
- No deployment in Environments → github-pages

### Pitfall 4: Focus Indicators Removed for Aesthetics

**What goes wrong:** Keyboard users can't see where focus is. Tab navigation becomes unusable.

**Why it happens:** Developers remove `outline` via CSS to match design mockups, unaware of accessibility impact.

**How to avoid:**
```css
/* WRONG - removes all focus indicators */
button:focus {
  outline: none;
}

/* CORRECT - remove on mouse, show on keyboard */
button:focus {
  outline: none;
}
button:focus-visible {
  outline: 2px solid #4A90E2;
  outline-offset: 2px;
}
```

**Warning signs:**
- Designer feedback: "I don't like the blue outline"
- No visible change when tabbing through buttons
- WCAG audit fails SC 2.4.7 Focus Visible (Level A)

### Pitfall 5: Missing ARIA Labels on Icon Buttons

**What goes wrong:** Screen reader announces "button" with no context. User doesn't know what button does.

**Why it happens:** Visible text exists but only as CSS content or child element without proper association.

**How to avoid:**
```fsharp
// WRONG - screen reader announces "button"
Html.button [
  prop.className "calc-button"
  prop.text "+"
]

// CORRECT - screen reader announces "Add button"
Html.button [
  prop.className "calc-button"
  prop.ariaLabel "Add"
  prop.text "+"
]
```

**Warning signs:**
- Screen reader announces "button" without action description
- Symbol-only buttons (÷, ×, ±) have no text alternative
- WCAG audit fails SC 4.1.2 Name, Role, Value (Level A)

### Pitfall 6: Testing Only in Development Mode

**What goes wrong:** Production build fails or behaves differently than dev server.

**Why it happens:** Vite dev server uses different code paths (ES modules, no minification). Production uses Rollup bundling, tree-shaking, and minification.

**How to avoid:**
- Run `npm run build` before committing
- Run `npm run preview` to test production build locally
- Test at both `localhost:4173/` (root) and with base path `/CalTwo/`

**Warning signs:**
- F# compiler errors only appear in production build
- JavaScript runtime errors after deployment
- Assets load in dev but 404 in production

### Pitfall 7: Forgetting .NET Setup in Workflow

**What goes wrong:** GitHub Actions workflow fails with "dotnet command not found" or Fable compilation errors.

**Why it happens:** F# + Fable projects require .NET SDK to compile .fsproj to JavaScript. Node-only setup isn't sufficient.

**How to avoid:**
```yaml
steps:
  - uses: actions/checkout@v5
  - uses: actions/setup-dotnet@v4  # REQUIRED for Fable
    with:
      dotnet-version: '10.0.x'
  - uses: actions/setup-node@v6
    with:
      node-version: lts/*
  - run: dotnet tool restore        # Restore Fable CLI
  - run: npm ci
  - run: npm run build              # Calls Fable + Vite
```

**Warning signs:**
- Build succeeds locally but fails in CI
- Error: "The term 'dotnet' is not recognized"
- Fable compilation step missing from workflow logs

## Code Examples

Verified patterns from official sources:

### Complete vite.config.js for GitHub Pages

```javascript
// Source: https://vite.dev/guide/static-deploy
import { defineConfig } from "vite";
import fable from "vite-plugin-fable";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [
    fable({
      fsproj: "src/App.fsproj",
      jsx: "automatic"
    }),
    react({
      include: /\.(fs|js|jsx|ts|tsx)$/
    })
  ],
  base: '/CalTwo/',  // Must match repo name exactly
  server: {
    port: 5173,
    strictPort: true,
  },
  build: {
    sourcemap: true,
    // Vite defaults handle:
    // - Minification (esbuild)
    // - Tree-shaking (Rollup)
    // - Asset hashing
    // - Code splitting
  }
});
```

### GitHub Actions Deployment Workflow

```yaml
# Source: https://vite.dev/guide/static-deploy
# File: .github/workflows/deploy.yml
name: Deploy to GitHub Pages

on:
  push:
    branches: ['main']
  workflow_dispatch:

permissions:
  contents: read
  pages: write
  id-token: write

concurrency:
  group: 'pages'
  cancel-in-progress: true

jobs:
  deploy:
    environment:
      name: github-pages
      url: ${{ steps.deployment.outputs.page_url }}
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v5

      - name: Set up .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Set up Node
        uses: actions/setup-node@v6
        with:
          node-version: lts/*
          cache: 'npm'

      - name: Restore dotnet tools
        run: dotnet tool restore

      - name: Install dependencies
        run: npm ci

      - name: Build
        run: npm run build

      - name: Setup Pages
        uses: actions/configure-pages@v5

      - name: Upload artifact
        uses: actions/upload-pages-artifact@v4
        with:
          path: './dist'

      - name: Deploy to GitHub Pages
        id: deployment
        uses: actions/deploy-pages@v4
```

### Accessible Calculator Button Component

```fsharp
// Source: https://www.w3.org/WAI/WCAG21/Techniques/aria/ARIA6.html
// Source: https://developer.mozilla.org/en-US/docs/Web/CSS/Reference/Selectors/:focus-visible

// F# component with accessibility
let calcButton (label: string) (ariaLabel: string) (onClick: unit -> unit) =
  Html.button [
    prop.className "calc-button"
    prop.onClick (fun _ -> onClick())
    prop.ariaLabel ariaLabel
    prop.text label
  ]

// CSS for focus states
let buttonStyles = """
.calc-button {
  /* Base styles */
  padding: 1rem;
  font-size: 1.5rem;
  border: 1px solid #ccc;
  background: #fff;
  cursor: pointer;
}

/* Remove default focus ring */
.calc-button:focus {
  outline: none;
}

/* Show focus ring for keyboard navigation */
.calc-button:focus-visible {
  outline: 3px solid #4A90E2;
  outline-offset: 2px;
}

/* Ensure hover and focus states match */
.calc-button:hover,
.calc-button:focus-visible {
  background: #f0f0f0;
}

/* Fallback for older browsers */
@supports not selector(:focus-visible) {
  .calc-button:focus {
    outline: 3px solid #4A90E2;
    outline-offset: 2px;
  }
}
"""

// Usage examples
calcButton "7" "Number 7" (fun () -> dispatch (NumberPressed "7"))
calcButton "+" "Add" (fun () -> dispatch (OperatorPressed Add))
calcButton "C" "Clear" (fun () -> dispatch Clear)
```

### Accessible Display Component

```fsharp
// Source: https://www.w3.org/WAI/WCAG21/Techniques/aria/ARIA6.html

let calcDisplay (value: string) =
  Html.div [
    prop.className "calc-display"
    prop.role "status"              // ARIA live region
    prop.ariaLabel "Calculator display"
    prop.ariaLive "polite"          // Announce changes
    prop.text value
  ]

// CSS
let displayStyles = """
.calc-display {
  padding: 1rem;
  font-size: 2rem;
  text-align: right;
  background: #f9f9f9;
  border: 2px solid #333;
  min-height: 3rem;
  /* Ensure text is readable (WCAG SC 1.4.3) */
  color: #000;
  background: #fff;
}
"""
```

### README.md Template

```markdown
# CalTwo

A simple calculator built with F#, Fable, Elmish, and Feliz.

## Features

- Basic arithmetic operations (add, subtract, multiply, divide)
- Keyboard and mouse input support
- Accessible (WCAG 2.1 Level A compliant)
- Deployed to GitHub Pages

## Demo

🔗 [Live Demo](https://ohama.github.io/CalTwo/)

## Development

### Prerequisites

- [.NET SDK 10.0+](https://dotnet.microsoft.com/download)
- [Node.js 20+ (LTS)](https://nodejs.org/)

### Setup

```bash
# Clone repository
git clone https://github.com/ohama/CalTwo.git
cd CalTwo

# Restore .NET tools
dotnet tool restore

# Install dependencies
npm install

# Start dev server
npm run dev
```

Visit http://localhost:5173/

### Testing

```bash
# Unit tests
npm test

# E2E tests
npm run test:e2e

# All tests
npm run test:all
```

### Building

```bash
# Production build
npm run build

# Preview production build
npm run preview
```

## Deployment

Automatically deploys to GitHub Pages on push to `main` via GitHub Actions.

## Accessibility

- Keyboard navigation (Tab, Enter, Space)
- Screen reader support (ARIA labels)
- Visible focus indicators
- Tested with NVDA and VoiceOver

## Tech Stack

- **F#** - Language
- **Fable** - F# to JavaScript compiler
- **Elmish** - MVU architecture
- **Feliz** - React DSL
- **Vite** - Build tool
- **React** - UI rendering

## License

MIT

## Contributing

Pull requests welcome! Please run tests before submitting.
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| :focus only | :focus-visible | March 2022 (Baseline) | Better UX - no focus ring on mouse clicks |
| peaceiris/actions-gh-pages | GitHub official actions | 2022+ | More control, native integration, OIDC auth |
| Manual deployment | GitHub Actions | 2020+ | Automated, reproducible, no local build artifacts |
| aria-describedby everywhere | aria-label for simple cases | N/A | Simpler, less DOM clutter for single-line labels |
| webpack | Vite | 2020+ | 10-100x faster dev server, simpler config |
| Deploy from branch | Deploy from Actions | 2022+ | No orphan gh-pages branch, cleaner git history |

**Deprecated/outdated:**

- **:focus without :focus-visible**: Creates poor UX for mouse users (focus ring on every click). Use :focus-visible with :focus fallback for old browsers.
- **peaceiris/actions-gh-pages**: Still works but GitHub official actions (configure-pages, deploy-pages) are more integrated and future-proof. Use official unless you need CNAME file support or custom domain automation.
- **Deploying dist/ to gh-pages branch**: Creates orphan branch with build artifacts. Use GitHub Actions to build fresh on every deploy.
- **Using <div> for buttons**: Not keyboard-accessible, requires custom ARIA. Use semantic <button> element.
- **Manual ARIA role="button"**: Only needed for non-button elements. Semantic <button> has implicit role.

## Open Questions

Things that couldn't be fully resolved:

1. **vite-plugin-fable Production Optimizations**
   - What we know: vite-plugin-fable 0.2.0 is current, supports .NET 8 runtime, integrates Fable into Vite flow
   - What's unclear: Specific production build optimizations beyond standard Vite defaults, F#-specific tree-shaking behavior
   - Recommendation: Rely on Vite's Rollup integration for optimization. Test bundle size with `npm run build` and verify tree-shaking works. If issues arise, consult Fable documentation or community (https://fable.io).

2. **Korean Tutorial Technical Depth**
   - What we know: Tutorials should target beginner developers, explain step-by-step with code examples
   - What's unclear: Exact F# knowledge level of target audience (complete beginner vs familiar with functional programming)
   - Recommendation: Assume reader knows F# basics (syntax, functions, types) but not Fable, Elmish, or Vite. Explain web-specific concepts (DOM, bundling, deployment) in detail.

3. **ARIA Live Regions for Calculator Display**
   - What we know: role="status" with aria-live="polite" announces changes to screen readers
   - What's unclear: Whether every digit press should announce or only final calculation result
   - Recommendation: Start with polite announcements on every change. If too verbose during testing with screen readers, consider announcing only on operator press or equals.

4. **Accessibility Testing Tools**
   - What we know: Tools like Axe, Lighthouse, Pa11y exist for automated testing
   - What's unclear: Which tool integrates best with Playwright E2E tests for CalTwo
   - Recommendation: Use Playwright's built-in accessibility assertions (expect.accessibility) or integrate @axe-core/playwright for automated WCAG checks. Manual testing with NVDA (Windows) or VoiceOver (macOS) is also required.

## Sources

### Primary (HIGH confidence)

- [Vite - Deploying a Static Site](https://vite.dev/guide/static-deploy) - Official Vite guide for GitHub Pages deployment, base path config
- [Vite - Building for Production](https://vite.dev/guide/build) - Official build configuration and optimization docs
- [GitHub Docs - Using custom workflows with GitHub Pages](https://docs.github.com/en/pages/getting-started-with-github-pages/using-custom-workflows-with-github-pages) - Official GitHub Actions deployment workflow structure
- [MDN - :focus-visible](https://developer.mozilla.org/en-US/docs/Web/CSS/Reference/Selectors/:focus-visible) - CSS pseudo-class specification and examples
- [W3C - ARIA6: Using aria-label](https://www.w3.org/WAI/WCAG21/Techniques/aria/ARIA6.html) - Official WCAG technique for screen reader labels
- [Fable - Build and Run](https://fable.io/docs/javascript/build-and-run.html) - Official Fable build documentation

### Secondary (MEDIUM confidence)

- [GitHub - vite-plugin-fable](https://github.com/nojaf/vite-plugin-fable) - Plugin source code and documentation
- [Draft.dev - How to Write Technical Tutorials](https://draft.dev/learn/technical-tutorials) - Industry best practices for developer tutorials
- [React Accessibility Docs](https://legacy.reactjs.org/docs/accessibility.html) - React-specific accessibility guidance (applies to Feliz/React)
- [GitHub - Best README Practices](https://github.com/jehna/readme-best-practices) - Open source README templates and standards

### Tertiary (LOW confidence, flagged for validation)

- Search results for "Fable Vite GitHub Pages 2026" - Limited current examples, most from 2021-2023
- Community discussions on GitHub Pages SPA routing - Relevant for future phases if adding routing

## Metadata

**Confidence breakdown:**

- **Standard stack:** HIGH - Official GitHub and Vite documentation provide clear guidance on deployment workflow, verified by multiple community examples
- **Architecture:** HIGH - :focus-visible and aria-label are W3C standards with comprehensive MDN documentation and browser support data
- **Pitfalls:** MEDIUM-HIGH - Common issues well-documented in GitHub community discussions and Vite troubleshooting guides, but Fable-specific issues less documented

**Research date:** 2026-02-14
**Valid until:** 2026-04-14 (60 days - stable technologies with infrequent breaking changes)

**Notes:**

- GitHub Actions syntax and versions may change; verify action versions before production use
- :focus-visible browser support is Baseline (March 2022), safe to use without polyfills
- Fable + Vite combination is less common than pure JS/TS + Vite; community resources limited but core Vite patterns apply
- Accessibility standards (WCAG 2.1) are stable; WCAG 2.2 exists but 2.1 Level A is sufficient for this phase
