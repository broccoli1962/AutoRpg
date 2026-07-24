---
name: add-gameplay-feature
description: AutoRpg 에 새 게임플레이 기능(GameSystem / Controller / RuntimePanel)을 계층 아키텍처에 맞게 추가한다. 규칙·상태(System)와 스폰·풀·연출(Controller/Panel)을 분리할 때 사용.
---

# Skill: 게임플레이 기능 추가 (System / Controller)

새 탐험/성장/이벤트 로직이나 씬 오브젝트를 추가할 때 사용한다.
`harness-boundaries` 를 준수한다: 기존 매니저/유틸/베이스는 **호출·구독만** 한다.

## 계층 원칙

| 계층 | 형태 | 책임 |
|------|------|------|
| **System** | `public static class` (`GameSystems/...`) | 규칙·상태·오케스트레이션·R3 이벤트 발행. 무상태 씬 오브젝트 X |
| **Controller / RuntimePanel** | `CachedMonobehaviour` 등 | Prefab 바인딩·풀·스폰·연출 ↔ System 구독 |
| **Object** | 액터·슬롯 등 | 개체 데이터·입력·연출, System API 호출 |

**모범 페어:** `ExplorationSystem` ↔ `ExplorationHudPanel` / `ExplorationStageRuntimePanel`  
(이벤트 발행 → Prefab 바인딩 UI/스테이지 반영).

## 절차

### 1) 탐색
- 가장 비슷한 기존 System/Panel 을 읽는다 (`GameSystems/Exploration/`, `Object/UI/Exploration/`).
- 데이터가 테이블/세션/유저 중 어디에 있는지 확인 (`TableManager`, 세션 세이브, `UserData`).

### 2) System 작성 — `Assets/GameResource/Scripts/GameSystems/{Domain}/{Name}System.cs`
- `public static class`, `Initialize()` / `Dispose()` 쌍.
- 구독은 `CompositeDisposable` 에 `.AddTo(...)`, `Dispose()` 에서 정리.
- 이벤트: `private static readonly Subject<T> _onXxx` + `public static Observable<T> OnXxx => _onXxx`.
- 상태: `ReactiveProperty<T>` + 읽기전용 노출.
- 다른 System 은 static 으로 직접 호출 (DI 없음).

```csharp
public static class ExampleSystem
{
    private static readonly Subject<int> _onChanged = new();
    public static Observable<int> OnChanged => _onChanged;
    private static CompositeDisposable _subscriptions;

    public static void Initialize()
    {
        _subscriptions = new CompositeDisposable();
        // 다른 System Observable 구독 예
        // ExplorationSystem.OnXxx.Subscribe(...).AddTo(_subscriptions);
    }

    public static void Dispose() => _subscriptions?.Dispose();
}
```

### 3) 생명주기 등록 — `GameManager`
- **직접 편집 금지 대상이지만 등록은 유일한 예외 지점.** `StartGameplay_Internal()` 에 `Initialize()`,
  `EndGameplay_Internal()` 에 `Dispose()` 를 **기존 순서 관례대로 한 줄씩 추가**한다.
- 그 외 `GameManager` 로직/시그니처는 바꾸지 않는다. 등록만으로 부족하면 에스컬레이션.

### 4) Controller / RuntimePanel (씬·HUD 표현이 필요할 때)
- Prefab 자식은 **이미 있어야** 한다. 없으면 `unity-mcp-prefab-ui` 로 먼저 추가.
- SerializeField 또는 경로 Resolve 로 바인딩. `new GameObject` / 런타임 레이아웃 강제 금지.
- System 이벤트를 `.Subscribe(...).AddTo(_disposables)` 로 구독.
- `OnDestroy` 에서 `GameStateUtil.IsQuitting` 체크 → dispose + 풀 반환.

### 5) 진입점 배선 — `SceneContext.OnEnterAsync()`
- 씬 구성/오픈 책임은 `SceneContext`에 둔다. 개별 System 은 스스로 씬을 구성하지 않는다.

### 6) 검증
- UI/테이블이 함께 필요하면 `add-ui-screen` / `add-table-data` 참조.
- `verify-in-unity` 로 컴파일·콘솔·플레이 검증.

## 체크리스트
- [ ] System 은 static + `Initialize/Dispose` + 구독 정리
- [ ] 이벤트는 `Subject`/`ReactiveProperty` → 읽기전용 `Observable`
- [ ] `GameManager` Start/End 에 등록/해제 한 줄씩 (순서 준수)
- [ ] Panel/Controller 는 Prefab 바인딩·연출만, 규칙 판정은 System
- [ ] 에셋은 `AddressableKeys` + 매니저 경유
- [ ] `OnDestroy`/`Dispose` + `IsQuitting` 체크
- [ ] 기반 코드 공개 API 미변경 (변경 필요 시 에스컬레이션)
