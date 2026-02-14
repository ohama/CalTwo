# Requirements: CalTwo

**Defined:** 2026-02-14
**Core Value:** 사칙연산이 정확하게 동작하는 계산기를 브라우저에서 바로 사용할 수 있어야 한다.

## v1 Requirements

### Input (INP)

- [x] **INP-01**: User can input digits 0-9 by clicking buttons
- [x] **INP-02**: User can input decimal point (.)
- [x] **INP-03**: User can delete last input character (Backspace)
- [x] **INP-04**: User can clear all input (C/AC)

### Operations (OPS)

- [x] **OPS-01**: User can perform addition (+)
- [x] **OPS-02**: User can perform subtraction (-)
- [x] **OPS-03**: User can perform multiplication (×)
- [x] **OPS-04**: User can perform division (÷)
- [x] **OPS-05**: User can execute calculation by pressing equals (=)
- [x] **OPS-06**: User sees "Error" when dividing by zero

### Display (DSP)

- [x] **DSP-01**: User sees current input on display
- [x] **DSP-02**: User sees calculation result after pressing equals
- [x] **DSP-03**: Display updates immediately on every input

### UI (UI)

- [x] **UI-01**: Calculator has clean button grid layout (numbers, operators, clear, equals)
- [x] **UI-02**: Buttons show visual feedback on click (hover/active states)
- [x] **UI-03**: Layout is responsive (works on desktop and mobile)

### Testing (TST)

- [x] **TST-01**: Unit tests cover all arithmetic operations (pure logic)
- [x] **TST-02**: Unit tests cover edge cases (divide by zero, decimal handling)
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
| INP-01 | Phase 2 | Complete |
| INP-02 | Phase 2 | Complete |
| INP-03 | Phase 3 | Complete |
| INP-04 | Phase 2 | Complete |
| OPS-01 | Phase 2 | Complete |
| OPS-02 | Phase 2 | Complete |
| OPS-03 | Phase 2 | Complete |
| OPS-04 | Phase 2 | Complete |
| OPS-05 | Phase 2 | Complete |
| OPS-06 | Phase 2 | Complete |
| DSP-01 | Phase 2 | Complete |
| DSP-02 | Phase 2 | Complete |
| DSP-03 | Phase 2 | Complete |
| UI-01 | Phase 3 | Complete |
| UI-02 | Phase 3 | Complete |
| UI-03 | Phase 3 | Complete |
| TST-01 | Phase 2 | Complete |
| TST-02 | Phase 2 | Complete |
| TST-03 | Phase 4 | Pending |
| TST-04 | Phase 4 | Pending |
| DEP-01 | Phase 1 | Complete |
| DEP-02 | Phase 5 | Pending |
| DEP-03 | Phase 5 | Pending |
| TUT-01 | Every phase | Pending |
| TUT-02 | Every phase | Pending |
| TUT-03 | Every phase | Pending |

**Coverage:**
- v1 requirements: 26 total
- Mapped to phases: 26
- Unmapped: 0

**Phase breakdown:**
- Phase 1: 4 requirements (DEP-01, TUT-01, TUT-02, TUT-03)
- Phase 2: 16 requirements (INP-01, INP-02, INP-04, OPS-01-06, DSP-01-03, TST-01-02, TUT-01-03)
- Phase 3: 7 requirements (UI-01-03, INP-03, TUT-01-03)
- Phase 4: 5 requirements (TST-03-04, TUT-01-03)
- Phase 5: 5 requirements (DEP-02-03, TUT-01-03)

---
*Requirements defined: 2026-02-14*
*Last updated: 2026-02-14 after Phase 3 completion*
