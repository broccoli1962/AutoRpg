---
name: verify-in-unity
description: 코드 변경 후 Unity 에디터에서 컴파일·콘솔·테스트·플레이를 검증한다. 자율 작업 루프의 검증 단계에서 unityMCP 도구로 실제 확인할 때 사용.
---

# Skill: Unity 에서 변경 검증 (unityMCP)

자율 작업 루프(`harness-workflow`)의 **검증 단계**에서 사용한다.
목표: "내 변경이 컴파일되고, 콘솔 에러가 없으며, 의도한 동작을 한다"를 실제로 확인.

> unityMCP 서버(`user-unityMCP`)의 도구로 검증한다. 도구 호출 전 필요하면 `GetMcpTools`
> 로 최신 스키마를 재확인한다. 서버가 `needsAuth`/`error` 면 `mcp_auth` 시도 후 재확인,
> 그래도 불가면 맨 아래 "폴백".

## 표준 검증 순서

### 1) 컴파일 (refresh_unity)
스크립트를 편집·추가·삭제한 뒤 컴파일을 요청하고 준비될 때까지 기다린다.

```
refresh_unity  { "scope": "scripts", "compile": "request", "wait_for_ready": true }
```
- 대규모 에셋 변경까지 반영하려면 `scope: "all"`.
- 강제 재임포트가 필요하면 `mode: "force"`.

### 2) 콘솔 에러 확인 (read_console)
컴파일/런타임 에러를 확인한다. **Error 0** 이 목표.

```
read_console  { "action": "get", "types": ["error"], "count": "50", "include_stacktrace": true }
```
- `count` 는 **따옴표 문자열**로 전달(클라이언트 호환). 필요 시 `filter_text` 로 좁힌다.
- 내 변경으로 생긴 에러/예외가 있으면 원인을 고치고 1)~2) 재수행.
- 새 작업 시작 전 노이즈를 지우려면 `{ "action": "clear" }` (휘발성 UI 상태만 초기화).

### 3) 단일 스크립트 진단 (validate_script, 선택)
특정 파일만 빠르게 점검할 때.

```
validate_script  { "uri": "Assets/GameResource/Scripts/.../Foo.cs", "level": "standard", "include_diagnostics": true }
```

### 4) 자동화 테스트 (run_tests + get_test_job, 있을 때)
EditMode/PlayMode 테스트가 있으면 실행한다. 비동기 → `job_id` 폴링.

```
run_tests     { "mode": "EditMode", "include_failed_tests": true }
get_test_job  { "job_id": "<...>", "wait_timeout": 60, "include_failed_tests": true }
```
- PlayMode 는 도메인 리로드로 느리다: `init_timeout` 을 120000 정도로.

### 5) 플레이 검증 (manage_editor, 런타임 확인 필요 시)
계획 단계의 성공 기준을 실제 재현해야 할 때만.

```
manage_editor { "action": "play" }
...  (read_console 로 런타임 로그/예외 확인, 필요 시 manage_scene/manage_gameobject 로 상태 점검)
manage_editor { "action": "stop" }
```
- 반드시 `stop` 으로 원복. 플레이 중 만든 변경은 저장되지 않으므로 주의.

## 자동 생성 파일 재생성
`AddressableKeys.cs` 등은 손편집 금지. 에셋 등록 후 메뉴로 재생성한다.

```
execute_menu_item { "menu_path": "Tools/Addressables/Force Generate Keys" }
```
(메뉴 경로는 프로젝트 상황에 맞게. 이후 `refresh_unity` → `read_console` 로 확인.)

## 폴백 (unityMCP 불가 시)
- 편집 파일 린트/정적 리뷰만 수행하고, **에디터에서 컴파일·재생 확인이 필요함을 사용자에게 명시**한다.
- 위험한 변경을 검증 없이 "완료"로 보고하지 않는다.

## 완료 기준
- [ ] `refresh_unity` 후 컴파일 성공(ready)
- [ ] `read_console` 에서 내 변경 관련 Error/Exception 0
- [ ] (있으면) 관련 테스트 통과
- [ ] 계획 단계 성공 기준이 재현으로 확인됨, 또는 미검증 항목을 사용자에게 보고
