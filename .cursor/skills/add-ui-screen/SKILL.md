---
name: add-ui-screen
description: AutoRpg 에 새 UI 화면(Panel/Popup/View)을 MVP 패턴과 UIManager 규약에 맞게 추가한다. 화면·팝업·HUD·탭 뷰를 만들 때 사용. Prefab 레이아웃은 unity-mcp-prefab-ui.
---

# Skill: UI 화면 추가 (MVP + UIManager)

새 화면/팝업/서브뷰를 추가할 때 사용한다. `harness-boundaries` 준수:
UI 베이스 클래스(`UIBase`/`UIPanel`/`UIPopup`/`UIView`/`UIPresenter`)와 `UIManager` 는
**상속·호출만** 하고 수정하지 않는다.

## 베이스 선택

| 만들려는 것 | 상속할 베이스 | 기본 Layer | BackButton |
|-------------|---------------|-----------|-----------|
| 메인 화면/메뉴 패널 | `UIPanel<TPresenter>` | Panel | false |
| 모달 팝업 | `UIPopup<TPresenter>` | Popup | true |
| Panel 위/Popup 아래 상주 UI (NavBar 류) | `UIPanel<TPresenter>` + `Layer` override `Navigation` | Navigation | false |
| 여닫는 HUD 화면 | `UIPanel<TPresenter>` + `Layer` override `HUD` | HUD | false |
| 부모 Panel 내부 탭 서브뷰 | `UIView<TPresenter>` (UIManager 비관리) | — | — |

> 탐험 HUD처럼 게임플레이에 상시 묶인 정적 HUD는 System + RuntimePanel 이 Prefab 자식을 바인딩한다.
> 레이아웃·버튼·타이포는 `unity-mcp-prefab-ui`. 새 게임플레이 표현만이면 `add-gameplay-feature`.

## 절차

### 1) 탐색
- 유사 화면을 모델로 삼는다: `ExplorationHudPanel(+Presenter)`, 오버레이 `*RuntimePanel`.

### 2) View 작성 — `Object/UI/{Scene}/{Name}Panel.cs`
```csharp
public class ExamplePanel : UIPanel<ExamplePresenter>
{
    [SerializeField] private Button _confirmButton;
    // 입력/표시만. 비즈니스 로직 금지.
}
```
- 관리형 UI 는 `MonoBehaviour` 직접 상속 금지. 반드시 UI 베이스 계열.
- 인스펙터 옵션은 기존 필드 관례 사용: `_useOpenSound`/`_openSoundKey`, `_useCloseAnimation`,
  `_handleBackButton`. 조건부 필드는 `[ShowIf]`.

### 3) Presenter 작성 — `{Name}Presenter.cs`
```csharp
public class ExamplePresenter : UIPresenter<ExamplePanel>
{
    public override void OnOpen()
    {
        base.OnOpen();
        // View 버튼 구독, System/데이터 Observable 구독
    }
    public override void OnClose()
    {
        // 구독 해제
        base.OnClose();
    }
}
```
- 비즈니스 로직·데이터 조회는 **Presenter** 담당.
- 정적 데이터는 `TableManager.Get*`, 세션/유저 데이터는 세션/`UserData`. View 가 직접 조회 X.
- System 이벤트는 `OnOpen` 에서 구독, `OnClose`(또는 CompositeDisposable/destroyCancellationToken)에서 해제.

### 4) Prefab (레이아웃은 MCP)
- 크기·글씨·버튼 바·스테이지 슬롯은 **코드로 만들지 않는다** → `unity-mcp-prefab-ui`.
- C# 은 SerializeField / 경로 Resolve + `onClick`·텍스트 갱신만.

### 5) 오픈/닫기 — 반드시 `UIManager` 경유
```csharp
var popup = await UIManager.OpenAsync<ConfirmPopup>();
UIManager.Open<ExplorationHudPanel>();
UIManager.Close(panel);
UIManager.CloseDynamic(popup);
```
- `Instantiate`/`SetActive`/`Destroy` 로 UI 직접 제어 금지.
- `UIBase.CloseAsync` 외부 호출 금지. 백/ESC 는 `UIManager.PopBack` 경로.

### 6) 씬 배선 — `SceneContext`
- 어떤 UI 를 언제 띄울지는 `SceneContext` 의 `OnEnterAsync()` 가 결정한다.
- 개별 Panel/View 는 형제 UI를 직접 오픈하지 않는다. UI 간 통신은 static `Observable` 사용.

### 7) Addressable 등록
- UI 프리팹을 Addressable 그룹에 등록 → `Tools/Addressables/Force Generate Keys` 로 `AddressableKeys` 재생성.
- `OpenAsync<T>()` 는 키 생략 시 `AddressableKeys.UI.Get<T>()` 를 자동 사용.
- `AddressableKeys.cs` 직접 편집 금지.

### 8) 검증
- `verify-in-unity` 로 컴파일·오픈/닫기·백버튼 동작 확인.

## 체크리스트
- [ ] 올바른 베이스 선택 + `Layer` 지정
- [ ] View=표시/입력, Presenter=로직 분리
- [ ] Prefab 레이아웃은 MCP (`unity-mcp-prefab-ui`), 런타임 크기/GO 생성 금지
- [ ] 오픈/닫기는 `UIManager` 만
- [ ] Addressable 키는 Generator 재생성
- [ ] 구독은 OnOpen 등록 / OnClose 해제
- [ ] UI 베이스·UIManager 미변경 (필요 시 에스컬레이션)
