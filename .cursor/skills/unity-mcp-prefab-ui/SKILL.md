---
name: unity-mcp-prefab-ui
description: Unity MCP로 UI 프리팹을 직접 편집하는 Prefab-first 워크플로우. 런타임 new GameObject/sizeDelta 우회 대신 ExplorationHudPanel 등 Addressable UI 프리팹을 MCP로 수정할 때 사용.
---

# Unity MCP Prefab-First UI

UI 크기·글씨·버튼·스테이지 액터는 **코드로 만들지 않는다.**  
`user-unityMCP`로 프리팹을 수정하고, C# Presenter는 **SerializeField / Find 바인딩 + 로직만** 담당한다.

## 금지 (우회 패턴)

- `new GameObject`로 HUD/오버레이/스테이지 크롬 생성
- `ExplorationHudLayoutApplier`류 런타임 LayoutElement 강제
- `fontSize`/`sizeDelta`를 Presenter OnOpen에서 덮어쓰기
- 탭바·액션 버튼을 코드로 Ensure/Create

허용 예외: 풀링 플로팅 텍스트, VFX 스폰, 매니저 인프라(`UIManager` blocker 등).

## 표준 절차

1. `set_active_instance` (다중 인스턴스 시)
2. `execute_code` + `PrefabUtility.LoadPrefabContents` 로 `Assets/GameResource/Prefabs/UI/ExplorationHudPanel.prefab` 편집  
   - LayoutElement, TMP fontSize, RectTransform, ActionButtonBar, StageViewport
3. `PrefabUtility.SaveAsPrefabAsset` → `AssetDatabase.SaveAssets`
4. C#은 버튼 `onClick` 연결·텍스트 갱신만
5. `refresh_unity` `{ scope:"all", compile:"request", wait_for_ready:true }`
6. `read_console` Error 0
7. `dotnet build Assembly-CSharp.csproj` Error 0

## 주요 Prefab 경로

| 대상 | Prefab 경로 |
|------|-------------|
| 탐험 HUD 전체 | `Assets/GameResource/Prefabs/UI/ExplorationHudPanel.prefab` |
| 스테이지 | `.../Body/CenterPanel/ExploreContent/StageViewport` |
| 강화 버튼 | `.../Overlays/EnhancePanel/ActionButtonBar` |
| 길드 버튼 | `.../Overlays/GuildFacilityPanel/ActionButtonBar` |
| 하단 탭 | `.../BottomTabBar/Tabs` |

## codedom 주의

`execute_code` 기본 컴파일러는 C# 6. 로컬 함수·`out var`·`stackalloc` 금지. `Func`/`Action` 익명 델리게이트 사용.
