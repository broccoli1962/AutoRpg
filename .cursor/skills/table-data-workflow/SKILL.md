---
name: table-data-workflow
description: GSSL(Google SpreadSheet Loader) 기반 테이블 데이터 파이프라인. 유닛/몬스터/퀘스트/스킬 등 표 데이터를 바꾸거나 새 시트/컬럼을 추가하고, 생성 스크립트·ScriptableObject·TableLinker·로컬라이즈를 재생성해야 할 때 사용.
---

# Skill: 테이블 데이터 워크플로우 (GSSL)

이 프로젝트의 표 데이터는 **Google Sheets 가 원본(source of truth)** 이고, GSSL 이 이를 내려받아
캐시 → 생성 스크립트 → ScriptableObject → `TableLinker` 로 변환한다. 런타임에서는 `TableManager` 가
`Resources/TableLinker` 를 로드해 사용한다.

`harness-boundaries` 준수: **GSSL 생성 산출물은 절대 손편집하지 않는다.** 데이터 변경은 시트에서 하고 GSSL 로 재생성한다.

## 데이터 경로 (단방향)

```
Google Sheets  (원본, 유일한 편집 지점)
   │  GSSL 다운로드
   ▼
Assets/GoogleSpreadSheetLoader/Generated/Cache/*.txt (+ cache_index.json)
   │  GSSL 생성
   ▼
Generated/Script/DataScript/*Data.cs
Generated/Script/TableScript/*Table.cs
Generated/Script/Enum/*.cs
Generated/Script/TableLinker.cs
Generated/SerializeObject/TableData/*Table.asset
Assets/Resources/TableLinker.asset      ← TableManager 가 로드
Assets/Resources/Localize_*.json        ← 로컬라이즈 (LocalizeTable 이 로드)
   │  런타임
   ▼
TableManager (수기 작성 glue) → GetUnitData / GetMonsterSpawns ...
```

## 절대 손편집 금지 (GSSL 소유 산출물)

- `Generated/Cache/*.txt`, `cache_index.json`
- `Generated/Script/**`(DataScript/TableScript/Enum/**), `Generated/Script/TableLinker.cs`
- `Generated/SerializeObject/TableData/*.asset`
- `Assets/Resources/TableLinker.asset`
- `Assets/Resources/Localize_*.json`

동기화 실패 시 캐시/생성물을 손으로 때우지 말 것. 원인을 진단하고, 필요하면 사용자에게 보고한다.
(단, 사용자가 "GSSL/생성 로직 자체를 디버깅해달라"고 명시하면 읽기·분석은 가능.)

## 편집 가능한 것
- **Google Sheets 셀/행/컬럼/시트** — 실제 데이터 변경 지점.
- **`SettingData.asset`** 의 설정(스프레드시트/시트 등록). 단 서비스 계정 JSON 경로는 머신·비밀정보이므로 커밋 금지.
- **`TableManager` partial (수기 glue)** — 조회 메서드 추가는 `add-table-data` 스킬 참고.

## 시트 규칙
- 헤더 형식은 `컬럼명-타입` (예: `id-string`, `idx-int`, `cost-string`).
- 시트명이 생성 타입으로 매핑됨: 예 `Unit` → `UnitData` / `UnitTable`, `MonsterSpawn` → `MonsterSpawnData` / `MonsterSpawnTable`.
- 등록된 시트/스프레드시트 정보는 `Generated/Cache/cache_index.json` 에서 확인(**읽기 전용**).
- 캐시에 없는 시트는 GSSL 윈도우에서 스프레드시트 단위 동기화를 먼저 해야 한다.

## 절차

### 1) 스키마 파악 (읽기 전용)
- `Generated/Cache/cache_index.json` 과 `Generated/Cache/<Sheet>.txt` 로 컬럼/타입/기존 값을 확인한다.

### 2) Google Sheets 편집
- **Sheets MCP 서버가 있으면** 그것으로 셀을 읽고 수정한다(먼저 `GetMcpTools` 로 확인).
- 없으면 변경할 시트·행·컬럼·값을 구체적으로 정리해 **사용자에게 시트 수정을 요청**한다(임의로 생성물을 고치지 않는다).

### 3) GSSL 재생성 (다운로드 → 생성)

**우선 (부분 동기):** `.cursor/gssl-pending.json` + `Tools/GSSL/Sync Pending Sheets`
→ 상세는 `gssl-agent-workflow` 스킬.

**폴백 (수동):** `execute_menu_item {"menu_path":"Tools/Google Spread Sheet Loader"}` 로
GSSL 윈도우를 열어 변경 시트 다운로드→생성(또는 OneButton).

시트를 바꾸지 않고 로컬 캐시만 다시 만들 때만 regenerate/regenerate. **시트 수정 직후 regenerate 금지.**

### 4) 컴파일·검증 (`verify-in-unity`)
- `refresh_unity`(compile) → `read_console`(Error 0).
- 생성 결과 확인: `Generated/Script/**`, `Generated/SerializeObject/TableData/*.asset`, `Resources/TableLinker.asset`,
  (로컬라이즈 시트면) `Resources/Localize_*.json` 이 변경을 반영했는지.

### 5) 런타임 연동 (필요 시)
- 새 컬럼/시트를 코드에서 읽어야 하면 `TableManager` partial 에 조회 메서드를 추가한다 → `add-table-data` 스킬.

### 6) 커밋 (`git-auto-commit`)
- 재생성된 생성물(`Generated/**`, `Resources/TableLinker.asset`, `Resources/Localize_*.json`)은 **짝 `.meta` 와 함께 정상 커밋**한다. scope 는 `table`.

## 체크리스트
- [ ] 데이터 변경은 Google Sheets 에서만, 생성물 손편집 X
- [ ] 캐시/생성물은 스키마 확인용 **읽기 전용**
- [ ] GSSL 윈도우에서 다운로드→생성으로 동기화
- [ ] `refresh_unity` + `read_console` 컴파일 에러 0
- [ ] 생성 결과가 변경을 반영(스크립트/SO/TableLinker/Localize)
- [ ] 코드 조회 필요 시 `TableManager` partial 추가(`add-table-data`)
- [ ] 서비스 계정 JSON 등 비밀정보 커밋 금지
