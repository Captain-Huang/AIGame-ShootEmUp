from __future__ import annotations

from collections import deque
from pathlib import Path
from typing import Iterable

from PIL import Image


ROOT = Path(__file__).resolve().parents[2]
ART = ROOT / "Assets" / "Game" / "Art"

SPRITES = ART / "Sprites"
ATLAS_DIR = SPRITES / "Atlases"

DIR_PLAYER = SPRITES / "Player"
DIR_ENEMIES = SPRITES / "Enemies"
DIR_BOSSES = SPRITES / "Bosses"
DIR_BULLETS = SPRITES / "Bullets"
DIR_PICKUPS = SPRITES / "Pickups"
DIR_BACKGROUNDS = SPRITES / "Backgrounds"
DIR_UI = ART / "UI"
DIR_VFX = ART / "VFX"


def ensure_dirs() -> None:
    for p in [
        DIR_PLAYER,
        DIR_ENEMIES,
        DIR_BOSSES,
        DIR_BULLETS,
        DIR_PICKUPS,
        DIR_BACKGROUNDS,
        DIR_UI,
        DIR_VFX,
    ]:
        p.mkdir(parents=True, exist_ok=True)


def load_rgba(path: Path) -> Image.Image:
    return Image.open(path).convert("RGBA")


def remove_keyed_bg(img: Image.Image, remove_black: bool = True, remove_green: bool = True) -> Image.Image:
    out = img.copy()
    px = out.load()
    w, h = out.size
    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            to_transparent = False
            if remove_black and r <= 10 and g <= 10 and b <= 10:
                to_transparent = True
            if remove_green and g > 80 and g > r + 25 and g > b + 25:
                to_transparent = True
            if to_transparent:
                px[x, y] = (r, g, b, 0)
            else:
                # mild despill
                if remove_green and g > r and g > b:
                    g = max(r, b)
                px[x, y] = (r, g, b, a)
    return out


def trim_alpha(img: Image.Image, pad: int = 2) -> Image.Image:
    alpha = img.split()[-1]
    bbox = alpha.getbbox()
    if bbox is None:
        return img
    x0, y0, x1, y1 = bbox
    x0 = max(0, x0 - pad)
    y0 = max(0, y0 - pad)
    x1 = min(img.width, x1 + pad)
    y1 = min(img.height, y1 + pad)
    return img.crop((x0, y0, x1, y1))


def fit_to_canvas(img: Image.Image, size: tuple[int, int], inner_pad: int = 2) -> Image.Image:
    tw, th = size
    safe_w = max(1, tw - inner_pad * 2)
    safe_h = max(1, th - inner_pad * 2)
    scale = min(safe_w / img.width, safe_h / img.height)
    nw = max(1, int(round(img.width * scale)))
    nh = max(1, int(round(img.height * scale)))
    resized = img.resize((nw, nh), Image.Resampling.LANCZOS)
    canvas = Image.new("RGBA", size, (0, 0, 0, 0))
    ox = (tw - nw) // 2
    oy = (th - nh) // 2
    canvas.alpha_composite(resized, (ox, oy))
    return canvas


def save_processed(
    src: Image.Image,
    out_path: Path,
    target_size: tuple[int, int],
    *,
    remove_black: bool,
    remove_green: bool,
) -> None:
    out_path.parent.mkdir(parents=True, exist_ok=True)
    keyed = remove_keyed_bg(src, remove_black=remove_black, remove_green=remove_green)
    trimmed = trim_alpha(keyed, pad=2)
    final = fit_to_canvas(trimmed, target_size, inner_pad=2)
    final.save(out_path)


def _components(img: Image.Image, min_pixels: int = 120, threshold: int = 28) -> list[dict]:
    px = img.convert("RGBA").load()
    w, h = img.size
    fg = [[False] * w for _ in range(h)]
    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            if a > 15 and (r + g + b > threshold):
                fg[y][x] = True

    vis = [[False] * w for _ in range(h)]
    out = []
    for y in range(h):
        for x in range(w):
            if not fg[y][x] or vis[y][x]:
                continue
            q = deque([(x, y)])
            vis[y][x] = True
            min_x = max_x = x
            min_y = max_y = y
            count = 0
            while q:
                cx, cy = q.popleft()
                count += 1
                min_x = min(min_x, cx)
                max_x = max(max_x, cx)
                min_y = min(min_y, cy)
                max_y = max(max_y, cy)
                for nx in (cx - 1, cx, cx + 1):
                    for ny in (cy - 1, cy, cy + 1):
                        if 0 <= nx < w and 0 <= ny < h and fg[ny][nx] and not vis[ny][nx]:
                            vis[ny][nx] = True
                            q.append((nx, ny))
            if count >= min_pixels:
                out.append(
                    {
                        "x0": min_x,
                        "y0": min_y,
                        "x1": max_x,
                        "y1": max_y,
                        "pixels": count,
                    }
                )
    return out


