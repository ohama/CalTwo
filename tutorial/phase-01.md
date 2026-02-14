# Phase 1: 기본 개발 환경 구축 튜토리얼

## 개요

이 튜토리얼에서는 F# + Fable + Elmish + React를 사용하여 브라우저 기반 계산기 애플리케이션을 개발하기 위한 기본 환경을 구축합니다.

### 학습 목표

- .NET SDK와 Node.js를 사용한 F# 웹 개발 환경 이해
- Fable 트랜스파일러를 통한 F#에서 JavaScript로의 변환 과정 이해
- Vite 빌드 도구와 HMR(Hot Module Replacement) 활용
- Elmish MVU 패턴의 기본 구조 이해

### 완성 후 결과물

- "Hello CalTwo" 메시지를 표시하는 React 앱
- 개발 서버에서 즉시 실행 가능한 환경
- 코드 변경 시 자동 새로고침되는 HMR
- 프로덕션 빌드로 정적 파일 생성 가능

---

## 사전 요구사항

개발을 시작하기 전에 다음 도구들이 설치되어 있어야 합니다:

### 1. .NET SDK 10.0.100

F# 컴파일러와 .NET 런타임을 제공합니다.

**설치 확인:**
```bash
dotnet --version
```

출력: `10.0.100` 또는 그 이상

**다운로드:** https://dotnet.microsoft.com/download

### 2. Node.js (v18 이상)

npm 패키지 관리자와 JavaScript 런타임을 제공합니다.

**설치 확인:**
```bash
node --version
npm --version
```

**다운로드:** https://nodejs.org/

---

## 1단계: 버전 고정 및 프로젝트 초기화

### global.json - .NET SDK 버전 고정

프로젝트 루트에 `global.json` 파일을 생성합니다:

```json
{
  "sdk": {
    "version": "10.0.100",
    "rollForward": "latestMinor"
  }
}
```

**역할:**
- 이 프로젝트는 .NET SDK 10.0.100을 사용
- `rollForward: "latestMinor"`는 10.0.x 버전 중 최신 마이너 버전 허용
- 팀원 간 동일한 SDK 버전 보장

### package.json - Node.js 패키지 관리

```json
{
  "name": "caltwo",
  "version": "0.1.0",
  "type": "module",
  "scripts": {
    "dev": "vite",
    "build": "vite build",
    "preview": "vite preview"
  },
  "dependencies": {
    "react": "18.3.1",
    "react-dom": "18.3.1"
  },
  "devDependencies": {
    "vite": "^7.0.0",
    "vite-plugin-fable": "0.2.0",
    "@vitejs/plugin-react": "^4.3.4"
  }
}
```

**주요 패키지:**
- **React 18.3.1:** Feliz 2.x와의 호환성을 위해 React 19가 아닌 18.3.1 사용
- **Vite 7.x:** 고속 개발 서버 및 빌드 도구
- **vite-plugin-fable 0.2.0:** F# 파일을 JavaScript로 트랜스파일
- **@vitejs/plugin-react:** React 지원 및 Fast Refresh

---

## 2단계: F# 프로젝트 구조

### App.fsproj - F# 프로젝트 파일

`src/App.fsproj` 파일:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
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

**패키지 설명:**
- **Fable.Core:** F#에서 JavaScript로 변환하는 핵심 라이브러리
- **Feliz:** React 컴포넌트를 F#으로 작성하기 위한 DSL
- **Fable.Elmish:** MVU(Model-View-Update) 아키텍처 구현
- **Fable.Elmish.React:** Elmish와 React 통합
- **Fable.Elmish.HMR:** 개발 중 코드 변경 시 상태 유지하며 새로고침
- **Fable.Browser.Dom:** 브라우저 DOM API 접근

### App.fs - Elmish MVU 구현

`src/App.fs` 파일:

```fsharp
module App

open Elmish
open Feliz
open Fable.Core.JsInterop

// MODEL
type Model = { Message: string }

let init () =
    { Message = "Hello CalTwo" }, Cmd.none

// UPDATE
type Msg =
    | NoOp

let update msg model =
    match msg with
    | NoOp -> model, Cmd.none

// VIEW
let view model dispatch =
    Html.div [
        Html.h1 model.Message
    ]
```

**MVU 패턴 구성 요소:**
- **Model:** 애플리케이션 상태 (`{ Message: string }`)
- **init:** 초기 상태 생성 (`"Hello CalTwo"` 메시지)
- **Msg:** 상태 변경을 나타내는 메시지 타입
- **update:** 메시지에 따라 모델을 업데이트
- **view:** 모델을 받아 UI를 렌더링

