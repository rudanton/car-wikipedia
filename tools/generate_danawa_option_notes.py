from __future__ import annotations

import html
import re
import subprocess
import sys
from dataclasses import dataclass, field
from pathlib import Path
from typing import Iterable

import openpyxl


REPO_ROOT = Path(__file__).resolve().parents[1]
LINEUP_XLSX = Path(r"C:\Users\User\Desktop\차 데이터\자동차 라인업.xlsx")
OUTPUT_ROOT = REPO_ROOT / "vehicles" / "_generated"

BRANDS = {
    "현대": 303,
    "기아": 307,
    "제네시스": 304,
}

NAME_ALIASES = {
    "캐스퍼EV": "캐스퍼 일렉트릭",
    "코나EV": "코나 일렉트릭",
    "아이오닉5": "아이오닉 5",
    "아이오닉5N": "아이오닉 5 N",
    "아이오닉6": "아이오닉 6",
    "아이오닉6N": "아이오닉 6 N",
    "아이오닉9": "아이오닉 9",
    "아반떼N": "아반떼 N",
    "G80 electric": "Electrified G80",
    "GV70 electric": "Electrified GV70",
}

MODEL_PREFIXES = (
    "더 뉴 ",
    "디 올 뉴 ",
    "올 뉴 ",
    "The new ",
    "The New ",
)

OPTION_KEYWORDS = [
    ("HUD", ["HUD", "헤드업 디스플레이"]),
    ("서라운드 뷰", ["서라운드 뷰", "서라운드뷰", "SVM"]),
    ("후측방 모니터", ["후측방 모니터", "BVM"]),
    ("후측방 충돌방지 보조", ["후측방 충돌방지", "후측방 충돌 경고"]),
    ("후방 교차 충돌방지 보조", ["후방 교차 충돌방지", "후방 교차 충돌 경고"]),
    ("전방 충돌방지 보조", ["전방 충돌방지"]),
    ("고속도로 주행 보조", ["고속도로 주행 보조", "HDA"]),
    ("스마트 크루즈 컨트롤", ["스마트 크루즈", "어댑티브 크루즈"]),
    ("내비게이션 기반 스마트 크루즈 컨트롤", ["내비게이션 기반 스마트 크루즈"]),
    ("차로 유지 보조", ["차로 유지", "차로 중앙 유지", "차로 이탈방지"]),
    ("원격 스마트 주차 보조", ["원격 스마트 주차", "원격 주차"]),
    ("후방 주차 충돌방지 보조", ["후방 주차 충돌방지"]),
    ("전방/측방/후방 주차 거리 경고", ["주차 거리 경고", "전방 주차", "후방 주차", "측방 주차"]),
    ("운전석 통풍시트", ["운전석 통풍"]),
    ("동승석 통풍시트", ["동승석 통풍"]),
    ("1열 통풍시트", ["1열 통풍", "앞좌석 통풍"]),
    ("2열 통풍시트", ["2열 통풍", "뒷좌석 통풍"]),
    ("1열 열선시트", ["1열 열선", "앞좌석 열선"]),
    ("2열 열선시트", ["2열 열선", "뒷좌석 열선"]),
    ("열선 스티어링 휠", ["열선 스티어링", "스티어링 휠(열선", "핸들 열선"]),
    ("운전석 전동시트", ["운전석 전동"]),
    ("동승석 전동시트", ["동승석 전동"]),
    ("메모리 시트", ["메모리 시트", "운전석 자세 메모리", "IMS"]),
    ("이지 억세스", ["이지 억세스", "스마트 자세제어"]),
    ("2열 리클라이닝", ["2열 리클라이닝", "뒷좌석 리클라이닝"]),
    ("2열 전동시트", ["2열 전동", "뒷좌석 전동"]),
    ("2열 전동식 도어커튼", ["2열 전동식 도어커튼", "뒷좌석 전동식 도어커튼", "전동식 도어커튼"]),
    ("스마트 파워 트렁크", ["스마트 파워 트렁크", "전동식 트렁크", "파워 테일게이트", "전동 테일게이트"]),
    ("버튼시동 & 스마트키", ["버튼시동", "스마트키", "디지털 키"]),
    ("하이패스", ["하이패스"]),
    ("풀오토 에어컨", ["풀오토 에어컨", "자동 온도조절"]),
    ("후방 모니터", ["후방 모니터", "후방 카메라"]),
    ("빌트인 캠", ["빌트인 캠", "내장 블랙박스"]),
    ("내비게이션", ["내비게이션", "인포테인먼트"]),
    ("무선 충전", ["무선 충전"]),
    ("선루프", ["선루프", "썬루프", "파노라마"]),
    ("프리뷰 전자제어 서스펜션", ["프리뷰 전자제어 서스펜션", "전자제어 서스펜션"]),
    ("BOSE 프리미엄 사운드", ["BOSE"]),
    ("렉시콘 프리미엄 사운드", ["렉시콘"]),
    ("앰비언트 무드램프", ["앰비언트"]),
    ("V2L", ["V2L"]),
]


