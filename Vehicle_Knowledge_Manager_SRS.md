# Vehicle Knowledge Manager (VKM)
## Software Requirements Specification

- 문서 버전: 0.1
- 작성일: 2026-07-21
- 대상 플랫폼: Windows Desktop
- 개발 기술: C# / .NET / WPF
- 기본 저장 형식: Markdown (`.md`)

---

# 1. 개요

## 1.1 프로젝트명

**Vehicle Knowledge Manager (VKM)**

## 1.2 목적

Vehicle Knowledge Manager는 자동차 장기렌트 영업사원이 차량 정보를 빠르게 검색하고, 열람하고, 수정하고, 관리하기 위한 Windows 데스크톱 애플리케이션이다.

기존에는 차량별 정보를 Microsoft Word 문서(`.docx`)로 관리하고 있으나, 다음과 같은 문제가 있다.

- 여러 문서에서 원하는 정보를 빠르게 찾기 어렵다.
- 문서마다 작성 형식이 달라 관리가 어렵다.
- 차량 간 옵션 비교가 어렵다.
- Git을 이용한 변경 이력 관리가 어렵다.
- AI 도구나 개발 도구가 내용을 읽고 활용하기 불편하다.
- 간단한 내용을 수정하는 데도 Word를 실행해야 한다.

VKM은 차량 정보의 기본 저장 형식을 Markdown으로 통일하고, 기존 DOCX 문서를 Markdown으로 가져오는 기능을 제공한다.

---

# 2. 제품 목표

## 2.1 핵심 목표

1. 차량 정보를 제조사 및 차종별로 정리한다.
2. Markdown 문서를 프로그램 안에서 바로 열람하고 수정한다.
3. 전체 차량 문서를 빠르게 검색한다.
4. 기존 DOCX 문서를 Markdown으로 변환하여 가져온다.
5. 파일 기반 구조를 사용하여 Git, VS Code, Obsidian 등 외부 도구와 호환한다.

## 2.2 비목표

V1에서는 다음 기능을 핵심 개발 범위에서 제외한다.

- 차량 가격 및 옵션 정보 자동 수집
- 현대·기아 등 제조사 홈페이지 크롤링
- AI를 이용한 자동 요약
- 차량 간 자동 옵션 비교
- 고객 관리 및 상담 이력 관리
- 사용자 계정 및 권한 관리
- 클라우드 동기화
- 모바일 앱
- 여러 사용자의 동시 편집
- 차량 데이터의 완전한 DB 구조화

위 기능은 향후 확장 기능으로 검토한다.

---

# 3. 사용자

## 3.1 주요 사용자

자동차 장기렌트 또는 자동차 판매 영업사원

## 3.2 사용자 특성

- Windows PC를 사용한다.
- 차량 정보를 상담 중 빠르게 확인해야 한다.
- 차량 옵션과 트림 정보를 자주 수정한다.
- Word 문서 사용 경험은 있으나 Markdown에 익숙하지 않을 수 있다.
- 복잡한 데이터 입력 화면보다 문서 형태의 자유로운 작성 방식을 선호한다.

---

# 4. 시스템 범위

## 4.1 시스템 역할

VKM은 로컬 폴더에 저장된 Markdown 문서를 차량 정보로 관리한다.

기본 데이터 흐름은 다음과 같다.

```text
기존 DOCX 문서
    ↓ Import
Markdown 문서
    ↓
VKM에서 검색 / 열람 / 수정
    ↓
로컬 파일 저장
    ↓
Git / VS Code / Obsidian / AI 도구에서 활용
```

## 4.2 데이터 원칙

- DOCX는 원본 데이터가 아니라 **가져오기 대상**이다.
- Markdown 파일을 최종 원본 데이터로 사용한다.
- 애플리케이션 전용 바이너리 형식으로 차량 정보를 잠그지 않는다.
- 프로그램을 사용하지 않아도 일반 텍스트 편집기로 데이터를 수정할 수 있어야 한다.
- 파일 이름과 폴더 구조는 사용자가 직접 확인하고 관리할 수 있어야 한다.

---

# 5. 권장 폴더 구조

```text
VehicleKnowledge/
├─ 현대/
│  ├─ 그랜저 하이브리드.md
│  ├─ 쏘나타.md
│  └─ 아이오닉6.md
├─ 기아/
│  ├─ K5.md
│  ├─ 스포티지.md
│  └─ 카니발.md
├─ 제네시스/
│  ├─ G80.md
│  └─ GV80.md
└─ 기타/
   └─ 미분류.md
```