### Main.fs - 애플리케이션 진입점

`src/Main.fs` 파일:

```fsharp
module Main

open Elmish
open Elmish.React
open Elmish.HMR

Program.mkProgram App.init App.update App.view
|> Program.withReactSynchronous "elmish-app"
|> Program.run
```

**역할:**
- `Program.mkProgram`으로 MVU 패턴 연결
- `withReactSynchronous`로 React에 동기적으로 렌더링
- `"elmish-app"` div 요소에 앱 마운트
- HMR 지원 (파일 상단에서 `open Elmish.HMR`)

---

## 3단계: Vite 설정

### vite.config.js - Vite 빌드 도구 설정

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
  build: {
    sourcemap: true
  }
});
```

**중요 포인트:**
- **플러그인 순서:** `fable()`이 `react()` 앞에 와야 함 (F# → JS 변환 후 React 처리)
- **fsproj 옵션:** F# 프로젝트 파일 경로 지정 (필수!)
- **jsx: "automatic":** React 17+ 자동 JSX 변환
- **sourcemap: true:** 디버깅을 위한 소스맵 생성

### index.html - HTML 진입점

```html
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

**작동 방식:**
1. `<div id="elmish-app">` 요소에 React 앱이 마운트됨
2. `<script type="module">` 블록에서 Main.fs를 직접 import
3. Vite가 개발 중에는 실시간 변환, 빌드 시에는 번들링 수행

---

## 4단계: 의존성 설치 및 실행

### npm 패키지 설치

```bash
npm install
```

**설치되는 내용:**
- React, ReactDOM (UI 라이브러리)
- Vite 및 플러그인들 (빌드 도구)
- 약 190개 패키지 설치
- `package-lock.json` 생성 (정확한 버전 기록)

### .NET 패키지 복원

```bash
dotnet restore src/App.fsproj
```

**복원되는 내용:**
- Fable.Core, Feliz, Elmish 등 F# 패키지
- `src/obj/project.assets.json` 생성
- NuGet 패키지 캐시에 다운로드

### 개발 서버 실행

```bash
npm run dev
```

**실행 결과:**
```
VITE v7.3.1  ready in 500 ms

➜  Local:   http://localhost:5173/
➜  Network: use --host to expose
```

**브라우저에서 확인:**
1. http://localhost:5173/ 접속
2. "Hello CalTwo" 텍스트 확인
3. 브라우저 개발자 도구 콘솔에서 에러 없는지 확인

---

## 5단계: HMR(Hot Module Replacement) 확인

HMR은 코드를 변경할 때 전체 페이지를 새로고침하지 않고 변경된 모듈만 교체하는 기능입니다.

### HMR 테스트 방법

1. 개발 서버가 실행 중인 상태에서 브라우저 열기
2. `src/App.fs` 파일 열기
3. `Message` 값 변경:

```fsharp
let init () =
    { Message = "안녕하세요 CalTwo!" }, Cmd.none
```

4. 파일 저장
5. 브라우저가 자동으로 새로고침되며 "안녕하세요 CalTwo!" 표시 확인

**HMR의 장점:**
- 빠른 피드백 루프
- 복잡한 상태를 유지하면서 UI만 업데이트 가능
- 개발 생산성 향상

---

## 6단계: 소스맵 확인

소스맵은 트랜스파일된 JavaScript를 원본 F# 코드와 연결해주는 파일입니다.

### 소스맵 확인 방법

1. 브라우저 개발자 도구 열기 (F12)
2. **Sources** 탭 선택
3. 파일 트리에서 `src/` 폴더 확인
4. `App.fs`, `Main.fs` 파일이 보이는지 확인
5. 중단점 설정 및 디버깅 가능

### 프로덕션 빌드

```bash
npm run build
```

**빌드 결과:**
```
dist/index.html                  0.29 kB
dist/assets/index-xxxxx.js     172.91 kB
```

**생성된 파일:**
- `dist/index.html`: 프로덕션 HTML
- `dist/assets/*.js`: 번들링된 JavaScript (해시 포함)
- `dist/assets/*.js.map`: 소스맵 파일

빌드된 파일은 정적 호스팅 서비스에 배포 가능합니다.

---

## 핵심 개념 정리

### Fable이란?

Fable은 F# 코드를 JavaScript로 트랜스파일하는 컴파일러입니다.

