from __future__ import annotations

import json
import math
import random
from pathlib import Path

from PIL import Image, ImageDraw, ImageFilter


ROOT = Path(__file__).resolve().parents[2]
ART_ROOT = ROOT / "Assets" / "Game" / "Art"

ATLAS_DIR = ART_ROOT / "Sprites" / "Atlases"
BG_DIR = ART_ROOT / "Sprites" / "Backgrounds"
UI_DIR = ART_ROOT / "UI"
VFX_DIR = ART_ROOT / "VFX"


def ensure_dirs() -> None:
    for p in [ATLAS_DIR, BG_DIR, UI_DIR, VFX_DIR]:
        p.mkdir(parents=True, exist_ok=True)


def rgba(hex_color: str, a: int = 255) -> tuple[int, int, int, int]:
    hex_color = hex_color.lstrip("#")
    return int(hex_color[0:2], 16), int(hex_color[2:4], 16), int(hex_color[4:6], 16), a


def draw_grid(img: Image.Image, size: int = 48, alpha: int = 40) -> None:
    d = ImageDraw.Draw(img)
    w, h = img.size
    c = (100, 160, 220, alpha)
    for x in range(0, w, size):
        d.line([(x, 0), (x, h)], fill=c, width=1)
    for y in range(0, h, size):
        d.line([(0, y), (w, y)], fill=c, width=1)


def draw_starfield(img: Image.Image, density: int = 380) -> None:
    d = ImageDraw.Draw(img)
    w, h = img.size
    for _ in range(density):
        x = random.randint(0, w - 1)
        y = random.randint(0, h - 1)
        r = random.choice([1, 1, 1, 2])
        a = random.randint(90, 220)
        d.ellipse((x - r, y - r, x + r, y + r), fill=(220, 235, 255, a))


def draw_ship_player(draw: ImageDraw.ImageDraw, box: tuple[int, int, int, int]) -> None:
    x0, y0, x1, y1 = box
    cx = (x0 + x1) / 2
    w = x1 - x0
    h = y1 - y0
    body = [
        (cx, y0 + h * 0.08),
        (x0 + w * 0.72, y0 + h * 0.50),
        (x0 + w * 0.58, y1 - h * 0.04),
        (x0 + w * 0.42, y1 - h * 0.04),
        (x0 + w * 0.28, y0 + h * 0.50),
    ]
    draw.polygon(body, fill=rgba("58B8FF"), outline=rgba("D4F1FF"), width=3)
    draw.polygon(
        [(cx, y0 + h * 0.20), (x0 + w * 0.60, y0 + h * 0.72), (x0 + w * 0.40, y0 + h * 0.72)],
        fill=rgba("D9F7FF"),
    )
    draw.rectangle((x0 + w * 0.33, y0 + h * 0.70, x0 + w * 0.67, y0 + h * 0.84), fill=rgba("1D82C9"))


def draw_ship_player_damaged(draw: ImageDraw.ImageDraw, box: tuple[int, int, int, int]) -> None:
    draw_ship_player(draw, box)
    x0, y0, x1, y1 = box
    draw.line((x0 + 16, y0 + 24, x1 - 22, y1 - 24), fill=rgba("FF7A6A"), width=5)
    draw.line((x0 + 24, y0 + 64, x1 - 30, y1 - 18), fill=rgba("FF4E4E"), width=3)


def draw_shield_ring(draw: ImageDraw.ImageDraw, box: tuple[int, int, int, int]) -> None:
    x0, y0, x1, y1 = box
    for i, a in enumerate([40, 70, 120, 160]):
        pad = 8 + i * 5
        draw.ellipse((x0 + pad, y0 + pad, x1 - pad, y1 - pad), outline=(120, 215, 255, a), width=3)


