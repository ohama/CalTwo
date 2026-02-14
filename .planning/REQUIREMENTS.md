# Requirements: CalTwo

**Defined:** 2026-02-14
**Core Value:** 사칙연산이 정확하게 동작하는 계산기를 브라우저에서 바로 사용할 수 있어야 한다.

## v1 Requirements

### Input (INP)

- [ ] **INP-01**: User can input digits 0-9 by clicking buttons
- [ ] **INP-02**: User can input decimal point (.)
- [ ] **INP-03**: User can delete last input character (Backspace)
- [ ] **INP-04**: User can clear all input (C/AC)

### Operations (OPS)

- [ ] **OPS-01**: User can perform addition (+)
- [ ] **OPS-02**: User can perform subtraction (-)
- [ ] **OPS-03**: User can perform multiplication (×)
- [ ] **OPS-04**: User can perform division (÷)
- [ ] **OPS-05**: User can execute calculation by pressing equals (=)
- [ ] **OPS-06**: User sees "Error" when dividing by zero

### Display (DSP)

- [ ] **DSP-01**: User sees current input on display
- [ ] **DSP-02**: User sees calculation result after pressing equals
- [ ] **DSP-03**: Display updates immediately on every input

### UI (UI)

- [ ] **UI-01**: Calculator has clean button grid layout (numbers, operators, clear, equals)
- [ ] **UI-02**: Buttons show visual feedback on click (hover/active states)
- [ ] **UI-03**: Layout is responsive (works on desktop and mobile)

### Testing (TST)

- [ ] **TST-01**: Unit tests cover all arithmetic operations (pure logic)
- [ ] **TST-02**: Unit tests cover edge cases (divide by zero, decimal handling)
- [ ] **TST-03**: E2E tests verify button click → display result in real browser (Playwright)
- [ ] **TST-04**: All tests run automatically in CI without human intervention (GitHub Actions)

### Deployment (DEP)

- [ ] **DEP-01**: App builds to static files (HTML + JS + CSS)
- [ ] **DEP-02**: App deploys to GitHub Pages via GitHub Actions on push to main
- [ ] **DEP-03**: Deployed app loads and functions correctly at GitHub Pages URL

### Tutorial (TUT)

- [ ] **TUT-01**: Each phase produces a Korean tutorial markdown file in `tutorial/` directory
- [ ] **TUT-02**: Each tutorial explains the phase's technical content step-by-step for beginner developers
- [ ] **TUT-03**: Tutorials include code examples, commands, and explanations a beginner can follow

## v2 Requirements

### Enhanced UX

- **UX-01**: User can use keyboard for input (number keys, operators, Enter, Esc)
- **UX-02**: User can see calculation history
- **UX-03**: User can copy result to clipboard

### Accessibility

- **A11Y-01**: Calculator has proper ARIA labels
- **A11Y-02**: Focus states visible for keyboard navigation

## Out of Scope

| Feature | Reason |
|---------|--------|
| Scientific functions (sin, cos, log) | v1은 사칙연산만 — 단순함 유지 |
| Formula text input | 버튼 기반 입력만 — 복잡도 제한 |
| Calculation history | v2로 이연 — 핵심 기능 아님 |
| Memory functions (M+, M-, MR) | v2로 이연 — 기본 계산기에 불필요 |
| Dark mode | v2로 이연 — polish feature |
| Mobile native app | 웹 전용 — 정적 배포 |
| Backend/API | 클라이언트 전용 계산기 |
| OAuth/Login | 인증 불필요 |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| INP-01 | TBD | Pending |
| INP-02 | TBD | Pending |
| INP-03 | TBD | Pending |
| INP-04 | TBD | Pending |
| OPS-01 | TBD | Pending |
| OPS-02 | TBD | Pending |
| OPS-03 | TBD | Pending |
| OPS-04 | TBD | Pending |
| OPS-05 | TBD | Pending |
| OPS-06 | TBD | Pending |
| DSP-01 | TBD | Pending |
| DSP-02 | TBD | Pending |
| DSP-03 | TBD | Pending |
| UI-01 | TBD | Pending |
| UI-02 | TBD | Pending |
| UI-03 | TBD | Pending |
| TST-01 | TBD | Pending |
| TST-02 | TBD | Pending |
| TST-03 | TBD | Pending |
| TST-04 | TBD | Pending |
| DEP-01 | TBD | Pending |
| DEP-02 | TBD | Pending |
| DEP-03 | TBD | Pending |
| TUT-01 | Every phase | Pending |
| TUT-02 | Every phase | Pending |
| TUT-03 | Every phase | Pending |

**Coverage:**
- v1 requirements: 26 total
- Mapped to phases: 0
- Unmapped: 26 (pending roadmap)

---
*Requirements defined: 2026-02-14*
*Last updated: 2026-02-14 after initial definition*