@dataclass
class ModelMatch:
    brand: str
    target_name: str
    danawa_name: str
    model_id: str


@dataclass
class TrimOptions:
    name: str
    basic_options: list[str] = field(default_factory=list)
    extra_options: list[str] = field(default_factory=list)


def fetch(url: str) -> str:
    result = subprocess.run(
        ["curl.exe", "-s", "-L", url],
        check=True,
        capture_output=True,
    )
    return result.stdout.decode("utf-8", errors="replace")


def normalize_name(value: str) -> str:
    value = NAME_ALIASES.get(value.strip(), value.strip())
    for prefix in MODEL_PREFIXES:
        if value.startswith(prefix):
            value = value[len(prefix) :]
    value = re.sub(r"\([^)]*\)", "", value)
    value = value.replace("일렉트릭", "electric")
    value = value.replace("전동화 모델", "electric")
    value = value.replace(" ", "")
    value = value.lower()
    return value


def extract_targets() -> dict[str, list[str]]:
    workbook = openpyxl.load_workbook(LINEUP_XLSX, data_only=True)
    sheet = workbook.active
    headers = [cell.value for cell in sheet[1]]
    targets: dict[str, list[str]] = {brand: [] for brand in BRANDS}

    for row in sheet.iter_rows(min_row=2, values_only=True):
        for brand in BRANDS:
            cell = row[headers.index(brand)]
            if not cell:
                continue
            for raw_line in str(cell).splitlines():
                line = raw_line.strip()
                if not line or line.startswith("ㄴ") or "단종" in line:
                    continue
                name = re.sub(r"\([^)]*\)", "", line).strip()
                name = re.sub(r"\s+", " ", name)
                name = NAME_ALIASES.get(name, name)
                if name and name not in targets[brand]:
                    targets[brand].append(name)

    return targets


def extract_current_models(brand: str, brand_id: int) -> dict[str, tuple[str, str]]:
    page = fetch(f"https://auto.danawa.com/auto/?Brand={brand_id}&Work=brand")
    marker = f"{brand}  판매중인 신차"
    start = page.find(marker)
    if start < 0:
        start = page.find("판매중인 신차")
    end = page.find("차종별 라인업", start)
    section = page[start:end if end > start else len(page)]

    models: dict[str, tuple[str, str]] = {}
    pattern = re.compile(
        r"<li\s+code='(?P<id>\d+)'[^>]*>.*?<img\s+alt='(?P<name>[^']+)'",
        re.DOTALL,
    )
    for match in pattern.finditer(section):
        name = html.unescape(match.group("name")).strip()
        model_id = match.group("id")
        models[normalize_name(name)] = (name, model_id)

    return models


def match_targets(targets: dict[str, list[str]]) -> tuple[list[ModelMatch], list[str]]:
    matches: list[ModelMatch] = []
    missing: list[str] = []

    for brand, names in targets.items():
        current_models = extract_current_models(brand, BRANDS[brand])
        for target_name in names:
            key = normalize_name(target_name)
            found = current_models.get(key)
            if found is None:
                for model_key, candidate in current_models.items():
                    if key in model_key or model_key in key:
                        found = candidate
                        break
            if found is None:
                missing.append(f"{brand}/{target_name}")
                continue
            danawa_name, model_id = found
            matches.append(ModelMatch(brand, target_name, danawa_name, model_id))

    return matches, missing