def draw_enemy(draw: ImageDraw.ImageDraw, box: tuple[int, int, int, int], style: str) -> None:
    x0, y0, x1, y1 = box
    w = x1 - x0
    h = y1 - y0
    cx = (x0 + x1) / 2
    if style == "E01":
        pts = [(cx, y1 - h * 0.05), (x0 + w * 0.80, y0 + h * 0.28), (x0 + w * 0.20, y0 + h * 0.28)]
        draw.polygon(pts, fill=rgba("E25757"), outline=rgba("FFD0D0"), width=3)
    elif style == "E02":
        pts = [(cx, y1 - h * 0.05), (x0 + w * 0.72, y0 + h * 0.12), (x0 + w * 0.28, y0 + h * 0.12)]
        draw.polygon(pts, fill=rgba("FF6A55"), outline=rgba("FFD8CC"), width=3)
        draw.polygon([(cx, y0 + h * 0.40), (x0 + w * 0.60, y0 + h * 0.78), (x0 + w * 0.40, y0 + h * 0.78)], fill=rgba("942E2E"))
    elif style == "E03":
        draw.polygon(
            [(cx, y1 - h * 0.04), (x0 + w * 0.84, y0 + h * 0.32), (x0 + w * 0.68, y0 + h * 0.10), (x0 + w * 0.32, y0 + h * 0.10), (x0 + w * 0.16, y0 + h * 0.32)],
            fill=rgba("D9543A"),
            outline=rgba("FFC2AE"),
            width=3,
        )
        draw.rectangle((x0 + w * 0.43, y0 + h * 0.42, x0 + w * 0.57, y0 + h * 0.62), fill=rgba("2D2A2A"))
    elif style == "E04":
        draw.polygon(
            [(cx, y1 - h * 0.03), (x0 + w * 0.70, y0 + h * 0.06), (x0 + w * 0.30, y0 + h * 0.06)],
            fill=rgba("F16A41"),
            outline=rgba("FFE1CC"),
            width=3,
        )
        draw.line((cx, y0 + h * 0.20, cx, y1 - h * 0.08), fill=rgba("FFFFFF"), width=3)
    elif style == "E05":
        draw.rounded_rectangle((x0 + w * 0.14, y0 + h * 0.12, x1 - w * 0.14, y1 - h * 0.10), radius=int(w * 0.08), fill=rgba("7B8088"), outline=rgba("C6CDD7"), width=3)
        draw.rectangle((x0 + w * 0.40, y0 + h * 0.38, x0 + w * 0.60, y0 + h * 0.62), fill=rgba("C73939"))
    elif style == "E06":
        pts = [(cx, y1 - h * 0.06), (x0 + w * 0.85, y0 + h * 0.40), (x0 + w * 0.68, y0 + h * 0.12), (x0 + w * 0.32, y0 + h * 0.12), (x0 + w * 0.15, y0 + h * 0.40)]
        draw.polygon(pts, fill=rgba("8D5BD3"), outline=rgba("E2D8FF"), width=3)
        for p in [0.36, 0.50, 0.64]:
            draw.ellipse((x0 + w * p - 6, y0 + h * 0.44, x0 + w * p + 6, y0 + h * 0.56), fill=rgba("2E1E50"))
    elif style == "E07":
        draw.polygon(
            [(cx, y1 - h * 0.05), (x0 + w * 0.82, y0 + h * 0.30), (x0 + w * 0.62, y0 + h * 0.10), (x0 + w * 0.38, y0 + h * 0.10), (x0 + w * 0.18, y0 + h * 0.30)],
            fill=rgba("A84565"),
            outline=rgba("FFD3DE"),
            width=3,
        )
        draw.ellipse((x0 + w * 0.38, y0 + h * 0.36, x0 + w * 0.62, y0 + h * 0.60), fill=rgba("FF69E2"))
    elif style == "E08":
        draw.polygon(
            [(cx, y1 - h * 0.04), (x0 + w * 0.90, y0 + h * 0.42), (x0 + w * 0.72, y0 + h * 0.12), (x0 + w * 0.28, y0 + h * 0.12), (x0 + w * 0.10, y0 + h * 0.42)],
            fill=rgba("7C2A2A"),
            outline=rgba("FFD0A8"),
            width=4,
        )
        draw.rectangle((x0 + w * 0.40, y0 + h * 0.30, x0 + w * 0.60, y0 + h * 0.58), fill=rgba("F29B45"))


