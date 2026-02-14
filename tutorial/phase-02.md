# Phase 2: 계산기 핵심 로직 (Core Calculator Logic)

## 개요

이 튜토리얼에서는 MVU(Model-View-Update) 패턴을 사용하여 계산기의 핵심 로직을 구현합니다. TDD(Test-Driven Development) 방법론을 따라 테스트를 먼저 작성하고, 이를 통과하는 순수 함수로 계산 로직을 구현합니다.

### 학습 목표

- MVU 패턴으로 상태 관리하는 방법 이해
- F# 판별 구분 조합(Discriminated Unions)으로 타입 안전한 설계
- 순수 함수(Pure Functions)로 예측 가능한 로직 구현
- TDD 방법론: RED → GREEN 사이클
- Fable.Mocha로 단위 테스트 작성 및 실행

### 완성 후 결과물

- 사칙연산(+, -, ×, ÷)이 정확히 동작하는 계산기
- 왼쪽에서 오른쪽으로 순차 계산하는 로직 (2 + 3 × 4 = 20)
- 0으로 나누기 등 엣지 케이스 처리
- 브라우저에서 실시간으로 동작하는 인터랙티브 UI
- 27개의 단위 테스트로 검증된 로직

---

## 1. MVU 패턴 이해하기

### MVU란?

MVU(Model-View-Update)는 Elm 언어에서 영감을 받은 아키텍처 패턴입니다. React의 Redux, Vuex와 비슷하지만, 더 단순하고 타입 안전합니다.

### MVU 데이터 흐름

```
┌─────────────────────────────────────────────┐
│                   Model                     │
│            (애플리케이션 상태)               │
│   { Display: "123", PendingOp: None, ... }  │
└──────────────┬──────────────────────────────┘
               │
               ▼
        ┌──────────────┐
        │     View     │ ──── 사용자에게 UI 표시
        │  (렌더링)    │
        └──────┬───────┘
               │
        사용자 클릭 "+"
               │
               ▼
        ┌──────────────┐
        │     Msg      │ ──── OperatorPressed Add
        │   (메시지)    │
        └──────┬───────┘
               │
               ▼
        ┌──────────────┐
        │    Update    │ ──── 순수 함수로 새 Model 생성
        │  (상태 변경)  │
        └──────┬───────┘
               │
               ▼
        ┌──────────────┐
        │  New Model   │
        └──────────────┘
               │
               └──────────── (처음으로 돌아감)
```

### MVU의 핵심 원칙

1. **단방향 데이터 흐름:** View → Msg → Update → Model → View
2. **불변성(Immutability):** Model은 절대 변경되지 않고, 항상 새로운 Model을 생성
3. **순수 함수(Pure Functions):** Update는 부작용 없이 동일 입력에 동일 출력
4. **타입 안전성:** 모든 메시지와 상태가 타입으로 정의됨

### 계산기에 적용한 MVU

**Model (상태):**
```fsharp
type Model = {
    Display: string              // 화면에 표시되는 값: "123", "45.67", "Error"
    PendingOp: (MathOp * float) option  // 대기 중인 연산: Some (Add, 2.0)
    StartNew: bool               // 다음 입력이 디스플레이 교체 여부
}
```

**Msg (사용자 행동):**
```fsharp
type Msg =
    | DigitPressed of int       // 숫자 버튼 클릭 (0-9)
    | DecimalPressed            // 소수점 버튼 클릭
    | OperatorPressed of MathOp // 연산자 버튼 클릭 (+, -, ×, ÷)
    | EqualsPressed             // = 버튼 클릭
    | ClearPressed              // C 버튼 클릭
```

**Update (상태 전이):**
```fsharp
let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | DigitPressed 3 -> { model with Display = model.Display + "3" }, Cmd.none
    // ... (각 메시지에 대한 순수한 로직)
```

---

## 2. 계산기 타입 설계 (Calculator.fs)

### 2.1 연산자 타입

F#의 판별 구분 조합(Discriminated Union)으로 4가지 연산을 표현합니다:

```fsharp
type MathOp =
    | Add
    | Subtract
    | Multiply
    | Divide
```

**왜 문자열이나 enum이 아닌가?**
- 타입 안전: `MathOp`가 아닌 값은 컴파일 에러
- 패턴 매칭: 모든 케이스를 처리했는지 컴파일러가 검증
- 표현력: `Add`는 `"+"`보다 명확함

### 2.2 계산 결과 타입

