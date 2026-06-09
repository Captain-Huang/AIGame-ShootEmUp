# GPT-Image2 作图提示词（省 Token 版）

基于《美术资源需求文档》，目标是用**最少聊天次数**完成 P1 静态美术。

## 1. 使用策略（最省轮次）

- 总共建议 **6 次出图**（6 条提示词）。
- 每次都新开一个会话，直接粘贴对应提示词，不加额外说明。
- 如果工具支持参数：`quality=high`，`size`按每条提示词建议填写。
- 透明图优先让模型直接透明背景；若工具不支持透明，统一用纯色键背景 `#00FF00` 再后处理抠图。

---

## 2. 提示词 01：玩家+敌机总图集

**建议尺寸**：`2048x2048`

```text
Create one sprite atlas image for a 2D arcade sci-fi vertical shoot'em up game, top-down view, clean silhouettes, high readability at small size, transparent background (or flat #00FF00 chroma key background if transparency is unavailable), no text, no watermark.

Include these separate non-overlapping sprites with clear spacing:
1) SPR_Player_Default, 128x128, facing upward, blue/white/cyan.
2) SPR_Player_Damaged, 128x128, facing upward, visible damage marks.
3) SPR_Player_Shield, 160x160, shield ring style.
4) SPR_Enemy_E01_SmallStraight, 96x96, red light enemy, facing downward.
5) SPR_Enemy_E02_Diagonal, 96x96, narrow sharp red enemy, facing downward.
6) SPR_Enemy_E03_Shooter, 128x128, orange medium shooter with visible cannon.
7) SPR_Enemy_E04_Assault, 96x96, fast sharp assault silhouette.
8) SPR_Enemy_E05_Armored, 160x160, gray heavy armor with red core.
9) SPR_Enemy_E06_Spread, 128x128, purple spread enemy with multiple cannon hints.
10) SPR_Enemy_E07_Tracking, 128x128, red-purple tracking enemy with energy core.
11) SPR_Enemy_E08_Elite, 192x192, dark red elite mini-boss feel.

All sprites must be separated, centered in their own areas, and not touching each other.
```

---

## 3. 提示词 02：5 个 Boss 图集

**建议尺寸**：`2048x1024`

```text
Create one boss sprite atlas for a 2D arcade sci-fi vertical shoot'em up, top-down view, transparent background (or flat #00FF00 chroma key), no text, no watermark.

Include exactly 5 separate non-overlapping boss sprites, all facing downward, dark metal body with bright energy accents:
1) SPR_Boss_01_PatrolLeader, 384x256, small flagship, training boss.
2) SPR_Boss_02_CloudBomber, 384x256, bomber style, thick wings, bomb-bay feeling.
3) SPR_Boss_03_HeavyGunboat, 512x320, heavy armor, multiple turrets.
4) SPR_Boss_04_TwinWingInterceptor, 512x320, twin-core high-mobility sharp shape.
5) SPR_Boss_05_FinalCarrier, 640x384, massive final carrier, strongest pressure.

Each boss must have unique silhouette and readable weak/core/cannon areas.
```

---

## 4. 提示词 03：子弹+道具+HUD图标图集

**建议尺寸**：`2048x1024`

```text
Create one sprite atlas for projectiles, pickups, and HUD icons of a 2D arcade sci-fi vertical shoot'em up, transparent background (or flat #00FF00 chroma key), no watermark.

Include these separate non-overlapping sprites:
Projectiles:
- SPR_Bullet_Player_Basic, 32x64, cyan.
- SPR_Bullet_Player_Power, 48x80, blue-white.
- SPR_Bullet_Player_Laser, 64x256, blue-white beam.
- SPR_Bullet_Enemy_Basic, 32x32, red.
- SPR_Bullet_Enemy_Fan, 32x32, orange.
- SPR_Bullet_Enemy_Tracking, 40x40, purple.
- SPR_Bullet_Boss_Heavy, 48x48, red-purple.

Pickups:
- SPR_Pickup_Power, 64x64, red/orange, P symbol.
- SPR_Pickup_Heal, 64x64, green, plus symbol.
- SPR_Pickup_Bomb, 64x64, yellow, bomb symbol.
- SPR_Pickup_Shield, 64x64, blue, shield symbol.
- SPR_Pickup_Score, 64x64, gold, star/coin symbol.

HUD Icons:
- UI_Icon_Health, 64x64.
- UI_Icon_Bomb, 64x64.
- UI_Icon_Power, 64x64.
- UI_Icon_Score, 64x64.

Readability is priority: player bullets and enemy bullets must be instantly distinguishable.
```