def draw_boss(draw: ImageDraw.ImageDraw, box: tuple[int, int, int, int], idx: int) -> None:
    x0, y0, x1, y1 = box
    w = x1 - x0
    h = y1 - y0
    metal = [rgba("4F5968"), rgba("596272"), rgba("4C525E"), rgba("46505A"), rgba("3D4654")][idx - 1]
    accent = [rgba("4AE3FF"), rgba("FFA64A"), rgba("FF6464"), rgba("A767FF"), rgba("FF2A79")][idx - 1]

    if idx == 1:
        hull = [(x0 + w * 0.50, y1 - h * 0.04), (x0 + w * 0.86, y0 + h * 0.58), (x0 + w * 0.76, y0 + h * 0.18), (x0 + w * 0.24, y0 + h * 0.18), (x0 + w * 0.14, y0 + h * 0.58)]
    elif idx == 2:
        hull = [(x0 + w * 0.50, y1 - h * 0.06), (x0 + w * 0.90, y0 + h * 0.50), (x0 + w * 0.82, y0 + h * 0.16), (x0 + w * 0.18, y0 + h * 0.16), (x0 + w * 0.10, y0 + h * 0.50)]
    elif idx == 3:
        hull = [(x0 + w * 0.50, y1 - h * 0.04), (x0 + w * 0.93, y0 + h * 0.60), (x0 + w * 0.88, y0 + h * 0.12), (x0 + w * 0.12, y0 + h * 0.12), (x0 + w * 0.07, y0 + h * 0.60)]
    elif idx == 4:
        hull = [(x0 + w * 0.50, y1 - h * 0.03), (x0 + w * 0.95, y0 + h * 0.54), (x0 + w * 0.78, y0 + h * 0.08), (x0 + w * 0.22, y0 + h * 0.08), (x0 + w * 0.05, y0 + h * 0.54)]
    else:
        hull = [(x0 + w * 0.50, y1 - h * 0.02), (x0 + w * 0.96, y0 + h * 0.58), (x0 + w * 0.90, y0 + h * 0.07), (x0 + w * 0.10, y0 + h * 0.07), (x0 + w * 0.04, y0 + h * 0.58)]

    draw.polygon(hull, fill=metal, outline=rgba("C5CEDB"), width=4)
    draw.rectangle((x0 + w * 0.39, y0 + h * 0.35, x0 + w * 0.61, y0 + h * 0.60), fill=accent)
    cannon_count = idx + 2
    for i in range(cannon_count):
        t = (i + 1) / (cannon_count + 1)
        cx = x0 + w * (0.15 + t * 0.70)
        cy = y0 + h * 0.52
        draw.ellipse((cx - 9, cy - 9, cx + 9, cy + 9), fill=rgba("1B1E23"))


def draw_bullet(draw: ImageDraw.ImageDraw, box: tuple[int, int, int, int], kind: str) -> None:
    x0, y0, x1, y1 = box
    w = x1 - x0
    h = y1 - y0
    if kind == "player_basic":
        draw.rounded_rectangle((x0 + w * 0.36, y0 + h * 0.08, x0 + w * 0.64, y1 - h * 0.08), radius=8, fill=rgba("7CFBFF"), outline=rgba("E9FFFF"), width=2)
    elif kind == "player_power":
        draw.rounded_rectangle((x0 + w * 0.28, y0 + h * 0.06, x0 + w * 0.72, y1 - h * 0.08), radius=10, fill=rgba("8EC9FF"), outline=rgba("FFFFFF"), width=2)
        draw.rectangle((x0 + w * 0.45, y0 + h * 0.14, x0 + w * 0.55, y1 - h * 0.14), fill=rgba("FFFFFF"))
    elif kind == "player_laser":
        draw.rounded_rectangle((x0 + w * 0.28, y0 + h * 0.04, x0 + w * 0.72, y1 - h * 0.04), radius=14, fill=rgba("A7DCFF"), outline=rgba("F2FAFF"), width=3)
        draw.rectangle((x0 + w * 0.44, y0 + h * 0.06, x0 + w * 0.56, y1 - h * 0.06), fill=rgba("FFFFFF"))
    elif kind == "enemy_basic":
        draw.ellipse((x0 + w * 0.18, y0 + h * 0.18, x1 - w * 0.18, y1 - h * 0.18), fill=rgba("FF5A5A"), outline=rgba("FFD5D5"), width=2)
    elif kind == "enemy_fan":
        draw.polygon([(x0 + w * 0.50, y0 + h * 0.10), (x0 + w * 0.88, y0 + h * 0.50), (x0 + w * 0.50, y1 - h * 0.10), (x0 + w * 0.12, y0 + h * 0.50)], fill=rgba("FF9A3C"), outline=rgba("FFE0BC"), width=2)
    elif kind == "enemy_tracking":
        draw.ellipse((x0 + w * 0.14, y0 + h * 0.14, x1 - w * 0.14, y1 - h * 0.14), fill=rgba("A870FF"), outline=rgba("E8DBFF"), width=2)
        draw.ellipse((x0 + w * 0.36, y0 + h * 0.36, x1 - w * 0.36, y1 - h * 0.36), fill=rgba("4D2E7A"))
    elif kind == "boss_heavy":
        draw.ellipse((x0 + w * 0.10, y0 + h * 0.10, x1 - w * 0.10, y1 - h * 0.10), fill=rgba("E9508A"), outline=rgba("FFD1E3"), width=3)
        draw.ellipse((x0 + w * 0.33, y0 + h * 0.33, x1 - w * 0.33, y1 - h * 0.33), fill=rgba("5E1C37"))


