# CalTwo

## What This Is

F#과 Elmish를 사용한 웹 기반 사칙연산 계산기. 브라우저에서 동작하며 깔끔한 UI를 갖추고, 자동화된 테스트와 GitHub Pages 배포를 포함한다.

## Core Value

사칙연산이 정확하게 동작하는 계산기를 브라우저에서 바로 사용할 수 있어야 한다.

## Requirements

### Validated

(None yet — ship to validate)

### Active

- [ ] 숫자 입력 (0-9, 소수점)
- [ ] 사칙연산 (+, -, ×, ÷)
- [ ] 결과 계산 (=)
- [ ] 전체 초기화 (C/AC)
- [ ] 깔끔한 계산기 UI (버튼 그리드, 디스플레이)
- [ ] Backspace (마지막 입력 삭제)
- [ ] 자동화된 단위 테스트 (계산 로직)
- [ ] E2E 테스트 (Playwright — 브라우저에서 자동 검증)
- [ ] GitHub Pages 배포
- [ ] 매 phase 종료 시 한글 튜토리얼 (tutorial/ 디렉토리)

### Out of Scope

- 공학용 함수 (sin, cos, log 등) — v1은 사칙연산만
- 수식 텍스트 입력 — 버튼 기반 입력만
- 계산 히스토리 — 단순함 유지
- 키보드 입력 — 마우스/터치 클릭만
- 모바일 앱 — 웹 전용

## Context

- Elmish는 F#에서 Elm Architecture(MVU 패턴)를 구현한 프레임워크
- Fable을 통해 F# → JavaScript 컴파일
- 사람 간섭 최소화: 테스트 자동화가 핵심 요구사항
- 배포 대상은 GitHub Pages (정적 호스팅)

## Constraints

- **Tech stack**: F# + Fable + Elmish — 사용자 지정
- **Complexity**: 최대한 간단하게 — 사용자 요청
- **Testing**: 자동화 우선, 사람 간섭 최소화 — 사용자 요청
- **Deploy**: GitHub Pages — 정적 파일 배포

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| F# + Elmish 선택 | 사용자 지정 기술 스택 | — Pending |
| 사칙연산만 v1 | 최대한 간단한 앱 요구 | — Pending |
| GitHub Pages 배포 | 무료 정적 호스팅, 설정 간단 | — Pending |

---
*Last updated: 2026-02-14 after initialization*
