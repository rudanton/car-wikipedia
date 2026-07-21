# Car Wikipedia SRS

## 1. Document Information

- Product name: Car Wikipedia
- Repository name: `car-wikipedia`
- Solution name: `CarWikipedia.sln`
- Executable name: `CarWikipedia.exe`
- Root namespace: `CarWikipedia`
- Target platform: Windows Desktop
- Primary technology: C# / WPF / MVVM
- Primary storage format: Markdown

## 2. Product Purpose

Car Wikipedia is a local Windows desktop application for long-term rental car sales consultations.

The application helps a salesperson quickly search and confirm vehicle prices, trims, major convenience and safety features, selectable options, and sales talking points during customer conversations.

Car Wikipedia is not intended to be a complete automotive specification encyclopedia. Low-consultation-use technical details such as engine structure, maximum horsepower, torque, curb weight, detailed drivetrain specifications, and similar engineering data are not required management targets unless the user explicitly chooses to record them.

## 3. Core Principle

Markdown is the storage and editing format, but the product must not be treated primarily as a Markdown editor.

The default experience is read-first vehicle information browsing. Markdown editing exists as a secondary mode for maintaining the underlying vehicle documents.

DOCX files are import sources only. Markdown files are the long-term editable source of truth.

## 4. Goals

- Allow the user to manage vehicle information as plain Markdown files.
- Let the user quickly find a vehicle by manufacturer, vehicle name, trim name, or option keyword.
- Present vehicle information in a readable sales consultation view by default.
- Support editing the underlying Markdown when needed.
- Support importing existing DOCX vehicle documents into Markdown.
- Preserve original DOCX files during import.
- Keep the file structure transparent and compatible with Git, VS Code, Obsidian, and AI tools.

## 5. Non-Goals for V1

- Full automatic vehicle comparison engine.
- Full vehicle database with all public technical specifications.
- Crawling or scraping manufacturer pages, Danawa, or other external websites.
- AI-powered summarization or automatic document restructuring.
- Perfect visual reproduction of Word document styling.
- Proprietary binary database as the only storage format.
- CRM, customer contract, quote, or payment management.

## 6. Target User

The primary user is a long-term rental car salesperson who repeatedly checks vehicle trims, options, and sales talking points during customer consultations.

The user needs fast retrieval and clear presentation more than exhaustive technical detail.

## 7. Recommended Repository Structure

```text
car-wikipedia/
  CarWikipedia.sln
  src/
    CarWikipedia/
      CarWikipedia.csproj
  vehicles/
    현대/
      그랜저_하이브리드.md
      쏘나타.md
    기아/
      K5.md
      스포티지.md
    제네시스/
      GV70.md
  imports/
    original-docx/
  docs/
    SRS.md
    TODO.md
```

The `vehicles` folder is the primary knowledge base. The `imports/original-docx` folder may be used to store untouched source DOCX files if the user wants to keep imported originals inside the repository.

## 8. Vehicle Document Convention

Each vehicle should normally be stored as one Markdown file.

Recommended path format:

```text
vehicles/{manufacturer}/{vehicle-name}.md
```

Examples:

```text
vehicles/현대/그랜저_하이브리드.md
vehicles/기아/스포티지.md
vehicles/제네시스/GV70.md
```

The application should not require a hidden database to understand the folder. It should be able to scan Markdown files from the selected repository folder.

## 9. Standard Vehicle Markdown Template

New vehicle documents should use the following structure.

```md
# 차량명

## 기본 정보

- 제조사:
- 연식:
- 차급:
- 연료:
- 가격대:

## 핵심 옵션

- 통풍시트:
- 메모리시트:
- 어라운드뷰:
- HUD:
- 스마트 크루즈:
- 전동 트렁크:
- 2열 편의사양:

## 트림

### 트림명

기본 사양

-

선택 옵션

-

상위 트림 추가 사양

-

### 트림명

기본 사양

-

선택 옵션

-

상위 트림 추가 사양

-

## 상담 포인트

### 장점

-

### 아쉬운 점

-

### 추천 고객

-

### 비교 차량

-
```

The template exists to support future search, filtering, and comparison features. The application should not block users from adding extra sections.

## 10. Main UX

### 10.1 Default Layout

The default screen should prioritize reading vehicle information.

```text
Manufacturer / Vehicle List | Vehicle Information View
                             | Optional Details / Search Results
```

The central experience is not a raw Markdown editor. It is a rendered vehicle information view designed for fast scanning during consultation.

### 10.2 Read-First Vehicle View

The vehicle view should show:

- Vehicle name
- Basic information
- Key options
- Trim sections
- Sales points
- Recently viewed or favorite state if supported

The view should make common sales questions easy to answer:

- Does this vehicle have HUD?
- Which trim adds surround view?
- Is memory seat included?
- Is ventilation available?
- Is smart cruise included?
- Which options are selectable?

### 10.3 Edit Mode

Markdown editing is available as a secondary mode.

Expected behavior:

- User opens a vehicle.
- Application shows the read-first view by default.
- User clicks Edit.
- Application shows the Markdown editor.
- User saves changes.
- Application updates the read-first view.

The editor should preserve Markdown content and avoid rewriting unrelated formatting.

## 11. Functional Requirements

### FR-1 Repository Selection

The application shall allow the user to select a local repository or vehicle document folder.

