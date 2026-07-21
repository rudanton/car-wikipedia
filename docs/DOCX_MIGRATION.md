# DOCX Migration

원본 위치: `C:\Users\User\Desktop\차 데이터`

원칙:

- 원본 `.docx` 파일은 수정하거나 삭제하지 않는다.
- 변환 결과는 Markdown으로 저장한다.
- 기본 변환은 C# 코드 기반 파이프라인으로 처리한다.
- AI는 변환 결과 검수, 누락 확인, 보수적 문장 정리 같은 보조 역할로만 사용한다.
- 차량 문서는 기본적으로 `vehicles/{manufacturer}/{vehicle-name}.md` 구조를 따른다.
- 제조사나 모델 구분이 애매한 파일은 변환 전에 확인한다.
- 한 번에 모두 처리하기보다, 파일 하나씩 변환하고 내용을 확인한다.

## 변환 대상

- [ ] `gv80.docx`
- [ ] `k8.docx`
- [ ] `그랜저26 하브.docx`
- [ ] `그랜저27 하브.docx`
- [ ] `레이.docx`
- [ ] `셀토스.docx`
- [ ] `스포티지, 쏘렌토.docx` -> `스포티지.md`, `쏘렌토.md`로 분리
- [ ] `아이오닉 6.docx`
- [ ] `카니발.docx`
- [ ] `투싼.docx`
- [ ] `팰리세이드.docx`

## 권장 변환 흐름

1. DOCX 하나를 선택한다.
2. 문서 내용을 추출한다.
3. Markdown 초안을 만든다.
4. 차량 문서 표준 템플릿에 맞게 정리한다.
5. 원본 내용이 빠지지 않았는지 확인한다.
6. `vehicles/` 아래 적절한 제조사 폴더에 저장한다.
7. 이 체크리스트에서 완료 표시한다.

## 현재 변환 도구

기본 변환 도구:

```text
tools/CarWikipedia.DocxMigrator
```

단일 파일 변환 예:

```powershell
dotnet run --project .\tools\CarWikipedia.DocxMigrator -- --source "C:\Users\User\Desktop\차 데이터\gv80.docx" --output ".\vehicles\_converted"
```

저장 전 미리보기 예:

```powershell
dotnet run --project .\tools\CarWikipedia.DocxMigrator -- --source "C:\Users\User\Desktop\차 데이터\gv80.docx" --preview
```

폴더 일괄 변환 예:

```powershell
dotnet run --project .\tools\CarWikipedia.DocxMigrator -- --source "C:\Users\User\Desktop\차 데이터" --output ".\vehicles\_converted" --split-sportage-sorento
```

현재 도구는 DOCX 내부 XML을 직접 읽는 기본 변환기다. 제목, 문단, 글머리표, 표를 Markdown으로 추출하고, 차량 문서용 기본 cleanup을 적용한다.

이 결과는 최종 차량 문서가 아니라 검수 전 초안이다. 변환 후에는 차량별 표준 템플릿에 맞춰 정리해야 한다.

## 확인 포인트

- 차량명과 제조사가 맞는가?
- 연식이나 세대 구분이 파일명 또는 본문에 남아 있는가?
- 가격대, 핵심 옵션, 트림, 상담 포인트가 빠르게 보이는가?
- 특수 bullet, 화살표, 과도한 빈 줄이 정리되었는가?
- 원본에서 의미 있는 내용이 누락되지 않았는가?