---

## 5. 提示词 04：UI 面板+VFX 图集（合并，减少轮次）

**建议尺寸**：`2048x2048`

```text
Create one combined atlas for UI elements and VFX sprites of a 2D sci-fi arcade vertical shoot'em up, transparent background (or flat #00FF00 chroma key), no text labels, no watermark.

UI elements (separate, non-overlapping):
- UI_Button_Normal, 256x96.
- UI_Button_Hover, 256x96.
- UI_Button_Pressed, 256x96.
- UI_Panel_Default, 420x220.
- UI_BossHealthBar_Frame, 420x48.
- UI_BossHealthBar_Fill, 408x36.

VFX sprites (separate, non-overlapping):
- VFX_Muzzle_Player, 96x96, cyan muzzle flash.
- VFX_Muzzle_Enemy, 96x96, orange-red muzzle flash.
- VFX_Hit_Small, 96x96.
- VFX_Explosion_Small, 128x128.
- VFX_Explosion_Medium, 128x128.
- VFX_Explosion_Boss, 160x160.
- VFX_Pickup, 96x96.
- VFX_Bomb_Clear, 160x160, clear wave ring style.
- VFX_Shield, 160x160, shield ring.

Style must stay clean and readable; effects can be bright but not overly noisy.
```

---

## 6. 提示词 05：背景 A（3 合 1，减少轮次）

**建议尺寸**：`3264x1920`（横向三栏，每栏 1088x1920）

```text
Create one wide image containing 3 vertical background panels for a 2D arcade sci-fi vertical shoot'em up.

Canvas layout:
- Left panel: BG_TrainingAirspace (clear sky training airspace, low interference).
- Middle panel: BG_CloudAssault (cloud layers and speed feeling, bright but not harsh).
- Right panel: BG_FireBlockade (warzone firelight and mechanical structures).

Rules:
- Three panels must be visually separated and easy to crop.
- Keep center gameplay lanes readable in each panel (avoid clutter in center).
- Consistent art style across all three panels.
- No text, no watermark.
```

---

## 7. 提示词 06：背景 B（2 合 1，减少轮次）

**建议尺寸**：`2176x1920`（横向两栏，每栏 1088x1920）

```text
Create one wide image containing 2 vertical background panels for a 2D arcade sci-fi vertical shoot'em up.

Canvas layout:
- Left panel: BG_EliteIntercept (night high-altitude interception, enemy base silhouettes).
- Right panel: BG_FinalCarrier (final stage near colossal carrier in space/high-altitude, strongest pressure).

Rules:
- Two panels must be visually separated and easy to crop.
- Keep center gameplay lanes readable for bullet-hell combat.
- Same style family as other stages.
- No text, no watermark.
```

---

## 8. 裁切与落盘（一次性执行）

- 背景 3 合 1 图：按 `1088` 宽等分裁三张，再各自横向居中裁到 `1080x1920`。
- 背景 2 合 1 图：按 `1088` 宽等分裁两张，再各自横向居中裁到 `1080x1920`。
- 命名按需求文档规范：
  - `SPR_Atlas_Ships_P1.png`
  - `SPR_Atlas_Bosses_P1.png`
  - `SPR_Atlas_Projectiles_Pickups_UIIcons.png`
  - `SPR_Atlas_UI_VFX_P1.png`（后续可再拆 UI 与 VFX）
  - `SPR_BG_TrainingAirspace.png`
  - `SPR_BG_CloudAssault.png`
  - `SPR_BG_FireBlockade.png`
  - `SPR_BG_EliteIntercept.png`
  - `SPR_BG_FinalCarrier.png`

---

## 9. 一句话返修模板（只在必要时追加）

当某张图不理想时，追加这一句即可（避免长对话）：

```text
Keep composition and style, but regenerate with clearer silhouette separation, stricter size consistency, and cleaner spacing between sprites; no overlap, no text.
```

