# Phase 4: E2E Testing & CI - Research

**Researched:** 2026-02-14
**Domain:** Browser automation testing (Playwright) and Continuous Integration (GitHub Actions)
**Confidence:** HIGH

## Summary

This research investigated the best practices for implementing end-to-end (E2E) testing using Playwright with a Fable/React application, along with GitHub Actions CI/CD integration. Playwright is the current industry standard for browser automation testing, supporting all major browsers (Chromium, Firefox, WebKit) with a robust auto-waiting mechanism that eliminates flaky tests.

The standard approach for a Vite-based React project is to install Playwright via npm, configure it to automatically start the dev server before tests, write tests using user-facing locators (particularly `getByRole`), and integrate with GitHub Actions for automated testing on every push. For F#/Fable projects, Playwright tests are written in TypeScript/JavaScript and interact with the compiled JavaScript output, testing the application as users would experience it.

Since the project uses Vite as the dev server and already has a unit test infrastructure with Fable.Mocha, Playwright will be added as a separate E2E testing layer that runs tests in real browsers. The tests will verify user interactions (clicking buttons, seeing display updates) rather than testing F# implementation details.

**Primary recommendation:** Use Playwright 1.58.x with TypeScript for E2E tests, configure webServer to auto-start Vite dev server, prioritize `getByRole` locators for accessibility and resilience, enable screenshot/trace capture on failure, and integrate with GitHub Actions using the official workflow template.

## Standard Stack