def _merge_components(rects: list[dict], margin: int = 20) -> list[dict]:
    rects = [r.copy() for r in rects]
    changed = True
    while changed:
        changed = False
        used = [False] * len(rects)
        merged: list[dict] = []
        for i, r in enumerate(rects):
            if used[i]:
                continue
            used[i] = True
            x0, y0, x1, y1, pix = r["x0"], r["y0"], r["x1"], r["y1"], r["pixels"]
            growing = True
            while growing:
                growing = False
                for j, s in enumerate(rects):
                    if used[j]:
                        continue
                    a0, b0, a1, b1 = s["x0"], s["y0"], s["x1"], s["y1"]
                    overlap = not (a0 > x1 + margin or a1 < x0 - margin or b0 > y1 + margin or b1 < y0 - margin)
                    if overlap:
                        used[j] = True
                        x0 = min(x0, a0)
                        y0 = min(y0, b0)
                        x1 = max(x1, a1)
                        y1 = max(y1, b1)
                        pix += s["pixels"]
                        growing = True
                        changed = True
            merged.append({"x0": x0, "y0": y0, "x1": x1, "y1": y1, "pixels": pix})
        rects = merged
    return rects


def _area(r: dict) -> int:
    return (r["x1"] - r["x0"] + 1) * (r["y1"] - r["y0"] + 1)


def _crop_rect(img: Image.Image, r: dict, pad: int = 2) -> Image.Image:
    x0 = max(0, r["x0"] - pad)
    y0 = max(0, r["y0"] - pad)
    x1 = min(img.width, r["x1"] + 1 + pad)
    y1 = min(img.height, r["y1"] + 1 + pad)
    return img.crop((x0, y0, x1, y1))


def _group_rows(rects: list[dict], y_tolerance: int = 80) -> list[list[dict]]:
    sorted_rects = sorted(rects, key=lambda r: (r["y0"] + r["y1"]) / 2)
    rows: list[list[dict]] = []
    row_centers: list[float] = []
    for r in sorted_rects:
        cy = (r["y0"] + r["y1"]) / 2
        placed = False
        for i, rc in enumerate(row_centers):
            if abs(cy - rc) <= y_tolerance:
                rows[i].append(r)
                row_centers[i] = (row_centers[i] * (len(rows[i]) - 1) + cy) / len(rows[i])
                placed = True
                break
        if not placed:
            rows.append([r])
            row_centers.append(cy)
    for row in rows:
        row.sort(key=lambda r: (r["x0"] + r["x1"]) / 2)
    rows.sort(key=lambda row: sum((r["y0"] + r["y1"]) / 2 for r in row) / len(row))
    return rows


def split_ships_and_enemies() -> None:
    atlas = load_rgba(ATLAS_DIR / "SPR_Atlas_Ships_P1.png")
    comps = _components(atlas, min_pixels=120, threshold=28)
    merged = _merge_components(comps, margin=20)
    merged = [r for r in merged if _area(r) >= 4000]
    rows = _group_rows(merged, y_tolerance=85)
    ordered = [r for row in rows for r in row]

    # Expected rows: 3,4,3,1 (11 total). Fallback: y/x sort.
    if len(ordered) != 11:
        ordered = sorted(merged, key=lambda r: (r["y0"], r["x0"]))[:11]

    names_and_targets = [
        ("SPR_Player_Default.png", (128, 128), DIR_PLAYER),
        ("SPR_Player_Damaged.png", (128, 128), DIR_PLAYER),
        ("SPR_Player_Shield.png", (160, 160), DIR_PLAYER),
        ("SPR_Enemy_E01_SmallStraight.png", (96, 96), DIR_ENEMIES),
        ("SPR_Enemy_E02_Diagonal.png", (96, 96), DIR_ENEMIES),
        ("SPR_Enemy_E03_Shooter.png", (128, 128), DIR_ENEMIES),
        ("SPR_Enemy_E04_Assault.png", (96, 96), DIR_ENEMIES),
        ("SPR_Enemy_E05_Armored.png", (160, 160), DIR_ENEMIES),
        ("SPR_Enemy_E06_Spread.png", (128, 128), DIR_ENEMIES),
        ("SPR_Enemy_E07_Tracking.png", (128, 128), DIR_ENEMIES),
        ("SPR_Enemy_E08_Elite.png", (192, 192), DIR_ENEMIES),
    ]

    for r, (name, size, out_dir) in zip(ordered, names_and_targets):
        crop = _crop_rect(atlas, r, pad=2)
        save_processed(crop, out_dir / name, size, remove_black=True, remove_green=True)


