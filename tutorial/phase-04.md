# Phase 4: E2E 테스트와 CI/CD (E2E Testing & CI/CD)

## 개요

이 튜토리얼에서는 Playwright를 사용한 브라우저 자동화 테스트와 GitHub Actions로 구현하는 CI/CD 파이프라인을 다룹니다. 단위 테스트가 개별 함수의 정확성을 검증했다면, E2E 테스트는 실제 사용자 관점에서 전체 애플리케이션이 제대로 동작하는지 확인합니다.

### 학습 목표

- E2E 테스트의 개념과 단위 테스트와의 차이점 이해
- Playwright 브라우저 자동화 도구 사용법 익히기
- 웹 우선 검증(Web-First Assertions)과 자동 대기(Auto-Waiting) 이해
- GitHub Actions로 CI/CD 파이프라인 구축하기
- 테스트 결과 아티팩트 수집 및 관리
- 로컬 환경에서 E2E 테스트 실행 및 디버깅

### 완성 후 결과물

- 10개의 E2E 테스트로 검증된 계산기 애플리케이션
- Playwright 헤드리스 모드 및 UI 모드 실행 환경
- GitHub에 코드 푸시 시 자동으로 실행되는 CI 워크플로
- 실패 시 스크린샷과 비디오를 자동 수집하는 테스트 리포트
- 로컬에서 pre-push 검증을 위한 `npm run test:all` 스크립트

---

## 1. E2E 테스트란?

### 1.1 E2E (End-to-End) 테스트의 정의

E2E 테스트는 애플리케이션을 처음부터 끝까지(End-to-End) 사용자 관점에서 검증하는 테스트입니다. 실제 브라우저를 자동으로 조작하여 클릭, 입력, 페이지 이동 등을 시뮬레이션하고, 화면에 표시되는 결과를 검증합니다.

**실제 예시:**
```
사용자 시나리오: 2 + 3 = 5 계산하기

1. 브라우저에서 http://localhost:5173 열기
2. "2" 버튼 클릭
3. "+" 버튼 클릭
4. "3" 버튼 클릭
5. "=" 버튼 클릭
6. 디스플레이에 "5"가 표시되는지 확인
```

### 1.2 단위 테스트 vs. E2E 테스트

**단위 테스트 (Unit Test):**
```fsharp
// Phase 2에서 작성한 단위 테스트
testCase "2 + 3 = shows '5'" <| fun () ->
    let m0 = init ()
    let m1 = update (DigitPressed 2) m0
    let m2 = update (OperatorPressed Add) m1
    let m3 = update (DigitPressed 3) m2
    let m4 = update EqualsPressed m3
    Expect.equal m4.Display "5" "Should calculate 2 + 3 = 5"
```

- **검증 대상:** `update` 함수의 로직만 검증
- **환경:** 브라우저 없이 F# 런타임에서 실행
- **속도:** 매우 빠름 (밀리초 단위)
- **범위:** 좁고 깊음 (특정 함수의 모든 엣지 케이스)

**E2E 테스트 (End-to-End Test):**
```typescript
// Phase 4에서 작성하는 E2E 테스트
test('adds two numbers: 2 + 3 = 5', async ({ page }) => {
  await page.getByRole('button', { name: '2' }).click();
  await page.getByRole('button', { name: '+' }).click();
  await page.getByRole('button', { name: '3' }).click();
  await page.getByRole('button', { name: '=' }).click();
  await expect(page.getByTestId('display')).toHaveText('5');
});
```

- **검증 대상:** 전체 애플리케이션 (UI + 로직 + 렌더링)
- **환경:** 실제 Chromium 브라우저에서 실행
- **속도:** 느림 (초 단위)
- **범위:** 넓고 얕음 (주요 사용자 시나리오)

### 1.3 계산기 예시로 이해하기

**단위 테스트가 확인하는 것:**
- `update` 함수가 `DigitPressed 2` 메시지를 받으면 Display가 "2"로 변하는가?
- `doMathOp Add 2.0 3.0`이 `Success 5.0`을 반환하는가?

**E2E 테스트가 확인하는 것:**
- 화면에 "2" 버튼이 보이는가?
- 그 버튼을 클릭하면 디스플레이에 "2"가 표시되는가?
- CSS 스타일이 제대로 적용되어 버튼이 클릭 가능한가?
- 여러 연산을 연속으로 수행해도 화면이 정상 작동하는가?

**결론:** 둘 다 필요합니다!
- 단위 테스트: 로직의 정확성 보장 (27개 테스트)
- E2E 테스트: 사용자 경험 검증 (10개 테스트)

---

## 2. Playwright 소개

