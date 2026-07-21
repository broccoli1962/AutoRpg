---
name: develop-from-plan
description: 기획 .md 파일을 읽고 자율적으로 Unity 개발을 진행한다. 사용자가 기획서/스펙 마크다운 파일을 가리키며 "이 기획대로 개발해줘", "기획 문서 읽고 진행해", "스펙 구현해줘" 처럼 요청할 때 사용한다. Use when the user points to a planning/spec markdown file and asks the agent to implement it autonomously.
---

# Develop From Plan (기획서 기반 자율 개발)

기획 `.md` 문서를 단일 진실 공급원(Single Source of Truth)으로 삼아, 요구사항을 작업으로 분해하고 Unity C# 코드로 구현한 뒤 컴파일까지 검증하는 워크플로우다.

## 전제 규칙 (반드시 준수)

- `.cursor/rules/`의 프로젝트 규칙을 최우선으로 따른다:
  - `unity-project-overview.mdc` — 디렉토리 구조, 핵심 시스템/패키지
  - `unity-csharp-conventions.mdc` — 네이밍, UniTask, 로그 접두어, XML 주석
  - `unity-ui-system.mdc`, `unity-scene-system.mdc`, `unity-assets.mdc`
- **`.meta` 파일은 절대 생성/수정하지 않는다.** Unity가 임포트 시 자동 생성한다.
- 스크립트 생성/수정은 unityMCP 도구(`create_script`, `manage_script`, `apply_text_edits`, `script_apply_edits`)를 우선 사용한다.

## 워크플로우

아래 체크리스트를 `TodoWrite`로 만들어 진행 상황을 추적한다.

```
- [ ] 1. 기획서 탐색 및 정독
- [ ] 2. 요구사항 → 작업 분해 (Todo 생성)
- [ ] 3. 기존 코드/컨벤션 파악
- [ ] 4. 구현
- [ ] 5. 컴파일/동작 검증
- [ ] 6. 기획서에 진행 상황 반영 및 보고
```

### 1. 기획서 탐색 및 정독

- 사용자가 경로를 지정하면 그 파일을, 아니면 `Glob`으로 프로젝트 내 `*.md`(예: `Docs/`, `기획/`)를 찾아 후보를 제시한다.
- 문서 **전체**를 읽는다. 헤딩, 표, 체크박스, 수용 기준(acceptance criteria), 우선순위를 모두 파악한다.
- 여러 기획 문서가 링크로 연결되어 있으면 관련 문서도 읽는다.

### 2. 요구사항 → 작업 분해

- 문서를 기능 단위의 구체적이고 실행 가능한 작업으로 분해해 `TodoWrite`에 등록한다.
- 의존 관계가 있으면 순서를 정한다. 데이터(ScriptableObject/Table) → 시스템 로직 → UI 순처럼 하위부터 쌓는다.

### 3. 기존 코드/컨벤션 파악

- 구현 전 관련 폴더(`Assets/GameResource/Scripts/...`)를 탐색해 재사용 가능한 매니저/유틸/베이스 클래스를 찾는다.
  - 예: `SingletonGameObject<T>`, `ResourceManager`, `ObjectPoolManager`, `UIManager`, `TableManager`, `Extension`.
- 새로 만들기보다 기존 패턴을 확장한다. 파일 배치는 `unity-project-overview.mdc`의 디렉토리 구조를 따른다.

### 4. 구현

- `unity-csharp-conventions.mdc`를 지켜 구현한다: `_camelCase` private 필드, UniTask 비동기(`.Forget()`), 로그에 `[ClassName]` 접두어, public 메서드 `<summary>`, `TryGetComponent` 우선.
- 스크립트 작성/수정은 unityMCP 도구로 수행한다.
- 앱 종료 대응: Manager 접근 전 `GameStateUtil.IsQuitting` 체크(규칙 준수).

### 5. 컴파일/동작 검증 (필수 피드백 루프)

- 스크립트를 생성/수정한 직후 unityMCP `read_console`(Error/Warning 필터)로 컴파일 오류를 확인한다.
- `editor_state` 리소스의 `isCompiling`이 끝날 때까지 대기한 뒤 확인한다. 필요 시 `refresh_unity`.
- 오류가 있으면 → 원인 분석 → 수정 → 다시 `read_console`. **오류 0이 될 때까지 반복.**
- 테스트가 있는 항목은 `run_tests`로 검증한다.
- 컴파일이 통과해야만 새 타입/컴포넌트를 다른 코드에서 사용할 수 있다.

### 6. 기획서에 진행 상황 반영

- 기획서에 체크박스가 있으면 완료 항목을 `[x]`로 갱신한다(문서 소유 방식이 명확할 때).
- 완료/미완/후속 작업, 내린 결정(기본값 선택 등)을 사용자에게 간단히 보고한다.

## 자율성 원칙

- 네이밍, 기본값, 동등한 접근법 선택 등 사소한 결정은 합리적으로 직접 정하고 보고만 한다.
- 다음의 경우에만 `AskQuestion`으로 질문한다:
  - 기획서가 상호 모순되거나 핵심 정보가 비어 진행이 막힐 때
  - 범위 확장 또는 파괴적(삭제/대규모 리팩터) 작업이 필요할 때
- 그 외에는 멈추지 말고 체크리스트를 끝까지 완료한다.
