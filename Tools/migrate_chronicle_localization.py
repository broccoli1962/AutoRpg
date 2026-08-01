#!/usr/bin/env python3
"""Migrate chronicle JSON to localization keys and emit Google Sheets rows."""
from __future__ import annotations

import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
CHRONICLE_DIR = ROOT / "Assets" / "GameResource" / "Data" / "Chronicle"
OUTPUT = ROOT / ".cursor" / "localization_sheet_rows.json"

# UI / system keys (ko, en, ja)
STATIC_ROWS = [
    ("ui.common.close", "닫기", "Close", "閉じる"),
    ("ui.common.confirm", "확인", "Confirm", "確認"),
    ("ui.hud.dispatch", "파견", "Dispatch", "派遣"),
    ("ui.hud.enhance", "강화", "Enhance", "強化"),
    ("ui.hud.summon", "소환", "Summon", "召喚"),
    ("ui.hud.return", "귀환", "Return", "帰還"),
    ("shop.title", "상점", "Shop", "ショップ"),
    ("shop.restore", "구매 복원", "Restore Purchases", "購入を復元"),
    ("shop.close", "닫기", "Close", "閉じる"),
    ("shop.sold_out", " (품절)", " (Sold Out)", " (売切)"),
    ("shop.product.starter_pack", "성장 스타터 패키지", "Starter Growth Pack", "成長スターターパック"),
    ("shop.product.abyss_stone_1", "심연석 ×300", "Abyss Stone ×300", "深淵石 ×300"),
    ("shop.product.abyss_stone_2", "심연석 ×1,100", "Abyss Stone ×1,100", "深淵石 ×1,100"),
    ("shop.product.abyss_stone_3", "심연석 ×3,500", "Abyss Stone ×3,500", "深淵石 ×3,500"),
    ("shop.product.abyss_stone_4", "심연석 ×12,500", "Abyss Stone ×12,500", "深淵石 ×12,500"),
    ("shop.product.monthly_contract", "월간 심연 계약", "Monthly Abyss Contract", "月間深淵契約"),
    ("shop.product.season_pass", "시즌 패스", "Season Pass", "シーズンパス"),
    ("shop.product.ad_removal", "광고 제거", "Ad Removal", "広告削除"),
    ("shop.product.growth_pack_50", "성장 패키지 (50층)", "Growth Pack (Floor 50)", "成長パック (50階)"),
    ("shop.product.growth_pack_100", "성장 패키지 (100층)", "Growth Pack (Floor 100)", "成長パック (100階)"),
    ("save_conflict.title", "세이브 충돌", "Save Conflict", "セーブ競合"),
    ("save_conflict.local", "로컬 세이브", "Local Save", "ローカルセーブ"),
    ("save_conflict.cloud", "클라우드 세이브", "Cloud Save", "クラウドセーブ"),
    ("save_conflict.use_local", "로컬 사용", "Use Local Save", "ローカルを使用"),
    ("save_conflict.use_cloud", "클라우드 사용", "Use Cloud Save", "クラウドを使用"),
    ("tutorial.step.first_dispatch", "첫 파견을 시작하세요", "Start your first dispatch", "最初の派遣を開始してください"),
    ("tutorial.step.first_kill", "적을 처치하는 모습을 관찰하세요", "Watch your party defeat enemies", "敵を倒す様子を観察してください"),
    ("tutorial.step.first_enhance", "장비를 강화해 보세요", "Try enhancing equipment", "装備を強化してみましょう"),
    ("tutorial.step.first_equip", "장비를 장착하세요", "Equip your gear", "装備を装着してください"),
    ("tutorial.step.first_summon", "소환으로 동료를 영입하세요", "Recruit allies through summoning", "召喚で仲間を獲得しましょう"),
    ("tutorial.step.first_offline", "오프라인 보상을 수령하세요", "Claim your offline rewards", "オフライン報酬を受け取ってください"),
    ("format.duration.days_hours", "{0}일 {1}시간", "{0}d {1}h", "{0}日 {1}時間"),
    ("format.duration.hours_minutes", "{0}시간 {1}분", "{0}h {1}m", "{0}時間 {1}分"),
    ("format.duration.minutes", "{0}분", "{0}m", "{0}分"),
    ("chronicle.fallback.default", "탐험대는 조용히 전진한다.", "The expedition quietly presses forward.", "探索隊は静かに前進する。"),
    ("chronicle.fallback.intro", "탐험대는", "The expedition", "探索隊は"),
    ("chronicle.fallback.action", "조용히", "quietly", "静かに"),
    ("chronicle.fallback.result", "전진한다", "presses forward", "前進する"),
    ("chronicle.fallback.afterglow", ".", ".", "。"),
]

