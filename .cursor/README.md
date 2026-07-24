# AutoRpg `.cursor` 인덱스

에이전트용 **Rules**(항상/조건부 적용)와 **Skills**(작업별 절차) 안내.

## Rules (`.cursor/rules/`)

| 파일 | 적용 | 역할 |
|------|------|------|
| `continuous-autonomous-agent` | always | 구현→검증→git 자동 루프 |
| `harness-workflow` | always | Explore→Plan→Build→Verify→Cleanup |
| `harness-boundaries` | always | 수정 금지 코어 / 허용 확장 |
| `karpathy-guidelines` | always | 가정 명시·단순성·수술적 변경 |
| `unity-project-overview` | always | 폴더·패키지·매니저 개요 + 스킬 맵 |
| `unity-assets` | always | `.meta` 생성/수정 금지 |
| `unity-csharp-conventions` | `**/*.cs` | 네이밍·UniTask·로그 |
| `unity-ui-system` | `**/*.cs` | UIManager / MVP / Prefab-first |
| `unity-scene-system` | `**/*.cs` | SceneContext·매니저·풀 |
| `gssl-agent-cache` | GSSL Cache/** | 캐시·Localize 손편집 금지 |

## Skills (`.cursor/skills/`)

### 자율 개발 / Git

| 스킬 | 언제 |
|------|------|
| `develop-from-plan` | 기획 `.md` 기반 연속 구현 |
| `git-auto-commit` | 작업 단위 완료 후 커밋·PR·머지 |

### 기능 추가

| 스킬 | 언제 |
|------|------|
| `add-gameplay-feature` | System / Controller / RuntimePanel |
| `add-ui-screen` | Panel/Popup/View + Presenter |
| `add-table-data` | `TableManager` partial Get 추가 |
| `setup-input-actions` | `.inputactions` + InputSystem 래퍼 |

### Unity MCP 계층

| 스킬 | 역할 |
|------|------|
| `unity-mcp-tools` | 도구 **카탈로그** (목록·용도) |
| `verify-in-unity` | 컴파일·콘솔·테스트·플레이 **검증 루프** |
| `unity-mcp-prefab-ui` | UI Prefab **레이아웃** (코드 우회 금지) |

### 테이블 / GSSL

| 스킬 | 역할 |
|------|------|
| `table-data-workflow` | 파이프라인 개요 + 수동/폴백 |
| `gssl-agent-workflow` | `gssl-pending.json` 부분 동기 브리지 |

## 빠른 선택

```
기획 구현          → develop-from-plan
새 탐험 로직       → add-gameplay-feature
새 UI 화면 코드    → add-ui-screen
UI 크기/버튼/글씨  → unity-mcp-prefab-ui  (코드 sizeDelta 금지)
시트 데이터 변경   → table-data-workflow → gssl-agent-workflow
조회 API만 추가    → add-table-data
변경 검증          → verify-in-unity
작업 완료 git      → git-auto-commit
```

## 정리 원칙

1. **Rules** = 항상/조건부 제약. **Skills** = 구체 절차.
2. MCP 스킬은 역할이 겹치지 않게: 카탈로그 / 검증 / Prefab UI.
3. GSSL: 개요(`table-data-workflow`) vs 브리지(`gssl-agent-workflow`) vs 캐시 규칙(`gssl-agent-cache`).
4. 구 프로젝트명(MobileChainRe 등) 금지 — 예시는 AutoRpg(`Exploration*`) 기준.