### 2.1 Playwright란?

Playwright는 Microsoft가 개발한 최신 브라우저 자동화 프레임워크입니다. 웹 애플리케이션을 실제 브라우저에서 자동으로 조작하고 테스트할 수 있습니다.

**주요 특징:**
- **크로스 브라우저:** Chromium, Firefox, WebKit (Safari 엔진) 지원
- **자동 대기 (Auto-Waiting):** 요소가 준비될 때까지 자동 대기, `sleep()` 불필요
- **웹 우선 검증:** 비동기 검증으로 불안정한 테스트 방지
- **헤드리스/헤드풀:** 화면 없이 실행 또는 브라우저 창 보면서 실행
- **강력한 디버깅:** UI 모드, 타임 트래블, 트레이스 뷰어

### 2.2 왜 Playwright인가?

**다른 도구와의 비교:**

| 도구       | 장점                       | 단점                          |
| ---------- | -------------------------- | ----------------------------- |
| Selenium   | 오래됨, 많은 예제          | 느림, 수동 대기 필요, 불안정  |
| Cypress    | 빠름, 개발자 친화적        | Chrome 전용, iframe 제한      |
| Playwright | 빠름, 자동 대기, 크로스 브라우저 | 비교적 최신 (2020년)         |

**이 프로젝트 선택 이유:**
- 자동 대기 덕분에 안정적인 테스트
- TypeScript 지원으로 타입 안전성
- Chromium만 사용하여 설치 크기 최소화
- GitHub Actions와 완벽 통합

### 2.3 Playwright 버전 및 의존성

**이 프로젝트에서 사용:**
```json
{
  "devDependencies": {
    "@playwright/test": "^1.58.2",
    "typescript": "^5.8.2",
    "@types/node": "^22.12.6"
  }
}
```

- **@playwright/test 1.58.2:** 테스트 러너 및 검증 라이브러리
- **typescript 5.8.2:** TypeScript 컴파일러 (E2E 테스트는 TypeScript로 작성)
- **@types/node:** Node.js 타입 정의 (TypeScript에서 Node API 사용)

---

## 3. Playwright 설치 및 설정

### 3.1 패키지 설치

```bash
npm install -D @playwright/test typescript @types/node
```

**설치되는 내용:**
- Playwright 테스트 러너
- TypeScript 컴파일러 및 타입 정의
- 약 50MB 정도의 패키지들

### 3.2 Chromium 브라우저 설치

```bash
npx playwright install chromium
```

**설치되는 내용:**
- Chromium 브라우저 바이너리 (~200MB)
- `~/.cache/ms-playwright/chromium-XXXX/` 경로에 설치
- 프로젝트마다 브라우저를 공유 (디스크 공간 절약)

**왜 Chromium만 설치하나?**
- 대부분의 사용자가 Chrome/Edge 사용 (Chromium 기반)
- Firefox/WebKit까지 설치하면 ~500MB 추가
- CI 환경에서 빌드 시간 단축

**전체 브라우저 설치 (옵션):**
```bash
npx playwright install  # Chromium + Firefox + WebKit
```

### 3.3 프로젝트 구조 변경

E2E 테스트를 위해 일부 파일을 수정해야 합니다.

**src/App.fs - data-testid 추가:**
```fsharp
// 디스플레이 div에 테스트 ID 추가
Html.div [
    prop.testId "display"  // 이 줄 추가!
    prop.style [
        style.fontSize 32
        // ...
    ]
    prop.text model.Display
]
```

**왜 필요한가?**
- E2E 테스트에서 디스플레이 요소를 찾기 위해
- CSS 클래스나 태그명은 변경될 수 있지만, `data-testid`는 안정적
- Playwright 권장 Best Practice

**vite.config.js - strictPort 추가:**
```javascript
export default defineConfig({
  // ...
  server: {
    port: 5173,
    strictPort: true,  // 포트가 이미 사용 중이면 에러
  },
  // ...
});
```

**왜 필요한가?**
- Playwright가 `http://localhost:5173`으로 접속 예정
- 포트 5173이 사용 중이면 Vite가 5174로 변경 → Playwright는 5173에 접속 시도 → 실패
- `strictPort: true`로 포트 충돌 시 명확한 에러 발생

---

## 4. playwright.config.ts 설명

### 4.1 전체 설정 파일

프로젝트 루트에 `playwright.config.ts` 파일을 생성합니다:

```typescript
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
  ],

  webServer: {
    command: 'npm run dev',
    url: 'http://localhost:5173',
    reuseExistingServer: !process.env.CI,
    timeout: 120 * 1000,
  },
});
```

