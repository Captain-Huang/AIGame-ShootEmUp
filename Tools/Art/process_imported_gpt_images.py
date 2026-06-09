from __future__ import annotations

from pathlib import Path

from collections import deque

from PIL import Image


ROOT = Path(__file__).resolve().parents[2]
ART_ROOT = ROOT / "Assets" / "Game" / "Art"


def ensure_dirs() -> None:
    for p in [
        ART_ROOT / "SourceGenerated",
        ART_ROOT / "Sprites" / "Atlases",
        ART_ROOT / "Sprites" / "Backgrounds",
        ART_ROOT / "UI",
        ART_ROOT / "VFX",
    ]:
        p.mkdir(parents=True, exist_ok=True)


def fg_mask(img: Image.Image, threshold: int = 14):
    rgb = img.convert("RGB")
    px = rgb.load()
    w, h = rgb.size
    mask = [[False] * w for _ in range(h)]
    for y in range(h):
        for x in range(w):
            r, g, b = px[x, y]
            if r > threshold or g > threshold or b > threshold:
                mask[y][x] = True
    return mask


def rows_density(mask) -> list[int]:
    return [sum(1 for v in row if v) for row in mask]


def trim_bbox(img: Image.Image, threshold: int = 14, pad: int = 8) -> Image.Image:
    rgb = img.convert("RGB")
    px = rgb.load()
    w, h = rgb.size
    min_x, min_y = w, h
    max_x, max_y = -1, -1
    for y in range(h):
        for x in range(w):
            r, g, b = px[x, y]
            if r > threshold or g > threshold or b > threshold:
                min_x = min(min_x, x)
                min_y = min(min_y, y)
                max_x = max(max_x, x)
                max_y = max(max_y, y)
    if max_x < 0 or max_y < 0:
        return img
    min_x = max(0, min_x - pad)
    min_y = max(0, min_y - pad)
    max_x = min(w - 1, max_x + pad)
    max_y = min(h - 1, max_y + pad)
    return img.crop((min_x, min_y, max_x + 1, max_y + 1))


def cover_resize(img: Image.Image, target_w: int, target_h: int) -> Image.Image:
    w, h = img.size
    scale = max(target_w / w, target_h / h)
    nw = int(round(w * scale))
    nh = int(round(h * scale))
    resized = img.resize((nw, nh), Image.Resampling.LANCZOS)
    left = (nw - target_w) // 2
    top = (nh - target_h) // 2
    return resized.crop((left, top, left + target_w, top + target_h))


def split_vertical_panels(img: Image.Image, n: int) -> list[Image.Image]:
    w, h = img.size
    panels = []
    x0 = 0
    for i in range(n):
        # Use exact partition to avoid cumulative rounding drift.
        x1 = round((i + 1) * w / n)
        panel = img.crop((x0, 0, x1, h))
        panels.append(panel)
        x0 = x1
    return panels