사용자는 루트 폴더를 설정할 수 있어야 하며, 프로그램은 하위 폴더와 Markdown 파일을 자동으로 탐색한다.

---

# 6. 사용자 인터페이스

## 6.1 메인 화면 구성

메인 화면은 다음 영역으로 구성한다.

### 상단 도구 모음

- 저장소 폴더 열기
- 새 문서
- 새 폴더
- DOCX 가져오기
- 저장
- 삭제
- 검색창
- 새로고침
- 설정

### 좌측 탐색 영역

- 제조사 및 폴더 트리
- Markdown 문서 목록
- 폴더 확장 및 축소
- 파일 선택
- 우클릭 메뉴

### 중앙 편집 영역

- Markdown 원문 편집기
- 문서 제목 표시
- 저장 상태 표시
- 변경 여부 표시

### 우측 미리보기 영역

- 렌더링된 Markdown 미리보기
- 제목
- 목록
- 표
- 굵은 글씨
- 구분선
- 인용문
- 코드 블록 표시

V1에서는 편집기와 미리보기 영역을 탭 또는 분할 화면으로 제공할 수 있다.

## 6.2 화면 배치 예시

```text
┌──────────────────────────────────────────────────────┐
│ 폴더 열기 | 새 문서 | DOCX 가져오기 | 저장 | 검색   │
├──────────────┬──────────────────────┬────────────────┤
│ 차량 목록    │ Markdown 편집기      │ 미리보기       │
│              │                      │                │
│ 현대         │ # 그랜저 하이브리드 │ 그랜저         │
│ ├ 그랜저     │                      │                │
│ ├ 쏘나타     │ ## 프리미엄          │ 프리미엄       │
│ 기아         │ - 통풍시트           │ • 통풍시트     │
│ ├ K5         │ - 스마트 크루즈     │ • 스마트...    │
└──────────────┴──────────────────────┴────────────────┘
```

---

# 7. 기능 요구사항

## FR-001 저장소 폴더 설정

시스템은 사용자가 차량 문서가 저장될 루트 폴더를 선택할 수 있도록 해야 한다.

### 세부 요구사항

- 폴더 선택 대화상자를 제공한다.
- 마지막으로 선택한 폴더를 기억한다.
- 앱 실행 시 마지막 저장소를 자동으로 연다.
- 저장소가 존재하지 않을 경우 사용자에게 알린다.
- 다른 저장소 폴더로 변경할 수 있다.

---

## FR-002 폴더 및 문서 탐색

시스템은 저장소 내 하위 폴더와 Markdown 파일을 트리 형태로 표시해야 한다.

### 세부 요구사항

- 하위 폴더를 재귀적으로 탐색한다.
- `.md` 파일만 기본 문서 목록에 표시한다.
- 폴더와 파일을 이름순으로 정렬한다.
- 선택한 파일을 편집기에 연다.
- 외부에서 추가되거나 삭제된 파일을 새로고침할 수 있다.
- 현재 열린 파일을 탐색 트리에서 강조한다.

---

## FR-003 Markdown 문서 열람

사용자는 Markdown 문서를 프로그램 내에서 읽을 수 있어야 한다.

### 지원 문법

- 제목: `#`, `##`, `###`
- 글머리표 목록
- 번호 목록
- 굵은 글씨
- 기울임
- 구분선
- 인용문
- 표
- 코드 블록
- 링크

---

## FR-004 Markdown 문서 편집

사용자는 선택한 Markdown 문서를 직접 수정할 수 있어야 한다.

### 세부 요구사항

- 일반 텍스트 편집을 지원한다.
- UTF-8 인코딩을 사용한다.
- 변경 내용이 있을 경우 저장 전 상태를 표시한다.
- `Ctrl + S`로 저장할 수 있다.
- 저장하지 않고 다른 파일을 열 경우 경고한다.
- 실행 취소 및 다시 실행을 지원한다.
- 문서 내 찾기를 지원한다.

---

## FR-005 Markdown 미리보기

시스템은 Markdown 편집 내용을 렌더링하여 미리보기로 제공해야 한다.

### 세부 요구사항

- 편집 내용 변경 시 미리보기를 갱신한다.
- 입력 지연을 막기 위해 짧은 디바운스를 적용할 수 있다.
- Markdown 표를 읽기 쉬운 형태로 표시한다.
- 미리보기 실패 시 편집 기능은 계속 사용할 수 있어야 한다.

---

