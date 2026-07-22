# 12. UI/UX 형태 설계 v2 (재기획)

> **목적:** 텍스트 위주·비율 깨짐·9-slice 왜곡 문제를 해결하기 위해 **형태(레이아웃·에셋·스케일 규칙)** 를 처음부터 다시 정의한다.  
> **범위:** 탐험 HUD (`ExplorationHudPanel`) 및 하위 런타임 패널. 기능 기획은 [12_UIUX.md](file:///C:/Users/aq001/OneDrive/문서/PlanMdFile/AutoRpg/12_UIUX.md) 유지, **표현·구조만 v2로 교체**.  
> **상태:** 설계 확정 전 — 구현은 본 문서 §9 체크리스트 통과 후 진행.

---

## 0. v1 구현에서 드러난 문제 (재기획 사유)

| # | 증상 | 원인 |
|---|------|------|
| 1 | 패널 테두리가 너무 두껍거나 찌그러짐 | `spritePixelsPerUnit=10` **+** `pixelsPerUnitMultiplier=10` 이중 스케일 |
| 2 | 같은 UI 요소마다 두께/비율이 다름 | 프리팹·런타임 생성·구형 Borders 에셋 혼용 |
| 3 | 히어로/배너 이미지가 잘리거나 늘어남 | **Simple** 이미지를 고정 Rect에 `preserveAspect` 없이 배치, 슬롯 비율 미정 |
| 4 | 전체가 옛날 텍스트 게임처럼 보임 | LegacyRuntime 단일 폰트, 카드/칩/아이콘 그리드 부재 |
| 5 | 좌표가 제각각 | `anchoredPosition` 수치 하드코딩, LayoutGroup 미사용 |
| 6 | AI 생성 PNG를 Sliced로 사용 | 장식 일러스트에 9-slice 규칙 적용 |

**결론:** 에셋을 “추가”하는 것만으로는 해결되지 않는다. **스케일 규칙 1개 + 그리드 1개 + 에셋 타입 분류** 가 선행되어야 한다.

---

## 1. 디자인 원칙

1. **Reference-first** — 모든 수치는 1920×1080 기준 px. 코드 상수는 `ExplorationHudLayoutMetrics` 단일 출처.
2. **Layout-first** — 수동 `anchoredPosition` 금지(§4 예외 슬롯만). `VerticalLayoutGroup` / `HorizontalLayoutGroup` / `LayoutElement` 사용.
3. **Asset-type-first** — 에셋마다 Image Type(Sliced / Simple / Filled)을 기획 단계에서 고정.
4. **Scale-single** — 스프라이트 스케일은 **PPU 100, multiplier 1** 만 사용(v1의 PPU 10·multiplier 10 폐기).
5. **Mobile-safe** — 하단 탭·CTA는 Safe Area inset 반영(좌우 0, 하단 34px 예약 @ 1080 기준).
6. **State-visible** — “길드 대기”와 “탐험 중”은 **다른 레이아웃 밀도**를 가진다(동일 HUD에 다른 정보 밀도).

---

## 2. 캔버스 & 스케일 시스템

### 2.1 UIRoot

| 항목 | 값 |
|------|-----|
| Canvas | Screen Space Overlay |
| CanvasScaler | Scale With Screen Size |
| Reference Resolution | **1920 × 1080** |
| Match | **0.5** (Width/Height 균형) |
| GraphicRaycaster | 필수 |

### 2.2 Safe Area (HUD 전체)

```
┌─ Full 1920×1080 ─────────────────────────────┐
│  Top safe: 0 (에디터 기준)                     │
│  ┌─ HUD Safe 1920 × 1006 ─────────────────┐   │
│  │  (하단 34px + 좌우 0px 예약)              │   │
│  └─────────────────────────────────────────┘   │
│  Bottom inset: 34px (홈 인디케이터/노치)        │
└──────────────────────────────────────────────┘
```

- `ExplorationHudPanel` 루트 Rect: anchor stretch, `offsetMin.y = 34`, `offsetMax = (0,0)`.

### 2.3 스프라이트 스케일 (통일 규칙)

| 규칙 | 값 | 비고 |
|------|-----|------|
| `spritePixelsPerUnit` | **100** | 모든 GameUI 스프라이트 |
| `Image.pixelsPerUnitMultiplier` | **1** (기본값) | 코드에서 10 설정 **금지** |
| `Image.type = Sliced` | 9-slice UI chrome만 | §5.1 |
| `Image.type = Simple` | 아이콘·일러스트 | §5.2 |
| `Image.type = Filled` | HP바·층 진행바 Fill | §5.3 |

**9-slice border 두께 목표:** Reference 해상도에서 **시각적 8~12px** (소스 텍스처 border ÷ PPU × multiplier).

---

## 3. 그리드 & 타이포

### 3.1 8px 그리드

- 모든 margin/padding/gap은 **8의 배수** (8, 16, 24, 32 …).
- 컬럼 간격 `ColumnGap = 16` (v1: 12 → 16).
- 패널 내부 padding = **16**.

### 3.2 타이포 (TMP 전환)

| 토큰 | 용도 | 크기 | 색 |
|------|------|------|-----|
| `Title/L` | 화면/구역 제목 | 24px Bold | `#F5D673` |
| `Title/M` | 섹션 헤더 | 18px Bold | `#E8EDF5` |
| `Body/M` | 본문·로그 | 15px Regular | `#E0E6F0` |
| `Body/S` | 보조·힌트 | 13px Regular | `#9AA8BC` |
| `Button` | CTA | 16px Bold | Primary `#1A1408` / Secondary `#E0E6F0` |

- **Phase UI-1:** TextMeshPro + Noto Sans KR (또는 프로젝트 기본 TMP 폰트) — LegacyRuntime **HUD에서 제거**.
- RichText 색상은 위 hex만 사용(임의 `<color>` 남발 금지).

### 3.3 컬러 토큰

| 토큰 | Hex | 용도 |
|------|-----|------|
| `bg/app` | `#0B0E14` | 앱 배경 |
| `bg/panel` | `#121824` @ 94% | 패널 fill |
| `stroke/panel` | `#3A4A66` @ 100% | 패널 테두리 |
| `accent/gold` | `#F5D673` | 탭 활성·제목 |
| `accent/cyan` | `#6ECFF5` | 진행·링크 |
| `log/combat` | `#F26D5B` | 전투 |
| `log/discovery` | `#6EE08A` | 발견 |
| `log/event` | `#F5D673` | 이벤트 |
| `log/narrative` | `#7EB8FF` | 서사 |

---

## 4. 화면 골격 (1920×1080)

### 4.1 영역 분할 (수치 확정)

```
┌──────────────────────────── TopBar 96px ────────────────────────────┐
├──────── 288px ────────┬────────── Center (가변) ──────────┬── 400px ──┤
│  Left / Party         │  Explore Visual                    │  Log     │
│  (padding 16)         │  (padding 16)                      │  (16)    │
├───────────────────────┴────────────────────────────────────┴──────────┤
│  BottomTabBar 72px (icon 28 + label 12 + padding)                      │
└─────────────────────────────────────────────────────────────────────────┘
Bottom safe inset 34px (TabBar 위가 아닌 HUD 루트 바깥)
```

| 상수 | v1 | **v2** |
|------|-----|--------|
| `TopBarHeight` | 120 | **96** |
| `LeftPanelWidth` | 300 | **288** |
| `RightPanelWidth` | 420 | **400** |
| `TabBarHeight` | 56 | **72** |
| `TabBarPadding` | 8 | **0** (TabBar 높이에 포함) |
| `HorizontalPadding` | 16 | **16** |
| `ColumnGap` | 12 | **16** |

`CenterPanelWidth` = `1920 - 16 - 288 - 16 - 400 - 16` = **1184px**

### 4.2 레이어 Z-order (아래 → 위)

1. `Backdrop` (단색/그radient, 클릭 불가)
2. `Body` 3-column
3. `TopBar`
4. `BottomTabBar`
5. `Overlay` (시작 카드·탭 패널·이벤트 팝업)

---

## 5. 에셋 바이블 (형태 설계 핵심)

에셋 경로: `Assets/GameResource/Images/GameUI/v2/` (v1 `Modern/`, `Borders/` **사용 중단**)

### 5.1 Sliced — UI Chrome (9-slice)

공통 Import:

- Texture Type: Sprite (Single)
- Mesh Type: Full Rect
- **PPU: 100**
- Filter: Bilinear
- Compression: High Quality (UI)

| Asset ID | 소스 크기 | Border (L,T,R,B) | Image Type | 사용처 |
|----------|-----------|------------------|------------|--------|
| `ui_panel_l` | **128×128** | 24,24,24,24 | Sliced | 좌/중/우 패널 배경 |
| `ui_panel_s` | **128×128** | 20,20,20,20 | Sliced | 로그 카드·소형 카드 |
| `ui_bar_top` | **256×64** | 16,12,16,12 | Sliced | TopBar 배경 |
| `ui_bar_bottom` | **256×72** | 16,12,16,16 | Sliced | TabBar 배경 |
| `ui_btn_primary` | **192×64** | 28,20,28,20 | Sliced | CTA (탐험 시작·귀환) |
| `ui_btn_secondary` | **192×56** | 24,16,24,16 | Sliced | 일시정지·재개 |
| `ui_tab_on` | **160×72** | 20,16,20,20 | Sliced | 활성 탭 |
| `ui_tab_off` | **160×72** | 20,16,20,20 | Sliced | 비활성 탭 |
| `ui_progress_track` | **256×32** | 12,8,12,8 | Sliced | 층 진행 트랙 |
| `ui_progress_fill` | **256×32** | 12,8,12,8 | Sliced | Filled + Horizontal |

**제작 규칙:** 절차 생성 시 border = `round(size * 0.1875)` (128→24). AI PNG를 Sliced로 쓰지 않는다.

### 5.2 Simple — 아이콘·일러스트

| Asset ID | 소스 크기 | 비율 | Image Type | 배치 슬롯 |
|----------|-----------|------|------------|-----------|
| `icon_tab_*` | **56×56** | 1:1 | Simple | Tab icon 28×28 표시 |
| `icon_log_*` | **48×48** | 1:1 | Simple | Log row 24×24 |
| `portrait_frame` | **96×96** | 1:1 | Simple | 초상 72×72 마스크 위 |
| `illust_zone_banner` | **1184×160** | **37:5** | Simple | Center 상단 배너 슬롯 |
| `illust_guild_start` | **560×200** | **14:5** | Simple | 시작 카드 상단 |

**배치 규칙:**

- 슬롯 Rect 비율 = 소스 비율과 **동일**. 다르면 `preserveAspect=true`, `AspectRatioFitter (Fit In Parent)`.
- 일러스트 슬롯 높이는 **폭에 연동** (`height = width / aspect`). 고정 height 단독 지정 금지.

### 5.3 Filled — 게이지

- Track: Sliced `ui_progress_track`
- Fill: Sliced `ui_progress_fill`, `Image.type = Filled`, `fillMethod = Horizontal`
- HP 미니바(파티 카드): 높이 **6px**, track `#1A2030`, fill `#6EE08A`

---

## 6. 영역별 와이어 (컴포넌트 트리)

### 6.1 TopBar (96px)

```
TopBar [ui_bar_top]
├─ StatusRow (HLayout, padding 16,8, spacing 12)
│   ├─ GuildStatusText (Title/M, flex)
│   └─ MetaChipRow (HLayout) — 골드·명성 등 칩
└─ ActionRow (HLayout, anchor bottom, padding 16,0)
    ├─ BtnSecondary 112×40 ×2
    └─ BtnPrimary 112×40
```

- 탐험 **대기** 중: ActionRow 숨김, StatusRow만.
- 탐험 **중**: Pause/Resume/Return 표시.

### 6.2 Left — Party (288px)

```
LeftPanel [ui_panel_l]
└─ PartyScroll (ScrollRect)
    └─ Content (VLayout, spacing 12, padding 16)
        └─ PartyMemberCard [ui_panel_s] ×4  (고정 height 128)
            ├─ PortraitSlot 72×72 [portrait_frame + role tint]
            ├─ Name (Title/M)
            ├─ HpBar 6px [Filled]
            └─ Tags row (icon chips)
```

- v1 텍스트 덤프(`[ 파티 ]` + multiline) **폐기** → 카드 4장 고정.

### 6.3 Center — Explore Visual (1184px)

**상태 A — 길드 대기**

```
CenterPanel [ui_panel_l]
└─ StartCard (anchor center, width 560)
    ├─ BannerSlot 560×200 [illust_guild_start]
    ├─ Title (Title/L)
    ├─ Summary (Body/M, max 4 lines)
    └─ BtnPrimary 280×56 "탐험 시작"
```

- 카드는 **탭 클릭으로 가려지지 않음**. Explore 탭에서만 표시(기존 의도 유지).

**상태 B — 탐험 중**

```
CenterPanel
├─ BannerSlot 1184×160 [illust_zone_banner]  ← 비율 37:5
├─ ZoneTitle (Title/L)
├─ FloorLine (Body/M)
├─ ProgressTrack 100% × 24 [ui_progress_track + fill]
├─ ProgressLabel (Body/S)
├─ PartyStrip (HLayout, 4× portrait 72)
└─ StatusLine (Body/S)
```

### 6.4 Right — Log (400px)

```
RightPanel [ui_panel_l]
├─ SectionHeader 40px [ui_bar_top slice] + "탐험 로그"
└─ LogFeed (ScrollRect)
    └─ Content (VLayout, spacing 8)
        └─ LogItem [ui_panel_s] minHeight 48
            ├─ Accent 4×32 (색상 bar, Simple gradient strip)
            ├─ Icon 24×24
            └─ Message (Body/M, wrap)
```

**대기 중:** Scroll 대신 EmptyState (illustration 120×120 + Body/S 3줄).

### 6.5 BottomTabBar (72px)

```
BottomTabBar [ui_bar_bottom]
└─ Tabs (HLayout, 5 equal, padding 8,4)
    └─ TabItem [ui_tab_on/off]  height 64
        ├─ Icon 28×28
        └─ Label (11px, max 1 line)
```

| 탭 | Icon key |
|----|----------|
| 탐험 | `icon_tab_explore` |
| 강화/장비 | `icon_tab_enhance` |
| 길드시설 | `icon_tab_guild` |
| 연대기 | `icon_tab_chronicle` |
| 도감 | `icon_tab_compendium` |

---

## 7. 화면 상태표

| 상태 | TopBar | Center | Log | Tabs |
|------|--------|--------|-----|------|
| 길드 대기 | Status only | StartCard | EmptyState | **활성** (Explore 외 패널 오픈) |
| 탐험 중 | Status + Actions | Zone visual | Live feed | **활성** |
| 일시정지 | + Pause 강조 | dim 20% | freeze scroll | 활성 |
| 탭 오버레이 | 유지 | 가림(Enhance/Guild/Chronicle) | 유지/가림 | **항상 최상단 클릭** |

---

## 8. 구현 아키텍처 (코드 쪽 설계)

### 8.1 단일 빌드 경로

| 항목 | 규칙 |
|------|------|
| 프리팹 | `ExplorationHudPanel.prefab` **만** — Addressable |
| 빌더 | `ExplorationHudPanelPrefabBuilder` v2 스펙 준수 |
| 런타임 `BuildUi()` | **금지** (Party/Center/Start 제외 legacy는 Phase UI-2에서 제거) |
| 스프라이트 로드 | `RuntimeUiSprites` → v2 키만, fallback 1단계(단색) |

### 8.2 상수 파일

`ExplorationHudLayoutMetrics` v2 수치로 **일괄 교체**. 파생값(`CenterPanelWidth` 등)은 코드 계산 유지.

### 8.3 Addressable

`AddressableKeys.InGame` — `GameUI/v2/*` 파일명 = 키명(snake_case).

---

## 9. 구현 Phase & 완료 기준

### Phase UI-0 — 설계 동결 (현재 문서)

- [ ] 본 문서 리뷰·수치 확정
- [ ] v1 `Modern/`, `Borders/` HUD 참조 제거 계획 합의

### Phase UI-1 — 에셋 & 스케일 정상화

- [x] `GameUI/v2/` Sliced 10종 + Simple 10종 생성(§5)
- [x] Import: PPU 100, border 표 준수
- [x] `pixelsPerUnitMultiplier` 코드 전수 **1 또는 미설정**
- [x] **합격:** 400×200 / 800×200 / 1200×200 패널에 같은 chrome 넣었을 때 border **시각 두께 동일**

### Phase UI-2 — 프리팹 레이아웃 재구축

- [x] `ExplorationHudLayoutMetrics` v2 반영
- [x] LayoutGroup 기반 트리(§6)로 prefab rebuild
- [x] 런타임 `BuildUi()` 제거·Presenter/View 바인딩만
- [x] **합격:** 1920×1080 Game View에서 §4 와이어와 ±4px 이내

### Phase UI-3 — 타이포 & 카드

- [x] TMP 전환 + 토큰(§3.2) — BMJUA 폰트 + v2 컬러 토큰 (TMP 패키지 미설치 → UI Text)
- [x] PartyMemberCard 4종
- [x] LogItem + EmptyState
- [x] **합격:** “텍스트 MUD” 인상 제거, 스크린샷 3장(대기/탐험/탭) 비교

### Phase UI-4 — 일러스트 슬롯

- [x] `illust_*` 비율 슬롯 + AspectRatioFitter
- [x] 구역별 banner variant (최소 1종)
- [x] **합격:** 배너/히어로 **늘어남·찌그러짐 0**

---

## 10. v1 대비 삭제·금지 목록

- `spritePixelsPerUnit = 10` (전 에셋)
- `Image.pixelsPerUnitMultiplier = 10`
- AI PNG → Sliced 적용
- `Assets/GameResource/Images/GameUI/Borders/*` HUD 신규 참조
- TopBar 120px + 수동 Y 좌표 (-152, -212 …)
- LegacyRuntime.ttf on HUD
- `[ 로그 ]`, `[ 파티 ]` 대괄호 섹션 문자열

---

## 11. 관련 문서

- 기능 UX: [12_UIUX.md](file:///C:/Users/aq001/OneDrive/문서/PlanMdFile/AutoRpg/12_UIUX.md)
- 코드 규칙: `.cursor/rules/unity-ui-system.mdc`
- 레이아웃 상수: `ExplorationHudLayoutMetrics.cs` (v2 반영 예정)

---

## 부록 A — Quick Reference (구현자용)

```csharp
// 스프라이트 적용 (v2 표준)
image.sprite = sprite;
image.type = Image.Type.Sliced; // chrome only
// pixelsPerUnitMultiplier 설정하지 않음 (기본 1)
// sprite import: PPU 100, border from §5.1 table
```

```csharp
// 일러스트 슬롯
image.type = Image.Type.Simple;
image.preserveAspect = true;
// + AspectRatioFitter: Fit In Parent, aspect = 37f/5f for zone banner
```

---

**다음 액션:** Phase UI-1부터 순서대로 구현. 본 문서 수치 변경 시 `ExplorationHudLayoutMetrics`와 §5 표를 **함께** 수정한다.
