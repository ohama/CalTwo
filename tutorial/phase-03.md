# Phase 3: UI 구현 (UI Implementation)

## 개요

이 튜토리얼에서는 Feliz를 사용하여 계산기의 UI를 구현합니다. CSS Grid 레이아웃으로 버튼 배치를 설계하고, 외부 CSS 파일로 호버/액티브 효과를 추가하며, 키보드 이벤트 처리와 Backspace 기능을 구현합니다.

### 학습 목표

- Feliz 뷰 함수로 React 컴포넌트를 F#으로 작성하는 방법 이해
- CSS Grid 레이아웃으로 2차원 버튼 배치 구현
- 외부 CSS 파일과 `:hover`, `:active` 의사 클래스(pseudo-classes) 사용
- 키보드 이벤트 핸들링 및 `preventDefault()` 메커니즘
- F# 문자열 슬라이싱으로 Backspace 기능 구현
- 반응형 디자인과 WCAG 접근성 기준

### 완성 후 결과물

- 4×5 CSS Grid 레이아웃으로 배치된 계산기 버튼
- 호버 시 확대, 클릭 시 축소되는 시각적 피드백
- 마우스와 키보드 양쪽에서 동작하는 완전한 UI
- Backspace 키로 마지막 문자 삭제 기능
- 모바일 환경에서도 사용 가능한 반응형 디자인
- WCAG 2.1 AAA 기준 터치 타겟 크기 준수

---

## 1. Feliz 뷰 함수 이해하기

### 1.1 Feliz란?

Feliz는 F#에서 React 컴포넌트를 작성하기 위한 DSL(Domain Specific Language)입니다. JSX를 사용하지 않고도 타입 안전하게 React 엘리먼트를 생성할 수 있습니다.

**JSX (JavaScript/TypeScript):**
```jsx
<div className="container">
  <h1>Hello</h1>
  <button onClick={() => alert("Clicked")}>Click</button>
</div>
```