## FR-006 새 문서 생성

사용자는 새로운 차량 Markdown 문서를 만들 수 있어야 한다.

### 입력 정보

- 문서 이름
- 저장할 폴더
- 선택적 템플릿

### 기본 템플릿

```md
# 차량명

## 기본 정보

- 제조사:
- 차급:
- 연료:
- 비고:

## 트림

### 트림명

기본 사양

- 

선택 옵션

- 

## 상담 포인트

- 
```

### 세부 요구사항

- 동일한 이름의 파일이 있으면 덮어쓰기 전에 경고한다.
- 파일명에 사용할 수 없는 문자를 검증한다.
- 문서 생성 후 즉시 편집 화면으로 연다.

---

## FR-007 폴더 생성

사용자는 제조사 또는 분류용 폴더를 생성할 수 있어야 한다.

### 세부 요구사항

- 선택한 폴더 하위에 새 폴더를 생성한다.
- 중복 폴더명이 있으면 사용자에게 알린다.
- 잘못된 폴더명 문자를 검증한다.

---

## FR-008 문서 이름 변경 및 이동

사용자는 Markdown 문서의 이름을 변경하거나 다른 폴더로 이동할 수 있어야 한다.

### 세부 요구사항

- 파일 이름 변경
- 드래그 앤 드롭 또는 이동 메뉴
- 대상 경로에 같은 이름이 존재하면 경고
- 열린 문서 이동 시 현재 경로 갱신

V1 구현 난이도에 따라 드래그 앤 드롭은 후순위로 둘 수 있다.

---

## FR-009 문서 삭제

사용자는 문서를 삭제할 수 있어야 한다.

### 세부 요구사항

- 삭제 전 확인 대화상자를 표시한다.
- 가능하면 즉시 영구 삭제하지 않고 휴지통으로 이동한다.
- 열린 문서를 삭제한 경우 편집 화면을 초기화한다.

---

## FR-010 전체 문서 검색

시스템은 저장소의 모든 Markdown 문서에서 키워드를 검색할 수 있어야 한다.

### 검색 대상

- 파일 이름
- 폴더 이름
- 문서 본문

### 검색 결과

- 파일 이름
- 상대 경로
- 검색어가 포함된 일부 문장
- 일치 개수

### 세부 요구사항

- 검색 결과 선택 시 해당 문서를 연다.
- 가능하면 검색어가 있는 위치로 이동한다.
- 한글 및 영문 검색을 지원한다.
- 대소문자 구분 여부는 기본적으로 무시한다.

---

## FR-011 DOCX 가져오기

사용자는 기존 DOCX 파일을 Markdown으로 변환하여 저장소에 가져올 수 있어야 한다.

### 기본 처리 흐름

```text
DOCX 선택
→ 변환 결과 미리보기
→ 저장 폴더 및 파일명 선택
→ Markdown 저장
```

### 변환 대상

- 일반 문단
- 제목 스타일
- 글머리표
- 번호 목록
- 표
- 굵은 글씨
- 기울임
- 링크
- 줄바꿈

### 변환 규칙 예시

| DOCX 요소 | Markdown 변환 |
|---|---|
| 제목 1 | `#` |
| 제목 2 | `##` |
| 제목 3 | `###` |
| 글머리표 | `-` |
| 번호 목록 | `1.` |
| 굵게 | `**텍스트**` |
| 표 | Markdown 표 |
| 일반 문단 | 일반 텍스트 |

### 추가 정리 규칙

- 불필요한 연속 공백 제거
- 빈 문단 과다 생성 방지
- Word 전용 특수 글머리표를 `-`로 변환
- ``, `▶`, `→` 등 옵션 표기를 `-` 또는 지정된 형태로 정규화
- UTF-8 Markdown으로 저장

### 제약사항

- DOCX의 디자인, 글꼴, 색상, 페이지 레이아웃은 보존하지 않는다.
- 변환 목표는 외형 복제가 아니라 문서 내용과 구조 보존이다.
- 복잡한 도형, 텍스트 박스, SmartArt, 차트는 V1 변환 대상에서 제외한다.
- 이미지 변환은 V1에서 제외하거나 별도 폴더 추출 방식으로 선택 구현한다.

---

## FR-012 DOCX 일괄 가져오기

사용자는 여러 DOCX 파일 또는 DOCX 파일이 있는 폴더를 선택하여 일괄 변환할 수 있어야 한다.

### 세부 요구사항