The established libraries/tools for browser E2E testing in 2026:

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| @playwright/test | 1.58.x | Browser automation testing framework | Official Microsoft framework with auto-waiting, cross-browser support, and excellent DX. Industry standard for E2E testing. |
| playwright | 1.58.x | Browser binaries (Chromium, Firefox, WebKit) | Bundled browsers ensure consistent behavior across environments. Auto-installed with @playwright/test. |
| Node.js | 20.x / 22.x / 24.x LTS | Runtime for Playwright tests | Required for running Playwright. Project already uses Node.js for Vite. |
| TypeScript | Latest | Type-safe test authoring | Playwright has first-class TypeScript support out of the box. Catches errors before runtime. |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| @types/node | Latest | Node.js type definitions | Required for TypeScript test files to recognize Node APIs |
| actions/setup-node | v5 | GitHub Actions Node.js setup | CI workflow automation |
| actions/cache | v4 | Cache dependencies in CI | Speed up CI runs by caching node_modules and Playwright binaries |
| actions/upload-artifact | v4 | Upload test reports/screenshots | Store test artifacts for debugging failures |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Playwright | Selenium WebDriver | Selenium is older, requires manual waits, more flaky. Playwright is faster and more reliable. |
| Playwright | Cypress | Cypress runs in-browser (can't test multiple tabs/origins easily), slower, paid features for parallelization. Playwright is more powerful and fully open-source. |
| TypeScript tests | JavaScript tests | TypeScript provides better IDE support, type safety, and catches errors before running tests. Minimal overhead. |
| GitHub Actions | Other CI platforms | GitHub Actions is free for public repos, deeply integrated with GitHub, and has official Playwright support. |

**Installation:**
```bash
# Install Playwright with test runner and browsers
npm init playwright@latest

# Or manually add to existing project
npm install -D @playwright/test
npx playwright install --with-deps

# Install TypeScript if not already present
npm install -D typescript @types/node
```

## Architecture Patterns

### Recommended Project Structure
```
CalTwo/
├── e2e/                     # E2E test directory (separate from unit tests)
│   ├── fixtures/            # Reusable test fixtures
│   ├── pages/               # Page Object Models (optional for larger suites)
│   └── calculator.spec.ts   # E2E test files
├── playwright.config.ts     # Playwright configuration
├── tests/                   # Existing unit tests (Fable.Mocha)
└── src/                     # Application source
```

**Rationale:** Separate E2E tests from unit tests because they:
- Use different tooling (Playwright vs Fable.Mocha)
- Run in browsers vs Node.js
- Test different concerns (user flows vs business logic)
- Have different execution speeds

### Pattern 1: Web Server Auto-Start
**What:** Configure Playwright to automatically start Vite dev server before tests
**When to use:** Always for local development and CI (eliminates manual "npm run dev" step)
**Example:**
```typescript
// playwright.config.ts
// Source: https://playwright.dev/docs/test-webserver
import { defineConfig } from '@playwright/test';

export default defineConfig({
  webServer: {
    command: 'npm run dev',
    url: 'http://localhost:5173',
    reuseExistingServer: !process.env.CI,
    timeout: 120000,
  },
  use: {
    baseURL: 'http://localhost:5173',
  },
});
```

### Pattern 2: User-Facing Locators
**What:** Use `getByRole`, `getByLabel`, `getByText` instead of CSS selectors
**When to use:** Always (primary locator strategy)
**Example:**
```typescript
// Source: https://playwright.dev/docs/locators
// GOOD: Resilient to UI changes, mirrors user behavior
await page.getByRole('button', { name: '7' }).click();
await page.getByRole('button', { name: '+' }).click();
await expect(page.getByTestId('display')).toHaveText('5');

// BAD: Brittle, breaks when CSS changes
await page.locator('.calc-button.number-7').click();
```

### Pattern 3: Page Object Model (Optional)
**What:** Encapsulate page interactions in reusable classes
**When to use:** When test suite grows beyond 5-10 test files, or when multiple tests share complex interactions
**Example:**
```typescript
// Source: https://playwright.dev/docs/pom
// e2e/pages/CalculatorPage.ts
export class CalculatorPage {
  constructor(private page: Page) {}

  async clickNumber(num: string) {
    await this.page.getByRole('button', { name: num }).click();
  }

  async clickOperator(op: string) {
    await this.page.getByRole('button', { name: op }).click();
  }

  async getDisplay() {
    return await this.page.getByTestId('display').textContent();
  }
}

// e2e/calculator.spec.ts
test('adds two numbers', async ({ page }) => {
  const calc = new CalculatorPage(page);
  await page.goto('/');
  await calc.clickNumber('2');
  await calc.clickOperator('+');
  await calc.clickNumber('3');
  await calc.clickOperator('=');
  await expect(page.getByTestId('display')).toHaveText('5');
});
```

### Pattern 4: Test Isolation with beforeEach
**What:** Each test starts with a fresh page state
**When to use:** Always (prevents test interdependence)
**Example:**
```typescript
// Source: https://playwright.dev/docs/best-practices
test.describe('Calculator E2E Tests', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test('adds two numbers', async ({ page }) => {
    // Test starts at homepage
  });

  test('handles divide by zero', async ({ page }) => {
    // Fresh start, not affected by previous test
  });
});
```

### Pattern 5: Headless CI with Screenshot on Failure
**What:** Run tests without visible browser, capture screenshots when tests fail
**When to use:** Always in CI, optional locally
**Example:**
```typescript
// Source: https://playwright.dev/docs/test-use-options
export default defineConfig({
  use: {
    headless: true,
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
    trace: 'retain-on-failure',
  },
});
```

### Anti-Patterns to Avoid
- **Manual waits:** Never use `page.waitForTimeout(5000)`. Playwright auto-waits for elements to be ready. Use `expect(locator).toBeVisible()` instead.
- **CSS/XPath selectors:** Avoid `.calc-button` or `//div[@class='display']`. Use `getByRole('button')` or `getByTestId('display')`.
- **Shared state:** Don't rely on previous tests running first. Each test should be independent.
- **Testing implementation details:** Don't test F# function internals. Test user-visible behavior (button clicks, display output).
- **100% coverage obsession:** Focus on critical user flows (calculation, error handling) not every edge case.
- **ElementHandles:** Avoid `const button = await page.$('.btn')`. Use Locators with auto-retry: `page.locator('.btn')`.

## Don't Hand-Roll

Problems that look simple but have existing solutions:

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Starting dev server before tests | Shell script or manual `npm run dev` | Playwright `webServer` config | Handles port waiting, process cleanup, CI detection automatically |
| Waiting for elements | Custom sleep/delay functions | Playwright auto-waiting + web-first assertions | Playwright retries assertions until timeout, eliminating flaky tests |
| Screenshot on failure | try/catch with screenshot() calls | `screenshot: 'only-on-failure'` config | Automatic, consistent, includes full page and trace viewer |
| Parallel test execution | Custom worker pool | Playwright built-in parallelization | Runs tests across multiple workers by default, optimized for CI |
| Browser management | Installing/updating Chrome manually | `npx playwright install` | Downloads exact browser versions, handles OS dependencies |
| CI test reports | Custom HTML generator | Playwright HTML Reporter | Built-in, shows screenshots, traces, filterable, no setup |
| Test retry logic | Manual retry loops | `retries: 2` in config | Smart retries only on failure, captures failure artifacts |

**Key insight:** Playwright is a batteries-included framework. Most "supporting infrastructure" you might think to build (retries, screenshots, parallel execution, browser management) is already built-in and production-tested. Leverage these features rather than implementing your own.

## Common Pitfalls

### Pitfall 1: Not Using data-testid for Dynamic Elements
**What goes wrong:** Tests break when button text changes or UI is refactored
**Why it happens:** Over-reliance on text-based selectors like `getByText('Calculate')` which change during localization or UI polish
**How to avoid:** Add `data-testid` attributes to key UI elements during development:
```fsharp
// src/Calculator.fs - F# component
Html.div [
  prop.testId "display"  // Feliz shorthand for data-testid
  prop.text model.Display
]
```
Then use in tests:
```typescript
await expect(page.getByTestId('display')).toHaveText('5');
```
**Warning signs:** Test failures after text changes, localization work, or CSS refactoring

### Pitfall 2: Forgetting --with-deps on CI
**What goes wrong:** Tests fail in CI with "Browser not found" or "Missing system dependencies" errors
**Why it happens:** `npx playwright install` downloads browsers but not OS-level dependencies (fonts, codecs, etc.)
**How to avoid:** Always use `npx playwright install --with-deps` in CI workflows
**Warning signs:** Tests pass locally but fail in GitHub Actions with cryptic browser errors

### Pitfall 3: Not Configuring Vite Port Correctly
**What goes wrong:** Playwright tries to connect to localhost:5173 but Vite is on a different port
**Why it happens:** Vite auto-increments port if 5173 is taken, or CI uses different port
**How to avoid:** Lock Vite port in `vite.config.js`:
```javascript
export default defineConfig({
  server: {
    port: 5173,
    strictPort: true, // Fail if port is taken
  },
  // ... rest of config
});
```
**Warning signs:** Tests timeout waiting for server, "ECONNREFUSED" errors

### Pitfall 4: Flaky Tests from Race Conditions
**What goes wrong:** Tests pass sometimes, fail other times (especially in CI)
**Why it happens:** Not waiting for asynchronous state updates in Elmish/React
**How to avoid:** Use web-first assertions that retry:
```typescript
// BAD: Checks immediately, might catch mid-update
const text = await page.getByTestId('display').textContent();
expect(text).toBe('5');

// GOOD: Retries assertion until timeout
await expect(page.getByTestId('display')).toHaveText('5');
```
**Warning signs:** Tests fail in CI but pass locally, intermittent failures

### Pitfall 5: Over-Complicated Page Objects Too Early
**What goes wrong:** Spend days building elaborate Page Object framework before writing first test
**Why it happens:** Following "enterprise patterns" from large test suites
**How to avoid:** Start with simple inline tests. Extract Page Objects only when you have 3+ tests duplicating the same interactions
**Warning signs:** More Page Object code than actual tests, tests still not written

### Pitfall 6: Not Caching Playwright Binaries in CI
**What goes wrong:** CI runs take 2+ minutes just to install browsers on every run
**Why it happens:** GitHub Actions doesn't cache `~/.cache/ms-playwright` by default
**How to avoid:** Add caching step in workflow (see CI configuration below)
**Warning signs:** CI logs show "Downloading browsers" on every run

## Code Examples

Verified patterns from official sources:

### Basic E2E Test Structure
```typescript
// Source: https://playwright.dev/docs/best-practices
// e2e/calculator.spec.ts
import { test, expect } from '@playwright/test';

test.describe('Calculator E2E', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test('performs addition', async ({ page }) => {
    await page.getByRole('button', { name: '2' }).click();
    await page.getByRole('button', { name: '+' }).click();
    await page.getByRole('button', { name: '3' }).click();
    await page.getByRole('button', { name: '=' }).click();

    await expect(page.getByTestId('display')).toHaveText('5');
  });

  test('shows error on divide by zero', async ({ page }) => {
    await page.getByRole('button', { name: '5' }).click();
    await page.getByRole('button', { name: '÷' }).click();
    await page.getByRole('button', { name: '0' }).click();
    await page.getByRole('button', { name: '=' }).click();

    await expect(page.getByTestId('display')).toHaveText('Error');
  });

  test('runs headless in CI', async ({ page }) => {
    // Headless mode configured in playwright.config.ts
    // This test verifies behavior is same in headless
    await page.getByRole('button', { name: '1' }).click();
    await expect(page.getByTestId('display')).toHaveText('1');
  });
});
```

### Complete Playwright Configuration
```typescript
// Source: https://playwright.dev/docs/test-configuration
// playwright.config.ts
import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: 'html',

  use: {
    baseURL: 'http://localhost:5173',
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
  },

  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    // Optional: Add Firefox and WebKit for cross-browser testing
    // {
    //   name: 'firefox',
    //   use: { ...devices['Desktop Firefox'] },
    // },
  ],

  webServer: {
    command: 'npm run dev',
    url: 'http://localhost:5173',
    reuseExistingServer: !process.env.CI,
    timeout: 120 * 1000,
  },
});
```

### GitHub Actions Workflow
```yaml
# Source: https://playwright.dev/docs/ci-intro
# .github/workflows/playwright.yml
name: Playwright Tests
on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  test:
    timeout-minutes: 60
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v5

    - uses: actions/setup-node@v5
      with:
        node-version: lts/*

    # Cache node_modules
    - name: Cache dependencies
      uses: actions/cache@v4
      with:
        path: |
          ~/.npm
          node_modules
        key: ${{ runner.os }}-node-${{ hashFiles('**/package-lock.json') }}

    - name: Install dependencies
      run: npm ci

    # Cache Playwright binaries
    - name: Cache Playwright browsers
      uses: actions/cache@v4
      id: playwright-cache
      with:
        path: ~/.cache/ms-playwright
        key: ${{ runner.os }}-playwright-${{ hashFiles('**/package-lock.json') }}

    - name: Install Playwright Browsers
      if: steps.playwright-cache.outputs.cache-hit != 'true'
      run: npx playwright install --with-deps

    - name: Install OS dependencies (if cache hit)
      if: steps.playwright-cache.outputs.cache-hit == 'true'
      run: npx playwright install-deps

    - name: Run unit tests
      run: npm test

    - name: Run Playwright tests
      run: npx playwright test

    - uses: actions/upload-artifact@v4
      if: always()
      with:
        name: playwright-report
        path: playwright-report/
        retention-days: 30
```

### Locator Strategy Examples
```typescript
// Source: https://playwright.dev/docs/locators
// Priority order: Role > Label > Placeholder > Text > TestId

// BEST: Accessible role-based (mirrors user/screen reader)
await page.getByRole('button', { name: '7' }).click();
await page.getByRole('heading', { name: 'Calculator' }).isVisible();

// GOOD: Form labels
await page.getByLabel('Calculation result').textContent();

// GOOD: Explicit test IDs (stable contract)
await expect(page.getByTestId('display')).toHaveText('5');

// OKAY: Text content (can change with copy updates)
await page.getByText('Error').isVisible();

// AVOID: CSS selectors (brittle, implementation detail)
await page.locator('.calc-button').click(); // BAD

// AVOID: XPath (hard to read, breaks easily)
await page.locator('//button[@class="calc-button"]').click(); // BAD
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Selenium WebDriver | Playwright | ~2020 (Playwright 1.0 released) | Auto-waiting eliminates manual sleeps, 10x faster execution, better debugging tools |
| Manual browser installation | `playwright install` | Playwright 1.8 (2021) | Consistent browser versions across environments, handles OS dependencies |
| Page.waitForSelector() | Auto-waiting Locators | Playwright 1.0 philosophy | Fewer flaky tests, cleaner test code |
| Custom retry logic | Built-in test retries | Playwright Test runner (~2021) | Automatic retry on failure, configurable per environment |
| Manual screenshot capture | Auto-capture on failure | Playwright 1.10+ | Always get screenshots/traces for failed tests without boilerplate |
| Cypress (in-browser) | Playwright (out-of-process) | 2020-2023 shift | Can test multiple tabs/origins, faster, no paid tiers for parallelization |

**Deprecated/outdated:**
- **@playwright/test <1.50:** Earlier versions had less stable trace viewer. Use 1.58.x (latest as of Feb 2026).
- **Selenium WebDriver for new projects:** Still maintained but considered legacy. Playwright is the modern choice.
- **Puppeteer for cross-browser testing:** Puppeteer is Chromium-only. Playwright supports Chromium, Firefox, WebKit.
- **ElementHandles:** Playwright docs recommend Locators over ElementHandles (ElementHandles don't auto-wait).

## Open Questions

Things that couldn't be fully resolved:

1. **F#/Fable-specific Playwright integration**
   - What we know: Playwright tests are written in TypeScript/JavaScript and test the compiled output
   - What's unclear: No documented patterns for F#/Fable projects specifically
   - Recommendation: Treat E2E tests as black-box testing. Playwright tests interact with the browser DOM, which is framework-agnostic. The F# source code doesn't need to be aware of Playwright. Just ensure UI elements have good `data-testid` attributes.

2. **Korean tutorial resources for Playwright**
   - What we know: Playwright official docs are in English, with community translations for some languages
   - What's unclear: No official Korean translation found for Playwright docs
   - Recommendation: Tutorial (tutorial/phase-04.md) should be original Korean content explaining Playwright setup step-by-step. Reference official English docs for technical details, but write explanations in Korean for beginners. Include Korean comments in code examples.

3. **Optimal test-to-coverage ratio**
   - What we know: Best practices recommend testing critical user flows, not 100% coverage
   - What's unclear: Exact number of E2E tests appropriate for a simple calculator app
   - Recommendation: Start with 3-5 core tests (addition, error handling, multi-step calculation, keyboard input, display clear). Add more if gaps found. E2E tests are slower than unit tests, so keep suite focused.

4. **Cross-browser testing necessity**
   - What we know: Playwright supports Chromium, Firefox, WebKit. Modern browsers have good standards compliance.
   - What's unclear: Whether calculator app needs all three browsers tested
   - Recommendation: Start with Chromium only for speed. Add Firefox/WebKit if user reports browser-specific bugs. Vite/React are well-tested across browsers, so issues are unlikely for a simple calculator.

## Sources

### Primary (HIGH confidence)
- [Playwright Official Documentation - Installation](https://playwright.dev/docs/intro) - Setup and installation steps
- [Playwright Official Documentation - Best Practices](https://playwright.dev/docs/best-practices) - Recommended patterns and test structure
- [Playwright Official Documentation - CI Setup](https://playwright.dev/docs/ci-intro) - GitHub Actions integration
- [Playwright Official Documentation - Locators](https://playwright.dev/docs/locators) - Locator strategies and examples
- [Playwright Official Documentation - Web Server](https://playwright.dev/docs/test-webserver) - Auto-start dev server configuration
- [Playwright Official Documentation - Configuration](https://playwright.dev/docs/test-configuration) - Full config options
- [Playwright Official Documentation - Test Use Options](https://playwright.dev/docs/test-use-options) - Screenshot/video/trace settings
- [Playwright Official Documentation - Page Object Model](https://playwright.dev/docs/pom) - POM pattern examples
- [Playwright Official Documentation - Test Retries](https://playwright.dev/docs/test-retries) - Retry mechanism configuration
- [Playwright Official Documentation - Trace Viewer](https://playwright.dev/docs/trace-viewer) - Debugging with trace viewer

### Secondary (MEDIUM confidence)
- [BrowserStack - Playwright Best Practices 2026](https://www.browserstack.com/guide/playwright-best-practices) - Community best practices compilation
- [BrowserStack - Playwright Selectors Best Practices](https://www.browserstack.com/guide/playwright-selectors-best-practices) - Selector strategy guidance
- [Better Stack - Playwright Best Practices and Pitfalls](https://betterstack.com/community/guides/testing/playwright-best-practices/) - Common pitfalls catalog
- [Better Stack - Avoiding Flaky Tests](https://betterstack.com/community/guides/testing/avoid-flaky-playwright-tests/) - Flaky test patterns
- [Medium - Stop Writing Flaky Tests](https://medium.com/@anna_tomka/stop-writing-flaky-tests-how-to-avoid-common-playwright-mistakes-425da48b82d4) - Mistake patterns
- [Medium - React Setup with Playwright and GitHub Actions](https://medium.com/@peturgeorgievv/react-setup-and-ci-cd-for-end-to-end-e2e-testing-with-playwright-typescript-and-github-actions-950809ead7ba) - React-specific setup
- [Medium - End-to-End Testing in React with Playwright](https://medium.com/@oshadhadushan/end-to-end-testing-in-react-with-playwright-a-step-by-step-integration-guide-00e32effbd15) - Step-by-step React integration
- [GitHub Issue - Caching Playwright Dependencies](https://github.com/jbranchaud/til/blob/master/github-actions/cache-playwright-dependencies-across-workflows.md) - CI caching patterns
- [Medium - Playwright with Vite Component Testing](https://mickydore.medium.com/adding-playwright-tests-to-your-vite-project-with-code-coverage-f6cfa65f0209) - Vite integration
- [The Candid Startup - Component Testing with Playwright and Vitest](https://www.thecandidstartup.org/2025/01/06/component-test-playwright-vitest.html) - Modern Vite/Playwright patterns
- [Medium - Playwright Page Object Model Best Practices](https://medium.com/@anandpak108/page-object-model-in-playwright-with-typescript-best-practices-133fb349c462) - POM implementation
- [Codilime - Page Object Model with Playwright and TypeScript](https://codilime.com/blog/page-object-model-with-playwright-and-typescript/) - POM patterns

### Tertiary (LOW confidence)
- Web search results on Playwright versions - npm registry data via search (could not directly access npmjs.com)
- Community discussions on F#/Fable + Playwright integration - no official documentation found, inferred from general Playwright principles

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - Official Playwright documentation is comprehensive and current. Version 1.58.x confirmed via multiple sources.
- Architecture: HIGH - Patterns sourced from official docs and verified community practices. WebServer config, locator strategies, and POM patterns are well-documented.
- Pitfalls: HIGH - Common mistakes catalogued from both official best practices docs and recent community articles (2025-2026).
- F#/Fable integration: MEDIUM - No specific documentation, but Playwright's framework-agnostic nature means standard TypeScript tests work fine. Approach is sound but not F#-specific.
- Korean tutorial resources: LOW - No official Korean Playwright docs found. Tutorial will need to be original content.

**Research date:** 2026-02-14
**Valid until:** ~2026-04-14 (60 days - Playwright is stable, E2E testing patterns change slowly)