0으로 나누기 에러를 타입으로 표현합니다:

```fsharp
type MathResult =
    | Success of float
    | DivideByZeroError
```

**이점:**
- 예외(Exception) 대신 타입으로 에러 처리
- 모든 에러 케이스가 명시적
- 런타임 에러 방지

**사용 예시:**
```fsharp
match doMathOp Divide 5.0 0.0 with
| Success result -> printfn "결과: %f" result
| DivideByZeroError -> printfn "0으로 나눌 수 없습니다"
```

### 2.3 완전한 Calculator.fs

```fsharp
module Calculator

/// 계산기가 지원하는 4가지 연산
type MathOp =
    | Add
    | Subtract
    | Multiply
    | Divide

/// 계산 결과 (성공 또는 0으로 나누기 에러)
type MathResult =
    | Success of float
    | DivideByZeroError

/// 사용자가 계산기에 보낼 수 있는 모든 메시지
type Msg =
    | DigitPressed of int       // 0-9
    | DecimalPressed            // .
    | OperatorPressed of MathOp // +, -, ×, ÷
    | EqualsPressed             // =
    | ClearPressed              // C/AC

/// 계산기의 현재 상태
type Model = {
    Display: string                     // 화면 표시 값: "0", "123", "45.67", "Error"
    PendingOp: (MathOp * float) option  // 대기 중인 연산: None 또는 Some (Add, 2.0)
    StartNew: bool                      // true = 다음 숫자 입력 시 디스플레이 교체
}

// ============================================================================
// 순수 함수들 (Pure Functions)
// ============================================================================

/// 두 숫자에 연산 적용 (0으로 나누기 처리 포함)
let doMathOp (op: MathOp) (a: float) (b: float) : MathResult =
    match op with
    | Add -> Success (a + b)
    | Subtract -> Success (a - b)
    | Multiply -> Success (a * b)
    | Divide ->
        if b = 0.0 then DivideByZeroError
        else Success (a / b)

/// 디스플레이 문자열을 숫자로 파싱
let parseDisplay (display: string) : float =
    match System.Double.TryParse(display) with
    | true, v -> v
    | false, _ -> 0.0

/// 계산 결과를 디스플레이 문자열로 포맷
let formatResult (value: float) : string =
    string value
```

**핵심 설계 결정:**
- `DigitPressed of int`: 0-9를 하나의 케이스로 표현 (간결함)
- `PendingOp`: Option 타입으로 "연산 없음" 상태 표현
- `StartNew`: 연산자 연쇄 입력을 위한 플래그

---

## 3. 순수 함수로 계산 로직 구현

### 3.1 doMathOp - 연산 수행 함수

```fsharp
let doMathOp (op: MathOp) (a: float) (b: float) : MathResult =
    match op with
    | Add -> Success (a + b)
    | Subtract -> Success (a - b)
    | Multiply -> Success (a * b)
    | Divide ->
        if b = 0.0 then DivideByZeroError
        else Success (a / b)
```

**왜 순수 함수인가?**
- 입력이 같으면 출력도 항상 같음
- 부작용(side effect) 없음 (파일 I/O, 네트워크, 콘솔 출력 등)
- 테스트하기 쉬움

**테스트 예시:**
```fsharp
testCase "2 + 3 = 5" <| fun () ->
    let result = doMathOp Add 2.0 3.0
    Expect.equal result (Success 5.0) "Should add correctly"

testCase "5 ÷ 0 = Error" <| fun () ->
    let result = doMathOp Divide 5.0 0.0
    Expect.equal result DivideByZeroError "Should handle divide by zero"
```

### 3.2 왼쪽에서 오른쪽 계산 (Left-to-Right Evaluation)

일반 계산기는 연산자 우선순위를 무시하고 입력 순서대로 계산합니다.

**예시: 2 + 3 × 4**

수학적으로는: 2 + (3 × 4) = 2 + 12 = 14
**계산기는: (2 + 3) × 4 = 5 × 4 = 20**

**구현 방법:**

1. 사용자가 `2` 입력 → Display: "2"
2. 사용자가 `+` 입력 → PendingOp: Some (Add, 2.0), StartNew: true
3. 사용자가 `3` 입력 → Display: "3"
4. **사용자가 `×` 입력 → 여기서 먼저 2 + 3 = 5 계산!**
   - 결과: Display: "5", PendingOp: Some (Multiply, 5.0), StartNew: true
