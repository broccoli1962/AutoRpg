#!/usr/bin/env python3
"""Populate AutoRpg Google Spreadsheet with GSSL table sheets."""

from __future__ import annotations

import json
import time
import urllib.parse
import urllib.request
import base64
from pathlib import Path

SA_PATH = Path(r"C:/Users/aq001/Downloads/ai-autorpg-304e705b3c26.json")
SPREADSHEET_ID = "1XuJHq_MKPIKwSYJL0PsQMfd8KLmfiI2iCJIMvHfWaS4"

ZONE_ROWS = [
    ["id-string", "displayName-string", "minFloor-int", "maxFloor-int", "rewardMultiplier-float", "riskMultiplier-float"],
    ["mossy_hollow", "이끼 동굴", "1", "15", "1.0", "1.0"],
    ["fungal_maze", "균사 미궁", "16", "30", "1.3", "1.4"],
    ["crystal_cavern", "수정 공동", "31", "45", "1.8", "1.9"],
    ["molten_depths", "용암 지대", "46", "60", "2.5", "2.8"],
    ["silent_ruins", "침묵의 폐허", "61", "75", "3.5", "3.8"],
    ["abyssal_threshold", "심연의 문턱", "76", "90", "4.5", "5.0"],
]

MONSTER_ROWS = [
    ["id-string", "zoneId-string", "displayName-string", "rarity-string", "hp-int", "attack-int", "defense-int", "goldReward-int"],
    ["fungus_slime", "mossy_hollow", "곰팡이 슬라임", "Common", "12", "4", "6", "8"],
    ["cave_rat", "mossy_hollow", "동굴 쥐", "Common", "10", "3", "5", "6"],
    ["silver_bat_swarm", "mossy_hollow", "은빛 박쥐떼", "Notable", "22", "8", "14", "18"],
    ["moss_guardian", "mossy_hollow", "이끼 수호자", "Rare", "40", "14", "24", "35"],
    ["hollow_boss", "mossy_hollow", "심연의 이끼 군주", "Boss", "80", "20", "40", "60"],
    ["spore_wisp", "fungal_maze", "포자 정령", "Common", "16", "5", "7", "10"],
    ["mycelium_tendril", "fungal_maze", "균사 촉수", "Notable", "28", "10", "12", "22"],
    ["infected_cave_bear", "fungal_maze", "감염된 동굴 곰", "Rare", "48", "16", "18", "40"],
    ["spore_matriarch", "fungal_maze", "포자 여왕", "Rare", "55", "18", "20", "45"],
    ["mycelial_overmind", "fungal_maze", "균사체 군주", "Boss", "95", "24", "26", "75"],
]

DISCOVERY_ROWS = [
    ["itemId-string", "zoneId-string", "displayName-string", "quantity-int", "goldValue-int"],
    ["mana_shard", "mossy_hollow", "마나결정", "1", "12"],
    ["old_coin_pouch", "mossy_hollow", "낡은 은화 주머니", "1", "25"],
    ["healing_herb", "mossy_hollow", "치유 이끼", "1", "8"],
    ["rusty_ring", "mossy_hollow", "낡은 반지", "1", "15"],
    ["spore_vial", "fungal_maze", "포자 시약", "1", "18"],
    ["fungal_cap", "fungal_maze", "발광 버섯갓", "1", "14"],
]

CHARACTER_ROWS = [
    ["id-string", "displayName-string", "role-string", "personality-string", "str-int", "agi-int", "intel-int", "vit-int", "luk-int"],
    ["char_lena", "레나", "Warrior", "Cautious", "14", "10", "8", "12", "6"],
    ["char_marco", "마르코", "Rogue", "Loyal", "10", "14", "8", "10", "8"],
    ["char_kyle", "카일", "Mage", "Greedy", "8", "10", "14", "9", "10"],
    ["char_sora", "소라", "Bard", "Cheerful", "9", "11", "12", "8", "12"],
    ["char_elena", "엘레나", "Cleric", "Loyal", "10", "9", "13", "11", "7"],
]