- 여러 파일 선택
- 폴더 내 `.docx` 파일 탐색
- 파일별 변환 성공 및 실패 결과 표시
- 같은 이름의 `.md` 파일이 있을 경우 처리 방식 선택
  - 건너뛰기
  - 덮어쓰기
  - 새 이름으로 저장
- 변환 로그 표시
- 변환 실패가 전체 작업을 중단시키지 않도록 한다.

---

## FR-013 자동 저장

자동 저장은 선택 기능으로 제공한다.

### 세부 요구사항

- 설정에서 자동 저장 활성화 여부 선택
- 기본값은 비활성화
- 활성화 시 변경 후 일정 시간 뒤 저장
- 자동 저장 실패 시 사용자에게 알림

---

## FR-014 최근 문서

시스템은 최근에 열어본 문서를 표시할 수 있어야 한다.

### 세부 요구사항

- 최근 문서 최대 10개
- 파일이 삭제되었으면 목록에서 제거
- 최근 문서 선택 시 바로 열기

V1 필수 기능은 아니며 우선순위는 낮다.

---

## FR-015 외부 프로그램으로 열기

사용자는 현재 Markdown 파일 또는 저장소 폴더를 외부 프로그램에서 열 수 있어야 한다.

### 지원 기능

- 파일 탐색기에서 위치 열기
- 기본 텍스트 편집기로 열기
- VS Code로 열기
- 저장소 폴더 열기

VS Code 실행 파일을 찾을 수 없는 경우 기본 편집기로 대체한다.

---

## FR-016 설정

시스템은 다음 설정을 저장해야 한다.

- 마지막 저장소 경로
- 편집기 글자 크기
- 자동 저장 여부
- 미리보기 표시 여부
- 기본 새 문서 템플릿
- DOCX 변환 시 덮어쓰기 정책
- 창 크기 및 위치

설정은 JSON 파일로 로컬 저장한다.

---

# 8. 비기능 요구사항

## NFR-001 플랫폼

- Windows 10 이상을 지원한다.
- x64 환경을 기본 대상으로 한다.
- .NET 8 이상 사용을 권장한다.

## NFR-002 성능

- 500개의 Markdown 문서가 있는 저장소를 3초 이내에 탐색하는 것을 목표로 한다.
- 일반적인 Markdown 문서는 1초 이내에 열려야 한다.
- 검색은 일반적인 로컬 저장소에서 2초 이내 결과 표시를 목표로 한다.
- 편집 중 미리보기로 인해 입력이 끊기지 않아야 한다.

## NFR-003 안정성

- 한 문서의 변환 실패가 전체 일괄 변환을 중단시키지 않아야 한다.
- 파일 저장 중 오류가 발생하면 기존 파일 손상을 최소화해야 한다.
- 가능하면 임시 파일에 먼저 저장한 뒤 교체한다.
- 처리되지 않은 예외로 애플리케이션이 종료되지 않도록 전역 예외 처리를 적용한다.

## NFR-004 데이터 보존

- 사용자의 Markdown 원문을 임의로 재정렬하거나 자동 수정하지 않는다.
- 사용자가 명시적으로 요청하지 않는 한 문서 내용을 변경하지 않는다.
- 삭제 및 덮어쓰기 작업 전 확인 절차를 제공한다.

## NFR-005 사용성

- 자주 사용하는 기능은 2회 이내 클릭으로 접근할 수 있어야 한다.
- 단축키를 지원한다.
- 오류 메시지는 사용자가 이해할 수 있는 한국어로 표시한다.
- 개발 용어보다 작업 중심 표현을 사용한다.

## NFR-006 유지보수성

- UI, 파일 시스템 처리, Markdown 처리, DOCX 변환 로직을 분리한다.
- DOCX 변환기는 인터페이스로 추상화하여 교체할 수 있도록 한다.
- 단위 테스트가 가능한 구조를 사용한다.
- MVVM 패턴 사용을 권장한다.

---

# 9. 기술 제안

## 9.1 권장 기술 스택

- C#
- .NET 8
- WPF
- MVVM
- CommunityToolkit.Mvvm
- Markdig
- WebView2 또는 WPF용 HTML 렌더러
- Mammoth 또는 DocumentFormat.OpenXml
- ReverseMarkdown
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Logging

## 9.2 DOCX 변환 권장 방식

V1에서는 다음 흐름을 우선 검토한다.

```text
DOCX
→ Mammoth
→ HTML
→ ReverseMarkdown
→ 후처리
→ Markdown
```

후처리 단계에서 차량 문서에 자주 등장하는 특수 기호와 불필요한 공백을 정리한다.