def draw_pickup(draw: ImageDraw.ImageDraw, box: tuple[int, int, int, int], kind: str) -> None:
    x0, y0, x1, y1 = box
    w = x1 - x0
    h = y1 - y0
    cfg = {
        "power": ("FF6B3D", "FFFFFF"),
        "heal": ("4FD475", "FFFFFF"),
        "bomb": ("F9D34E", "2B2500"),
        "shield": ("58B8FF", "FFFFFF"),
        "score": ("F3C14A", "4A3700"),
    }
    main, icon = cfg[kind]
    draw.ellipse((x0 + w * 0.08, y0 + h * 0.08, x1 - w * 0.08, y1 - h * 0.08), fill=rgba(main), outline=rgba("FFFFFF"), width=2)
    draw.ellipse((x0 + w * 0.02, y0 + h * 0.02, x1 - w * 0.02, y1 - h * 0.02), outline=rgba(main, 140), width=3)
    cx = (x0 + x1) / 2
    cy = (y0 + y1) / 2
    if kind == "power":
        draw.text((cx - 10, cy - 15), "P", fill=rgba(icon))
    elif kind == "heal":
        draw.rectangle((cx - 4, cy - 14, cx + 4, cy + 14), fill=rgba(icon))
        draw.rectangle((cx - 14, cy - 4, cx + 14, cy + 4), fill=rgba(icon))
    elif kind == "bomb":
        draw.ellipse((cx - 12, cy - 9, cx + 12, cy + 15), fill=rgba(icon))
        draw.rectangle((cx - 2, cy - 18, cx + 2, cy - 8), fill=rgba(icon))
    elif kind == "shield":
        pts = [(cx, cy - 16), (cx + 14, cy - 4), (cx + 10, cy + 12), (cx, cy + 18), (cx - 10, cy + 12), (cx - 14, cy - 4)]
        draw.polygon(pts, fill=rgba(icon))
    else:
        pts = [(cx, cy - 16), (cx + 5, cy - 5), (cx + 16, cy - 5), (cx + 7, cy + 3), (cx + 10, cy + 15), (cx, cy + 8), (cx - 10, cy + 15), (cx - 7, cy + 3), (cx - 16, cy - 5), (cx - 5, cy - 5)]
        draw.polygon(pts, fill=rgba(icon))


def draw_ui_icon(draw: ImageDraw.ImageDraw, box: tuple[int, int, int, int], kind: str) -> None:
    x0, y0, x1, y1 = box
    w = x1 - x0
    h = y1 - y0
    if kind == "health":
        draw.ellipse((x0 + w * 0.10, y0 + h * 0.10, x1 - w * 0.10, y1 - h * 0.10), fill=rgba("37C962"))
        draw.rectangle((x0 + w * 0.45, y0 + h * 0.24, x0 + w * 0.55, y1 - h * 0.24), fill=rgba("FFFFFF"))
        draw.rectangle((x0 + w * 0.24, y0 + h * 0.45, x1 - w * 0.24, y0 + h * 0.55), fill=rgba("FFFFFF"))
    elif kind == "bomb":
        draw.ellipse((x0 + w * 0.12, y0 + h * 0.18, x1 - w * 0.12, y1 - h * 0.12), fill=rgba("F7CE46"))
        draw.rectangle((x0 + w * 0.47, y0 + h * 0.05, x0 + w * 0.53, y0 + h * 0.20), fill=rgba("513E00"))
    elif kind == "power":
        draw.ellipse((x0 + w * 0.10, y0 + h * 0.10, x1 - w * 0.10, y1 - h * 0.10), fill=rgba("FF7048"))
        draw.text((x0 + w * 0.36, y0 + h * 0.28), "P", fill=rgba("FFFFFF"))
    elif kind == "score":
        draw.ellipse((x0 + w * 0.10, y0 + h * 0.10, x1 - w * 0.10, y1 - h * 0.10), fill=rgba("F2B93E"))
        pts = [
            (x0 + w * 0.50, y0 + h * 0.20),
            (x0 + w * 0.58, y0 + h * 0.42),
            (x0 + w * 0.82, y0 + h * 0.42),
            (x0 + w * 0.63, y0 + h * 0.55),
            (x0 + w * 0.70, y0 + h * 0.78),
            (x0 + w * 0.50, y0 + h * 0.64),
            (x0 + w * 0.30, y0 + h * 0.78),
            (x0 + w * 0.37, y0 + h * 0.55),
            (x0 + w * 0.18, y0 + h * 0.42),
            (x0 + w * 0.42, y0 + h * 0.42),
        ]
        draw.polygon(pts, fill=rgba("4D3700"))