5. 사용자가 `4` 입력 → Display: "4"
6. 사용자가 `=` 입력 → 5 × 4 = 20 계산
   - 결과: Display: "20", PendingOp: None

**핵심 코드 (OperatorPressed 처리):**

```fsharp
| OperatorPressed op ->
    let currentValue = parseDisplay model.Display
    match model.PendingOp with
    | None ->
        // 첫 번째 연산자 - 그냥 저장
        { model with PendingOp = Some (op, currentValue); StartNew = true }, Cmd.none
    | Some (pendingOp, firstOperand) ->
        if model.StartNew then
            // 연산자 연속 입력 (예: + 누른 직후 × 누름) - 연산자만 교체
            { model with PendingOp = Some (op, firstOperand) }, Cmd.none
        else
            // 두 번째 피연산자 입력됨 - 먼저 계산 후 새 연산자 저장
            match doMathOp pendingOp firstOperand currentValue with
            | Success result ->
                let resultStr = formatResult result
                { Display = resultStr; PendingOp = Some (op, result); StartNew = true }, Cmd.none
            | DivideByZeroError ->
                { Display = "Error"; PendingOp = None; StartNew = true }, Cmd.none
```

### 3.3 parseDisplay와 formatResult

**parseDisplay - 문자열 → 숫자 변환:**

```fsharp
let parseDisplay (display: string) : float =
    match System.Double.TryParse(display) with
    | true, v -> v
    | false, _ -> 0.0  // "Error" 같은 비숫자는 0으로 처리
```

**formatResult - 숫자 → 문자열 변환:**

```fsharp
let formatResult (value: float) : string =
    string value  // F#이 자동으로 .0 처리 (5.0 → "5")
```

---

## 4. Update 함수 구현 (App.fs)

### 4.1 초기 상태 (init)

```fsharp
let init () : Model * Cmd<Msg> =
    { Display = "0"; PendingOp = None; StartNew = false }, Cmd.none
```

### 4.2 각 메시지 처리

**DigitPressed - 숫자 버튼 클릭:**

```fsharp
| DigitPressed digit ->
    // 에러 상태면 리셋
    if model.Display = "Error" then
        { Display = string digit; PendingOp = None; StartNew = false }, Cmd.none
    // StartNew 플래그가 true면 교체
    elif model.StartNew then
        { model with Display = string digit; StartNew = false }, Cmd.none
    // 초기 "0"에 0 입력 - 무시
    elif model.Display = "0" && digit = 0 then
        model, Cmd.none
    // 초기 "0" - 숫자로 교체
    elif model.Display = "0" then
        { model with Display = string digit }, Cmd.none
    // 일반 추가
    else
        { model with Display = model.Display + string digit }, Cmd.none
```

**DecimalPressed - 소수점 버튼 클릭:**

```fsharp
| DecimalPressed ->
    // StartNew면 "0."으로 시작
    if model.StartNew then
        { model with Display = "0."; StartNew = false }, Cmd.none
    // 이미 소수점 있으면 무시
    elif not (model.Display.Contains(".")) then
        { model with Display = model.Display + "." }, Cmd.none
    else
        model, Cmd.none
```

**ClearPressed - 초기화:**

```fsharp
| ClearPressed ->
    init () |> fst, Cmd.none
```

### 4.3 StartNew 플래그의 역할

**StartNew = true일 때:**
- 다음 숫자 입력이 디스플레이를 **교체**함 (추가가 아님)
- 연산자를 누르거나 `=`를 누른 직후 상태

**예시:**

```
1. 사용자가 "5" 입력 → Display: "5", StartNew: false
2. 사용자가 "+" 입력 → Display: "5", StartNew: true
3. 사용자가 "3" 입력 → Display: "3" (교체됨!), StartNew: false
```

**StartNew가 없다면?**
- "5 + 3"을 입력하면 Display가 "53"이 되어버림 (잘못됨!)

---

## 5. 테스트 주도 개발 (TDD)

### 5.1 TDD란?

TDD(Test-Driven Development)는 **테스트를 먼저 작성하고, 이를 통과하는 코드를 나중에 작성하는** 개발 방법론입니다.

**TDD 사이클: RED → GREEN → REFACTOR**

```
┌──────────────┐
│     RED      │ ──── 실패하는 테스트 작성
│ (테스트 실패) │
└──────┬───────┘
       │
       ▼
┌──────────────┐
│    GREEN     │ ──── 최소한의 코드로 테스트 통과
│ (테스트 통과) │
└──────┬───────┘
       │
       ▼
┌──────────────┐
│   REFACTOR   │ ──── 코드 개선 (테스트는 계속 통과)
└──────────────┘
       │
       └───────────── (다음 기능으로)
```

