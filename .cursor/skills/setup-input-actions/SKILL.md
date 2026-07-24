---
name: setup-input-actions
description: 프로젝트/기능에 맞는 Input System 입력을 새로 생성·교체·적용한다. Input Actions(.inputactions) 에셋 생성, C# 래퍼 생성, 게임측 InputSystem 래퍼 배선이 필요할 때 사용.
---

# Skill: Input System 입력 생성·적용

입력 바인딩은 **프로젝트·기능마다 다르다.** 기존 `PuzzleControl.cs`/`.inputactions` 를 그대로 쓰지 말고,
필요에 맞게 **새 Input Actions 에셋을 만들고 래퍼를 생성해 적용**한다.
`harness-boundaries` 준수: 생성된 래퍼(.cs)는 손편집 금지, 게임측 `InputSystem` 래퍼의 **공개 Observable 계약은 유지**.

## 계층 (반드시 이 흐름을 유지)

```
.inputactions (에셋, 편집/생성 O)
   │  (Generate C# Class 재임포트)
   ▼
생성 래퍼 클래스 (예: PuzzleControl.cs, 손편집 X)
   │  (게임측 래퍼가 인스턴스화 + 콜백/폴링)
   ▼
Backend.Object.GameSystems.InputSystem  ← 안정적 이음새
   │  R3 Observable (OnPointerPressed/Moved/Released ...)
   ▼
구독자 (게임 System / UIManager 등) — 이 Observable 계약에만 의존
```
- 뒤로가기/취소는 `UIManager` 가 래퍼의 Cancel 액션(예: `PuzzleControl.UI.Cancel`)을 구독한다.

## 절차

### 1) 탐색
- 기존 입력 흐름을 모델로 읽는다: `Assets/Settings/*.inputactions`, `Util/Input/PuzzleControl.cs`(생성물),
  `Object/GameSystems/InputSystem.cs`(래퍼), `UIManager` 의 Cancel 구독부.
- 이번 기능에 필요한 **액션 맵/액션/컨트롤 타입**(예: Press=Button, Position=Value/Vector2, Cancel=Button)을 정한다.

### 2) Input Actions 에셋 생성/편집
- 새 프로젝트/기능이면 `.inputactions` 를 새로 만든다(기존 것을 재사용/복제하거나 새 경로).
  - `.inputactions` 는 JSON 에셋이다. `manage_asset` 으로 생성/수정하거나 에디터에서 편집.
  - 기존 계약을 유지하려면 액션 이름(예: `Puzzle/Press`, `Puzzle/Position`, `UI/Cancel`)을 맞춘다.
- 바인딩 변경은 **에셋에서만** 한다. 생성된 `.cs` 를 직접 고치지 않는다.

### 3) C# 래퍼 생성 (Generate C# Class)
에셋 임포터의 "Generate C# Class" 를 켜고 클래스명/네임스페이스/경로를 지정한 뒤 재임포트한다.
`unityMCP` 의 `execute_code` 로 자동화할 수 있다(속성명은 Input System 버전에 따라 다를 수 있으니
실패 시 `unity_reflect`/임포터로 확인):

```csharp
// execute_code (UnityEditor 컨텍스트)
var path = "Assets/Settings/PuzzleAction.inputactions";
var importer = UnityEditor.AssetImporter.GetAtPath(path);
var so = new UnityEditor.SerializedObject(importer);
so.FindProperty("m_GenerateWrapperCode").boolValue = true;
so.FindProperty("m_WrapperClassName").stringValue = "PuzzleControl";
so.FindProperty("m_WrapperCodeNamespace").stringValue = "Backend.Util.Input";
// so.FindProperty("m_WrapperCodePath").stringValue = "Assets/GameResource/Scripts/Util/Input/PuzzleControl.cs";
so.ApplyModifiedPropertiesWithoutUndo();
importer.SaveAndReimport();
return "reimported";
```
- 생성 후 `refresh_unity` → `read_console` 로 컴파일·에러 확인.

### 4) 게임측 InputSystem 래퍼 배선
- `InputSystem.Initialize()/Dispose()` 에서 생성 래퍼를 인스턴스화하고 액션 콜백/폴링을 연결한다
  (현재: `Press.started/canceled` + `Position` 폴링 → `onPointer*Subject.OnNext`).
- **공개 Observable(`OnPointerPressed/Moved/Released` 등)의 이름·타입·의미는 그대로 유지**한다.
  새 입력 개념이 필요하면 기존 것을 바꾸지 말고 **새 Observable 을 추가**한다.
- UI 히트 테스트(`IsPointerOverUI`)·이동 트래킹 등 기존 방어 로직 패턴을 따른다.
- `InputSystem` 은 `GameManager.StartGameplay_Internal()` 에서 `Initialize()` 되고 있음(입력 시스템 등록 흐름 유지).

### 5) 취소/뒤로가기 배선
- Cancel 액션을 바꿨다면 `UIManager` 의 Cancel 구독부를 새 액션에 맞게 갱신한다. 그 외 `UIManager` 로직은 건드리지 않는다.

### 6) 검증 (`verify-in-unity`)
- `refresh_unity`(compile) → `read_console`(Error 0) → 필요 시 `manage_editor` play 로 실제 입력 반응 확인 후 `stop`.

## 체크리스트
- [ ] 바인딩은 `.inputactions` 에서만 편집, 생성 래퍼(.cs) 손편집 X
- [ ] C# 래퍼는 임포터 "Generate C# Class" 로 생성/재생성
- [ ] 게임측 `InputSystem` 래퍼의 공개 Observable 계약 유지(변경 시 구독자 동반 갱신)
- [ ] Cancel/뒤로가기 변경 시 `UIManager` 연결부 갱신
- [ ] `GameManager` Start/End 의 `InputSystem` 등록 흐름 유지
- [ ] `refresh_unity` + `read_console` 로 검증, `.meta` 미생성