ENUM_ROWS = [
    [
        "CharacterRole", "CharacterRole-idx",
        "CharacterTier", "CharacterTier-idx",
        "PersonalityTag", "PersonalityTag-idx",
        "InjurySeverity", "InjurySeverity-idx",
        "EventType", "EventType-idx",
        "SalienceGrade", "SalienceGrade-idx",
        "CombatOutcome", "CombatOutcome-idx",
        "MonsterRarity", "MonsterRarity-idx",
        "LogCategory", "LogCategory-idx",
    ],
    ["Warrior", "0", "Apprentice", "0", "Cautious", "0", "None", "0", "Move", "0", "Trivial", "0", "Victory", "0", "Common", "0", "Move", "0"],
    ["Rogue", "1", "Adept", "1", "Greedy", "1", "Light", "1", "CombatResult", "1", "Notable", "1", "Defeat", "1", "Notable", "1", "Combat", "1"],
    ["Mage", "2", "Artisan", "2", "Reckless", "2", "Moderate", "2", "Discovery", "2", "Significant", "2", "Retreat", "2", "Rare", "2", "Discovery", "2"],
    ["Bard", "3", "Hero", "3", "Cheerful", "3", "Severe", "3", "Trap", "3", "Milestone", "3", "", "", "Boss", "3", "Status", "3"],
    ["Cleric", "4", "Legend", "4", "Loyal", "4", "Fatal", "4", "Rest", "4", "", "", "", "", "", "", "Milestone", "4"],
    ["", "", "", "", "Cynical", "5", "", "", "Injury", "5", "", "", "", "", "", "", ""],
    ["", "", "", "", "", "", "", "", "Death", "6", "", "", "", "", "", "", ""],
    ["", "", "", "", "", "", "", "", "FloorClear", "7", "", "", "", "", "", "", ""],
    ["", "", "", "", "", "", "", "", "ZoneTransition", "8", "", "", "", "", "", "", ""],
    ["", "", "", "", "", "", "", "", "OfflineSummary", "9", "", "", "", "", "", "", ""],
]

LOCALIZATION_ROWS = [
    ["key-string", "ko-string", "en-string"],
    ["zone.mossy_hollow", "이끼 동굴", "Mossy Hollow"],
    ["zone.fungal_maze", "균사 미궁", "Fungal Maze"],
    ["ui.guild_waiting", "길드 대기", "Guild Standby"],
    ["ui.exploration_log", "탐험 로그", "Exploration Log"],
]

DYNAMIC_EVENT_ROWS = [
    ["id-string", "category-string", "triggerType-string", "zoneMin-string", "zoneMax-string", "probability-float", "intensity-string"],
    ["fork_002", "fork_choice", "floor_enter", "mossy_hollow", "fungal_maze", "0.12", "standard"],
    ["encounter_merchant_01", "encounter", "random", "mossy_hollow", "fungal_maze", "0.08", "standard"],
    ["trap_pressure_01", "trap", "random", "mossy_hollow", "fungal_maze", "0.1", "standard"],
    ["artifact_mural_01", "artifact", "random", "mossy_hollow", "crystal_cavern", "0.06", "standard"],
    ["golden_chamber_01", "golden", "random", "mossy_hollow", "abyssal_threshold", "0.02", "golden"],
]

SHEETS = {
    "Zone": ZONE_ROWS,
    "Monster": MONSTER_ROWS,
    "Discovery": DISCOVERY_ROWS,
    "Character": CHARACTER_ROWS,
    "DynamicEvent": DYNAMIC_EVENT_ROWS,
    "EnumDef": ENUM_ROWS,
    "Localization": LOCALIZATION_ROWS,
}


def b64url(data: bytes) -> str:
    return base64.urlsafe_b64encode(data).rstrip(b"=").decode("ascii")