def split_bosses() -> None:
    atlas = load_rgba(ATLAS_DIR / "SPR_Atlas_Bosses_P1.png")
    comps = _components(atlas, min_pixels=120, threshold=28)
    merged = _merge_components(comps, margin=22)
    merged = [r for r in merged if _area(r) >= 25000]

    # Generated atlas includes one extra non-boss sprite; keep the 5 largest.
    merged = sorted(merged, key=_area, reverse=True)[:5]
    merged = sorted(merged, key=lambda r: (r["y0"], r["x0"]))

    names_and_targets = [
        ("SPR_Boss_01_PatrolLeader.png", (384, 256)),
        ("SPR_Boss_02_CloudBomber.png", (384, 256)),
        ("SPR_Boss_03_HeavyGunboat.png", (512, 320)),
        ("SPR_Boss_04_TwinWingInterceptor.png", (512, 320)),
        ("SPR_Boss_05_FinalCarrier.png", (640, 384)),
    ]

    for r, (name, size) in zip(merged, names_and_targets):
        crop = _crop_rect(atlas, r, pad=2)
        save_processed(crop, DIR_BOSSES / name, size, remove_black=True, remove_green=True)


def _tint_to(img: Image.Image, rgb: tuple[int, int, int]) -> Image.Image:
    out = img.copy().convert("RGBA")
    px = out.load()
    tr, tg, tb = rgb
    for y in range(out.height):
        for x in range(out.width):
            r, g, b, a = px[x, y]
            if a == 0:
                continue
            # Preserve luminance while steering hue.
            lum = (r + g + b) / 765.0
            nr = int(min(255, tr * (0.35 + 0.9 * lum)))
            ng = int(min(255, tg * (0.35 + 0.9 * lum)))
            nb = int(min(255, tb * (0.35 + 0.9 * lum)))
            px[x, y] = (nr, ng, nb, a)
    return out


def split_projectiles_pickups_icons() -> None:
    atlas = load_rgba(ATLAS_DIR / "SPR_Atlas_Projectiles_Pickups_UIIcons.png")

    # Manual stable crops from this generated atlas (with labels).
    rects = {
        "player_basic": (122, 179, 173, 358),
        "player_power": (291, 142, 376, 365),
        "player_laser": (818, 40, 937, 366),
        "enemy_basic": (484, 219, 554, 357),
        "enemy_fan": (648, 218, 724, 361),
        "pickup_power": (75, 581, 212, 717),
        "pickup_heal": (260, 592, 391, 717),
        "pickup_bomb": (440, 584, 574, 714),
        "pickup_shield": (617, 592, 752, 716),
        "pickup_score": (789, 585, 931, 714),
        "ui_health": (96, 898, 194, 985),
        "ui_bomb": (242, 896, 339, 985),
        "ui_power": (526, 896, 630, 984),
        "ui_score": (682, 895, 782, 985),
    }

    def c(key: str) -> Image.Image:
        x0, y0, x1, y1 = rects[key]
        return atlas.crop((x0, y0, x1, y1))

    bullet_player_basic = remove_keyed_bg(c("player_basic"), remove_black=True, remove_green=True)
    bullet_player_power = remove_keyed_bg(c("player_power"), remove_black=True, remove_green=True)
    bullet_player_laser = remove_keyed_bg(c("player_laser"), remove_black=True, remove_green=True)
    bullet_enemy_basic = remove_keyed_bg(c("enemy_basic"), remove_black=True, remove_green=True)
    bullet_enemy_fan = remove_keyed_bg(c("enemy_fan"), remove_black=True, remove_green=True)
    bullet_enemy_tracking = _tint_to(bullet_enemy_basic, (165, 100, 255))
    bullet_boss_heavy = _tint_to(bullet_enemy_basic, (255, 90, 175))

    bullets = [
        ("SPR_Bullet_Player_Basic.png", bullet_player_basic, (32, 64)),
        ("SPR_Bullet_Player_Power.png", bullet_player_power, (48, 80)),
        ("SPR_Bullet_Player_Laser.png", bullet_player_laser, (64, 256)),
        ("SPR_Bullet_Enemy_Basic.png", bullet_enemy_basic, (32, 32)),
        ("SPR_Bullet_Enemy_Fan.png", bullet_enemy_fan, (32, 32)),
        ("SPR_Bullet_Enemy_Tracking.png", bullet_enemy_tracking, (40, 40)),
        ("SPR_Bullet_Boss_Heavy.png", bullet_boss_heavy, (48, 48)),
    ]
    for name, img, size in bullets:
        keyed = trim_alpha(img, pad=2)
        fit_to_canvas(keyed, size, inner_pad=1).save(DIR_BULLETS / name)

    pickups = [
        ("SPR_Pickup_Power.png", c("pickup_power")),
        ("SPR_Pickup_Heal.png", c("pickup_heal")),
        ("SPR_Pickup_Bomb.png", c("pickup_bomb")),
        ("SPR_Pickup_Shield.png", c("pickup_shield")),
        ("SPR_Pickup_Score.png", c("pickup_score")),
    ]
    for name, img in pickups:
        save_processed(img, DIR_PICKUPS / name, (64, 64), remove_black=True, remove_green=True)

    ui_icons = [
        ("UI_Icon_Health.png", c("ui_health")),
        ("UI_Icon_Bomb.png", c("ui_bomb")),
        ("UI_Icon_Power.png", c("ui_power")),
        ("UI_Icon_Score.png", c("ui_score")),
    ]
    for name, img in ui_icons:
        save_processed(img, DIR_UI / name, (64, 64), remove_black=True, remove_green=True)