### 5.2 테스트 예시: 2 + 3 = 5

**RED 단계 - 테스트 먼저 작성:**

```fsharp
testCase "2 + 3 = shows '5'" <| fun () ->
    let m0 = init ()
    // 2 입력
    let m1 = update (DigitPressed 2) m0
    // + 입력
    let m2 = update (OperatorPressed Add) m1
    // 3 입력
    let m3 = update (DigitPressed 3) m2
    // = 입력
    let m4 = update EqualsPressed m3

    Expect.equal m4.Display "5" "Should calculate 2 + 3 = 5"
```

**GREEN 단계 - 코드 구현:**

update 함수에 OperatorPressed와 EqualsPressed 로직 추가.

**REFACTOR 단계 - 개선:**

중복 코드 제거, 변수명 개선 등.

### 5.3 실제 프로젝트에 적용한 TDD

이 프로젝트는 **27개의 테스트를 먼저 작성**하고, 이를 통과하는 코드를 구현했습니다.

**테스트 카테고리:**

1. **Digit Entry** - 숫자 입력 (5개 테스트)
2. **Decimal Point** - 소수점 처리 (5개 테스트)
3. **Basic Operations** - 기본 연산 (4개 테스트)
4. **Left-to-Right Evaluation** - 순차 계산 (3개 테스트)
5. **Division by Zero** - 0 나누기 (2개 테스트)
6. **Clear** - 초기화 (2개 테스트)
7. **Edge Cases** - 엣지 케이스 (6개 테스트)

---

## 6. 테스트 인프라 설정

### 6.1 Fable.Mocha란?

Fable.Mocha는 F#에서 Mocha 테스트 프레임워크를 사용하기 위한 라이브러리입니다.

**특징:**
- F#으로 테스트 작성
- Fable이 JavaScript로 변환
- Node.js의 Mocha로 실행
- Expect 스타일 검증

### 6.2 Tests.fsproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <Compile Include="Tests.fs" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="../src/App.fsproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Fable.Core" Version="4.3.0" />
    <PackageReference Include="Fable.Mocha" Version="3.0.0" />
  </ItemGroup>
</Project>
```

**핵심:**
- `ProjectReference`로 App.fsproj 참조
- Fable.Mocha 패키지 포함

### 6.3 Tests.fs 구조

```fsharp
module Tests

open Fable.Mocha
open Calculator
open App

// 헬퍼 함수: Msg 체인을 모델에 적용
let applyMsgs (msgs: Msg list) (initialModel: Model) : Model =
    msgs |> List.fold (fun model msg -> update msg model |> fst) initialModel

let calculatorTests =
    testList "CalTwo" [
        testList "Digit Entry" [
            testCase "Pressing 1 shows '1'" <| fun () ->
                let model = init ()
                let result = update (DigitPressed 1) model |> fst
                Expect.equal result.Display "1" "Should show 1"
        ]
        // ... 더 많은 테스트
    ]

[<EntryPoint>]
let main args =
    Mocha.runTests calculatorTests
```

### 6.4 npm 테스트 스크립트

**package.json:**

```json
{
  "scripts": {
    "test": "npm run test:compile && npm run test:run",
    "test:compile": "dotnet fable tests/Tests.fsproj -o tests/out --noCache",
    "test:run": "npx mocha tests/out/Tests.js --timeout 10000"
  }
}
```

**실행 흐름:**

1. `npm test` 실행
2. `test:compile`: Fable이 Tests.fs → tests/out/Tests.js 변환
3. `test:run`: Mocha가 Tests.js 실행

**실행 예시:**

```bash
$ npm test

> caltwo@0.1.0 test
> npm run test:compile && npm run test:run

Fable compilation finished in 3884ms

  CalTwo
    Digit Entry
      ✔ Pressing 1 shows '1'
      ✔ Pressing 1 then 2 shows '12'
    ...

  27 passing (8ms)
```

---

## 7. 엣지 케이스 처리

### 7.1 여러 번 소수점 입력

**문제:** 사용자가 `3`, `.`, `.`, `1`을 입력하면?

**올바른 동작:** `3.1` (두 번째 소수점 무시)

**구현:**

```fsharp
| DecimalPressed ->
    if not (model.Display.Contains(".")) then
        { model with Display = model.Display + "." }, Cmd.none
    else
        model, Cmd.none  // 이미 소수점 있으면 무시