Mammoth 변환 결과가 문서 구조를 충분히 보존하지 못하는 경우, `DocumentFormat.OpenXml`을 이용한 전용 변환 로직을 추가한다.

## 9.3 권장 프로젝트 구조

```text
VehicleKnowledgeManager.sln

src/
├─ VehicleKnowledgeManager.App/
│  ├─ Views/
│  ├─ ViewModels/
│  ├─ Controls/
│  ├─ Converters/
│  └─ App.xaml
│
├─ VehicleKnowledgeManager.Core/
│  ├─ Models/
│  ├─ Interfaces/
│  ├─ Services/
│  └─ Settings/
│
├─ VehicleKnowledgeManager.Infrastructure/
│  ├─ FileSystem/
│  ├─ Markdown/
│  ├─ Docx/
│  └─ Logging/
│
└─ VehicleKnowledgeManager.Tests/
   ├─ FileSystem/
   ├─ Markdown/
   └─ Docx/
```

---

# 10. 주요 모델

## 10.1 DocumentItem

```csharp
public sealed class DocumentItem
{
    public required string Name { get; init; }
    public required string FullPath { get; init; }
    public required string RelativePath { get; init; }
    public bool IsDirectory { get; init; }
    public List<DocumentItem> Children { get; init; } = [];
}
```

## 10.2 SearchResult

```csharp
public sealed class SearchResult
{
    public required string FileName { get; init; }
    public required string FullPath { get; init; }
    public required string RelativePath { get; init; }
    public required string PreviewText { get; init; }
    public int MatchCount { get; init; }
}
```

## 10.3 ConversionResult

```csharp
public sealed class ConversionResult
{
    public required string SourcePath { get; init; }
    public string? OutputPath { get; init; }
    public bool IsSuccess { get; init; }
    public string? Markdown { get; init; }
    public string? ErrorMessage { get; init; }
    public List<string> Warnings { get; init; } = [];
}
```

---

# 11. 주요 서비스 인터페이스

## 11.1 IDocumentRepository

```csharp
public interface IDocumentRepository
{
    Task<IReadOnlyList<DocumentItem>> LoadTreeAsync(
        string rootPath,
        CancellationToken cancellationToken = default);

    Task<string> ReadAsync(
        string filePath,
        CancellationToken cancellationToken = default);

    Task SaveAsync(
        string filePath,
        string content,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string filePath,
        CancellationToken cancellationToken = default);

    Task MoveAsync(
        string sourcePath,
        string destinationPath,
        CancellationToken cancellationToken = default);
}
```

## 11.2 IDocxToMarkdownConverter

```csharp
public interface IDocxToMarkdownConverter
{
    Task<ConversionResult> ConvertAsync(
        string sourcePath,
        CancellationToken cancellationToken = default);
}
```

## 11.3 ISearchService

```csharp
public interface ISearchService
{
    Task<IReadOnlyList<SearchResult>> SearchAsync(
        string rootPath,
        string keyword,
        CancellationToken cancellationToken = default);
}
```

---

# 12. 오류 처리

## 12.1 예상 오류

- 저장소 폴더가 존재하지 않음
- 파일 접근 권한 부족
- 파일이 다른 프로그램에서 사용 중
- DOCX 파일 손상
- 지원하지 않는 DOCX 요소
- 동일 이름 파일 존재
- Markdown 저장 실패
- 미리보기 렌더링 실패

## 12.2 처리 원칙

- 오류 원인을 한국어로 표시한다.
- 사용자 데이터가 손상되지 않도록 작업 전후 상태를 관리한다.
- 일괄 변환에서는 실패 파일만 기록하고 나머지 작업을 계속한다.
- 자세한 오류 내용은 로그 파일에 기록한다.

---

# 13. 로그

애플리케이션은 다음 작업을 로그로 남긴다.

- 앱 시작 및 종료
- 저장소 변경
- 파일 저장
- 파일 이동 및 삭제
- DOCX 변환 시작 및 결과
- 일괄 변환 결과
- 처리되지 않은 예외

로그에는 문서 전체 내용을 기록하지 않는다.

---

# 14. 단축키

| 단축키 | 기능 |
|---|---|
| `Ctrl + S` | 현재 문서 저장 |
| `Ctrl + N` | 새 문서 |
| `Ctrl + O` | 저장소 폴더 열기 |
| `Ctrl + F` | 현재 문서에서 찾기 |
| `Ctrl + Shift + F` | 전체 문서 검색 |
| `F5` | 저장소 새로고침 |
| `Ctrl + W` | 현재 문서 닫기 |