### 4.2 각 옵션 상세 설명

**testDir: './e2e'**
- E2E 테스트 파일이 있는 디렉토리
- Playwright는 이 폴더에서 `*.spec.ts` 파일을 찾음

**fullyParallel: true**
- 모든 테스트를 병렬로 실행 (빠른 속도)
- 각 테스트가 독립적이므로 안전

**forbidOnly: !!process.env.CI**
- CI 환경에서 `test.only()`를 금지
- `test.only()`는 특정 테스트만 실행하는 디버깅 도구
- CI에서는 모든 테스트가 실행되어야 하므로 금지

**retries: process.env.CI ? 2 : 0**
- CI 환경에서는 실패 시 2번 재시도
- 로컬 환경에서는 재시도 없음
- 네트워크 불안정 등 일시적 실패를 처리

**workers: process.env.CI ? 1 : undefined**
- CI 환경에서는 1개의 워커만 사용 (리소스 절약)
- 로컬에서는 CPU 코어 수만큼 병렬 실행 (undefined = 자동)

**reporter: 'html'**
- 테스트 결과를 HTML 리포트로 생성
- `npx playwright show-report`로 확인 가능

### 4.3 use 옵션 (모든 테스트에 적용)

**baseURL: 'http://localhost:5173'**
- 모든 페이지 이동의 기본 URL
- `page.goto('/')` → `http://localhost:5173/` 접속

**trace: 'retain-on-failure'**
- 실패한 테스트의 실행 과정을 Trace 파일로 저장
- Trace Viewer로 타임 트래블 디버깅 가능
- 성공 시에는 저장 안 함 (디스크 공간 절약)

**screenshot: 'only-on-failure'**
- 실패한 테스트의 스크린샷만 저장
- 시각적 디버깅에 유용

**video: 'retain-on-failure'**
- 실패한 테스트의 비디오 녹화본 저장
- 어떤 과정에서 실패했는지 눈으로 확인 가능

### 4.4 projects 옵션

```typescript
projects: [
  {
    name: 'chromium',
    use: { ...devices['Desktop Chrome'] },
  },
],
```

- **name: 'chromium':** 프로젝트 이름 (리포트에 표시)
- **devices['Desktop Chrome']:** Playwright의 사전 정의된 브라우저 설정
  - User-Agent, 화면 크기, 뷰포트 등 설정
  - Desktop Chrome의 기본 설정 사용

**여러 브라우저 테스트 (옵션):**
```typescript
projects: [
  { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
  { name: 'firefox', use: { ...devices['Desktop Firefox'] } },
  { name: 'webkit', use: { ...devices['Desktop Safari'] } },
],
```

### 4.5 webServer 옵션 (자동 개발 서버 시작)

**command: 'npm run dev'**
- 테스트 전에 실행할 명령어
- Vite 개발 서버를 자동으로 시작

**url: 'http://localhost:5173'**
- 이 URL이 응답할 때까지 대기
- 서버가 준비되면 테스트 시작

**reuseExistingServer: !process.env.CI**
- 로컬에서는 이미 실행 중인 서버 재사용 (개발 중 편리)
- CI에서는 매번 새 서버 시작 (깨끗한 환경)

**timeout: 120 * 1000**
- 서버 시작 대기 시간 120초
- Fable 컴파일이 오래 걸릴 수 있으므로 넉넉하게 설정

**작동 방식:**
1. `npx playwright test` 실행
2. Playwright가 5173 포트 확인
3. 서버 없으면 `npm run dev` 실행
4. `http://localhost:5173`이 응답할 때까지 대기
5. 테스트 실행
6. 테스트 끝나면 서버 종료 (로컬에서는 종료 안 함)

---

## 5. E2E 테스트 작성

### 5.1 테스트 파일 구조

`e2e/calculator.spec.ts` 파일을 생성합니다:

```typescript
import { test, expect } from '@playwright/test';

test.describe('Calculator E2E', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test('displays initial state of 0', async ({ page }) => {
    await expect(page.getByTestId('display')).toHaveText('0');
  });

  // ... 더 많은 테스트
});
```

**구조 설명:**
- `test.describe()`: 테스트 그룹 (선택적)
- `test.beforeEach()`: 각 테스트 전에 실행 (페이지 로드)
- `test()`: 개별 테스트 케이스
- `async ({ page })`: 비동기 함수, `page` 객체는 Playwright가 주입

### 5.2 Locator 전략 (요소 찾기)

Playwright는 여러 방법으로 요소를 찾을 수 있습니다:

**getByRole (권장 - 접근성 기반):**
```typescript
await page.getByRole('button', { name: '2' }).click();
```
- ARIA role 기반 (button, link, heading 등)
- 스크린 리더가 인식하는 방식과 동일
- 가장 안정적 (구조 변경에 강함)

**getByTestId (데이터 속성 기반):**
```typescript
await page.getByTestId('display').textContent();
```
- `data-testid` 속성으로 찾기
- 테스트 전용 식별자
- 유일한 요소에 사용 (디스플레이, 특정 섹션)

**다른 방법들 (비권장):**
```typescript
// CSS 셀렉터 (구조 변경에 취약)
await page.locator('.calc-button').click();

// XPath (읽기 어려움)
await page.locator('//button[text()="2"]').click();
```

**이 프로젝트 전략:**
- 버튼: `getByRole('button', { name: '...' })`
- 디스플레이: `getByTestId('display')`

### 5.3 웹 우선 검증 (Web-First Assertions)

Playwright의 검증은 자동으로 재시도합니다:

```typescript
await expect(page.getByTestId('display')).toHaveText('5');
```

**작동 방식:**
1. 디스플레이 요소 찾기
2. 텍스트가 "5"인지 확인
3. 아니면 100ms 대기 후 다시 확인
4. 최대 5초까지 재시도
5. 5초 안에 "5"가 되면 통과, 아니면 실패

**왜 중요한가?**
```typescript
// 나쁜 예 (일반 테스트 프레임워크)
await page.click('button:has-text("=")');
await sleep(1000);  // 계산 완료까지 대기
expect(await page.textContent('[data-testid="display"]')).toBe('5');
```

- `sleep(1000)`: 항상 1초 대기 (느림, 불안정)
- 빠른 환경에서는 낭비, 느린 환경에서는 부족

```typescript
// 좋은 예 (Playwright)
await page.click('button:has-text("=")');
await expect(page.getByTestId('display')).toHaveText('5');
```

- 자동 대기: "5"가 나타나는 즉시 통과 (빠름, 안정적)
- 타임아웃까지 재시도 (느린 환경 대응)

### 5.4 실제 테스트 케이스 예시

**기본 연산 테스트:**
```typescript
test('adds two numbers: 2 + 3 = 5', async ({ page }) => {
  await page.getByRole('button', { name: '2' }).click();
  await page.getByRole('button', { name: '+' }).click();
  await page.getByRole('button', { name: '3' }).click();
  await page.getByRole('button', { name: '=' }).click();
  await expect(page.getByTestId('display')).toHaveText('5');
});
```

**0으로 나누기 에러 테스트:**
```typescript
test('shows Error on divide by zero', async ({ page }) => {
  await page.getByRole('button', { name: '5' }).click();
  await page.getByRole('button', { name: '÷' }).click();
  await page.getByRole('button', { name: '0' }).click();
  await page.getByRole('button', { name: '=' }).click();
  await expect(page.getByTestId('display')).toHaveText('Error');
});
```

**연산 연쇄 테스트 (왼쪽에서 오른쪽 계산):**
```typescript
test('chains operations left to right: 2 + 3 × 4 = 20', async ({ page }) => {
  await page.getByRole('button', { name: '2' }).click();
  await page.getByRole('button', { name: '+' }).click();
  await page.getByRole('button', { name: '3' }).click();
  await page.getByRole('button', { name: '×' }).click();
  // 이 시점에서 2 + 3 = 5 계산됨, 디스플레이에 "5" 표시
  await expect(page.getByTestId('display')).toHaveText('5');
  await page.getByRole('button', { name: '4' }).click();
  await page.getByRole('button', { name: '=' }).click();
  await expect(page.getByTestId('display')).toHaveText('20');
});
```

**Backspace 테스트:**
```typescript
test('backspace removes last digit', async ({ page }) => {
  await page.getByRole('button', { name: '1' }).click();
  await page.getByRole('button', { name: '2' }).click();
  await page.getByRole('button', { name: '3' }).click();
  await expect(page.getByTestId('display')).toHaveText('123');
  await page.getByRole('button', { name: '←' }).click();
  await expect(page.getByTestId('display')).toHaveText('12');
});
```

### 5.5 전체 테스트 스위트

이 프로젝트는 10개의 E2E 테스트를 포함합니다:

1. **초기 상태:** "0" 표시
2. **덧셈:** 2 + 3 = 5
3. **뺄셈:** 9 - 4 = 5
4. **곱셈:** 6 × 7 = 42
5. **나눗셈:** 8 ÷ 2 = 4
6. **0으로 나누기:** 5 ÷ 0 = Error
7. **Clear 버튼:** C 클릭 시 "0"으로 리셋
8. **소수점 입력:** 3.5 입력 가능
9. **연산 연쇄:** 2 + 3 × 4 = 20 (왼쪽에서 오른쪽)
10. **Backspace:** 123 → ← → 12