def draw_vfx(draw: ImageDraw.ImageDraw, box: tuple[int, int, int, int], kind: str) -> None:
    x0, y0, x1, y1 = box
    w = x1 - x0
    h = y1 - y0
    cx = (x0 + x1) / 2
    cy = (y0 + y1) / 2
    if kind == "muzzle_player":
        draw.polygon([(cx, y0 + h * 0.05), (x0 + w * 0.65, y1 - h * 0.10), (x0 + w * 0.35, y1 - h * 0.10)], fill=rgba("9AE9FF"))
    elif kind == "muzzle_enemy":
        draw.polygon([(cx, y1 - h * 0.05), (x0 + w * 0.65, y0 + h * 0.10), (x0 + w * 0.35, y0 + h * 0.10)], fill=rgba("FF8A6A"))
    elif kind == "hit_small":
        for r, a in [(24, 220), (14, 180), (7, 255)]:
            draw.ellipse((cx - r, cy - r, cx + r, cy + r), outline=(255, 220, 120, a), width=3)
    elif kind == "explosion_small":
        draw.ellipse((cx - 30, cy - 30, cx + 30, cy + 30), fill=rgba("FF8E49"))
        draw.ellipse((cx - 18, cy - 18, cx + 18, cy + 18), fill=rgba("FFE073"))
    elif kind == "explosion_medium":
        draw.ellipse((cx - 46, cy - 46, cx + 46, cy + 46), fill=rgba("FF6A45"))
        draw.ellipse((cx - 28, cy - 28, cx + 28, cy + 28), fill=rgba("FFD274"))
        draw.ellipse((cx - 10, cy - 10, cx + 10, cy + 10), fill=rgba("FFF8D9"))
    elif kind == "explosion_boss":
        draw.ellipse((cx - 56, cy - 56, cx + 56, cy + 56), fill=rgba("FF4D5E"))
        draw.ellipse((cx - 36, cy - 36, cx + 36, cy + 36), fill=rgba("FF9A4A"))
        draw.ellipse((cx - 18, cy - 18, cx + 18, cy + 18), fill=rgba("FFF39D"))
    elif kind == "pickup":
        draw.ellipse((cx - 32, cy - 32, cx + 32, cy + 32), outline=rgba("7AFFB4"), width=4)
        draw.ellipse((cx - 16, cy - 16, cx + 16, cy + 16), outline=rgba("D5FFE7"), width=3)
    elif kind == "bomb_clear":
        draw.ellipse((cx - 58, cy - 58, cx + 58, cy + 58), outline=rgba("A0D2FF"), width=4)
        draw.ellipse((cx - 38, cy - 38, cx + 38, cy + 38), outline=rgba("D9ECFF"), width=3)
    elif kind == "shield":
        draw.ellipse((cx - 48, cy - 48, cx + 48, cy + 48), outline=rgba("66C7FF"), width=4)
        draw.ellipse((cx - 34, cy - 34, cx + 34, cy + 34), outline=rgba("BCE8FF"), width=3)


def paste_tile(atlas: Image.Image, name: str, size: tuple[int, int], pos: tuple[int, int], painter) -> dict:
    tile = Image.new("RGBA", size, (0, 0, 0, 0))
    d = ImageDraw.Draw(tile)
    painter(d, (0, 0, size[0], size[1]))
    atlas.alpha_composite(tile, pos)
    return {
        "name": name,
        "x": pos[0],
        "y": pos[1],
        "width": size[0],
        "height": size[1],
        "pivot": [0.5, 0.5],
    }


def generate_ships_atlas() -> tuple[Path, list[dict]]:
    atlas = Image.new("RGBA", (1024, 1024), (0, 0, 0, 0))
    rects: list[dict] = []
    pad = 16
    x = pad
    y = pad
    row_h = 0

    items = [
        ("SPR_Player_Default", (128, 128), lambda d, b: draw_ship_player(d, b)),
        ("SPR_Player_Damaged", (128, 128), lambda d, b: draw_ship_player_damaged(d, b)),
        ("SPR_Player_Shield", (160, 160), lambda d, b: draw_shield_ring(d, b)),
        ("SPR_Enemy_E01_SmallStraight", (96, 96), lambda d, b: draw_enemy(d, b, "E01")),
        ("SPR_Enemy_E02_Diagonal", (96, 96), lambda d, b: draw_enemy(d, b, "E02")),
        ("SPR_Enemy_E03_Shooter", (128, 128), lambda d, b: draw_enemy(d, b, "E03")),
        ("SPR_Enemy_E04_Assault", (96, 96), lambda d, b: draw_enemy(d, b, "E04")),
        ("SPR_Enemy_E05_Armored", (160, 160), lambda d, b: draw_enemy(d, b, "E05")),
        ("SPR_Enemy_E06_Spread", (128, 128), lambda d, b: draw_enemy(d, b, "E06")),
        ("SPR_Enemy_E07_Tracking", (128, 128), lambda d, b: draw_enemy(d, b, "E07")),
        ("SPR_Enemy_E08_Elite", (192, 192), lambda d, b: draw_enemy(d, b, "E08")),
    ]

    for name, size, painter in items:
        w, h = size
        if x + w + pad > atlas.width:
            x = pad
            y += row_h + pad
            row_h = 0
        rects.append(paste_tile(atlas, name, size, (x, y), painter))
        x += w + pad
        row_h = max(row_h, h)

    out = ATLAS_DIR / "SPR_Atlas_Ships_P1.png"
    atlas.save(out)
    return out, rects


