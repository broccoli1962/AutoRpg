---
name: gssl-agent-workflow
description: GSSL Agent Bridge로 시트 부분 동기화(.cursor/gssl-pending.json → Tools/GSSL/Sync Pending Sheets). 시트 수정 후 선택 시트만 내려받아 재생성할 때 사용. 개요·수동 경로는 table-data-workflow.
disable-model-invocation: true
---

# GSSL Agent Workflow (부분 동기 브리지)

채팅으로 표 데이터를 바꾸고 **변경된 시트만** GSSL 로 동기화할 때 사용한다.
파이프라인 개요·손편집 금지는 `table-data-workflow` / `gssl-agent-cache` 규칙을 따른다.

## 전제

- Unity 에디터가 이 프로젝트로 열려 있음
- `user-unityMCP` 사용 가능 (`execute_menu_item`)
- Sheets MCP(`user-mcp-gsheets` 등)로 Google Sheets 읽기/쓰기 가능하면 우선 사용
- GSSL `SettingData` 에 서비스 계정 JSON **프로젝트 밖** 절대 경로 설정
- 대상 스프레드시트가 서비스 계정에 공유됨
- 대상 시트가 `Generated/Cache/cache_index.json` 에 이미 등록됨

## 데이터 경로

```
Google Sheets
  → .cursor/gssl-pending.json (mode: update)
  → Tools/GSSL/Sync Pending Sheets
  → Generated/Cache → scripts/SO/TableLinker/Localize_*.json
```

## 절차

1. 스키마: `Generated/Cache/cache_index.json`, `Generated/Cache/<Sheet>.txt` (**읽기 전용**)
2. Google Sheets 수정 (MCP gsheets 또는 사용자 요청)
3. pending 작성:

```json
{
  "mode": "update",
  "sheets": ["SheetName"]
}
```

경로: `.cursor/gssl-pending.json`

4. 메뉴 실행 (`unityMCP`):

```
execute_menu_item { "menu_path": "Tools/GSSL/Sync Pending Sheets" }
```

5. `.cursor/gssl-result.json` 폴링 — `status` 가 `success` / `error` (not `running`)
6. `verify-in-unity`: `refresh_unity` → `read_console` Error 0
7. 생성물 diff 확인 후 필요 시 `git-auto-commit` (scope: `table`)

### regenerate only

시트 미변경·로컬 캐시만 재생성할 때:

```json
{ "mode": "regenerate", "sheets": ["SheetName"] }
```

→ `Tools/GSSL/Regenerate Pending Sheets`  
**시트 MCP 수정 직후 regenerate 금지** (반드시 `update`).

## mode / status

| mode | 의미 |
|------|------|
| `update` | 시트 다운로드 후 생성 |
| `regenerate` | 로컬 캐시만 재생성 |

| status | 의미 |
|--------|------|
| `running` | 진행 중 |
| `success` | 완료, pending 삭제됨 |
| `error` | 실패 — `message` 확인 |
| `busy` | 다른 GSSL 프로세스 실행 중 |

## 절대 금지

- `Generated/Cache/*.txt`, `Localize_*.json`, GSSL 스크립트/SO 손편집
- sync 실패 시 캐시 패치
- 시트 수정 직후 `regenerate`
- 채팅 부분 갱신에 OneButton 전체 동기 남용

## 폴백

브리지/메뉴가 불가하면 `table-data-workflow` 의 GSSL 윈도우 수동 다운로드→생성 경로를 쓴다.