def split_ui_elements() -> None:
    atlas = load_rgba(DIR_UI / "SPR_Atlas_UI_Common.png")
    rects = {
        "UI_Button_Normal.png": ((13, 13, 364, 94), (256, 96)),
        "UI_Button_Hover.png": ((14, 249, 363, 330), (256, 96)),
        "UI_Button_Pressed.png": ((381, 249, 729, 327), (256, 96)),
        "UI_Panel_Default.png": ((381, 12, 859, 233), (420, 220)),
        "UI_BossHealthBar_Frame.png": ((14, 346, 505, 404), (420, 48)),
        "UI_BossHealthBar_Fill.png": ((61, 361, 485, 389), (408, 36)),
    }
    for name, (r, size) in rects.items():
        x0, y0, x1, y1 = r
        crop = atlas.crop((x0, y0, x1, y1))
        save_processed(crop, DIR_UI / name, size, remove_black=False, remove_green=False)


def split_vfx_elements() -> None:
    atlas = load_rgba(DIR_VFX / "SPR_Atlas_VFX_P1.png")
    rects = {
        "VFX_Muzzle_Player.png": ((144, 12, 233, 193), (96, 96)),
        "VFX_Muzzle_Enemy.png": ((12, 11, 130, 228), (96, 96)),
        "VFX_Hit_Small.png": ((643, 12, 732, 94), (96, 96)),
        "VFX_Explosion_Small.png": ((499, 12, 630, 109), (128, 128)),
        "VFX_Explosion_Medium.png": ((291, 240, 473, 379), (128, 128)),
        "VFX_Explosion_Boss.png": ((746, 12, 977, 191), (160, 160)),
        "VFX_Pickup.png": ((10, 434, 137, 528), (96, 96)),
        "VFX_Bomb_Clear.png": ((12, 241, 277, 421), (160, 160)),
        "VFX_Shield.png": ((781, 241, 924, 360), (160, 160)),
    }
    for name, (r, size) in rects.items():
        x0, y0, x1, y1 = r
        crop = atlas.crop((x0, y0, x1, y1))
        save_processed(crop, DIR_VFX / name, size, remove_black=True, remove_green=True)


def list_outputs(paths: Iterable[Path]) -> None:
    for p in sorted(paths):
        print(p.relative_to(ROOT).as_posix())


def main() -> None:
    ensure_dirs()
    split_ships_and_enemies()
    split_bosses()
    split_projectiles_pickups_icons()
    split_ui_elements()
    split_vfx_elements()

    created = []
    created.extend(DIR_PLAYER.glob("*.png"))
    created.extend(DIR_ENEMIES.glob("*.png"))
    created.extend(DIR_BOSSES.glob("*.png"))
    created.extend(DIR_BULLETS.glob("*.png"))
    created.extend(DIR_PICKUPS.glob("*.png"))
    created.extend(DIR_UI.glob("UI_*.png"))
    created.extend(DIR_UI.glob("UI_BossHealthBar_*.png"))
    created.extend(DIR_UI.glob("UI_Button_*.png"))
    created.extend(DIR_VFX.glob("VFX_*.png"))
    print("Split completed. Created/updated files:")
    list_outputs(created)


if __name__ == "__main__":
    main()