def generate_boss_atlas() -> tuple[Path, list[dict]]:
    atlas = Image.new("RGBA", (2048, 1024), (0, 0, 0, 0))
    rects: list[dict] = []
    pad = 24
    x = pad
    y = pad
    row_h = 0
    items = [
        ("SPR_Boss_01_PatrolLeader", (384, 256), 1),
        ("SPR_Boss_02_CloudBomber", (384, 256), 2),
        ("SPR_Boss_03_HeavyGunboat", (512, 320), 3),
        ("SPR_Boss_04_TwinWingInterceptor", (512, 320), 4),
        ("SPR_Boss_05_FinalCarrier", (640, 384), 5),
    ]
    for name, size, idx in items:
        w, h = size
        if x + w + pad > atlas.width:
            x = pad
            y += row_h + pad
            row_h = 0
        rects.append(paste_tile(atlas, name, size, (x, y), lambda d, b, i=idx: draw_boss(d, b, i)))
        x += w + pad
        row_h = max(row_h, h)
    out = ATLAS_DIR / "SPR_Atlas_Bosses_P1.png"
    atlas.save(out)
    return out, rects


def generate_projectiles_pickups_icons_atlas() -> tuple[Path, list[dict]]:
    atlas = Image.new("RGBA", (1024, 1024), (0, 0, 0, 0))
    rects: list[dict] = []
    pad = 16
    x = pad
    y = pad
    row_h = 0
    items = [
        ("SPR_Bullet_Player_Basic", (32, 64), lambda d, b: draw_bullet(d, b, "player_basic")),
        ("SPR_Bullet_Player_Power", (48, 80), lambda d, b: draw_bullet(d, b, "player_power")),
        ("SPR_Bullet_Player_Laser", (64, 256), lambda d, b: draw_bullet(d, b, "player_laser")),
        ("SPR_Bullet_Enemy_Basic", (32, 32), lambda d, b: draw_bullet(d, b, "enemy_basic")),
        ("SPR_Bullet_Enemy_Fan", (32, 32), lambda d, b: draw_bullet(d, b, "enemy_fan")),
        ("SPR_Bullet_Enemy_Tracking", (40, 40), lambda d, b: draw_bullet(d, b, "enemy_tracking")),
        ("SPR_Bullet_Boss_Heavy", (48, 48), lambda d, b: draw_bullet(d, b, "boss_heavy")),
        ("SPR_Pickup_Power", (64, 64), lambda d, b: draw_pickup(d, b, "power")),
        ("SPR_Pickup_Heal", (64, 64), lambda d, b: draw_pickup(d, b, "heal")),
        ("SPR_Pickup_Bomb", (64, 64), lambda d, b: draw_pickup(d, b, "bomb")),
        ("SPR_Pickup_Shield", (64, 64), lambda d, b: draw_pickup(d, b, "shield")),
        ("SPR_Pickup_Score", (64, 64), lambda d, b: draw_pickup(d, b, "score")),
        ("UI_Icon_Health", (64, 64), lambda d, b: draw_ui_icon(d, b, "health")),
        ("UI_Icon_Bomb", (64, 64), lambda d, b: draw_ui_icon(d, b, "bomb")),
        ("UI_Icon_Power", (64, 64), lambda d, b: draw_ui_icon(d, b, "power")),
        ("UI_Icon_Score", (64, 64), lambda d, b: draw_ui_icon(d, b, "score")),
    ]
    for name, size, painter in items:
        w, h = size
        if x + w + pad > atlas.width:
            x = pad
            y += row_h + pad
            row_h = 0
        rects.append(paste_tile(atlas, name, size, (x, y), painter))
        x += w + pad
        row_h = max(row_h, h)
    out = ATLAS_DIR / "SPR_Atlas_Projectiles_Pickups_UIIcons.png"
    atlas.save(out)
    return out, rects