---

# 15. 우선순위

## Must Have

- 저장소 폴더 설정
- 폴더 및 Markdown 파일 탐색
- Markdown 열람
- Markdown 편집 및 저장
- Markdown 미리보기
- 새 문서 생성
- 전체 문서 검색
- DOCX 단일 가져오기
- DOCX 일괄 가져오기
- 기본 설정 저장
- 오류 처리 및 로그

## Should Have

- 파일 이동 및 이름 변경
- 휴지통 삭제
- 외부 프로그램으로 열기
- 최근 문서
- 자동 저장
- 문서 템플릿 선택

## Could Have

- 드래그 앤 드롭
- 탭 방식 다중 문서
- 이미지 추출
- 문서 즐겨찾기
- 태그
- 검색 필터
- 다크 모드
- 문서 변경 이력 보기

## Won't Have in V1

- AI 자동 요약
- 자동 차량 비교
- 웹 크롤링
- 고객 CRM
- 클라우드 동기화
- 사용자 계정

---

# 16. 개발 단계

## Phase 1: 파일 기반 Markdown 뷰어

- WPF 프로젝트 생성
- 저장소 폴더 선택
- 폴더 트리
- Markdown 파일 열기
- Markdown 편집 및 저장
- 미리보기

## Phase 2: 문서 관리

- 새 문서
- 새 폴더
- 이름 변경
- 이동
- 삭제
- 전체 검색
- 설정 저장

## Phase 3: DOCX 가져오기

- 단일 DOCX 변환
- 변환 미리보기
- 후처리 규칙
- 일괄 변환
- 변환 로그

## Phase 4: 사용성 개선

- 단축키
- 최근 문서
- 자동 저장
- 외부 프로그램으로 열기
- 오류 메시지 개선
- 배포 패키지

---

# 17. 완료 조건

V1은 다음 조건을 모두 만족할 때 완료된 것으로 판단한다.

1. 사용자가 저장소 폴더를 선택할 수 있다.
2. 폴더 내 Markdown 문서를 트리에서 확인할 수 있다.
3. Markdown 문서를 열고 수정하고 저장할 수 있다.
4. Markdown 미리보기를 볼 수 있다.
5. 저장소 전체에서 키워드를 검색할 수 있다.
6. DOCX 파일 하나를 Markdown으로 변환할 수 있다.
7. 여러 DOCX 파일을 일괄 변환할 수 있다.
8. 변환 결과를 저장하기 전에 확인할 수 있다.
9. 앱을 다시 실행해도 마지막 저장소 및 설정이 유지된다.
10. 일반적인 파일 오류로 앱 전체가 종료되지 않는다.

---

# 18. 향후 확장

## 18.1 차량 비교

두 개 이상의 Markdown 문서를 선택하여 공통 사양과 차이점을 비교한다.

초기에는 단순 텍스트 비교로 시작하고, 이후 구조화된 차량 데이터 모델을 도입할 수 있다.

## 18.2 구조화 데이터

Markdown 상단에 YAML Front Matter를 추가할 수 있다.

```yaml
---
manufacturer: 현대
model: 그랜저
fuel: hybrid
model_year: 2027
segment: 준대형 세단
---
```

## 18.3 AI 기능

- 차량 문서 요약
- 고객 설명 문구 생성
- 경쟁 차량 추천
- 옵션 차이 분석
- 문서 형식 자동 정리

AI 기능은 사용자가 명시적으로 실행할 때만 동작하도록 한다.

## 18.4 상담 프로그램 연동

기존 상담 관리 프로그램에서 VKM을 실행하거나 특정 차량 문서를 열 수 있도록 한다.

예시:

```text
VehicleKnowledgeManager.exe
    --open "현대/그랜저 하이브리드.md"
```

VKM과 상담 프로그램은 별도 애플리케이션으로 유지한다.

---

# 19. 핵심 설계 원칙

1. Markdown이 최종 원본이다.
2. DOCX는 가져오기 형식일 뿐이다.
3. 데이터는 일반 파일로 남아야 한다.
4. 프로그램 없이도 파일을 열고 수정할 수 있어야 한다.
5. V1은 차량 정보의 검색, 열람, 수정, 변환에 집중한다.
6. 비교, AI, 크롤링은 V1 완료 이후에 추가한다.
7. 상담 CRM과 차량 지식 관리 시스템은 분리한다.