def process_ui_vfx_combined(src: Path, ui_out: Path, vfx_out: Path) -> None:
    img = Image.open(src).convert("RGBA")
    w, h = img.size
    px = img.load()

    # Chroma-key cleanup: remove green backdrop and reduce green spill.
    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            if g > 80 and g > r + 35 and g > b + 35:
                px[x, y] = (r, g, b, 0)
            else:
                if g > r and g > b:
                    g2 = max(r, b)
                else:
                    g2 = g
                px[x, y] = (r, g2, b, a)

    alpha = img.split()[-1]
    a = alpha.load()

    fg = [[a[x, y] > 20 for x in range(w)] for y in range(h)]
    vis = [[False] * w for _ in range(h)]
    components: list[dict] = []

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
                for nx, ny in ((cx + 1, cy), (cx - 1, cy), (cx, cy + 1), (cx, cy - 1)):
                    if 0 <= nx < w and 0 <= ny < h and fg[ny][nx] and not vis[ny][nx]:
                        vis[ny][nx] = True
                        q.append((nx, ny))

            if count < 180:
                continue
            components.append(
                {
                    "count": count,
                    "min_x": min_x,
                    "min_y": min_y,
                    "max_x": max_x,
                    "max_y": max_y,
                    "w": max_x - min_x + 1,
                    "h": max_y - min_y + 1,
                }
            )

    # UI components are the 5 large panel/bar elements near the top area.
    ui_components = [c for c in components if c["max_y"] <= 420 and c["w"] >= 200]
    vfx_components = [c for c in components if c not in ui_components]

    if len(ui_components) < 5 or len(vfx_components) == 0:
        # Fallback to simple split if component detection fails unexpectedly.
        mask = fg_mask(img)
        density = rows_density(mask)
        lo = max(120, h // 5)
        hi = min(h - 120, h * 4 // 5)
        split_y = min(range(lo, hi), key=lambda y: density[y])
        trim_bbox(img.crop((0, 0, img.width, split_y))).save(ui_out)
        trim_bbox(img.crop((0, split_y, img.width, img.height))).save(vfx_out)
        return

    def pack_components(comps: list[dict], out_path: Path, atlas_w: int = 1024, pad: int = 12) -> None:
        comps = sorted(comps, key=lambda c: (c["min_y"], c["min_x"]))
        crops = []
        for c in comps:
            l = max(0, c["min_x"] - 2)
            t = max(0, c["min_y"] - 2)
            r = min(w, c["max_x"] + 3)
            b = min(h, c["max_y"] + 3)
            crops.append(img.crop((l, t, r, b)))

        x = pad
        y = pad
        row_h = 0
        placed = []
        for crop in crops:
            cw, ch = crop.size
            if x + cw + pad > atlas_w:
                x = pad
                y += row_h + pad
                row_h = 0
            placed.append((crop, x, y))
            x += cw + pad
            row_h = max(row_h, ch)
        atlas_h = y + row_h + pad
        atlas = Image.new("RGBA", (atlas_w, atlas_h), (0, 0, 0, 0))
        for crop, px_, py_ in placed:
            atlas.alpha_composite(crop, (px_, py_))
        atlas.save(out_path)

    pack_components(ui_components, ui_out, atlas_w=1024, pad=10)
    pack_components(vfx_components, vfx_out, atlas_w=1024, pad=10)


def main() -> None:
    ensure_dirs()
    source_dir = ART_ROOT / "SourceGenerated"
    expected_sources = [
        source_dir / "GEN_Prompt01_ShipsEnemies.png",
        source_dir / "GEN_Prompt02_Bosses.png",
        source_dir / "GEN_Prompt03_ProjectilesPickupsIcons.png",
        source_dir / "GEN_Prompt04_UIVFXCombined.png",
        source_dir / "GEN_Prompt05_BG_3Panels.png",
        source_dir / "GEN_Prompt06_BG_2Panels.png",
    ]

    if all(p.exists() for p in expected_sources):
        src_targets = expected_sources
    else:
        root_pngs = sorted(
            [p for p in ART_ROOT.glob("*.png") if p.is_file()],
            key=lambda p: p.stat().st_mtime,
        )
        if len(root_pngs) < 6:
            raise SystemExit(f"Expected at least 6 PNG files under {ART_ROOT}, found {len(root_pngs)}")
        p1, p2, p3, p4, p5, p6 = root_pngs[:6]
        src_targets = expected_sources
        for src, dst in zip([p1, p2, p3, p4, p5, p6], src_targets):
            if dst.exists():
                dst.unlink()
            src.rename(dst)

    # Atlases
    (ART_ROOT / "Sprites" / "Atlases" / "SPR_Atlas_Ships_P1.png").write_bytes(src_targets[0].read_bytes())
    (ART_ROOT / "Sprites" / "Atlases" / "SPR_Atlas_Bosses_P1.png").write_bytes(src_targets[1].read_bytes())
    (ART_ROOT / "Sprites" / "Atlases" / "SPR_Atlas_Projectiles_Pickups_UIIcons.png").write_bytes(src_targets[2].read_bytes())

    # Split combined UI+VFX atlas.
    process_ui_vfx_combined(
        src_targets[3],
        ART_ROOT / "UI" / "SPR_Atlas_UI_Common.png",
        ART_ROOT / "VFX" / "SPR_Atlas_VFX_P1.png",
    )

    # Split background 3-panel source: Training / Cloud / Fire
    bg3 = Image.open(src_targets[4]).convert("RGBA")
    panels3 = split_vertical_panels(bg3, 3)
    bg_names_3 = [
        "SPR_BG_TrainingAirspace.png",
        "SPR_BG_CloudAssault.png",
        "SPR_BG_FireBlockade.png",
    ]
    for panel, name in zip(panels3, bg_names_3):
        out = ART_ROOT / "Sprites" / "Backgrounds" / name
        cover_resize(panel, 1080, 1920).save(out)

    # Split background 2-panel source: Elite / Final
    bg2 = Image.open(src_targets[5]).convert("RGBA")
    panels2 = split_vertical_panels(bg2, 2)
    bg_names_2 = [
        "SPR_BG_EliteIntercept.png",
        "SPR_BG_FinalCarrier.png",
    ]
    for panel, name in zip(panels2, bg_names_2):
        out = ART_ROOT / "Sprites" / "Backgrounds" / name
        cover_resize(panel, 1080, 1920).save(out)

    print("Processed 6 imported GPT images.")
    for p in src_targets:
        print(f"source: {p.relative_to(ROOT).as_posix()}")
    print("outputs:")
    print("  Assets/Game/Art/Sprites/Atlases/SPR_Atlas_Ships_P1.png")
    print("  Assets/Game/Art/Sprites/Atlases/SPR_Atlas_Bosses_P1.png")
    print("  Assets/Game/Art/Sprites/Atlases/SPR_Atlas_Projectiles_Pickups_UIIcons.png")
    print("  Assets/Game/Art/UI/SPR_Atlas_UI_Common.png")
    print("  Assets/Game/Art/VFX/SPR_Atlas_VFX_P1.png")
    print("  Assets/Game/Art/Sprites/Backgrounds/SPR_BG_TrainingAirspace.png")
    print("  Assets/Game/Art/Sprites/Backgrounds/SPR_BG_CloudAssault.png")
    print("  Assets/Game/Art/Sprites/Backgrounds/SPR_BG_FireBlockade.png")
    print("  Assets/Game/Art/Sprites/Backgrounds/SPR_BG_EliteIntercept.png")
    print("  Assets/Game/Art/Sprites/Backgrounds/SPR_BG_FinalCarrier.png")


if __name__ == "__main__":
    main()