def generate_ui_common_atlas() -> tuple[Path, list[dict]]:
    atlas = Image.new("RGBA", (1024, 512), (0, 0, 0, 0))
    d = ImageDraw.Draw(atlas)
    rects: list[dict] = []

    def ui_panel_style(box: tuple[int, int, int, int], color_hex: str, border_hex: str):
        x0, y0, x1, y1 = box
        d.rounded_rectangle(box, radius=12, fill=rgba(color_hex, 220), outline=rgba(border_hex), width=3)
        d.rounded_rectangle((x0 + 8, y0 + 8, x1 - 8, y1 - 8), radius=8, outline=rgba("FFFFFF", 50), width=1)

    button_w, button_h = 256, 96
    panel_w, panel_h = 420, 220
    bar_w, bar_h = 420, 48

    ui_panel_style((24, 24, 24 + button_w, 24 + button_h), "2F4C6E", "8EC5FF")
    rects.append({"name": "UI_Button_Normal", "x": 24, "y": 24, "width": button_w, "height": button_h, "pivot": [0.5, 0.5]})
    ui_panel_style((24, 136, 24 + button_w, 136 + button_h), "3A5E85", "B5DBFF")
    rects.append({"name": "UI_Button_Hover", "x": 24, "y": 136, "width": button_w, "height": button_h, "pivot": [0.5, 0.5]})
    ui_panel_style((24, 248, 24 + button_w, 248 + button_h), "253C58", "79B7F7")
    rects.append({"name": "UI_Button_Pressed", "x": 24, "y": 248, "width": button_w, "height": button_h, "pivot": [0.5, 0.5]})

    ui_panel_style((324, 24, 324 + panel_w, 24 + panel_h), "18263A", "5F8FC8")
    rects.append({"name": "UI_Panel_Default", "x": 324, "y": 24, "width": panel_w, "height": panel_h, "pivot": [0.5, 0.5]})

    x0, y0 = 324, 272
    d.rounded_rectangle((x0, y0, x0 + bar_w, y0 + bar_h), radius=10, fill=rgba("2A2E36"), outline=rgba("A4AEBD"), width=3)
    rects.append({"name": "UI_BossHealthBar_Frame", "x": x0, "y": y0, "width": bar_w, "height": bar_h, "pivot": [0.5, 0.5]})
    d.rounded_rectangle((x0 + 6, y0 + 6, x0 + bar_w - 6, y0 + bar_h - 6), radius=7, fill=rgba("DF335F"), outline=rgba("FF8AA7"), width=2)
    rects.append({"name": "UI_BossHealthBar_Fill", "x": x0 + 6, "y": y0 + 6, "width": bar_w - 12, "height": bar_h - 12, "pivot": [0.5, 0.5]})

    out = UI_DIR / "SPR_Atlas_UI_Common.png"
    atlas.save(out)
    return out, rects


def generate_vfx_atlas() -> tuple[Path, list[dict]]:
    atlas = Image.new("RGBA", (1024, 1024), (0, 0, 0, 0))
    rects: list[dict] = []
    pad = 16
    x = pad
    y = pad
    row_h = 0
    items = [
        ("VFX_Muzzle_Player", (96, 96), "muzzle_player"),
        ("VFX_Muzzle_Enemy", (96, 96), "muzzle_enemy"),
        ("VFX_Hit_Small", (96, 96), "hit_small"),
        ("VFX_Explosion_Small", (128, 128), "explosion_small"),
        ("VFX_Explosion_Medium", (128, 128), "explosion_medium"),
        ("VFX_Explosion_Boss", (160, 160), "explosion_boss"),
        ("VFX_Pickup", (96, 96), "pickup"),
        ("VFX_Bomb_Clear", (160, 160), "bomb_clear"),
        ("VFX_Shield", (160, 160), "shield"),
    ]
    for name, size, kind in items:
        w, h = size
        if x + w + pad > atlas.width:
            x = pad
            y += row_h + pad
            row_h = 0
        rects.append(paste_tile(atlas, name, size, (x, y), lambda d, b, k=kind: draw_vfx(d, b, k)))
        x += w + pad
        row_h = max(row_h, h)

    atlas = atlas.filter(ImageFilter.GaussianBlur(radius=0.2))
    out = VFX_DIR / "SPR_Atlas_VFX_P1.png"
    atlas.save(out)
    return out, rects


def draw_linear_gradient(img: Image.Image, top: tuple[int, int, int], bottom: tuple[int, int, int]) -> None:
    w, h = img.size
    p = img.load()
    for y in range(h):
        t = y / (h - 1)
        r = int(top[0] * (1 - t) + bottom[0] * t)
        g = int(top[1] * (1 - t) + bottom[1] * t)
        b = int(top[2] * (1 - t) + bottom[2] * t)
        for x in range(w):
            p[x, y] = (r, g, b, 255)


def make_bg_training() -> Image.Image:
    img = Image.new("RGBA", (1080, 1920), (0, 0, 0, 255))
    draw_linear_gradient(img, (45, 95, 145), (16, 35, 62))
    d = ImageDraw.Draw(img)
    for y in range(200, 1900, 280):
        d.rectangle((120, y, 960, y + 3), fill=(170, 205, 240, 55))
    draw_starfield(img, 130)
    return img


