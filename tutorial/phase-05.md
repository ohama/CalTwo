# Phase 5: 배포와 접근성 (Deployment & Accessibility)

## 개요

이 튜토리얼에서는 Vite 프로덕션 빌드와 GitHub Pages 배포, 그리고 웹 접근성 표준(WCAG 2.1)을 구현하는 방법을 다룹니다. Phase 4까지 완성된 계산기를 실제로 배포하고, 키보드 사용자와 스크린 리더 사용자가 모두 사용할 수 있도록 접근성을 개선합니다.

### 학습 목표

- Vite 프로덕션 빌드의 원리와 base path 설정 이해하기
- GitHub Actions를 사용한 GitHub Pages 자동 배포 구현하기
- WCAG 2.1 웹 접근성 표준과 키보드 네비게이션 이해하기
- :focus-visible CSS로 키보드 포커스 표시자 구현하기
- ARIA 레이블로 스크린 리더 지원 추가하기

### 완성 후 결과물

- GitHub Pages에 배포된 실제 동작하는 계산기 (https://ohama.github.io/CalTwo/)
- main 브랜치에 푸시하면 자동으로 배포되는 CI/CD 파이프라인
- 키보드만으로 모든 기능을 사용할 수 있는 접근 가능한 UI
- 스크린 리더가 버튼과 디스플레이를 정확히 읽어주는 ARIA 구조
- 프로젝트 문서화를 위한 README.md

---

## 1. Vite 프로덕션 빌드

### 1.1 개발 모드 vs. 프로덕션 모드

**개발 모드 (`npm run dev`):**
```bash
npm run dev
# Vite dev server가 http://localhost:5173 에서 실행
# - HMR (Hot Module Replacement) 활성화 - 코드 변경 시 자동 새로고침
# - Source maps 포함 - 브라우저에서 원본 F# 코드 확인 가능
# - 최적화 없음 - 빠른 빌드를 위해 번들링/압축 생략
# - ES modules로 직접 로드 - 파일별로 HTTP 요청
```

**프로덕션 모드 (`npm run build`):**
```bash
npm run build
# Vite가 dist/ 디렉토리에 최적화된 정적 파일 생성
# - Tree-shaking - 사용하지 않는 코드 제거
# - Minification - 공백/주석 제거, 변수명 축약
# - Code splitting - 큰 번들을 작은 청크로 분할
# - Asset hashing - 파일명에 해시 추가 (캐싱 최적화)
```

### 1.2 Base Path 설정의 필요성

GitHub Pages는 두 가지 형태의 URL을 제공합니다:

**사용자/조직 사이트:**
```
https://<USERNAME>.github.io/
→ 이 경우 base: '/' (기본값)
```

**프로젝트 사이트 (우리의 경우):**
```
https://ohama.github.io/CalTwo/
→ 이 경우 base: '/CalTwo/' (반드시 설정 필요)
```

**왜 base path가 필요한가?**

Vite는 기본적으로 루트 경로(`/`)를 기준으로 에셋 경로를 생성합니다:

```html
<!-- base: '/' (기본값)로 빌드한 경우 -->
<script type="module" src="/assets/index-a1b2c3d4.js"></script>
<link rel="stylesheet" href="/assets/index-e5f6g7h8.css" />

<!-- 이 경로들은 https://ohama.github.io/assets/... 를 요청
     하지만 실제 파일은 https://ohama.github.io/CalTwo/assets/... 에 있음
     결과: 404 에러, 빈 화면 -->
```

올바른 설정:

```javascript
// vite.config.js
export default defineConfig({
  base: '/CalTwo/',  // 저장소 이름과 정확히 일치해야 함
  // ...
});
```

이제 생성되는 HTML:

```html
<!-- base: '/CalTwo/'로 빌드한 경우 -->
<script type="module" src="/CalTwo/assets/index-a1b2c3d4.js"></script>
<link rel="stylesheet" href="/CalTwo/assets/index-e5f6g7h8.css" />

<!-- 올바른 경로: https://ohama.github.io/CalTwo/assets/... -->
```

### 1.3 vite.config.js 전체 구성

CalTwo 프로젝트의 실제 Vite 설정:

```javascript
import { defineConfig } from "vite";
import fable from "vite-plugin-fable";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [
    fable({
      fsproj: "src/App.fsproj",  // F# 프로젝트 파일
      jsx: "automatic"            // React JSX 자동 변환
    }),
    react({
      include: /\.(fs|js|jsx|ts|tsx)$/  // .fs 파일도 React로 처리
    })
  ],
  base: '/CalTwo/',  // GitHub Pages 프로젝트 사이트 경로
  server: {
    port: 5173,        // 개발 서버 포트
    strictPort: true,  // 포트가 사용 중이면 실패 (다른 포트로 자동 변경 안 함)
  },
  build: {
    sourcemap: true    // 프로덕션에서도 source map 생성 (디버깅용)
  }
});
```

**주요 설정 설명:**

- `plugins`: F# 코드를 JavaScript로 컴파일하는 Fable 플러그인
- `base`: GitHub Pages 배포 시 필수 (저장소 이름과 일치)
- `server.port`: 개발 서버 포트 (http://localhost:5173)
- `build.sourcemap`: 배포 후 브라우저에서 F# 원본 코드 추적 가능

### 1.4 로컬에서 프로덕션 빌드 테스트

배포 전에 반드시 로컬에서 프로덕션 빌드를 테스트해야 합니다:

```bash
# 1. 프로덕션 빌드 실행
npm run build
# → dist/ 디렉토리에 최적화된 파일 생성

# 2. 빌드 결과 확인
ls -lh dist/
# index.html
# assets/
#   ├── index-a1b2c3d4.js      (압축된 JavaScript)
#   ├── index-e5f6g7h8.css     (압축된 CSS)
#   └── ...

# 3. 프로덕션 빌드 미리보기
npm run preview
# → http://localhost:4173 에서 dist/ 폴더 서빙

# 4. 브라우저에서 테스트
# - 계산기가 정상 작동하는지 확인
# - 브라우저 DevTools → Network 탭에서 404 에러 없는지 확인
# - 모든 에셋이 /CalTwo/assets/... 경로에서 로드되는지 확인
```

**일반적인 문제:**

```bash
# 문제: npm run preview 실행 시 빈 화면
# 원인: base path 설정 누락 또는 잘못됨
# 해결: vite.config.js에서 base: '/CalTwo/' 확인

# 문제: 개발 모드는 동작하지만 빌드 후 에러
# 원인: F# 컴파일 에러가 개발 모드에서만 무시됨
# 해결: 빌드 에러 로그 확인, F# 타입 에러 수정

# 문제: CSS가 적용되지 않음
# 원인: CSS 파일 import 경로 오류
# 해결: main.jsx에서 import "./styles.css" 경로 확인
```

---

## 2. GitHub Actions 배포 워크플로

### 2.1 GitHub Pages 설정

먼저 저장소에서 GitHub Pages를 활성화해야 합니다:

**1단계: Repository Settings 이동**
```
https://github.com/ohama/CalTwo
→ Settings 탭 클릭
→ 왼쪽 사이드바에서 Pages 클릭
```

**2단계: Source 설정**
```
Build and deployment
  Source: [GitHub Actions]  ← 이것을 선택 (Deploy from branch 아님!)
```

**왜 "Deploy from branch"가 아닌가?**

- **Deploy from branch**: Jekyll 같은 정적 사이트 생성기용 (빌드 없음)
- **GitHub Actions**: Vite 같은 빌드 도구가 필요한 경우 (우리의 경우)

Vite는 빌드 과정이 필요하므로 GitHub Actions를 사용해야 합니다.

### 2.2 배포 워크플로 구조

GitHub Actions 워크플로는 `.github/workflows/` 디렉토리에 YAML 파일로 정의합니다.

**디렉토리 구조:**
```
.github/
└── workflows/
    ├── ci.yml        # Phase 4에서 만든 테스트 워크플로
    └── deploy.yml    # Phase 5에서 만들 배포 워크플로
```

**deploy.yml 전체 구조:**

```yaml
name: Deploy to GitHub Pages

# 워크플로 트리거 조건
on:
  push:
    branches: ['main']    # main 브랜치에 푸시 시 실행
  workflow_dispatch:      # GitHub UI에서 수동 실행 가능

# 권한 설정 (중요!)
permissions:
  contents: read          # 저장소 읽기
  pages: write            # GitHub Pages 배포
  id-token: write         # OIDC 토큰 (인증용)

# 동시 실행 제어
concurrency:
  group: 'pages'                    # 같은 그룹은 한 번에 하나만
  cancel-in-progress: true          # 새 배포 시작되면 이전 것 취소

jobs:
  deploy:
    environment:
      name: github-pages
      url: ${{ steps.deployment.outputs.page_url }}
    runs-on: ubuntu-latest

    steps:
      # 1. 소스 코드 체크아웃
      - name: Checkout
        uses: actions/checkout@v5

      # 2. .NET SDK 설치 (Fable 컴파일용)
      - name: Set up .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      # 3. Node.js 설치 (Vite 빌드용)
      - name: Set up Node
        uses: actions/setup-node@v6
        with:
          node-version: lts/*    # 최신 LTS 버전
          cache: 'npm'           # npm 캐싱

      # 4. .NET 도구 복원 (Fable CLI)
      - name: Restore dotnet tools
        run: dotnet tool restore

      # 5. npm 패키지 설치
      - name: Install dependencies
        run: npm ci              # npm install보다 빠르고 안정적

      # 6. Vite 프로덕션 빌드
      - name: Build
        run: npm run build       # dist/ 생성

      # 7. GitHub Pages 메타데이터 설정
      - name: Setup Pages
        uses: actions/configure-pages@v5

      # 8. 빌드 결과물 업로드
      - name: Upload artifact
        uses: actions/upload-pages-artifact@v4
        with:
          path: './dist'         # dist/ 폴더 업로드

      # 9. GitHub Pages 배포
      - name: Deploy to GitHub Pages
        id: deployment
        uses: actions/deploy-pages@v4
```

### 2.3 워크플로 각 단계 상세 설명

**Step 1-3: 환경 설정**

```yaml
- uses: actions/checkout@v5
# → Git 저장소의 최신 코드를 Runner에 복제

- uses: actions/setup-dotnet@v4
  with:
    dotnet-version: '10.0.x'
# → .NET SDK 10.0 설치 (Fable이 F# 코드를 JS로 컴파일하려면 필요)

- uses: actions/setup-node@v6
  with:
    node-version: lts/*
    cache: 'npm'
# → Node.js LTS 설치 + npm 패키지 캐싱 (빌드 속도 향상)
```

**Step 4-6: 빌드 실행**

```yaml
- run: dotnet tool restore
# → .config/dotnet-tools.json에 정의된 도구 설치 (fable-compiler)

- run: npm ci
# → package-lock.json 기준으로 정확히 동일한 버전 설치
#   (npm install보다 빠르고 재현 가능)

- run: npm run build
# → package.json의 "build": "vite build" 실행
#   → vite-plugin-fable이 F# → JS 컴파일
#   → Vite가 dist/ 디렉토리에 최적화된 파일 생성
```

**Step 7-9: GitHub Pages 배포**

```yaml
- uses: actions/configure-pages@v5
# → GitHub Pages 환경 변수 설정, 메타데이터 준비

- uses: actions/upload-pages-artifact@v4
  with:
    path: './dist'
# → dist/ 폴더를 gzip으로 압축하여 GitHub에 업로드
#   (최대 10GB, 일반적으로 수 MB)

- uses: actions/deploy-pages@v4
# → 업로드된 아티팩트를 GitHub Pages 환경에 배포
#   → https://ohama.github.io/CalTwo/ 에서 접근 가능
```

### 2.4 Permissions 설정의 중요성

```yaml
permissions:
  contents: read      # 저장소 읽기
  pages: write        # GitHub Pages 배포
  id-token: write     # OIDC 토큰 (인증용)
```

**왜 필요한가?**

GitHub Actions의 기본 `GITHUB_TOKEN`은 읽기 전용 권한만 가집니다. Pages에 배포하려면 다음 권한이 필요합니다:

- `pages: write`: GitHub Pages API를 호출하여 배포
- `id-token: write`: OIDC (OpenID Connect) 토큰 생성 - 보안 인증용

**만약 권한이 없다면?**

```
Error: Resource not accessible by integration
  at requestDispatch (...)

→ 워크플로가 빌드는 성공하지만 배포 단계에서 실패
```

### 2.5 로컬 테스트와 배포 확인

**워크플로 푸시 전 체크리스트:**

```bash
# 1. 로컬에서 빌드 테스트
npm run build
npm run preview
# → http://localhost:4173 에서 동작 확인

# 2. Git에 변경사항 커밋
git add .github/workflows/deploy.yml
git commit -m "feat: add GitHub Pages deployment workflow"
git push origin main

# 3. GitHub에서 워크플로 실행 확인
# https://github.com/ohama/CalTwo/actions
# → "Deploy to GitHub Pages" 워크플로 클릭
# → 각 step의 로그 확인

# 4. 배포 완료 후 사이트 확인
# https://ohama.github.io/CalTwo/
# → 계산기가 정상 작동하는지 확인
```

**일반적인 배포 문제:**

```bash
# 문제 1: 워크플로가 실행되지 않음
# 원인: .github/workflows/ 경로가 잘못됨
# 해결: .github/workflows/deploy.yml 경로 확인

# 문제 2: "Resource not accessible" 에러
# 원인: permissions 설정 누락
# 해결: permissions 블록 추가 (위 예시 참고)

# 문제 3: 배포 성공했지만 사이트가 빈 화면
# 원인: base path 설정 오류
# 해결: vite.config.js에서 base: '/CalTwo/' 확인

# 문제 4: Fable 컴파일 에러
# 원인: .NET SDK 설치 누락
# 해결: setup-dotnet step 추가
```

---

## 3. 웹 접근성 (Web Accessibility)

### 3.1 WCAG 2.1이란?

**WCAG (Web Content Accessibility Guidelines) 2.1**은 W3C에서 제정한 웹 접근성 표준입니다.

**4가지 원칙 (POUR):**

1. **Perceivable (인식 가능)**: 사용자가 콘텐츠를 인식할 수 있어야 함
   - 예: 텍스트 대안, 충분한 대비, 명확한 구조

2. **Operable (조작 가능)**: 사용자가 인터페이스를 조작할 수 있어야 함
   - 예: 키보드 접근, 충분한 클릭 영역, 포커스 표시

3. **Understandable (이해 가능)**: 콘텐츠와 인터페이스를 이해할 수 있어야 함
   - 예: 명확한 레이블, 일관된 네비게이션

4. **Robust (견고함)**: 다양한 보조 기술에서 동작해야 함
   - 예: 시맨틱 HTML, ARIA 속성

**준수 레벨:**

- **Level A**: 최소 요구사항 (필수)
- **Level AA**: 권장 수준 (대부분의 법규에서 요구)
- **Level AAA**: 최고 수준 (선택적)

CalTwo는 **Level A** 준수를 목표로 합니다.

### 3.2 키보드 네비게이션

**왜 중요한가?**

- 시각장애인은 마우스를 사용할 수 없고 스크린 리더 + 키보드만 사용
- 운동 장애인은 마우스보다 키보드가 더 쉬울 수 있음
- 파워 유저는 키보드가 더 빠름

**WCAG 2.1 SC 2.1.1 (Level A): Keyboard**

> 모든 기능을 키보드로 사용할 수 있어야 함

**CalTwo의 키보드 지원:**

```fsharp
// App.fs - 키보드 이벤트 핸들러
Html.div [
  prop.tabIndex 0  // Tab 키로 포커스 가능
  prop.onKeyDown (fun e ->
    match e.key with
    | "0" -> dispatch (DigitPressed 0); e.preventDefault()
    | "1" -> dispatch (DigitPressed 1); e.preventDefault()
    // ... (숫자 2-9)
    | "+" -> dispatch (OperatorPressed Add); e.preventDefault()
    | "-" -> dispatch (OperatorPressed Subtract); e.preventDefault()
    | "*" -> dispatch (OperatorPressed Multiply); e.preventDefault()
    | "/" -> dispatch (OperatorPressed Divide); e.preventDefault()
    | "." -> dispatch DecimalPressed; e.preventDefault()
    | "Enter" -> dispatch EqualsPressed; e.preventDefault()
    | "Escape" -> dispatch ClearPressed; e.preventDefault()
    | "Backspace" -> dispatch BackspacePressed; e.preventDefault()
    | _ -> ()  // 처리되지 않은 키는 기본 동작 유지
  )
  prop.children [ (* 계산기 UI *) ]
]
```

**키 매핑:**

| 키 | 동작 | 이유 |
|----|------|------|
| 0-9 | 숫자 입력 | 직관적 |
| + - * / | 연산자 | 일반 계산기와 동일 |
| . | 소수점 | 숫자 키패드와 동일 |
| Enter | 계산 (=) | 일반적인 "확인" 동작 |
| Escape | 초기화 (C) | 일반적인 "취소" 동작 |
| Backspace | 한 글자 삭제 | 텍스트 입력과 동일 |

### 3.3 Focus Visible - 키보드 포커스 표시자

**WCAG 2.1 SC 2.4.7 (Level A): Focus Visible**

> 키보드로 포커스를 받은 요소는 시각적으로 구별되어야 함

**문제: :focus만 사용하면?**

```css
/* 잘못된 방법 */
.calc-button:focus {
  outline: 3px solid blue;
}

/* 문제점:
   - 마우스로 클릭해도 파란 테두리가 나타남 (미관상 안 좋음)
   - 사용자가 outline: none으로 제거하고 싶어 함
   - 접근성 저하 위험
*/
```

**해결책: :focus-visible 사용**

```css
/* styles.css - 올바른 방법 */

/* 기본 포커스 링 제거 (모든 경우) */
.calc-button:focus {
  outline: none;
}

/* 키보드 포커스 시에만 표시 */
.calc-button:focus-visible {
  outline: 3px solid #4A90E2;
  outline-offset: 2px;
}

/* 구형 브라우저 폴백 */
@supports not selector(:focus-visible) {
  .calc-button:focus {
    outline: 3px solid #4A90E2;
    outline-offset: 2px;
  }
}
```

**동작 방식:**

```
사용자가 마우스로 버튼 클릭
→ :focus는 활성화되지만 :focus-visible은 활성화 안 됨
→ 파란 테두리 없음

사용자가 Tab 키로 버튼 포커스
→ :focus와 :focus-visible 둘 다 활성화
→ 파란 테두리 표시
```

**브라우저 지원:**

- Chrome 86+ (2020년 10월)
- Firefox 85+ (2021년 1월)
- Safari 15.4+ (2022년 3월)
- Edge 86+ (2020년 10월)

→ Baseline: Widely available (2022년 3월부터)

### 3.4 ARIA Labels - 스크린 리더 지원

**WCAG 2.1 SC 4.1.2 (Level A): Name, Role, Value**

> 모든 UI 컴포넌트는 이름(name)과 역할(role)을 가져야 함

**스크린 리더가 읽어주는 방식:**

```
사용자가 Tab 키로 버튼에 포커스
→ 스크린 리더: "Add, button" (이름 + 역할)
→ 사용자가 Enter 키로 클릭
```

**문제: ARIA 없이 버튼만 있으면?**

```fsharp
// ARIA 레이블 없는 버튼
Html.button [
  prop.className "calc-button"
  prop.text "+"
  prop.onClick (fun _ -> dispatch (OperatorPressed Add))
]

// 스크린 리더가 읽는 내용: "Plus, button"
// → 시각적으로는 "+"이지만 의미가 불명확
```

**해결책: aria-label 추가**

현재 CalTwo에는 ARIA 레이블이 없습니다. Phase 5에서 추가한다면:

```fsharp
// ARIA 레이블 추가한 버튼
Html.button [
  prop.className "calc-button calc-operator"
  prop.ariaLabel "Add"        // 스크린 리더용 설명
  prop.text "+"               // 시각적 표시
  prop.onClick (fun _ -> dispatch (OperatorPressed Add))
]

// 스크린 리더가 읽는 내용: "Add, button"
// → 명확하게 "더하기" 버튼임을 인식
```

**모든 버튼에 적용:**

```fsharp
// 숫자 버튼
Html.button [
  prop.ariaLabel "Number 7"
  prop.text "7"
  // ...
]

// 연산자 버튼
Html.button [
  prop.ariaLabel "Divide"
  prop.text "÷"
  // ...
]

Html.button [
  prop.ariaLabel "Multiply"
  prop.text "×"
  // ...
]

// 기능 버튼
Html.button [
  prop.ariaLabel "Clear"
  prop.text "C"
  // ...
]

Html.button [
  prop.ariaLabel "Backspace"
  prop.text "←"
  // ...
]

Html.button [
  prop.ariaLabel "Equals"
  prop.text "="
  // ...
]
```

### 3.5 Live Regions - 동적 콘텐츠 알림

**WCAG 2.1 SC 4.1.3 (Level A): Status Messages**

> 상태 메시지는 보조 기술에 전달되어야 함

**문제: 디스플레이 값 변경 시**

```fsharp
// 현재 디스플레이 (ARIA 없음)
Html.div [
  prop.testId "display"
  prop.text model.Display  // "5"로 변경되어도 스크린 리더는 모름
]

// 스크린 리더 사용자는:
// - 버튼을 클릭해도 결과를 들을 수 없음
// - 매번 디스플레이로 이동해서 값을 확인해야 함
```

**해결책: role="status" + aria-live**

```fsharp
// ARIA live region 추가
Html.div [
  prop.testId "display"
  prop.role "status"              // 상태 메시지 역할
  prop.ariaLive "polite"          // 변경 시 알림 (polite = 현재 발화 끝난 후)
  prop.ariaLabel "Calculator display"
  prop.text model.Display
]

// 동작:
// 1. 사용자가 "2" 버튼 클릭 → 스크린 리더: "2"
// 2. 사용자가 "+" 버튼 클릭 → 스크린 리더: "2" (변경 없음)
// 3. 사용자가 "3" 버튼 클릭 → 스크린 리더: "3"
// 4. 사용자가 "=" 버튼 클릭 → 스크린 리더: "5"
```

**aria-live 값:**

- `off`: 알림 안 함 (기본값)
- `polite`: 현재 발화가 끝난 후 알림 (권장)
- `assertive`: 즉시 알림 (긴급한 경우만)

계산기 디스플레이는 `polite`가 적합합니다 (긴급하지 않고 자연스러운 흐름).

---

## 4. 실제 코드 예제

### 4.1 styles.css - Focus Visible 구현

CalTwo의 실제 `styles.css`:

```css
.calc-button {
    min-height: 44px;
    min-width: 44px;
    padding: 12px;
    font-size: 18px;
    border: 1px solid #ccc;
    background: #f0f0f0;
    cursor: pointer;
    border-radius: 4px;
    transition: all 0.1s ease;
    font-family: monospace;
}

.calc-button:hover {
    background: #e0e0e0;
    transform: scale(1.05);
}

.calc-button:active {
    background: #d0d0d0;
    transform: scale(0.95);
}

/* 키보드 포커스 표시자 (WCAG 2.1 SC 2.4.7) */
.calc-button:focus {
    outline: none;  /* 마우스 클릭 시 테두리 제거 */
}

.calc-button:focus-visible {
    outline: 3px solid #4A90E2;  /* 키보드 포커스 시 파란 테두리 */
    outline-offset: 2px;          /* 테두리와 버튼 사이 간격 */
}

/* 구형 브라우저 폴백 (Safari 15.1 이전) */
@supports not selector(:focus-visible) {
    .calc-button:focus {
        outline: 3px solid #4A90E2;
        outline-offset: 2px;
    }
}
```

**테스트 방법:**

```bash
# 1. 개발 서버 실행
npm run dev

# 2. 브라우저에서 http://localhost:5173 열기

# 3. 마우스로 버튼 클릭
# → 파란 테두리가 나타나지 않음 (좋음)

# 4. Tab 키로 버튼 포커스
# → 파란 테두리가 나타남 (좋음)

# 5. 화살표 키나 Shift+Tab으로 포커스 이동
# → 포커스가 이동하면서 테두리도 따라감
```

### 4.2 vite.config.js - Base Path 설정

```javascript
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
  base: '/CalTwo/',  // ⭐ GitHub Pages 프로젝트 사이트용 base path
  server: {
    port: 5173,
    strictPort: true,
  },
  build: {
    sourcemap: true  // 배포 후에도 F# 원본 코드 디버깅 가능
  }
});
```

### 4.3 .github/workflows/deploy.yml - 전체 워크플로

```yaml
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

### 4.4 App.fs - 키보드 네비게이션

CalTwo의 실제 키보드 이벤트 핸들러:

```fsharp
Html.div [
  prop.tabIndex 0  // Tab 키로 이 div에 포커스 가능
  prop.onKeyDown (fun e ->
    match e.key with
    | "0" -> dispatch (DigitPressed 0); e.preventDefault()
    | "1" -> dispatch (DigitPressed 1); e.preventDefault()
    | "2" -> dispatch (DigitPressed 2); e.preventDefault()
    | "3" -> dispatch (DigitPressed 3); e.preventDefault()
    | "4" -> dispatch (DigitPressed 4); e.preventDefault()
    | "5" -> dispatch (DigitPressed 5); e.preventDefault()
    | "6" -> dispatch (DigitPressed 6); e.preventDefault()
    | "7" -> dispatch (DigitPressed 7); e.preventDefault()
    | "8" -> dispatch (DigitPressed 8); e.preventDefault()
    | "9" -> dispatch (DigitPressed 9); e.preventDefault()
    | "." -> dispatch DecimalPressed; e.preventDefault()
    | "+" -> dispatch (OperatorPressed Add); e.preventDefault()
    | "-" -> dispatch (OperatorPressed Subtract); e.preventDefault()
    | "*" -> dispatch (OperatorPressed Multiply); e.preventDefault()
    | "/" -> dispatch (OperatorPressed Divide); e.preventDefault()
    | "Enter" -> dispatch EqualsPressed; e.preventDefault()
    | "Escape" -> dispatch ClearPressed; e.preventDefault()
    | "Backspace" -> dispatch BackspacePressed; e.preventDefault()
    | _ -> ()  // 처리되지 않은 키는 기본 동작 유지
  )
  prop.children [
    (* 계산기 UI *)
  ]
]
```

**e.preventDefault()를 호출하는 이유:**

- 브라우저의 기본 키 동작 방지 (예: Backspace로 뒤로 가기)
- 계산기 UI 내에서만 키 입력 처리

---

## 5. 문제 해결 (Troubleshooting)

### 5.1 배포 후 404 에러

**증상:**
- GitHub Pages URL 접속 시 404 Not Found
- 또는 빈 화면 표시

**원인과 해결:**

```bash
# 원인 1: Repository Settings에서 Pages가 활성화되지 않음
# 해결:
# GitHub → Settings → Pages → Source: GitHub Actions 선택

# 원인 2: base path 설정 누락
# 해결:
# vite.config.js에 base: '/CalTwo/' 추가

# 원인 3: 워크플로가 실행되지 않음
# 해결:
# GitHub → Actions 탭에서 워크플로 실행 확인
# 수동 실행: Actions → Deploy to GitHub Pages → Run workflow
```

### 5.2 빌드는 성공했지만 배포 실패

**증상:**
- "Build" step은 성공
- "Deploy to GitHub Pages" step에서 에러

**원인과 해결:**

```yaml
# 원인: permissions 설정 누락
# 해결: deploy.yml에 permissions 블록 추가

permissions:
  contents: read
  pages: write      # ⭐ 이것이 없으면 배포 실패
  id-token: write   # ⭐ OIDC 인증용
```

**에러 메시지 예시:**

```
Error: Resource not accessible by integration
  at requestDispatch

→ pages: write 권한이 없어서 발생
```

### 5.3 로컬에서는 동작하지만 배포 후 빈 화면

**증상:**
- `npm run dev`는 정상 동작
- `npm run build && npm run preview`도 정상 동작
- GitHub Pages에서만 빈 화면

**원인과 해결:**

```bash
# 원인: base path가 로컬 테스트와 다름
# 로컬 preview: http://localhost:4173/ (루트)
# GitHub Pages: https://ohama.github.io/CalTwo/ (서브디렉토리)

# 해결 1: vite.config.js에서 base 확인
base: '/CalTwo/',  # 저장소 이름과 정확히 일치해야 함

# 해결 2: 브라우저 DevTools 확인
# Network 탭에서 404 에러 찾기
# /assets/index.js (X) → /CalTwo/assets/index.js (O)

# 해결 3: 로컬에서 base path 테스트
# vite.config.js를 base: '/CalTwo/'로 설정한 상태에서
npm run build
npm run preview
# http://localhost:4173/CalTwo/ 로 접속 (주의: /CalTwo/ 경로 포함)
```

### 5.4 Fable 컴파일 에러

**증상:**
- 워크플로가 "Build" step에서 실패
- 에러 메시지: "dotnet command not found" 또는 "Fable error"

**원인과 해결:**

```yaml
# 원인: .NET SDK 설치 누락
# 해결: deploy.yml에 setup-dotnet step 추가

steps:
  - uses: actions/checkout@v5

  - uses: actions/setup-dotnet@v4  # ⭐ 이 step이 없으면 Fable 컴파일 불가
    with:
      dotnet-version: '10.0.x'

  - run: dotnet tool restore        # ⭐ Fable CLI 설치

  - uses: actions/setup-node@v6
  - run: npm ci
  - run: npm run build              # Fable + Vite 빌드
```

### 5.5 :focus-visible가 동작하지 않음

**증상:**
- 키보드로 Tab 키를 눌러도 포커스 표시자가 나타나지 않음
- 또는 마우스 클릭 시에도 표시자가 나타남

**원인과 해결:**

```css
/* 문제 1: :focus-visible 철자 오류 */
.calc-button:focus-visable {  /* ❌ visable은 오타 */
  outline: 3px solid blue;
}

/* 해결 */
.calc-button:focus-visible {  /* ✅ visible */
  outline: 3px solid blue;
}

/* 문제 2: :focus가 :focus-visible을 덮어씀 */
.calc-button:focus {
  outline: 2px solid red;     /* ❌ 이게 우선순위가 더 높음 */
}
.calc-button:focus-visible {
  outline: 3px solid blue;
}

/* 해결: :focus에서 outline 제거 */
.calc-button:focus {
  outline: none;              /* ✅ 기본 테두리 제거 */
}
.calc-button:focus-visible {
  outline: 3px solid blue;    /* ✅ 키보드 포커스만 표시 */
}
```

**브라우저 호환성 확인:**

```bash
# Safari 15.4+ 에서만 지원
# 구형 브라우저 테스트:
# - BrowserStack (https://www.browserstack.com/)
# - Can I use (https://caniuse.com/?search=focus-visible)

# 폴백 코드 필수:
@supports not selector(:focus-visible) {
  .calc-button:focus {
    outline: 3px solid blue;  /* Safari 15.1 이하에서 동작 */
  }
}
```

---

## 6. 요약

이번 Phase에서 배운 핵심 내용:

### Vite 프로덕션 빌드
- `npm run build`로 dist/ 디렉토리에 최적화된 정적 파일 생성
- `base: '/CalTwo/'` 설정으로 GitHub Pages 서브디렉토리 배포 지원
- `npm run preview`로 배포 전 로컬 테스트 필수

### GitHub Actions 배포
- `.github/workflows/deploy.yml`에 배포 워크플로 정의
- `permissions: { pages: write, id-token: write }` 설정 필수
- 공식 액션 사용: `configure-pages`, `upload-pages-artifact`, `deploy-pages`
- main 브랜치 푸시 시 자동 배포, 수동 실행도 가능

### 웹 접근성
- **키보드 네비게이션**: 모든 기능을 키보드로 사용 가능 (WCAG SC 2.1.1)
- **:focus-visible**: 키보드 포커스만 표시, 마우스 클릭 시 제거 (WCAG SC 2.4.7)
- **ARIA 레이블**: 스크린 리더가 버튼 설명 읽어줌 (WCAG SC 4.1.2)
- **Live regions**: 디스플레이 값 변경 시 스크린 리더 알림 (WCAG SC 4.1.3)

### 다음 단계
- Phase 5까지 완성된 CalTwo는 실제 배포 가능한 프로덕션 애플리케이션
- 추가 기능: 계산 기록, 테마 설정, 더 많은 연산자
- 성능 최적화: Lighthouse 점수 측정, 번들 크기 최적화
- 접근성 향상: Level AA 준수, 스크린 리더 실제 테스트

---

**축하합니다!** 🎉

F# + Fable + Elmish + Feliz로 만든 계산기를 GitHub Pages에 배포하고, 접근 가능한 웹 애플리케이션으로 완성했습니다. 이제 실제 사용자들이 사용할 수 있는 프로덕션 앱을 만들 수 있는 기술을 갖추게 되었습니다!

**프로젝트 URL**: https://ohama.github.io/CalTwo/