EN_MAP = {
    "탐험대는": "The expedition",
    "일행은": "The party",
    "파티는": "The party",
    "선두의 {character}는": "Lead {character}",
    "{character}는": "{character}",
    "어둠 속에서": "In the darkness",
    "조용히": "quietly",
    "천천히": "slowly",
    "과감히": "boldly",
    "전진한다": "presses forward",
    "나아간다": "advances",
    "멈춘다": "halts",
    "후퇴한다": "falls back",
    ".": ".",
}


def slugify(text: str) -> str:
    slug = re.sub(r"[^a-zA-Z0-9]+", "_", text.strip().lower())
    return slug.strip("_")[:48] or "line"


def translate_en(ko: str) -> str:
    if ko in EN_MAP:
        return EN_MAP[ko]
    if ko.startswith("Intro ") or ko.startswith("Action "):
        return ko.replace("샘플", "sample")
    return ko


def translate_ja(ko: str) -> str:
    table = {
        "탐험대는": "探索隊は",
        "일행은": "一行は",
        "파티는": "パーティは",
        "조용히": "静かに",
        "천천히": "ゆっくり",
        "과감히": "大胆に",
        "전진한다": "前進する",
        "나아간다": "進む",
        "멈춘다": "止まる",
        "후퇴한다": "後退する",
        ".": "。",
    }
    if ko in table:
        return table[ko]
    if "{character}" in ko:
        return ko.replace("는", "は")
    return translate_en(ko)


def migrate_chronicle() -> dict[str, tuple[str, str, str]]:
    rows: dict[str, tuple[str, str, str]] = {}

    for path in sorted(CHRONICLE_DIR.glob("*.json")):
        data = json.loads(path.read_text(encoding="utf-8"))
        event_type = data.get("eventType") or path.stem
        slots = data.get("slots") or {}
        changed = False

        for slot_name, entries in slots.items():
            for index, entry in enumerate(entries, start=1):
                ko = entry.get("text") or entry.get("key") or ""
                if not ko:
                    continue

                key = entry.get("key")
                if not key:
                    key = f"chronicle.{event_type}.{slot_name.lower()}.{index:03d}"
                    entry["key"] = key
                    entry.pop("text", None)
                    changed = True
                elif "text" in entry:
                    ko = entry["text"]
                    entry.pop("text", None)
                    changed = True

                if key not in rows:
                    rows[key] = (ko, translate_en(ko), translate_ja(ko))

        if changed:
            path.write_text(
                json.dumps(data, ensure_ascii=False, indent=2) + "\n",
                encoding="utf-8",
            )

    return rows


def main() -> None:
    chronicle_rows = migrate_chronicle()
    all_rows = {}

    for key, ko, en, ja in STATIC_ROWS:
        all_rows[key] = [key, ko, en, ja]

    for key, (ko, en, ja) in chronicle_rows.items():
        if key not in all_rows:
            all_rows[key] = [key, ko, en, ja]

    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    OUTPUT.write_text(
        json.dumps(list(all_rows.values()), ensure_ascii=False, indent=2),
        encoding="utf-8",
    )
    print(f"Wrote {len(all_rows)} rows to {OUTPUT}")


if __name__ == "__main__":
    main()