```

**테스트:**

```fsharp
testCase "3, ., . → second decimal ignored, still '3.'" <| fun () ->
    let model = init ()
    let msgs = [DigitPressed 3; DecimalPressed; DecimalPressed]
    let result = applyMsgs msgs model
    Expect.equal result.Display "3." "Second decimal should be ignored"
```

### 7.2 선행 0 (Leading Zeros)

**문제:** 사용자가 `0`, `0`, `7`을 입력하면?

**올바른 동작:** `7` (선행 0 무시)

**구현:**

```fsharp
| DigitPressed digit ->
    // "0"에 0 입력 - 무시
    if model.Display = "0" && digit = 0 then
        model, Cmd.none
    // "0"에 다른 숫자 - 교체
    elif model.Display = "0" then
        { model with Display = string digit }, Cmd.none
```

### 7.3 0으로 나누기

**문제:** `5 ÷ 0 =` 입력 시?

**올바른 동작:** `"Error"` 표시

**구현:**

```fsharp
let doMathOp (op: MathOp) (a: float) (b: float) : MathResult =
    match op with
    | Divide ->
        if b = 0.0 then DivideByZeroError
        else Success (a / b)
```

**에러 후 복구:**

```fsharp
| DigitPressed digit ->
    // 에러 상태에서 숫자 입력 - 리셋
    if model.Display = "Error" then
        { Display = string digit; PendingOp = None; StartNew = false }, Cmd.none
```

### 7.4 음수 결과

**문제:** `3 - 8 =` 입력 시?

**올바른 동작:** `-5` (음수 지원)

F#의 float 타입이 자동으로 음수를 처리합니다.

### 7.5 연산자 연속 입력

**문제:** `5 + + =` 입력 시?

**올바른 동작:** 두 번째 `+`는 첫 번째 `+`를 교체

**구현:**

```fsharp
| OperatorPressed op ->
    match model.PendingOp with
    | Some (pendingOp, firstOperand) ->
        if model.StartNew then
            // 연산자 교체 (계산 안 함)
            { model with PendingOp = Some (op, firstOperand) }, Cmd.none
```

---

## 8. 실행해보기

### 8.1 테스트 실행

```bash
npm test
```

**기대 결과:**

```
  CalTwo
    Digit Entry
      ✔ Pressing 1 shows '1' (replaces initial '0')
      ✔ Pressing 1 then 2 shows '12'
      ✔ Pressing 1, 2, 3 shows '123'
      ✔ Pressing 0 when display is '0' keeps '0' (no leading zeros)
      ✔ Pressing 5 then 0 shows '50' (trailing zero OK)
    Decimal Point
      ✔ Pressing decimal shows '0.' (appends to zero)
      ✔ Pressing 3 then decimal shows '3.'
      ✔ Pressing 3, decimal, 1 shows '3.1'
      ✔ Pressing 3, decimal, decimal — second decimal ignored
      ✔ Pressing 3, decimal, 1, decimal — second decimal ignored
    Basic Operations
      ✔ 2 + 3 = shows '5'
      ✔ 9 - 4 = shows '5'
      ✔ 3 × 4 = shows '12'
      ✔ 8 ÷ 2 = shows '4'
    Left-to-Right Evaluation
      ✔ 2 + 3 + 4 = shows '9'
      ✔ 10 - 3 + 2 = shows '9'
      ✔ 2 × 3 + 1 = shows '7'
    Division by Zero
      ✔ 5 ÷ 0 = shows 'Error'
      ✔ After error, pressing digit resets
    Clear
      ✔ Clear resets display to '0'
      ✔ After operation, Clear resets everything
    Edge Cases
      ✔ Pressing equals with no pending operation
      ✔ Negative result: 3 - 8 = shows '-5'
      ✔ Decimal arithmetic: 1.5 + 2.5 = shows '4'
      ✔ Large number: 999999999 + 1 = shows '1000000000'
      ✔ Pressing operator right after init
      ✔ Pressing operator twice: second operator replaces first

  27 passing (8ms)
