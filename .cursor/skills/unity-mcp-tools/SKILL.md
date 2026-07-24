---
name: unity-mcp-tools
description: unityMCP(user-unityMCP) 서버로 Unity 에디터를 직접 조작·검증하는 방법. 컴파일/콘솔/테스트/씬/게임오브젝트/프리팹/에셋/메뉴 실행이 필요할 때 참조.
---

# Skill: unityMCP 도구 사용 레퍼런스 (카탈로그)

`user-unityMCP` 도구 **목록·용도** 레퍼런스. 작업별 절차는 아래 스킬을 우선한다.

| 목적 | 스킬 |
|------|------|
| 컴파일·콘솔·테스트·플레이 검증 | `verify-in-unity` |
| UI Prefab 레이아웃·버튼·타이포 | `unity-mcp-prefab-ui` |
| GSSL 부분 동기 메뉴 | `gssl-agent-workflow` |

호출 전 `GetMcpTools` 로 스키마 확인. 서버 `needsAuth`/`error` 면 `mcp_auth` 후 재확인.

`harness-boundaries` 준수: 씬/프리팹을 임의로 망가뜨리지 않으며 `.meta` 는 만지지 않는다.

## 스크립트 편집 방침

소스는 디스크에 있으므로 **Cursor 기본 편집 도구(Read/StrReplace/Write)로 편집**하고,
이후 `refresh_unity` → `read_console` 로 컴파일·에러를 확인한다(상세: `verify-in-unity`).
unityMCP 스크립트 도구는 보조 수단으로 필요할 때만 사용한다.

| 도구 | 용도 |
|------|------|
| `validate_script` | 단일 C# 파일 진단(`level: basic/standard`) — 편집 후 빠른 점검 |
| `create_script` / `delete_script` | 파일 생성/삭제(디스크 편집으로 대체 가능) |
| `apply_text_edits` | 좌표 기반 텍스트 편집(1-indexed, 정확한 위치 필요) |
| `script_apply_edits` | 메서드/클래스 단위 구조적 편집(anchor 기반, 더 안전) |
| `find_in_file` / `get_sha` | 파일 내 정규식 검색 / 내용 없이 해시·메타 확인 |

## 검증 (자세한 절차는 `verify-in-unity`)

| 도구 | 용도 |
|------|------|
| `refresh_unity` | 에셋 리프레시 + 컴파일 요청(`scope`, `compile:"request"`, `wait_for_ready`) |
| `read_console` | 콘솔 로그 조회/클리어(`types:["error"]`, `count` 는 따옴표 문자열) |
| `run_tests` + `get_test_job` | EditMode/PlayMode 테스트(비동기 job 폴링) |
| `manage_editor` | `play`/`pause`/`stop`, 태그·레이어, undo/redo |

## 씬 / 게임오브젝트 / 프리팹 / 컴포넌트

| 도구 | 용도 |
|------|------|
| `find_gameobjects` | 이름/태그/레이어/컴포넌트/경로로 GO 검색(인스턴스 ID 반환, CRUD 아님) |
| `manage_gameobject` | GO CRUD(create/modify/delete/duplicate/move_relative/look_at) |
| `manage_components` | 컴포넌트 add/remove/set_property |
| `manage_scene` | 씬 CRUD·계층(get_hierarchy/get_active/create/load/save/validate) |
| `manage_prefabs` | 프리팹 편집(headless `modify_contents` 또는 `open/save/close_prefab_stage`) |
| `manage_asset` | 에셋 import/create/modify/delete/search(search 시 `generate_preview=false`) |
| `manage_scriptable_object` | ScriptableObject 값 생성/수정(SerializedObject 경로) |

- GO/컴포넌트 **읽기**는 리소스 `mcpforunity://scene/gameobject/{id}/components` 활용.
- 여러 오브젝트/컴포넌트를 한 번에 다룰 땐 `batch_execute` 로 묶어 10~100배 빠르게 처리.

## 메뉴 / 코드 실행 / 문서

| 도구 | 용도 |
|------|------|
| `execute_menu_item` | 에디터 메뉴 실행. 예: `{"menu_path":"Tools/Addressables/Force Generate Keys"}` (자동 생성 파일 재생성) |
| `execute_code` | 에디터 내 임시 C# 실행(파일 미생성). 일회성 점검/자동화에만, 게임 로직 구현엔 쓰지 말 것 |
| `batch_execute` | 여러 MCP 커맨드 일괄 실행(성능) |
| `unity_docs` / `unity_reflect` | Unity 문서 조회 / 타입 리플렉션 |

## 자동 생성 산출물 재생성

생성된 `.cs` 는 손편집 금지(→ `harness-boundaries`). 소스에서 재생성한다.
- `AddressableKeys.cs`: `execute_menu_item {"menu_path":"Tools/Addressables/Force Generate Keys"}` → `refresh_unity` → `read_console`.
- Input 래퍼(`PuzzleControl.cs` 등): `.inputactions` 임포터의 "Generate C# Class" 재임포트(`execute_code`) → `refresh_unity`. 상세는 `setup-input-actions` 스킬.

## 주의
- 씬/프리팹을 `manage_*` 로 바꾼 뒤에는 `manage_scene(save)` / `save_prefab_stage` 로 저장하고,
  플레이 모드 중 변경은 저장되지 않음을 유의(`manage_editor` play 후 반드시 `stop`).
- `deploy_package`/`restore_package`, 패키지 add/remove, 빌드 등 되돌리기 어려운 작업은 사용자 승인 후.