---

## 6. 테스트 실행

### 6.1 헤드리스 모드 (화면 없이)

```bash
npx playwright test
```

**실행 과정:**
1. `npm run dev` 실행 (Vite 서버 시작)
2. 서버 준비 대기 (http://localhost:5173 응답 확인)
3. Chromium 브라우저 백그라운드 실행
4. 10개 테스트 실행
5. 결과 출력:
   ```
   Running 10 tests using 4 workers
   ✓ Calculator E2E › displays initial state of 0 (245ms)
   ✓ Calculator E2E › adds two numbers: 2 + 3 = 5 (312ms)
   ...
   10 passed (3.2s)
   ```

**언제 사용?**
- CI 환경
- 빠른 로컬 검증

### 6.2 헤드풀 모드 (브라우저 창 보면서)

```bash
npx playwright test --headed
```

**차이점:**
- Chromium 창이 열림
- 테스트 실행 과정을 눈으로 확인 가능
- 디버깅 시 유용

**언제 사용?**
- 테스트가 왜 실패하는지 모를 때
- 새 테스트 작성 중
- 시각적 확인 필요

### 6.3 UI 모드 (인터랙티브)

```bash
npx playwright test --ui
```

**기능:**
- 웹 UI에서 테스트 선택 및 실행
- 타임 트래블: 각 단계를 앞뒤로 이동
- 로케이터 하이라이트: 어떤 요소를 찾고 있는지 표시
- 스크린샷 비교

**언제 사용?**
- 테스트 개발 및 디버깅
- 로케이터 전략 실험
- 교육 목적

### 6.4 HTML 리포트 보기

```bash
npx playwright show-report
```

**내용:**
- 각 테스트의 성공/실패 상태
- 실행 시간
- 실패 시 에러 메시지, 스크린샷, 비디오
- Trace 파일 다운로드 링크

**리포트 위치:**
- `playwright-report/index.html`
- CI에서 아티팩트로 업로드됨

### 6.5 특정 테스트만 실행

```bash
# 파일명으로 필터
npx playwright test calculator.spec.ts

# 테스트 이름으로 필터
npx playwright test -g "adds two numbers"

# 디버그 모드 (한 단계씩 실행)
npx playwright test --debug
```

---

## 7. GitHub Actions CI 구축

### 7.1 CI/CD란?

**CI (Continuous Integration):**
- 코드 변경 시 자동으로 빌드 및 테스트
- 버그를 빠르게 발견 (몇 시간 내에)
- 팀원들이 항상 동작하는 코드 유지

**CD (Continuous Deployment):**
- 테스트 통과 시 자동으로 배포
- 수동 배포 과정 제거
- 이 프로젝트는 CI만 구현 (배포는 Phase 5)

### 7.2 GitHub Actions란?

GitHub에서 제공하는 CI/CD 서비스입니다.

**특징:**
- GitHub 저장소에 통합
- YAML 파일로 워크플로 정의
- Linux, Windows, macOS 러너 제공
- 무료 티어: 공개 저장소 무제한, 비공개 저장소 월 2000분

### 7.3 CI 워크플로 파일

`.github/workflows/ci.yml` 파일을 생성합니다:

```yaml
name: CI

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
    - uses: actions/checkout@v4

    - uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '10.0.x'

    - uses: actions/setup-node@v4
      with:
        node-version: 'lts/*'

    - name: Cache node modules
      uses: actions/cache@v4
      with:
        path: |
          ~/.npm
          node_modules
        key: ${{ runner.os }}-node-${{ hashFiles('**/package-lock.json') }}

    - name: Install dependencies
      run: npm ci

    - name: Restore dotnet tools
      run: dotnet tool restore

    - name: Run unit tests
      run: npm test

    - name: Cache Playwright browsers
      uses: actions/cache@v4
      id: playwright-cache
      with:
        path: ~/.cache/ms-playwright
        key: ${{ runner.os }}-playwright-${{ hashFiles('**/package-lock.json') }}

    - name: Install Playwright browsers
      if: steps.playwright-cache.outputs.cache-hit != 'true'
      run: npx playwright install --with-deps chromium

    - name: Install Playwright OS dependencies
      if: steps.playwright-cache.outputs.cache-hit == 'true'
      run: npx playwright install-deps chromium

    - name: Run E2E tests
      run: npx playwright test

    - name: Upload test report
      uses: actions/upload-artifact@v4
      if: ${{ !cancelled() }}
      with:
        name: playwright-report
        path: playwright-report/
        retention-days: 30

    - name: Upload test results
      uses: actions/upload-artifact@v4
      if: ${{ !cancelled() }}
      with:
        name: test-results
        path: test-results/
        retention-days: 30
```

### 7.4 워크플로 단계별 설명

**on: push / pull_request**
```yaml
on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]
```
- `main` 브랜치에 push할 때 실행
- `main`으로의 PR 생성 시 실행

**jobs: test**
```yaml
jobs:
  test:
    timeout-minutes: 60
    runs-on: ubuntu-latest
```
- `test`라는 이름의 작업
- 60분 타임아웃 (무한 루프 방지)
- Ubuntu 최신 버전 러너 사용

**1단계: 코드 체크아웃**
```yaml
- uses: actions/checkout@v4
```
- GitHub 저장소 코드를 러너에 복사
- 모든 워크플로의 첫 단계

**2-3단계: .NET SDK 및 Node.js 설치**
```yaml
- uses: actions/setup-dotnet@v4
  with:
    dotnet-version: '10.0.x'

- uses: actions/setup-node@v4
  with:
    node-version: 'lts/*'
```
- .NET SDK 10.0.x 설치 (global.json과 일치)
- Node.js LTS 버전 설치

**4단계: node_modules 캐싱**
```yaml
- name: Cache node modules
  uses: actions/cache@v4
  with:
    path: |
      ~/.npm
      node_modules
    key: ${{ runner.os }}-node-${{ hashFiles('**/package-lock.json') }}
```
- `package-lock.json` 변경 시에만 재설치
- 변경 없으면 캐시에서 복원 (빠름: 30초 → 5초)

**5단계: npm ci**
```yaml
- name: Install dependencies
  run: npm ci
```
- `npm install` 대신 `npm ci` 사용
- CI 전용: 빠르고 재현 가능
- `package-lock.json`을 엄격히 따름

**6단계: dotnet tool restore**
```yaml
- name: Restore dotnet tools
  run: dotnet tool restore
```
- `.config/dotnet-tools.json`의 도구 복원
- Fable CLI 등

**7단계: 단위 테스트 실행**
```yaml
- name: Run unit tests
  run: npm test
```
- F# 단위 테스트 27개 실행
- 실패 시 워크플로 중단

**8-10단계: Playwright 브라우저 캐싱 및 설치**
```yaml
- name: Cache Playwright browsers
  uses: actions/cache@v4
  id: playwright-cache
  with:
    path: ~/.cache/ms-playwright
    key: ${{ runner.os }}-playwright-${{ hashFiles('**/package-lock.json') }}

- name: Install Playwright browsers
  if: steps.playwright-cache.outputs.cache-hit != 'true'
  run: npx playwright install --with-deps chromium

- name: Install Playwright OS dependencies
  if: steps.playwright-cache.outputs.cache-hit == 'true'
  run: npx playwright install-deps chromium
```

**작동 방식:**
- 캐시 히트 (package-lock.json 동일):
  - 브라우저는 캐시에서 복원
  - OS 의존성만 설치 (`install-deps`, 빠름)
- 캐시 미스 (package-lock.json 변경):
  - 브라우저와 OS 의존성 모두 설치 (`--with-deps`, 느림)

**왜 이렇게 복잡한가?**
- Chromium 브라우저: ~200MB (다운로드 오래 걸림)
- 캐싱으로 빌드 시간 5분 → 2분으로 단축

**11단계: E2E 테스트 실행**
```yaml
- name: Run E2E tests
  run: npx playwright test
```
- 10개 E2E 테스트 실행
- 실패 시 스크린샷, 비디오, Trace 자동 수집

**12-13단계: 테스트 아티팩트 업로드**
```yaml
- name: Upload test report
  uses: actions/upload-artifact@v4
  if: ${{ !cancelled() }}
  with:
    name: playwright-report
    path: playwright-report/
    retention-days: 30
```

- `if: ${{ !cancelled() }}`: 실패해도 업로드 (성공 시에만 업로드하지 않음)
- `retention-days: 30`: 30일 후 자동 삭제
- GitHub UI에서 다운로드 가능

**아티팩트 내용:**
- `playwright-report/`: HTML 리포트
- `test-results/`: 스크린샷, 비디오, Trace 파일

### 7.5 CI 실행 확인

**1. GitHub 저장소의 Actions 탭:**
- 각 커밋/PR에 대한 워크플로 실행 기록
- 성공/실패 상태
- 각 단계의 로그

**2. 커밋 체크 마크:**
- 커밋 옆에 녹색 ✓ (통과) 또는 빨간 ✗ (실패)
- PR에서 "All checks have passed" 메시지

**3. 아티팩트 다운로드:**
- 워크플로 실행 상세 페이지 → Artifacts 섹션
- `playwright-report.zip` 다운로드 후 압축 해제
- `index.html` 열어서 테스트 리포트 확인

---

## 8. 로컬 통합 테스트

### 8.1 npm run test:all 스크립트

Git에 push하기 전에 로컬에서 모든 테스트를 실행하는 스크립트입니다.

**package.json에 추가:**
```json
{
  "scripts": {
    "test:all": "npm test && npx playwright test"
  }
}
```

**실행:**
```bash
npm run test:all
```

**순서:**
1. 단위 테스트 27개 실행 (Fable.Mocha)
2. 단위 테스트 통과 시 E2E 테스트 10개 실행 (Playwright)
3. 모두 통과하면 안전하게 push 가능

**언제 사용?**
- PR 생성 전
- 중요한 변경 후
- 배포 전 최종 확인

### 8.2 Pre-push Hook (옵션)

Git hook으로 자동화할 수 있습니다 (선택 사항):

**`.git/hooks/pre-push` 파일 생성:**
```bash
#!/bin/sh
npm run test:all
```

**권한 부여:**
```bash
chmod +x .git/hooks/pre-push
```

**작동:**
- `git push` 실행 시 자동으로 `npm run test:all` 실행
- 테스트 실패 시 push 차단
- 실수로 깨진 코드 push 방지

---

## 9. 문제 해결

### 9.1 "Browser not found" 에러

**증상:**
```
browserType.launch: Executable doesn't exist at /Users/.../chromium-XXXX/chrome-mac/Chromium.app
```

**원인:**
- Chromium 브라우저가 설치되지 않음
- `npm install`만 하고 `npx playwright install` 안 함

**해결:**
```bash
npx playwright install chromium
```

### 9.2 "Port 5173 already in use" 에러

**증상:**
```
Error: Port 5173 is already in use
```

**원인:**
- Vite 개발 서버가 이미 실행 중
- `strictPort: true` 설정 때문에 다른 포트로 변경 안 됨

**해결 1: 기존 프로세스 종료**
```bash
# macOS/Linux
lsof -ti:5173 | xargs kill -9

# Windows
netstat -ano | findstr :5173
taskkill /PID <PID> /F
```

**해결 2: 서버 재사용**
- 테스트 실행 전에 `npm run dev`를 별도 터미널에서 실행
- `playwright.config.ts`의 `reuseExistingServer: !process.env.CI`가 자동으로 재사용

### 9.3 "Test timeout" 에러

**증상:**
```
Test timeout of 30000ms exceeded
```

**원인:**
- 개발 서버 시작이 느림 (Fable 컴파일)
- 네트워크 응답 느림

**해결 1: webServer 타임아웃 증가**
```typescript
webServer: {
  // ...
  timeout: 120 * 1000,  // 120초로 증가
}
```

**해결 2: 개별 테스트 타임아웃 증가**
```typescript
test('slow test', async ({ page }) => {
  test.setTimeout(60000);  // 이 테스트만 60초
  // ...
});
```

### 9.4 "Element not found" 에러

**증상:**
```
Error: locator.click: Target closed
Error: page.getByRole('button', { name: '2' }): not found
```

**원인:**
- 로케이터가 잘못됨 (버튼 이름 오타, testId 누락)
- 요소가 아직 렌더링 안 됨

**해결 1: UI 모드로 확인**
```bash
npx playwright test --ui
```
- 로케이터가 어떤 요소를 찾고 있는지 하이라이트
- 페이지 구조 확인

**해결 2: 로케이터 전략 변경**
```typescript
// 안 좋음: CSS 셀렉터 (취약)
await page.locator('.calc-button').click();

// 좋음: Role 기반
await page.getByRole('button', { name: '2' }).click();
```

### 9.5 CI에서만 실패하는 테스트

**증상:**
- 로컬에서는 통과
- GitHub Actions에서는 실패

**원인 1: 타이밍 이슈**
- CI 서버가 느림
- 로컬은 빠른 네트워크/디스크

**해결:**
```typescript
// 나쁜 예
await page.click('button');
await expect(page.locator('.result')).toHaveText('5');

// 좋은 예 (자동 대기)
await page.click('button');
await expect(page.locator('.result')).toHaveText('5', { timeout: 10000 });
```

**원인 2: 환경 차이**
- 로컬: macOS, CI: Linux
- 폰트 렌더링 차이

**해결:**
- 픽셀 단위 비교 대신 텍스트 비교
- 스크린샷 테스트는 신중히 사용

---

## 10. 핵심 개념 정리

### 10.1 E2E 테스트의 역할

**단위 테스트와 보완 관계:**
- 단위 테스트: 많은 엣지 케이스, 빠름 (27개)
- E2E 테스트: 주요 사용자 시나리오, 느림 (10개)
- 둘 다 필요: 피라미드 구조 (많은 단위, 적은 E2E)

**E2E 테스트가 잡는 버그:**
- CSS 스타일 문제로 버튼 클릭 안 됨
- 이벤트 핸들러 연결 누락
- 브라우저 호환성 문제
- UI와 로직 통합 오류

### 10.2 자동 대기의 중요성

**Playwright의 장점:**
```typescript
// 자동 대기 - 명시적 sleep 불필요
await page.click('button');
await expect(page.locator('.result')).toHaveText('5');
```

**다른 도구 (Selenium):**
```javascript
// 수동 대기 - 불안정
await driver.findElement(By.css('button')).click();
await driver.sleep(1000);  // 하드코딩된 대기
const text = await driver.findElement(By.css('.result')).getText();
expect(text).toBe('5');
```

### 10.3 CI/CD의 가치

**CI 없이:**
- 개발자가 수동으로 테스트 실행 (잊어버림)
- 버그가 늦게 발견됨 (며칠 후)
- 누가 깨뜨렸는지 찾기 어려움

**CI 있으면:**
- 모든 커밋이 자동 테스트됨
- 버그가 몇 분 내에 발견됨
- 정확한 커밋 식별

### 10.4 캐싱 전략

**캐싱 대상:**
1. `node_modules`: package-lock.json 기준
2. Playwright 브라우저: package-lock.json 기준
3. .npm 캐시: 추가 최적화

**효과:**
- 캐시 히트: 빌드 시간 5분 → 2분
- 대역폭 절약: 매번 200MB 다운로드 안 함

---

## 11. 다음 단계

Phase 4를 완료했습니다! 계산기가 자동화된 E2E 테스트와 CI/CD 파이프라인을 갖추게 되었습니다.

**Phase 5: 프로덕션 배포**

- GitHub Pages 자동 배포
- 프로덕션 빌드 최적화
- PWA (Progressive Web App) 기능
- 성능 측정 (Lighthouse)
- 캐시 전략 및 CDN

**더 나아가기:**

- **Playwright 고급 기능:**
  - API 모킹 (`page.route()`)
  - 네트워크 조작 (오프라인 시뮬레이션)
  - 파일 업로드/다운로드 테스트
  - 모바일 에뮬레이션

- **CI 확장:**
  - 여러 브라우저 테스트 (Firefox, WebKit)
  - 시각적 회귀 테스트 (Percy, Chromatic)
  - 성능 벤치마크 자동화
  - 배포 자동화 (CD)

---

## 12. 참고 자료

### Playwright 공식 문서

- **Playwright 홈페이지:** https://playwright.dev/
- **시작 가이드:** https://playwright.dev/docs/intro
- **로케이터 Best Practices:** https://playwright.dev/docs/locators
- **웹 우선 검증:** https://playwright.dev/docs/test-assertions

### GitHub Actions

- **GitHub Actions 문서:** https://docs.github.com/en/actions
- **Workflow 구문:** https://docs.github.com/en/actions/using-workflows/workflow-syntax-for-github-actions
- **캐싱 전략:** https://docs.github.com/en/actions/using-workflows/caching-dependencies-to-speed-up-workflows

### E2E 테스트 Best Practices

- **Testing Trophy:** https://kentcdodds.com/blog/the-testing-trophy-and-testing-classifications
- **테스트 피라미드:** https://martinfowler.com/articles/practical-test-pyramid.html
- **E2E 테스트 안티패턴:** https://playwright.dev/docs/best-practices

### TypeScript

- **TypeScript 핸드북:** https://www.typescriptlang.org/docs/handbook/intro.html
- **Async/Await:** https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/async_function

---

**축하합니다!** Phase 4를 완료했습니다. 이제 계산기 애플리케이션이 완전한 테스트 커버리지를 갖추었습니다:

- ✅ 27개 단위 테스트 (Phase 2)
- ✅ 10개 E2E 테스트 (Phase 4)
- ✅ GitHub Actions CI 파이프라인 (Phase 4)
- ✅ 자동 테스트 리포트 및 아티팩트 수집 (Phase 4)

모든 커밋이 자동으로 검증되며, 버그가 프로덕션에 도달하기 전에 잡힐 수 있는 견고한 개발 프로세스가 구축되었습니다!