def get_token() -> str:
    from cryptography.hazmat.primitives import hashes, serialization
    from cryptography.hazmat.primitives.asymmetric import padding

    sa = json.loads(SA_PATH.read_text(encoding="utf-8"))
    header = b64url(json.dumps({"alg": "RS256", "typ": "JWT"}).encode())
    now = int(time.time())
    claim = {
        "iss": sa["client_email"],
        "scope": "https://www.googleapis.com/auth/spreadsheets",
        "aud": "https://oauth2.googleapis.com/token",
        "exp": now + 3600,
        "iat": now,
    }
    payload = b64url(json.dumps(claim).encode())
    signing_input = f"{header}.{payload}".encode()
    private_key = serialization.load_pem_private_key(sa["private_key"].encode(), password=None)
    signature = private_key.sign(signing_input, padding.PKCS1v15(), hashes.SHA256())
    jwt = f"{header}.{payload}.{b64url(signature)}"

    req = urllib.request.Request(
        "https://oauth2.googleapis.com/token",
        data=urllib.parse.urlencode(
            {"grant_type": "urn:ietf:params:oauth:grant-type:jwt-bearer", "assertion": jwt}
        ).encode(),
        method="POST",
    )
    with urllib.request.urlopen(req) as resp:
        return json.loads(resp.read())["access_token"]


def api(token: str, method: str, url: str, body: dict | None = None):
    data = None if body is None else json.dumps(body).encode("utf-8")
    req = urllib.request.Request(
        url,
        data=data,
        method=method,
        headers={
            "Authorization": f"Bearer {token}",
            "Content-Type": "application/json",
        },
    )
    with urllib.request.urlopen(req) as resp:
        raw = resp.read()
        return json.loads(raw) if raw else {}


def ensure_sheets(token: str):
    meta = api(
        token,
        "GET",
        f"https://sheets.googleapis.com/v4/spreadsheets/{SPREADSHEET_ID}?fields=sheets.properties(title,sheetId)",
    )
    existing = {s["properties"]["title"] for s in meta.get("sheets", [])}
    requests = []
    for title in SHEETS:
        if title not in existing:
            requests.append({"addSheet": {"properties": {"title": title}}})

    if requests:
        api(
            token,
            "POST",
            f"https://sheets.googleapis.com/v4/spreadsheets/{SPREADSHEET_ID}:batchUpdate",
            {"requests": requests},
        )
        print(f"Added sheets: {[r['addSheet']['properties']['title'] for r in requests]}")


def write_values(token: str):
    data = []
    for title, rows in SHEETS.items():
        data.append({"range": f"{title}!A1", "majorDimension": "ROWS", "values": rows})

    api(
        token,
        "POST",
        f"https://sheets.googleapis.com/v4/spreadsheets/{SPREADSHEET_ID}/values:batchUpdate",
        {"valueInputOption": "RAW", "data": data},
    )
    print(f"Wrote {len(data)} sheets")


def rename_default_sheet(token: str):
    meta = api(
        token,
        "GET",
        f"https://sheets.googleapis.com/v4/spreadsheets/{SPREADSHEET_ID}?fields=sheets.properties(title,sheetId)",
    )
    requests = []
    for sheet in meta.get("sheets", []):
        title = sheet["properties"]["title"]
        sheet_id = sheet["properties"]["sheetId"]
        if title in ("시트1", "Sheet1"):
            requests.append(
                {
                    "updateSheetProperties": {
                        "properties": {"sheetId": sheet_id, "title": "#Notes"},
                        "fields": "title",
                    }
                }
            )

    if requests:
        api(
            token,
            "POST",
            f"https://sheets.googleapis.com/v4/spreadsheets/{SPREADSHEET_ID}:batchUpdate",
            {"requests": requests},
        )
        print("Renamed default sheet to #Notes")


def main():
    token = get_token()
    rename_default_sheet(token)
    ensure_sheets(token)
    write_values(token)
    print("GSSL sheet setup complete.")


if __name__ == "__main__":
    main()