The selected folder shall be remembered between launches.

### FR-2 Vehicle Tree

The application shall scan Markdown files and display them grouped by folder, usually by manufacturer.

The tree should support at least:

- Manufacturer folders
- Vehicle Markdown files
- Refresh
- Open selected vehicle

### FR-3 Vehicle Read View

The application shall render selected Markdown files into a readable vehicle information view.

The view shall be the default mode for selected vehicles.

### FR-4 Markdown Edit Mode

The application shall allow the user to edit the underlying Markdown file.

The application shall support:

- Open
- Edit
- Save
- Unsaved change indicator
- Confirm before losing unsaved changes

### FR-5 Search

The application shall support search across vehicle Markdown files.

The search should support:

- Vehicle name search
- Trim name search
- Option keyword search
- Full text search

Search results should show enough context for the user to decide which vehicle to open.

### FR-6 Recent Vehicles

The application should keep a list of recently opened vehicles.

Recent vehicles should be accessible without browsing the full tree again.

### FR-7 Favorites

The application should allow the user to mark frequently used vehicles as favorites.

Favorites should be visible in an easy-to-access area.

### FR-8 New Vehicle Document

The application shall allow creating a new vehicle Markdown file from the standard template.

The user should be able to choose manufacturer and vehicle name.

### FR-9 DOCX Import

The application shall support importing one or more DOCX files and converting them to Markdown.

DOCX import should:

- Preserve headings when possible.
- Preserve bullet lists when possible.
- Preserve tables when practical.
- Preserve plain text content.
- Avoid modifying or deleting the original DOCX.
- Save the converted Markdown into the selected vehicle repository.

### FR-10 Vehicle-Specific DOCX Cleanup

After generic DOCX conversion, the application should apply vehicle-document-specific cleanup rules.

Examples:

- Normalize unusual bullet symbols such as ``.
- Remove excessive blank lines.
- Normalize trim heading spacing.
- Convert repeated arrow-style option lines into Markdown bullets.
- Preserve Korean text correctly in UTF-8.

The cleanup rules should be conservative and should not remove content automatically unless the rule is clearly safe.

### FR-11 Settings

The application shall remember:

- Last selected repository folder
- Window size and position if practical
- Recently opened vehicles
- Favorite vehicles
- Import output preference if supported

### FR-12 Logging

The application should log import errors and file read/write errors in a user-understandable way.

## 12. Data Storage

Markdown files are the primary source of truth.

The application may use a small settings file for local preferences, but vehicle content should remain in normal Markdown files.

Recommended settings file:

```text
%AppData%/CarWikipedia/settings.json
```

The application should not require a database server.

## 13. DOCX Conversion Approach

Recommended implementation path:

- Use a C# console/service layer or WPF service class.
- Convert DOCX to HTML using a library such as Mammoth.
- Convert HTML to Markdown using a library such as ReverseMarkdown.
- Apply Car Wikipedia cleanup rules.
- Save the generated Markdown as UTF-8.

Possible NuGet packages:

- `Mammoth`
- `ReverseMarkdown`
- `DocumentFormat.OpenXml`

The first implementation should prioritize reliable content extraction over perfect formatting.

## 14. Suggested Architecture

```text
CarWikipedia
  App.xaml
  MainWindow.xaml

  ViewModels/
    MainViewModel.cs
    VehicleTreeViewModel.cs
    VehicleDocumentViewModel.cs
    SearchViewModel.cs

  Views/
    MainWindow.xaml
    VehicleReadView.xaml
    MarkdownEditView.xaml
    SearchResultsView.xaml

  Services/
    RepositoryService.cs
    MarkdownDocumentService.cs
    DocxImportService.cs
    SearchService.cs
    SettingsService.cs

  Models/
    VehicleDocument.cs
    VehicleSearchResult.cs
    AppSettings.cs
```

## 15. V1 Scope

V1 should focus on making the local knowledge base useful before adding advanced automation.

V1 includes:

- Repository folder selection
- Vehicle Markdown tree
- Read-first vehicle information view
- Markdown edit mode
- Save and unsaved-change handling
- Standard vehicle document template
- Vehicle name, trim, option, and full-text search
- Recent vehicles
- Favorites
- DOCX single-file import
- DOCX batch import
- Vehicle-specific import cleanup
- Original DOCX preservation

V1 does not include:

- Automatic web crawling
- AI summarization
- Automatic vehicle comparison
- Customer management
- Quote generation

## 16. Future Scope

Future versions may add:

- Structured option extraction from Markdown.
- Automatic side-by-side vehicle comparison.
- AI-assisted summary generation.
- AI-assisted sales talk generation.
- Import from manufacturer PDF price lists.
- Export to PDF or DOCX.
- Integration with a separate consultation or CRM program.

## 17. Implementation Guidance for Codex

Do not treat this application primarily as a Markdown editor.

Car Wikipedia is a read-first vehicle sales knowledge application. Markdown is the underlying storage and editing format, but the main user experience must prioritize quickly finding and reading vehicle, trim, option, and sales-point information.

Keep the implementation conservative:

- Preserve plain Markdown files.
- Avoid introducing a custom database in V1.
- Keep UI focused and fast.
- Do not add unrelated CRM features.
- Do not scrape websites in V1.
- Do not overbuild DOCX conversion beyond useful Markdown extraction.