def make_bg_cloud_assault() -> Image.Image:
    img = Image.new("RGBA", (1080, 1920), (0, 0, 0, 255))
    draw_linear_gradient(img, (150, 188, 220), (82, 121, 165))
    d = ImageDraw.Draw(img)
    for i in range(26):
        x = random.randint(-120, 980)
        y = random.randint(20, 1900)
        w = random.randint(160, 340)
        h = random.randint(50, 100)
        d.ellipse((x, y, x + w, y + h), fill=(255, 255, 255, random.randint(30, 70)))
    for i in range(10):
        x = random.randint(0, 1080)
        y = random.randint(0, 1920)
        d.line((x, y, x - 90, y + 220), fill=(220, 235, 255, 40), width=2)
    return img


def make_bg_fire_blockade() -> Image.Image:
    img = Image.new("RGBA", (1080, 1920), (0, 0, 0, 255))
    draw_linear_gradient(img, (66, 45, 60), (24, 26, 38))
    d = ImageDraw.Draw(img)
    for y in range(150, 1900, 250):
        d.rectangle((0, y, 1080, y + 8), fill=(130, 90, 90, 35))
    for i in range(12):
        x = random.randint(80, 1000)
        y = random.randint(180, 1820)
        d.polygon([(x, y), (x + 90, y - 40), (x + 180, y), (x + 90, y + 40)], fill=(90, 100, 115, 75))
    for i in range(80):
        x = random.randint(0, 1079)
        y = random.randint(0, 1919)
        d.ellipse((x, y, x + 2, y + 2), fill=(255, random.randint(120, 200), 80, random.randint(50, 100)))
    return img


def make_bg_elite_intercept() -> Image.Image:
    img = Image.new("RGBA", (1080, 1920), (0, 0, 0, 255))
    draw_linear_gradient(img, (22, 29, 58), (8, 13, 26))
    d = ImageDraw.Draw(img)
    draw_starfield(img, 260)
    for i in range(10):
        x = random.randint(100, 900)
        y = random.randint(180, 1760)
        d.rectangle((x, y, x + random.randint(80, 220), y + random.randint(12, 24)), fill=(70, 85, 120, 70))
    return img


def make_bg_final_carrier() -> Image.Image:
    img = Image.new("RGBA", (1080, 1920), (0, 0, 0, 255))
    draw_linear_gradient(img, (30, 25, 45), (8, 10, 20))
    d = ImageDraw.Draw(img)
    draw_starfield(img, 320)
    draw_grid(img, size=84, alpha=28)
    for i in range(5):
        y = 320 + i * 300
        d.polygon([(120, y), (960, y), (880, y + 80), (200, y + 80)], fill=(75, 85, 118, 45))
    d.ellipse((390, 880, 690, 1180), outline=(255, 120, 180, 70), width=3)
    return img


def generate_backgrounds() -> list[Path]:
    files = []
    mapping = [
        ("SPR_BG_TrainingAirspace.png", make_bg_training),
        ("SPR_BG_CloudAssault.png", make_bg_cloud_assault),
        ("SPR_BG_FireBlockade.png", make_bg_fire_blockade),
        ("SPR_BG_EliteIntercept.png", make_bg_elite_intercept),
        ("SPR_BG_FinalCarrier.png", make_bg_final_carrier),
    ]
    for name, fn in mapping:
        out = BG_DIR / name
        fn().save(out)
        files.append(out)
    return files


def main() -> None:
    random.seed(42)
    ensure_dirs()

    manifest: dict[str, dict] = {"atlases": {}}

    out, rects = generate_ships_atlas()
    manifest["atlases"][str(out.relative_to(ROOT)).replace("\\", "/")] = rects

    out, rects = generate_boss_atlas()
    manifest["atlases"][str(out.relative_to(ROOT)).replace("\\", "/")] = rects

    out, rects = generate_projectiles_pickups_icons_atlas()
    manifest["atlases"][str(out.relative_to(ROOT)).replace("\\", "/")] = rects

    out, rects = generate_ui_common_atlas()
    manifest["atlases"][str(out.relative_to(ROOT)).replace("\\", "/")] = rects

    out, rects = generate_vfx_atlas()
    manifest["atlases"][str(out.relative_to(ROOT)).replace("\\", "/")] = rects

    bgs = generate_backgrounds()
    manifest["backgrounds"] = [str(p.relative_to(ROOT)).replace("\\", "/") for p in bgs]

    manifest_path = ATLAS_DIR / "SPR_Atlas_SpriteRects_P1.json"
    manifest_path.write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8")

    print("Generated:")
    for atlas_path in manifest["atlases"].keys():
        print(f"  {atlas_path}")
    for bg in manifest["backgrounds"]:
        print(f"  {bg}")
    print(f"  {manifest_path.relative_to(ROOT).as_posix()}")


if __name__ == "__main__":
    main()