**Feliz (F#):**
```fsharp
Html.div [
    prop.className "container"
    prop.children [
        Html.h1 "Hello"
        Html.button [
            prop.onClick (fun _ -> Browser.Dom.window.alert("Clicked"))
            prop.text "Click"
        ]
    ]
]
```

### 1.2 Feliz의 핵심 개념

**Html 모듈:**
- `Html.div`, `Html.button`, `Html.h1` 등 HTML 요소 생성
- 각 함수는 `prop` 리스트를 받아 React 엘리먼트 반환

**prop 모듈:**
- `prop.text`: 텍스트 콘텐츠
- `prop.onClick`: 클릭 이벤트 핸들러
- `prop.className`: CSS 클래스명
- `prop.style`: 인라인 스타일 리스트
- `prop.children`: 자식 엘리먼트 리스트
- `prop.onKeyDown`: 키보드 이벤트 핸들러
- `prop.tabIndex`: 키보드 포커스 순서

**style 모듈:**
- `style.width`: 너비
- `style.backgroundColor`: 배경색
- `style.padding`: 안쪽 여백
- `style.display.grid`: CSS Grid 레이아웃
- `style.custom`: 타입에 없는 CSS 속성

### 1.3 계산기 뷰 함수 전체 구조

```fsharp
let view (model: Model) (dispatch: Msg -> unit) =
    Html.div [
        prop.style [ style.width (length.percent 90); style.maxWidth 400; style.margin.auto ]
        prop.children [
            Html.h2 [
                prop.text "CalTwo Calculator"
            ]
            Html.div [
                prop.tabIndex 0  // 키보드 포커스 가능
                prop.onKeyDown (fun e -> (* 키보드 이벤트 처리 *))
                prop.children [
                    (* 디스플레이 *)
                    (* 버튼 그리드 *)
                ]
            ]
        ]
    ]
```

**구조 설명:**
1. 최상위 `Html.div`: 전체 컨테이너 (90% 너비, 최대 400px, 중앙 정렬)
2. `Html.h2`: 제목
3. 키보드 이벤트 핸들러를 가진 `Html.div`
4. 디스플레이 영역 (검정 배경, 녹색 텍스트)
5. CSS Grid 버튼 그리드

---

## 2. 디스플레이 영역 구현

### 2.1 디스플레이 요구사항

- 계산기 화면에 표시되는 숫자/결과
- 터미널 스타일 (검정 배경 `#222`, 녹색 글자 `#0f0`)
- 오른쪽 정렬 (일반 계산기처럼)
- 고정된 최소 높이 (내용이 없어도 레이아웃 유지)

### 2.2 디스플레이 코드

```fsharp
Html.div [
    prop.style [
        style.fontSize 32
        style.padding 15
        style.backgroundColor "#222"
        style.color "#0f0"
        style.textAlign.right
        style.marginBottom 10
        style.borderRadius 5
        style.minHeight 50
        style.lineHeight 50
    ]
    prop.text model.Display
]
```

**스타일 설명:**
- `fontSize 32`: 큰 글씨로 가독성 확보
- `padding 15`: 텍스트와 테두리 사이 여백
- `backgroundColor "#222"`: 어두운 회색 (순수 검정보다 눈이 편함)
- `color "#0f0"`: 밝은 녹색 (터미널 스타일)
- `textAlign.right`: 오른쪽 정렬 (숫자 입력이 오른쪽부터 쌓이는 느낌)
- `marginBottom 10`: 디스플레이와 버튼 사이 간격
- `borderRadius 5`: 둥근 모서리
- `minHeight 50`, `lineHeight 50`: 수직 중앙 정렬 및 고정 높이

### 2.3 왜 인라인 스타일을 사용했나?

**장점:**
- 컴포넌트와 스타일이 한 곳에 있어 관리 용이
- F# 타입 안전성 (오타 시 컴파일 에러)
- 동적 스타일 적용 가능 (예: `if error then "#f00" else "#0f0"`)

**단점:**
- `:hover`, `:active` 같은 의사 클래스 사용 불가
- 버튼 같은 반복 요소에서는 중복 코드 증가

→ 디스플레이는 인라인, 버튼은 외부 CSS 사용 (하이브리드 접근)

---

## 3. CSS Grid 레이아웃

### 3.1 CSS Grid란?

CSS Grid는 2차원 레이아웃 시스템입니다. 행(row)과 열(column)을 정의하고, 자식 요소를 격자에 배치합니다.

**Flexbox vs. Grid:**
- **Flexbox**: 1차원 (한 방향으로 정렬, 예: 수평 메뉴)
- **Grid**: 2차원 (행과 열 동시에 정의, 예: 계산기 버튼)

### 3.2 계산기 그리드 레이아웃

**목표:**
```
[ C ] [←] [  ] [ ÷ ]
[ 7 ] [ 8 ] [ 9 ] [ × ]
[ 4 ] [ 5 ] [ 6 ] [ - ]
[ 1 ] [ 2 ] [ 3 ] [ + ]
[   0     ] [ . ] [ = ]
```

**4열 그리드:**
- 1-4행: 각 셀에 버튼 하나씩
- 5행: 0 버튼이 2열을 차지, 나머지는 1열씩

### 3.3 CSS Grid 구현 코드

```fsharp
Html.div [
    prop.style [
        style.display.grid
        style.custom ("gridTemplateColumns", "repeat(4, 1fr)")
        style.gap 8
    ]
    prop.children [
        (* 버튼들 *)
    ]
]
```

**스타일 설명:**
- `style.display.grid`: 이 div를 Grid 컨테이너로 설정
- `style.custom ("gridTemplateColumns", "repeat(4, 1fr)")`: 4개의 열, 각 열은 `1fr` (fraction, 균등 분할)
- `style.gap 8`: 버튼 사이 간격 8px

**왜 `style.custom`을 사용했나?**
- Feliz의 타입 안전 API에 `gridTemplateColumns`가 없음
- `style.custom`으로 임의의 CSS 속성 설정 가능
- 트레이드오프: 타입 안전성 상실, 런타임에 오타 발견

### 3.4 `1fr` 단위란?

`fr` (fraction)은 사용 가능한 공간의 비율을 나타냅니다.

**예시:**
- `repeat(4, 1fr)`: 4개 열, 각각 25% 너비
- `2fr 1fr 1fr`: 3개 열, 첫 번째가 50%, 나머지 각 25%

**장점:**
- 픽셀(`px`) 대신 비율 사용 → 반응형
- `auto`처럼 콘텐츠에 따라 변하지 않음 → 일관된 레이아웃

### 3.5 0 버튼 2열 차지시키기

```fsharp
Html.button [
    prop.className "calc-button"
    prop.style [ style.custom ("gridColumn", "1 / 3") ]
    prop.text "0"
    prop.onClick (fun _ -> dispatch (DigitPressed 0))
]
```

**`gridColumn "1 / 3"`의 의미:**
- 그리드 라인 1부터 3까지 차지
- 라인 1-2: 첫 번째 열
- 라인 2-3: 두 번째 열
- 결과: 2개 열을 가로로 병합

**Grid 라인 번호:**
```
라인1  라인2  라인3  라인4  라인5
  |     |     |     |     |
  [  1  ][  2  ][  3  ][  4  ]
```

---

## 4. 외부 CSS와 호버/액티브 효과

### 4.1 왜 외부 CSS가 필요한가?

인라인 스타일(`prop.style`)로는 의사 클래스(pseudo-classes)를 사용할 수 없습니다.

**의사 클래스:**
- `:hover`: 마우스를 올렸을 때
- `:active`: 마우스를 클릭하는 순간
- `:focus`: 키보드 포커스를 받았을 때

**시도 (불가능):**
```fsharp
prop.style [
    style.backgroundColor "#f0f0f0"
    (* ❌ :hover를 인라인으로 정의할 수 없음! *)
]
```

**해결책:**
- 외부 CSS 파일 (`styles.css`) 생성
- CSS 클래스에 의사 클래스 정의
- Feliz에서 `prop.className`으로 클래스 적용

### 4.2 styles.css 파일

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
```

**스타일 설명:**
- `min-height`, `min-width: 44px`: WCAG 2.1 AAA 권장 터치 타겟 크기
- `transition: all 0.1s ease`: 부드러운 애니메이션 (0.1초)
- `:hover`: 배경색 어둡게 + 크기 5% 확대
- `:active`: 배경색 더 어둡게 + 크기 5% 축소 (클릭 피드백)
- `cursor: pointer`: 마우스 커서를 손가락 모양으로

### 4.3 버튼별 색상 클래스

**연산자 버튼 (오렌지):**
```css
.calc-operator {
    background: #ff9500;
    color: white;
    border-color: #e68600;
}
.calc-operator:hover {
    background: #ff8000;
}
.calc-operator:active {
    background: #e67300;
}
```

**= 버튼 (녹색):**
```css
.calc-equals {
    background: #4cd964;
    color: white;
    border-color: #3bb853;
}
```

**C 버튼 (빨강):**
```css
.calc-clear {
    background: #ff3b30;
    color: white;
    border-color: #e6352b;
}
```

**← 버튼 (회색):**
```css
.calc-backspace {
    background: #8e8e93;
    color: white;
    border-color: #7c7c80;
}
```

### 4.4 Main.fs에서 CSS 임포트

```fsharp
module Main

open Elmish
open Elmish.React
open Elmish.HMR
open Fable.Core.JsInterop

importSideEffects "./styles.css"

Program.mkProgram App.init App.update App.view
|> Program.withReactSynchronous "elmish-app"
|> Program.run
```

**`importSideEffects`의 역할:**
- Vite에게 CSS 파일을 번들에 포함하도록 지시
- "Side Effects" = 값을 반환하지 않지만 실행이 필요한 임포트
- 런타임에 CSS가 `<head>`에 자동 삽입됨

**왜 App.fs가 아닌 Main.fs에서?**
- Main.fs는 애플리케이션 진입점 (한 번만 실행)
- App.fs는 HMR로 여러 번 재로드될 수 있음 → CSS 중복 삽입 방지

### 4.5 Feliz에서 클래스 적용

```fsharp
Html.button [
    prop.className "calc-button calc-operator"  // 공백으로 여러 클래스
    prop.text "÷"
    prop.onClick (fun _ -> dispatch (OperatorPressed Divide))
]
```

**클래스 결합:**
- `calc-button`: 기본 스타일 (크기, 패딩, 호버 효과)
- `calc-operator`: 오렌지 색상
- CSS는 두 클래스의 스타일을 병합

---

## 5. 키보드 이벤트 처리

### 5.1 키보드 지원의 중요성

**접근성 (Accessibility):**
- 마우스를 사용할 수 없는 사용자 지원
- 키보드 파워 유저의 생산성 향상
- 스크린 리더와 호환

**사용자 경험:**
- 숫자 키패드로 빠른 입력
- Enter로 계산 실행
- Escape로 초기화

### 5.2 키보드 이벤트 핸들러 구조

```fsharp
Html.div [
    prop.tabIndex 0  // 이 div를 포커스 가능하게 만듦
    prop.onKeyDown (fun e ->
        match e.key with
        | "0" -> dispatch (DigitPressed 0); e.preventDefault()
        | "1" -> dispatch (DigitPressed 1); e.preventDefault()
        (* ... *)
        | "Enter" -> dispatch EqualsPressed; e.preventDefault()
        | "Escape" -> dispatch ClearPressed; e.preventDefault()
        | "Backspace" -> dispatch BackspacePressed; e.preventDefault()
        | _ -> ()  // 처리하지 않은 키는 브라우저 기본 동작 유지
    )
    prop.children [
        (* 디스플레이 및 버튼 *)
    ]
]
```

**핵심 요소:**
- `prop.tabIndex 0`: 키보드 포커스를 받을 수 있게 설정 (탭 키로 포커스 이동)
- `prop.onKeyDown`: 키를 누를 때 실행되는 이벤트 핸들러
- `e.key`: 눌린 키의 문자열 표현 (예: `"1"`, `"Enter"`, `"Backspace"`)
- `e.preventDefault()`: 브라우저 기본 동작 방지

### 5.3 `e.key` vs. 구식 `e.keyCode`

**e.key (권장):**
```fsharp
match e.key with
| "Enter" -> (* ... *)
| "Backspace" -> (* ... *)
```
- 명확한 문자열 (`"Enter"`, `"ArrowUp"`)
- 국제화 지원 (다른 키보드 레이아웃에서도 작동)

**e.keyCode (구식, 비권장):**
```fsharp
match e.keyCode with
| 13 -> (* Enter *)
| 8 -> (* Backspace *)
```
- 숫자 코드 (외우기 어려움)
- W3C에서 deprecated 선언

### 5.4 `e.preventDefault()`의 역할

브라우저는 특정 키에 대해 기본 동작을 가지고 있습니다:

**예시:**
- `Backspace`: 브라우저 뒤로 가기
- `/`: Firefox에서 "빠른 찾기" 열기
- `Ctrl+S`: 페이지 저장 다이얼로그
- `Space`: 페이지 스크롤

**문제:**
```fsharp
| "/" -> dispatch (OperatorPressed Divide)
// 사용자가 /를 누르면:
// 1. dispatch 실행 (계산기에 나누기 입력)
// 2. 브라우저가 빠른 찾기 열림 (원치 않음!)
```

**해결:**
```fsharp
| "/" -> dispatch (OperatorPressed Divide); e.preventDefault()
// e.preventDefault()로 브라우저 기본 동작 차단
```

### 5.5 처리하지 않은 키는 기본 동작 유지

```fsharp
| _ -> ()  // preventDefault() 호출 안 함!
```

**왜?**
- Tab 키: 포커스 이동 (브라우저 접근성)
- F5: 새로고침
- Ctrl+C: 복사
- 이런 기본 동작들은 유지해야 사용자 경험 향상

**원칙:**
- 계산기가 처리하는 키만 `preventDefault()`
- 나머지는 브라우저에 맡김

---

## 6. Backspace 기능 구현

### 6.1 Backspace 요구사항

**일반 동작:**
- "123" → Backspace → "12"
- "45.67" → Backspace → "45.6"

**엣지 케이스:**
- "7" (단일 문자) → Backspace → "0" (빈 문자열이 아님!)
- "Error" → Backspace → 무시 (에러는 숫자 입력으로만 복구)
- "0" → Backspace → "0" (이미 초기 상태)

### 6.2 Calculator.fs에 메시지 추가

```fsharp
type Msg =
    | DigitPressed of int
    | DecimalPressed
    | OperatorPressed of MathOp
    | EqualsPressed
    | ClearPressed
    | BackspacePressed  // 새로 추가!
```

### 6.3 App.fs에서 Backspace 처리

```fsharp
| BackspacePressed ->
    if model.Display = "Error" || model.Display = "0" then
        model, Cmd.none  // 에러 상태나 "0"일 때는 무시
    elif model.Display.Length = 1 then
        { model with Display = "0" }, Cmd.none  // 단일 문자 → "0"으로 리셋
    else
        { model with Display = model.Display.[0..model.Display.Length-2] }, Cmd.none
```

**코드 설명:**
1. **에러 상태 체크:** `"Error"`는 Backspace로 삭제 불가 (사용자가 숫자를 입력해야 복구)
2. **초기 "0" 체크:** 이미 초기 상태면 아무 일도 안 함
3. **단일 문자 체크:** 길이가 1이면 삭제 후 빈 문자열이 되므로 "0"으로 리셋
4. **일반 삭제:** F# 문자열 슬라이싱으로 마지막 문자 제거

### 6.4 F# 문자열 슬라이싱

```fsharp
let s = "12345"

s.[0]           // '1' (첫 문자)
s.[4]           // '5' (마지막 문자)
s.[0..2]        // "123" (인덱스 0부터 2까지)
s.[2..4]        // "345" (인덱스 2부터 4까지)
s.[0..s.Length-2]  // "1234" (마지막 문자 제외)
```

**Backspace 구현에서:**
```fsharp
model.Display.[0..model.Display.Length-2]
```
- `Length-2`: 마지막 인덱스 - 1 (마지막 문자 직전까지)
- 예: `"123"` (Length=3) → `[0..1]` → `"12"`

**왜 `Length-1`이 아닌가?**
- `[0..Length-1]`은 전체 문자열 (마지막 문자 포함)
- `[0..Length-2]`는 마지막 문자 제외

### 6.5 단위 테스트로 검증

```fsharp
testList "Backspace" [
    testCase "Backspace on '123' → '12'" <| fun () ->
        let model = { Display = "123"; PendingOp = None; StartNew = false }
        let result = update BackspacePressed model |> fst
        Expect.equal result.Display "12" "Should remove last character"

    testCase "Backspace on '7' (single char) → '0'" <| fun () ->
        let model = { Display = "7"; PendingOp = None; StartNew = false }
        let result = update BackspacePressed model |> fst
        Expect.equal result.Display "0" "Should reset to '0', not empty string"

    testCase "Backspace on 'Error' → no change" <| fun () ->
        let model = { Display = "Error"; PendingOp = None; StartNew = true }
        let result = update BackspacePressed model |> fst
        Expect.equal result.Display "Error" "Should ignore backspace on error"

    testCase "Backspace on '0' → no change" <| fun () ->
        let model = init ()
        let result = update BackspacePressed model |> fst
        Expect.equal result.Display "0" "Should ignore backspace on initial '0'"
]
```

### 6.6 키보드에서 Backspace 연결

```fsharp
prop.onKeyDown (fun e ->
    match e.key with
    | "Backspace" -> dispatch BackspacePressed; e.preventDefault()
    // ...
)
```

**브라우저 기본 동작:**
- Backspace 키는 브라우저에서 "뒤로 가기" 실행
- `preventDefault()`로 차단하지 않으면 사용자가 Backspace 누를 때마다 페이지 뒤로 이동!

---

## 7. 반응형 디자인

### 7.1 반응형 디자인이란?

다양한 화면 크기(데스크톱, 태블릿, 모바일)에서 최적의 사용자 경험을 제공하는 디자인.

**핵심 기술:**
- **유연한 단위:** `%`, `vw`, `fr` (픽셀 대신)
- **미디어 쿼리:** 화면 크기에 따라 다른 CSS 적용
- **터치 타겟 크기:** 모바일에서 손가락으로 누를 수 있는 크기

### 7.2 계산기의 반응형 전략

**컨테이너:**
```fsharp
prop.style [
    style.width (length.percent 90)  // 화면 너비의 90%
    style.maxWidth 400               // 최대 400px (데스크톱에서 너무 커지지 않도록)
    style.margin.auto                // 수평 중앙 정렬
]
```

**작동 방식:**
- **모바일 (375px 화면):** `90% = 337px` → 화면에 꽉 참
- **태블릿 (768px 화면):** `90% = 691px`이지만 `maxWidth 400px`로 제한 → 중앙 정렬
- **데스크톱 (1920px 화면):** `maxWidth 400px` 고정 → 중앙 정렬

### 7.3 CSS Grid의 반응형 특성

```fsharp
style.custom ("gridTemplateColumns", "repeat(4, 1fr)")
```

**`1fr` 단위의 장점:**
- 화면 너비에 따라 자동으로 버튼 크기 조정
- 픽셀 고정 대신 비율 사용 → 모든 화면에서 균등 배치

**예시:**
- 컨테이너 400px → 각 열 100px
- 컨테이너 320px (모바일) → 각 열 80px

### 7.4 미디어 쿼리로 모바일 터치 타겟 확대

```css
.calc-button {
    min-height: 44px;
    min-width: 44px;
}

@media (max-width: 375px) {
    .calc-button {
        min-height: 48px;
        min-width: 48px;
        font-size: 16px;
    }
}
```

**미디어 쿼리 설명:**
- `@media (max-width: 375px)`: 화면 너비가 375px 이하일 때 적용
- 모바일 환경에서 버튼 크기를 44px → 48px로 확대

### 7.5 WCAG 접근성 기준

**WCAG 2.1 AAA - 터치 타겟 크기:**
- 최소 크기: 44×44 CSS 픽셀
- 권장 크기: 48×48 CSS 픽셀 (사용자가 손가락으로 쉽게 누를 수 있음)

**왜 중요한가?**
- 손가락 평균 크기: 약 40-48px
- 작은 버튼: 오터치 증가, 사용자 불만
- 접근성: 운동 장애가 있는 사용자도 쉽게 사용

**이 프로젝트:**
- 기본: `44px` (AAA 최소 기준)
- 모바일: `48px` (권장 크기)

---

## 8. 전체 코드 정리

### 8.1 Calculator.fs (전체)

```fsharp
module Calculator

type MathOp =
    | Add
    | Subtract
    | Multiply
    | Divide

type MathResult =
    | Success of float
    | DivideByZeroError

type Msg =
    | DigitPressed of int
    | DecimalPressed
    | OperatorPressed of MathOp
    | EqualsPressed
    | ClearPressed
    | BackspacePressed  // Phase 3 추가

type Model = {
    Display: string
    PendingOp: (MathOp * float) option
    StartNew: bool
}

let doMathOp (op: MathOp) (a: float) (b: float) : MathResult =
    match op with
    | Add -> Success (a + b)
    | Subtract -> Success (a - b)
    | Multiply -> Success (a * b)
    | Divide ->
        if b = 0.0 then DivideByZeroError
        else Success (a / b)

let parseDisplay (display: string) : float =
    match System.Double.TryParse(display) with
    | true, v -> v
    | false, _ -> 0.0

let formatResult (value: float) : string =
    string value
```

### 8.2 App.fs (전체)

```fsharp
module App

open Feliz
open Elmish
open Calculator

let init () : Model * Cmd<Msg> =
    { Display = "0"; PendingOp = None; StartNew = false }, Cmd.none

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | DigitPressed digit ->
        if model.Display = "Error" then
            { Display = string digit; PendingOp = None; StartNew = false }, Cmd.none
        elif model.StartNew then
            { model with Display = string digit; StartNew = false }, Cmd.none
        elif model.Display = "0" && digit = 0 then
            model, Cmd.none
        elif model.Display = "0" then
            { model with Display = string digit }, Cmd.none
        else
            { model with Display = model.Display + string digit }, Cmd.none

    | DecimalPressed ->
        if model.StartNew then
            { model with Display = "0."; StartNew = false }, Cmd.none
        elif not (model.Display.Contains(".")) then
            { model with Display = model.Display + "." }, Cmd.none
        else
            model, Cmd.none

    | OperatorPressed op ->
        let currentValue = parseDisplay model.Display
        match model.PendingOp with
        | None ->
            { model with PendingOp = Some (op, currentValue); StartNew = true }, Cmd.none
        | Some (pendingOp, firstOperand) ->
            if model.StartNew then
                { model with PendingOp = Some (op, firstOperand) }, Cmd.none
            else
                match doMathOp pendingOp firstOperand currentValue with
                | Success result ->
                    let resultStr = formatResult result
                    { Display = resultStr; PendingOp = Some (op, result); StartNew = true }, Cmd.none
                | DivideByZeroError ->
                    { Display = "Error"; PendingOp = None; StartNew = true }, Cmd.none

    | EqualsPressed ->
        match model.PendingOp with
        | None ->
            model, Cmd.none
        | Some (op, firstOperand) ->
            let secondOperand = parseDisplay model.Display
            match doMathOp op firstOperand secondOperand with
            | Success result ->
                let resultStr = formatResult result
                { Display = resultStr; PendingOp = None; StartNew = true }, Cmd.none
            | DivideByZeroError ->
                { Display = "Error"; PendingOp = None; StartNew = true }, Cmd.none

    | ClearPressed ->
        init () |> fst, Cmd.none

    | BackspacePressed ->
        if model.Display = "Error" || model.Display = "0" then
            model, Cmd.none
        elif model.Display.Length = 1 then
            { model with Display = "0" }, Cmd.none
        else
            { model with Display = model.Display.[0..model.Display.Length-2] }, Cmd.none

let view (model: Model) (dispatch: Msg -> unit) =
    Html.div [
        prop.style [ style.width (length.percent 90); style.maxWidth 400; style.margin.auto; style.fontFamily "monospace" ]
        prop.children [
            Html.h2 [
                prop.style [ style.textAlign.center ]
                prop.text "CalTwo Calculator"
            ]
            // Keyboard-enabled container
            Html.div [
                prop.tabIndex 0
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
                    | _ -> ()
                )
                prop.children [
                    // Display
                    Html.div [
                        prop.style [
                            style.fontSize 32
                            style.padding 15
                            style.backgroundColor "#222"
                            style.color "#0f0"
                            style.textAlign.right
                            style.marginBottom 10
                            style.borderRadius 5
                            style.minHeight 50
                            style.lineHeight 50
                        ]
                        prop.text model.Display
                    ]
                    // Button grid (4 columns)
                    Html.div [
                        prop.style [
                            style.display.grid
                            style.custom ("gridTemplateColumns", "repeat(4, 1fr)")
                            style.gap 8
                        ]
                        prop.children [
                            // Row 1: C, ←, (empty), ÷
                            Html.button [
                                prop.className "calc-button calc-clear"
                                prop.text "C"
                                prop.onClick (fun _ -> dispatch ClearPressed)
                            ]
                            Html.button [
                                prop.className "calc-button calc-backspace"
                                prop.text "←"
                                prop.onClick (fun _ -> dispatch BackspacePressed)
                            ]
                            Html.div []  // Empty cell
                            Html.button [
                                prop.className "calc-button calc-operator"
                                prop.text "÷"
                                prop.onClick (fun _ -> dispatch (OperatorPressed Divide))
                            ]
                            // Row 2: 7, 8, 9, ×
                            Html.button [
                                prop.className "calc-button"
                                prop.text "7"
                                prop.onClick (fun _ -> dispatch (DigitPressed 7))
                            ]
                            Html.button [
                                prop.className "calc-button"
                                prop.text "8"
                                prop.onClick (fun _ -> dispatch (DigitPressed 8))
                            ]
                            Html.button [
                                prop.className "calc-button"
                                prop.text "9"
                                prop.onClick (fun _ -> dispatch (DigitPressed 9))
                            ]
                            Html.button [
                                prop.className "calc-button calc-operator"
                                prop.text "×"
                                prop.onClick (fun _ -> dispatch (OperatorPressed Multiply))
                            ]
                            // Row 3: 4, 5, 6, -
                            Html.button [
                                prop.className "calc-button"
                                prop.text "4"
                                prop.onClick (fun _ -> dispatch (DigitPressed 4))
                            ]
                            Html.button [
                                prop.className "calc-button"
                                prop.text "5"
                                prop.onClick (fun _ -> dispatch (DigitPressed 5))
                            ]
                            Html.button [
                                prop.className "calc-button"
                                prop.text "6"
                                prop.onClick (fun _ -> dispatch (DigitPressed 6))
                            ]
                            Html.button [
                                prop.className "calc-button calc-operator"
                                prop.text "-"
                                prop.onClick (fun _ -> dispatch (OperatorPressed Subtract))
                            ]
                            // Row 4: 1, 2, 3, +
                            Html.button [
                                prop.className "calc-button"
                                prop.text "1"
                                prop.onClick (fun _ -> dispatch (DigitPressed 1))
                            ]
                            Html.button [
                                prop.className "calc-button"
                                prop.text "2"
                                prop.onClick (fun _ -> dispatch (DigitPressed 2))
                            ]
                            Html.button [
                                prop.className "calc-button"
                                prop.text "3"
                                prop.onClick (fun _ -> dispatch (DigitPressed 3))
                            ]
                            Html.button [
                                prop.className "calc-button calc-operator"
                                prop.text "+"
                                prop.onClick (fun _ -> dispatch (OperatorPressed Add))
                            ]
                            // Row 5: 0 (span 2), ., =
                            Html.button [
                                prop.className "calc-button"
                                prop.style [ style.custom ("gridColumn", "1 / 3") ]
                                prop.text "0"
                                prop.onClick (fun _ -> dispatch (DigitPressed 0))
                            ]
                            Html.button [
                                prop.className "calc-button"
                                prop.text "."
                                prop.onClick (fun _ -> dispatch DecimalPressed)
                            ]
                            Html.button [
                                prop.className "calc-button calc-equals"
                                prop.text "="
                                prop.onClick (fun _ -> dispatch EqualsPressed)
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]
```

### 8.3 styles.css (전체)

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
.calc-operator {
    background: #ff9500;
    color: white;
    border-color: #e68600;
}
.calc-operator:hover {
    background: #ff8000;
}
.calc-operator:active {
    background: #e67300;
}
.calc-equals {
    background: #4cd964;
    color: white;
    border-color: #3bb853;
}
.calc-equals:hover {
    background: #3cc954;
}
.calc-equals:active {
    background: #2db844;
}
.calc-clear {
    background: #ff3b30;
    color: white;
    border-color: #e6352b;
}
.calc-clear:hover {
    background: #ff2b20;
}
.calc-clear:active {
    background: #e6251a;
}
.calc-backspace {
    background: #8e8e93;
    color: white;
    border-color: #7c7c80;
}
.calc-backspace:hover {
    background: #7e7e83;
}
.calc-backspace:active {
    background: #6e6e73;
}
@media (max-width: 375px) {
    .calc-button {
        min-height: 48px;
        min-width: 48px;
        font-size: 16px;
    }
}
```

### 8.4 Main.fs (전체)

```fsharp
module Main

open Elmish
open Elmish.React
open Elmish.HMR
open Fable.Core.JsInterop

importSideEffects "./styles.css"

Program.mkProgram App.init App.update App.view
|> Program.withReactSynchronous "elmish-app"
|> Program.run
```

---

## 9. 실행 및 테스트

### 9.1 개발 서버 실행

```bash
npm run dev
```

브라우저에서 http://localhost:5173/ 접속.

**확인 사항:**
1. **레이아웃:** 4×5 그리드에 버튼이 정렬되어 있는지
2. **호버 효과:** 마우스를 버튼 위에 올리면 크기가 약간 커지는지
3. **클릭 효과:** 버튼을 클릭하면 크기가 약간 줄어드는지
4. **색상:** C(빨강), ←(회색), 연산자(오렌지), =(녹색), 숫자(밝은 회색)
5. **디스플레이:** 검정 배경에 녹색 텍스트

### 9.2 키보드 테스트

**포커스 설정:**
- 페이지 로드 후 계산기 영역 클릭 (키보드 포커스 활성화)

**테스트 시나리오:**
1. `2`, `+`, `3`, `Enter` → "5" 표시
2. `Escape` → "0" (초기화)
3. `1`, `2`, `3`, `Backspace` → "12"
4. `5`, `*`, `4`, `Enter` → "20" (왼쪽에서 오른쪽 계산)
5. `/` 키 누를 때 Firefox 빠른 찾기가 열리지 않는지 확인 (preventDefault 작동)

### 9.3 모바일 반응형 테스트

**브라우저 개발자 도구:**
1. F12 열기
2. 디바이스 툴바 활성화 (Ctrl+Shift+M)
3. 화면 크기 변경:
   - iPhone SE (375×667)
   - iPad (768×1024)
   - Desktop (1920×1080)

**확인 사항:**
- 모든 화면 크기에서 버튼이 균등하게 배치되는지
- 모바일에서 터치 타겟 크기가 충분한지 (손가락으로 쉽게 누를 수 있는지)
- 컨테이너가 화면 중앙에 정렬되는지

### 9.4 단위 테스트 실행

```bash
npm test
```

**기대 결과:**
```
  CalTwo
    Backspace
      ✔ Backspace on '123' → '12'
      ✔ Backspace on '7' (single char) → '0'
      ✔ Backspace on 'Error' → no change
      ✔ Backspace on '0' → no change
    (... 기존 27개 테스트도 모두 통과)

  31 passing (10ms)
```

---

## 10. 핵심 개념 복습

### 10.1 Feliz DSL의 타입 안전성

**컴파일 타임 에러:**
```fsharp
Html.div [
    prop.text 123  // ❌ 컴파일 에러! (prop.text는 string만 허용)
    prop.onClick "not-a-function"  // ❌ 컴파일 에러! (함수 필요)
]
```

**올바른 사용:**
```fsharp
Html.div [
    prop.text "123"  // ✅ string
    prop.onClick (fun _ -> (* ... *))  // ✅ 함수
]
```

### 10.2 CSS Grid vs. Flexbox 선택 기준

**CSS Grid 사용 (이 프로젝트):**
- 2차원 레이아웃 (행과 열)
- 고정된 그리드 구조
- 예: 계산기 버튼, 사진 갤러리, 달력

**Flexbox 사용:**
- 1차원 레이아웃 (한 방향)
- 동적 콘텐츠 크기
- 예: 네비게이션 바, 카드 리스트, 폼 레이아웃

### 10.3 인라인 스타일 vs. 외부 CSS

**인라인 스타일 (prop.style):**
- 장점: 컴포넌트와 함께 관리, F# 타입 안전성
- 단점: 의사 클래스 불가 (`:hover`, `:active`)
- 사용처: 디스플레이, 컨테이너 레이아웃

**외부 CSS (className):**
- 장점: 의사 클래스, 미디어 쿼리, 재사용 가능
- 단점: 타입 안전성 상실 (클래스명 오타 런타임 발견)
- 사용처: 버튼 스타일, 호버 효과, 애니메이션

**이 프로젝트의 하이브리드 접근:**
- 디스플레이: 인라인 스타일 (단순, 동적 가능성)
- 버튼: 외부 CSS (호버/액티브 효과 필수)

### 10.4 키보드 접근성 체크리스트

- [ ] `tabIndex` 설정으로 포커스 가능
- [ ] `onKeyDown` 핸들러로 키보드 이벤트 처리
- [ ] 처리하는 키만 `preventDefault()` (Tab, F5 등은 유지)
- [ ] Enter로 주요 액션 실행 가능
- [ ] Escape로 취소/초기화 가능
- [ ] 스크린 리더 사용자를 위한 ARIA 속성 (고급)

---

## 11. 다음 단계

Phase 3을 완료했습니다! 계산기가 완전히 동작하는 UI를 갖추게 되었습니다.

**Phase 4: E2E 테스트**

- Playwright로 브라우저 자동화 테스트
- 사용자 시나리오 검증 (클릭, 키보드 입력)
- 스크린샷 비교 테스트
- CI/CD 파이프라인 통합

**Phase 5: 배포 및 프로덕션**

- GitHub Pages 배포
- 프로덕션 빌드 최적화 (코드 스플리팅, 트리 쉐이킹)
- PWA (Progressive Web App) 기능 (오프라인 지원, 홈 화면 추가)
- 성능 측정 (Lighthouse)

**고급 기능 아이디어:**

- 다크 모드 / 라이트 모드 전환
- 계산 히스토리 (localStorage 저장)
- 과학용 계산기 모드 (sin, cos, log 등)
- 음성 입력 (Web Speech API)

---

## 12. 참고 자료

### Feliz 및 React

- **Feliz 공식 문서:** https://zaid-ajaj.github.io/Feliz/
- **Feliz GitHub:** https://github.com/Zaid-Ajaj/Feliz
- **React 이벤트 핸들링:** https://react.dev/learn/responding-to-events

### CSS Grid

- **CSS Tricks Guide:** https://css-tricks.com/snippets/css/complete-guide-grid/
- **Grid by Example:** https://gridbyexample.com/
- **MDN Grid 가이드:** https://developer.mozilla.org/en-US/docs/Web/CSS/CSS_Grid_Layout

### 접근성

- **WCAG 2.1 AAA:** https://www.w3.org/WAI/WCAG21/quickref/
- **터치 타겟 크기:** https://www.w3.org/WAI/WCAG21/Understanding/target-size.html
- **키보드 접근성:** https://webaim.org/techniques/keyboard/

### F# 문자열

- **F# 문자열 슬라이싱:** https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/strings
- **System.String API:** https://learn.microsoft.com/en-us/dotnet/api/system.string

---

**축하합니다!** Phase 3을 완료했습니다. 이제 계산기가 완전히 동작하는 UI를 갖추었고, CSS Grid 레이아웃, 외부 CSS 스타일링, 키보드 이벤트 처리, Backspace 기능, 반응형 디자인까지 모두 구현되었습니다. 마우스와 키보드 양쪽에서 사용할 수 있는 접근성 높은 웹 애플리케이션이 완성되었습니다!