def latest_lineup_info(model_id: str) -> tuple[str, list[str]] | None:
    page = fetch(f"https://auto.danawa.com/auto/?Work=model&Model={model_id}")
    match = re.search(r"button_updown\s+on'[^>]*data-lineup='(?P<lineup>\d+)'", page)
    if not match:
        match = re.search(r"data-lineup='(?P<lineup>\d+)'", page)
    if not match:
        return None

    lineup_id = match.group("lineup")
    trims = [
        clean_trim_name(html.unescape(trim_match.group("trim")))
        for trim_match in re.finditer(
            rf"lineup='{lineup_id}'[^>]*trimNameT='(?P<trim>[^']+)'",
            page,
        )
    ]
    return lineup_id, unique(trims)


def plain_text_from_html(page: str) -> str:
    page = re.sub(r"<br\s*/?>", "\n", page, flags=re.I)
    page = re.sub(r"</(tr|li|p|div|h[1-6]|td|th)>", "\n", page, flags=re.I)
    text = re.sub(r"<[^>]+>", " ", page)
    text = html.unescape(text)
    text = re.sub(r"[ \t\r\f\v]+", " ", text)
    text = re.sub(r"\n\s+", "\n", text)
    text = re.sub(r"\n{3,}", "\n\n", text)
    return text


def clean_trim_name(value: str) -> str:
    value = value.strip()
    for drive in ["2WD", "4WD", "AWD", "RWD", "FWD"]:
        value = value.replace(f"{drive}A/T", drive)
    value = re.sub(r"\s+", " ", value)
    value = re.sub(r"\s*A/T$", "", value)
    return value.strip() or "확인 필요"


def normalize_trim_name(value: str) -> str:
    return clean_trim_name(value).replace(" ", "").lower()


def option_hits(text: str) -> list[str]:
    found: list[str] = []
    for label, needles in OPTION_KEYWORDS:
        if any(needle in text for needle in needles):
            found.append(label)
    return found


def unique(values: Iterable[str]) -> list[str]:
    result: list[str] = []
    for value in values:
        if value and value not in result:
            result.append(value)
    return result


def has_price_context(following: list[str]) -> bool:
    joined = " ".join(following[:10])
    return "차량가격" in joined or "세제혜택 적용 전" in joined or "세제혜택 적용 후" in joined


def looks_like_trim_header(line: str, following: list[str]) -> bool:
    if not line or len(line) > 45:
        return False
    if line == "세부모델":
        return False
    if re.match(r"^\d", line) or re.match(r"^[\d,]+$", line):
        return False
    if line.startswith(("•", "▶", "-", "※")):
        return False
    if any(token in line for token in ["상세정보", "선택품목", "옵션", "가격", "합계", "견적", "판매", "기본 품목"]):
        return False
    return has_price_context(following)


def split_trim_blocks(text: str, expected_trims: list[str] | None = None) -> list[tuple[str, str]]:
    table_marker = "세부모델"
    start = text.find(table_marker)
    if start >= 0:
        text = text[start:]

    lines = [line.strip() for line in text.splitlines()]
    headers: list[tuple[int, str]] = []
    drive_labels = {"2WD", "4WD", "AWD", "RWD", "FWD", "A/T"}
    expected_lookup = {normalize_trim_name(trim): trim for trim in expected_trims or []}

    index = 0
    while index < len(lines):
        line = lines[index]
        if expected_lookup:
            next_line = lines[index + 1] if index + 1 < len(lines) else ""
            candidates = [line]
            if next_line in drive_labels:
                candidates.append(f"{line} {next_line}")
            elif next_line and len(next_line) <= 20 and not has_price_context([next_line]):
                candidates.append(f"{line} {next_line}")
            matched_trim = next(
                (expected_lookup[normalize_trim_name(candidate)] for candidate in candidates if normalize_trim_name(candidate) in expected_lookup),
                None,
            )
            if matched_trim and has_price_context(lines[index + 1 : index + 16]):
                headers.append((index, matched_trim))
                index += 8
                continue
            index += 1
            continue

        if looks_like_trim_header(line, lines[index + 1 : index + 16]):
            trim_name = line
            next_line = lines[index + 1] if index + 1 < len(lines) else ""
            if next_line in drive_labels:
                trim_name = f"{trim_name} {next_line}"
            headers.append((index, clean_trim_name(trim_name)))
            index += 8
            continue
        index += 1

    blocks: list[tuple[str, str]] = []
    for header_index, (line_index, trim_name) in enumerate(headers):
        block_end_line = headers[header_index + 1][0] if header_index + 1 < len(headers) else len(lines)
        blocks.append((trim_name, "\n".join(lines[line_index:block_end_line])))
    return blocks


