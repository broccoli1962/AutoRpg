from pathlib import Path
import re

prefabs = Path(r"D:\Unity\AI_AutoGame\AutoRpg\Assets\GameResource\Prefabs\UI").glob("*.prefab")
pattern = re.compile(r"(  m_Text: ).+")

for path in prefabs:
    text = path.read_text(encoding="utf-8")
    new = pattern.sub(r"\1", text)
    if new != text:
        path.write_text(new, encoding="utf-8")
        print("cleared", path.name)
