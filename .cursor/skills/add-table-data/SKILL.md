---
name: add-table-data
description: AutoRpg 에 정적 테이블 데이터 조회를 추가한다. TableManager partial 에 새 Get 메서드나 새 데이터 도메인을 더할 때 사용.
---

# Skill: 테이블 데이터 조회 추가 (TableManager)

정적 기획 데이터 조회를 추가할 때 사용한다.
`harness-boundaries` 준수: `TableManager` 의 **기존 로직·시그니처는 바꾸지 않고**,
partial 파일에 **조회 메서드를 추가**하는 방식만 사용한다.

> 수기 glue(`TableManager`) 만 다룬다. `*Data`/`*Table`/`TableLinker` 와
> `TableLinker.asset` 은 GSSL 생성물 — 손편집 금지(→ `table-data-workflow` / `gssl-agent-workflow`).

## 구조 이해

- `TableManager : SingletonGameObject<TableManager>`, 도메인별 **partial** 로 분리.
- 데이터 소스: `Resources/TableLinker` (GSSL 생성 ScriptableObject).
- 사용처: System/Controller/Presenter 에서 **정적 메서드 직접 호출**.

## 절차

### A) 기존 도메인에 조회 메서드 추가
1. 해당 partial 파일을 연다.
2. 기존 `Get*` 시그니처/딕셔너리 관례를 따라 새 Get 메서드를 추가한다.
   - `X_Internal()` + `public static X()` 쌍 유지.
   - 없는 키는 로그 `[TableManager]` + 기본값/`null`.
3. 새 인덱싱이 필요하면 해당 도메인의 `Create*Dict()` 안에서만 구축한다.

### B) 새 데이터 도메인 추가
1. **먼저 GSSL 로 시트/생성물을 만든다** (→ `table-data-workflow`).
2. 새 partial `TableManager.{Domain}.cs` 작성: `Create{Domain}Dict()` + `Get{Domain}`.
3. `TableManager.Init_Internal()` 에 `Create{Domain}Dict();` **한 줄만** 추가.

## 체크리스트
- [ ] 기존 partial Get 패턴/네이밍 준수
- [ ] `X_Internal` + `static X` 쌍, 방어 로그 `[TableManager]`
- [ ] 새 도메인은 GSSL 생성 후 partial + `Init_Internal` 한 줄
- [ ] GSSL 산출물 손편집 X
- [ ] 기존 조회 시그니처/로직 미변경