def extract_package_options(text: str) -> list[str]:
    start = text.find("패키지 선택품목")
    if start < 0:
        start = text.find("선택 품목")
    if start < 0:
        return []
    package_text = text[start:]
    cut_markers = ["다나와 자동차에서", "책임의 한계", "판매가격"]
    for marker in cut_markers:
        cut = package_text.find(marker)
        if cut > 0:
            package_text = package_text[:cut]
    return option_hits(package_text)


def extract_options(model: ModelMatch, lineup_id: str, expected_trims: list[str]) -> list[TrimOptions]:
    page = fetch(
        f"https://auto.danawa.com/auto/modelPopup.php?Lineup={lineup_id}&Trims=&Type=price&pcUse=y"
    )
    text = plain_text_from_html(page)
    package_options = extract_package_options(text)
    trims: list[TrimOptions] = []

    for trim_name, block in split_trim_blocks(text, expected_trims):
        basic = option_hits(block)
        extras = [option for option in package_options if option not in basic]
        trims.append(TrimOptions(trim_name, unique(basic) or ["확인 필요"], unique(extras) or ["확인 필요"]))

    if trims:
        return trims

    item_page = fetch(
        f"https://auto.danawa.com/auto/modelPopup.php?Lineup={lineup_id}&Trims=&Type=item&pcUse=y"
    )
    item_text = plain_text_from_html(item_page)
    feature_start = item_text.find("트림별 주요 특징")
    option_start = item_text.find("인기 사양/옵션")
    feature_text = item_text[feature_start:option_start if option_start > feature_start else len(item_text)]
    options = option_hits(item_text[option_start:]) if option_start >= 0 else []
    trim_names = [
        clean_trim_name(match.group(1))
        for match in re.finditer(r"\n\s*([^\n]{1,35})\s+A/T\s*\n", feature_text)
    ]
    return [
        TrimOptions(trim_name, ["확인 필요"], unique(options) or ["확인 필요"])
        for trim_name in unique(trim_names)
    ] or [TrimOptions("확인 필요", ["확인 필요"], unique(options) or ["확인 필요"])]


def markdown_for(model: ModelMatch, trims: list[TrimOptions]) -> str:
    lines = [f"### {model.target_name}", ""]
    for trim in trims:
        lines.extend([f"# {trim.name}", "기본 옵션"])
        lines.extend(f"- {option}" for option in trim.basic_options)
        lines.extend(["", "추가 옵션"])
        lines.extend(f"- {option}" for option in trim.extra_options)
        lines.append("")
    return "\n".join(lines).strip() + "\n"


def write_markdown(model: ModelMatch, trims: list[TrimOptions]) -> Path:
    directory = OUTPUT_ROOT / model.brand
    directory.mkdir(parents=True, exist_ok=True)
    path = directory / f"{model.target_name}.md"
    path.write_text(markdown_for(model, trims), encoding="utf-8")
    return path


def main() -> int:
    targets = extract_targets()
    matches, missing = match_targets(targets)
    failures: list[str] = []
    created: list[Path] = []

    for model in matches:
        try:
            lineup_info = latest_lineup_info(model.model_id)
            if not lineup_info:
                failures.append(f"{model.brand}/{model.target_name}: Lineup 없음")
                continue
            lineup, expected_trims = lineup_info
            trims = extract_options(model, lineup, expected_trims)
            created.append(write_markdown(model, trims))
            print(f"OK {model.brand}/{model.target_name} <- {model.danawa_name} Lineup={lineup}")
        except Exception as exc:  # noqa: BLE001 - batch collection should keep going
            failures.append(f"{model.brand}/{model.target_name}: {exc}")

    if missing:
        print("\nMISSING")
        print("\n".join(missing))
    if failures:
        print("\nFAILURES")
        print("\n".join(failures))

    print(f"\nGenerated or kept {len(created)} files under {OUTPUT_ROOT}")
    return 1 if failures else 0


if __name__ == "__main__":
    sys.exit(main())