```

### 8.2 개발 서버 실행

```bash
npm run dev
```

브라우저에서 http://localhost:5173/ 접속 후:

1. **디스플레이 확인:** 초기값 "0" 표시
2. **숫자 입력:** 1, 2, 3 클릭 → "123" 표시
3. **연산 테스트:** C 클릭 → 2 클릭 → + 클릭 → 3 클릭 → = 클릭 → "5" 표시
4. **소수점:** C → 3 → . → 1 → "3.1" 표시
5. **에러:** C → 5 → ÷ → 0 → = → "Error" 표시

**현재 UI (Phase 2):**
- 터미널 스타일 디스플레이 (녹색 글자, 검정 배경)
- 임시 테스트 버튼들 (0-9, +, -, ×, ÷, =, ., C)
- Phase 3에서 완전한 그리드 레이아웃으로 대체 예정

---

## 9. 핵심 개념 복습

### 9.1 MVU 패턴의 이점

**타입 안전성:**
```fsharp
// 잘못된 메시지는 컴파일 에러
dispatch (DigitPressed 10)  // ❌ 컴파일 에러! (0-9만 허용)
dispatch (OperatorPressed "plus")  // ❌ 컴파일 에러! (MathOp 타입만 허용)
```

**예측 가능성:**
```fsharp
// update는 순수 함수 - 동일 입력에 동일 출력
let model1 = update (DigitPressed 5) initialModel
let model2 = update (DigitPressed 5) initialModel
// model1 == model2 (항상 같음)
```

**테스트 용이성:**
```fsharp
// 별도 설정 없이 함수만 호출하면 테스트 가능
let result = update (DigitPressed 3) model
Expect.equal result.Display "3"
```

### 9.2 판별 구분 조합 vs. 다른 방법

**DU 사용 (권장):**
```fsharp
type Msg =
    | DigitPressed of int
    | OperatorPressed of MathOp

match msg with
| DigitPressed d -> ...
| OperatorPressed op -> ...
// 컴파일러가 모든 케이스 처리 검증
```

**문자열 사용 (비권장):**
```fsharp
type Msg = {
    Type: string
    Payload: obj
}

if msg.Type = "DIGIT_PRESSED" then
    let digit = msg.Payload :?> int  // 런타임 캐스팅!
    ...
// 오타 가능, 런타임 에러 가능
```

### 9.3 순수 함수의 위력

**순수 함수:**
```fsharp
let add x y = x + y  // 부작용 없음, 예측 가능
```

**비순수 함수:**
```fsharp
let mutable total = 0
let add x =
    total <- total + x  // 부작용! (전역 상태 변경)
    total
```

**계산기에서:**
- `doMathOp`, `parseDisplay`, `formatResult` → 순수 함수
- `update` → 순수 함수 (새 Model 반환, 기존 Model 변경 안 함)
- `view` → 순수 함수 (동일 Model에 동일 UI 반환)

---

## 10. 다음 단계

Phase 2를 완료했습니다! 계산기의 핵심 로직이 완성되고 테스트되었습니다.

**Phase 3: UI 구현 (UI Implementation)**

- 4×5 그리드 레이아웃 (버튼 배치)
- 버튼 스타일링 (색상, 크기, 호버 효과)
- 반응형 디자인 (모바일 대응)
- 키보드 입력 지원
- 계산 히스토리 표시

**Phase 4: 배포 및 최적화**

- GitHub Pages 배포
- 프로덕션 빌드 최적화
- PWA (Progressive Web App) 기능
- 성능 측정 및 개선

**Phase 5: 고급 기능**

- 다크 모드 / 라이트 모드
- 계산 기록 저장 (localStorage)
- 과학용 계산기 모드
- 접근성(Accessibility) 개선

---

## 11. 참고 자료

### F# 관련

- **F# 판별 구분 조합:** https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/discriminated-unions
- **F# 패턴 매칭:** https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/pattern-matching
- **F# 함수형 프로그래밍:** https://fsharpforfunandprofit.com/

### Elmish 관련

- **Elmish Book:** https://zaid-ajaj.github.io/the-elmish-book/
- **MVU 패턴:** https://guide.elm-lang.org/architecture/
- **Elmish 공식 문서:** https://elmish.github.io/elmish/

### 테스트 관련

- **Fable.Mocha:** https://github.com/Zaid-Ajaj/Fable.Mocha
- **TDD 가이드:** https://www.testdriven.io/
- **단위 테스트 모범 사례:** https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices

---

**축하합니다!** Phase 2를 완료했습니다. 이제 계산기의 핵심 로직이 27개의 테스트로 검증되었고, 브라우저에서 실시간으로 동작하는 것을 확인했습니다. TDD 방법론과 MVU 패턴으로 타입 안전하고 예측 가능한 코드를 작성하는 방법을 배웠습니다!