- **F# → JavaScript 변환:** 타입 안전성 유지하면서 브라우저에서 실행
- **완전한 JavaScript 생태계 접근:** npm 패키지 사용 가능
- **최신 JavaScript 표준:** ES6+ 코드 생성

### Elmish MVU 패턴이란?

MVU(Model-View-Update)는 Elm 언어에서 영감을 받은 아키텍처 패턴입니다.

**데이터 흐름:**
```
User → View → Msg → Update → Model → View
```

**특징:**
- **단방향 데이터 흐름:** 예측 가능한 상태 관리
- **불변성:** 모든 상태는 불변 데이터 구조
- **순수 함수:** update와 view는 부작용 없는 순수 함수
- **타임 트래블 디버깅:** 모든 상태 변경이 메시지로 기록

### Feliz란?

Feliz는 F#에서 React 컴포넌트를 작성하기 위한 DSL(Domain Specific Language)입니다.

**예시:**
```fsharp
Html.div [
    prop.className "container"
    prop.children [
        Html.h1 "제목"
        Html.p "내용"
    ]
]
```

**특징:**
- **타입 안전:** 컴파일 타임에 prop 오류 검출
- **IntelliSense 지원:** IDE 자동완성
- **React 호환:** 기존 React 생태계와 완벽 통합

### HMR(Hot Module Replacement)이란?

코드 변경 시 브라우저를 새로고침하지 않고 모듈만 교체하는 기술입니다.

**작동 방식:**
1. 파일 저장 감지
2. Fable이 F# → JS 트랜스파일
3. Vite가 변경된 모듈만 브라우저로 전송
4. 브라우저가 모듈 교체 (페이지 새로고침 없이)

**Elmish.HMR의 역할:**
- 상태 보존하며 뷰만 업데이트
- `Program.run`을 HMR 지원 버전으로 교체

---

## 자주 발생하는 문제

### 1. "No .fsproj file was found" 에러

**원인:** vite.config.js에 fsproj 경로가 지정되지 않음

**해결:**
```javascript
fable({
  fsproj: "src/App.fsproj",  // 이 줄 추가!
  jsx: "automatic"
})
```

### 2. npm install 실패 (peer dependency 충돌)

**원인:** vite-plugin-fable 버전과 Vite 버전 불일치

**해결:**
- vite-plugin-fable 0.2.x → Vite ^7.0.0 사용
- vite-plugin-fable 0.1.x → Vite ^6.0.0 사용

### 3. "Expected ';', '}' or <eof>" 빌드 에러

**원인:** Fable 플러그인이 F# 파일을 제대로 트랜스파일하지 못함

**해결:**
1. `fsproj` 옵션이 올바른지 확인
2. `dotnet restore` 실행
3. 캐시 삭제 후 재시도: `rm -rf node_modules/.vite`

### 4. 브라우저에서 빈 화면만 보임

**확인 사항:**
1. 브라우저 콘솔에 에러 메시지 확인
2. `<div id="elmish-app">`가 index.html에 있는지 확인
3. `Program.withReactSynchronous "elmish-app"` ID가 일치하는지 확인

### 5. HMR이 작동하지 않음

**확인 사항:**
1. `Main.fs`에 `open Elmish.HMR` 포함되었는지 확인
2. Fable.Elmish.HMR 패키지가 설치되었는지 확인
3. 개발 서버 재시작

---

## 다음 단계

Phase 1을 완료했습니다! 이제 다음 단계로 넘어갈 준비가 되었습니다:

**Phase 2: 계산기 UI 컴포넌트 구현**
- 버튼 그리드 레이아웃
- 디스플레이 영역
- Tailwind CSS 스타일링
- 반응형 디자인

**Phase 3: 계산 로직 구현**
- 사칙연산 처리
- 정확한 소수점 계산
- 에러 핸들링

**Phase 4: 고급 기능 추가**
- 키보드 입력 지원
- 계산 히스토리
- 테마 전환

**Phase 5: 배포**
- GitHub Pages 설정
- 프로덕션 최적화
- CI/CD 파이프라인

---

## 참고 자료

- **Fable 공식 문서:** https://fable.io/
- **Elmish 가이드:** https://elmish.github.io/elmish/
- **Feliz 문서:** https://zaid-ajaj.github.io/Feliz/
- **Vite 문서:** https://vitejs.dev/
- **F# 공식 사이트:** https://fsharp.org/

---

**축하합니다!** Phase 1을 완료했습니다. 개발 환경이 구축되었고, HMR과 소스맵이 작동하는 것을 확인했습니다. 이제 본격적으로 계산기 기능을 구현할 준비가 되었습니다!
