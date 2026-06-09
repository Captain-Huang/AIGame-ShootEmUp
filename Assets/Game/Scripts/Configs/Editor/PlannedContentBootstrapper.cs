using System.IO;
using AIGame.ShootEmUp.Bosses;
using AIGame.ShootEmUp.Bullets;
using AIGame.ShootEmUp.Core;
using AIGame.ShootEmUp.Enemies;
using AIGame.ShootEmUp.Pickups;
using AIGame.ShootEmUp.Player;
using AIGame.ShootEmUp.UI;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace AIGame.ShootEmUp.Configs.Editor
{
    internal static class PlannedContentBootstrapper
    {
        private const string PrefabRoot = "Assets/Game/Prefabs";
        private const string AutoGenerateGuardKey = "AIGame.ShootEmUp.PlannedContentBootstrapper.IsAutoGenerating";

        [DidReloadScripts]
        private static void AutoGenerateWhenMissing()
        {
            var hasLevelDatabase = AssetDatabase.LoadAssetAtPath<LevelDatabase>(ConfigAssetPaths.LevelDatabase) != null;
            var hasPlayerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabRoot}/Player/Player.prefab") != null;
            if (hasLevelDatabase && hasPlayerPrefab)
            {
                return;
            }

            if (SessionState.GetBool(AutoGenerateGuardKey, false))
            {
                return;
            }

            SessionState.SetBool(AutoGenerateGuardKey, true);
            try
            {
                GeneratePlannedContent();
            }
            finally
            {
                SessionState.SetBool(AutoGenerateGuardKey, false);
            }
        }

        // [MenuItem("Tools/ShootEmUp/Generate Planned Configs And Prefabs")]
        public static void GeneratePlannedContent()
        {
            ConfigAssetBootstrapper.EnsureDefaultsExist();
            EnsurePrefabDirectories();

            var refs = EnsureExtendedConfigs();
            var prefabs = EnsurePrefabs();
            BindConfigAndPresentation(refs, prefabs);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("ShootEmUp planned configs and prefabs are generated and bound.");
        }

        private static ContentRefs EnsureExtendedConfigs()
        {
            var refs = new ContentRefs
            {
                Player = AssetDatabase.LoadAssetAtPath<PlayerConfig>(ConfigAssetPaths.PlayerDefault),
                Weapon = AssetDatabase.LoadAssetAtPath<WeaponConfig>(ConfigAssetPaths.WeaponPlayerDefault),
                BulletPlayerBasic = AssetDatabase.LoadAssetAtPath<BulletConfig>(ConfigAssetPaths.BulletPlayerBasic),
                BulletEnemyBasic = AssetDatabase.LoadAssetAtPath<BulletConfig>(ConfigAssetPaths.BulletEnemyBasic),

                BulletPlayerPower = CreateIfMissing<BulletConfig>(ConfigAssetPaths.BulletPlayerPower, asset =>
                {
                    asset.bulletId = "Bullet_PlayerPower";
                    asset.speed = 14f;
                    asset.damage = 2;
                    asset.lifetime = 2.5f;
                    asset.radius = 0.14f;
                    asset.tint = new Color(0.75f, 0.93f, 1f, 1f);
                    asset.size = new Vector2(0.24f, 0.48f);
                    asset.pierceCount = 0;
                }),
                BulletPlayerLaser = CreateIfMissing<BulletConfig>(ConfigAssetPaths.BulletPlayerLaser, asset =>
                {
                    asset.bulletId = "Bullet_PlayerLaser";
                    asset.speed = 18f;
                    asset.damage = 3;
                    asset.lifetime = 1.8f;
                    asset.radius = 0.18f;
                    asset.tint = new Color(0.78f, 0.94f, 1f, 1f);
                    asset.size = new Vector2(0.32f, 1.28f);
                    asset.pierceCount = 1;
                }),
                BulletEnemyFan = CreateIfMissing<BulletConfig>(ConfigAssetPaths.BulletEnemyFan, asset =>
                {
                    asset.bulletId = "Bullet_EnemyFan";
                    asset.speed = 6.4f;
                    asset.damage = 1;
                    asset.lifetime = 4.2f;
                    asset.radius = 0.11f;
                    asset.tint = new Color(1f, 0.6f, 0.2f, 1f);
                    asset.size = new Vector2(0.2f, 0.2f);
                }),
                BulletEnemyTracking = CreateIfMissing<BulletConfig>(ConfigAssetPaths.BulletEnemyTracking, asset =>
                {
                    asset.bulletId = "Bullet_EnemyTracking";
                    asset.speed = 5.7f;
                    asset.damage = 1;
                    asset.lifetime = 4.6f;
                    asset.radius = 0.12f;
                    asset.tint = new Color(0.68f, 0.45f, 1f, 1f);
                    asset.size = new Vector2(0.26f, 0.26f);
                }),
                BulletBossHeavy = CreateIfMissing<BulletConfig>(ConfigAssetPaths.BulletBossHeavy, asset =>
                {
                    asset.bulletId = "Bullet_BossHeavy";
                    asset.speed = 5f;
                    asset.damage = 2;
                    asset.lifetime = 5f;
                    asset.radius = 0.16f;
                    asset.tint = new Color(0.95f, 0.35f, 0.62f, 1f);
                    asset.size = new Vector2(0.3f, 0.3f);
                }),

                EnemyE01 = AssetDatabase.LoadAssetAtPath<EnemyConfig>(ConfigAssetPaths.EnemyE01),
                EnemyE02 = AssetDatabase.LoadAssetAtPath<EnemyConfig>(ConfigAssetPaths.EnemyE02),
                EnemyE03 = AssetDatabase.LoadAssetAtPath<EnemyConfig>(ConfigAssetPaths.EnemyE03),
                EnemyE04 = CreateIfMissing<EnemyConfig>(ConfigAssetPaths.EnemyE04, asset =>
                {
                    asset.enemyId = "E04";
                    asset.displayName = "Assault";
                    asset.maxHealth = 2;
                    asset.moveSpeed = 3.2f;
                    asset.contactDamage = 1;
                    asset.score = 180;
                    asset.spawnWeight = 20;
                    asset.movementPattern = MovementPattern.DiagonalRight;
                    asset.firePattern = FirePattern.None;
                    asset.tint = new Color(0.95f, 0.42f, 0.2f, 1f);
                    asset.size = new Vector2(0.62f, 0.62f);
                }),
                EnemyE05 = CreateIfMissing<EnemyConfig>(ConfigAssetPaths.EnemyE05, asset =>
                {
                    asset.enemyId = "E05";
                    asset.displayName = "Armored";
                    asset.maxHealth = 7;
                    asset.moveSpeed = 1.7f;
                    asset.contactDamage = 2;
                    asset.score = 420;
                    asset.spawnWeight = 14;
                    asset.movementPattern = MovementPattern.StopAndShoot;
                    asset.firePattern = FirePattern.SingleForward;
                    asset.fireInterval = 1.3f;
                    asset.tint = new Color(0.62f, 0.66f, 0.72f, 1f);
                    asset.size = new Vector2(1f, 1f);
                }),
                EnemyE06 = CreateIfMissing<EnemyConfig>(ConfigAssetPaths.EnemyE06, asset =>
                {
                    asset.enemyId = "E06";
                    asset.displayName = "Spread";
                    asset.maxHealth = 5;
                    asset.moveSpeed = 2f;
                    asset.contactDamage = 1;
                    asset.score = 380;
                    asset.spawnWeight = 14;
                    asset.movementPattern = MovementPattern.StopAndShoot;
                    asset.firePattern = FirePattern.FiveFan;
                    asset.fireInterval = 1.1f;
                    asset.tint = new Color(0.58f, 0.42f, 0.95f, 1f);
                    asset.size = new Vector2(0.82f, 0.82f);
                }),
                EnemyE07 = CreateIfMissing<EnemyConfig>(ConfigAssetPaths.EnemyE07, asset =>
                {
                    asset.enemyId = "E07";
                    asset.displayName = "Tracking";
                    asset.maxHealth = 4;
                    asset.moveSpeed = 2.1f;
                    asset.contactDamage = 1;
                    asset.score = 360;
                    asset.spawnWeight = 12;
                    asset.movementPattern = MovementPattern.TrackPlayerX;
                    asset.firePattern = FirePattern.AimedSingle;
                    asset.fireInterval = 1f;
                    asset.tint = new Color(0.86f, 0.3f, 0.46f, 1f);
                    asset.size = new Vector2(0.86f, 0.86f);
                }),
                EnemyE08 = CreateIfMissing<EnemyConfig>(ConfigAssetPaths.EnemyE08, asset =>
                {
                    asset.enemyId = "E08";
                    asset.displayName = "Elite";
                    asset.maxHealth = 12;
                    asset.moveSpeed = 1.8f;
                    asset.contactDamage = 2;
                    asset.score = 700;
                    asset.spawnWeight = 8;
                    asset.movementPattern = MovementPattern.Sine;
                    asset.firePattern = FirePattern.AimedTriple;
                    asset.fireInterval = 0.85f;
                    asset.tint = new Color(0.82f, 0.24f, 0.24f, 1f);
                    asset.size = new Vector2(1.18f, 1.18f);
                }),

                Boss01 = AssetDatabase.LoadAssetAtPath<BossConfig>(ConfigAssetPaths.Boss01),
                Boss02 = AssetDatabase.LoadAssetAtPath<BossConfig>(ConfigAssetPaths.Boss02),
                Boss03 = AssetDatabase.LoadAssetAtPath<BossConfig>(ConfigAssetPaths.Boss03),
                Boss04 = AssetDatabase.LoadAssetAtPath<BossConfig>(ConfigAssetPaths.Boss04),
                Boss05 = AssetDatabase.LoadAssetAtPath<BossConfig>(ConfigAssetPaths.Boss05),

                PickupPower = AssetDatabase.LoadAssetAtPath<PickupConfig>(ConfigAssetPaths.PickupPowerUp),
                PickupHeal = AssetDatabase.LoadAssetAtPath<PickupConfig>(ConfigAssetPaths.PickupHeal),
                PickupBomb = AssetDatabase.LoadAssetAtPath<PickupConfig>(ConfigAssetPaths.PickupBomb),
                PickupShield = AssetDatabase.LoadAssetAtPath<PickupConfig>(ConfigAssetPaths.PickupShield),
                PickupScore = CreateIfMissing<PickupConfig>(ConfigAssetPaths.PickupScore, asset =>
                {
                    asset.pickupId = "Pickup_Score";
                    asset.type = PickupType.Score;
                    asset.value = 100;
                    asset.duration = 0f;
                    asset.moveSpeed = 1.7f;
                }),

                Level01 = AssetDatabase.LoadAssetAtPath<LevelConfig>(ConfigAssetPaths.Level01),
                Level02 = AssetDatabase.LoadAssetAtPath<LevelConfig>(ConfigAssetPaths.Level02),
                Level03 = AssetDatabase.LoadAssetAtPath<LevelConfig>(ConfigAssetPaths.Level03),
                Level04 = AssetDatabase.LoadAssetAtPath<LevelConfig>(ConfigAssetPaths.Level04),
                Level05 = AssetDatabase.LoadAssetAtPath<LevelConfig>(ConfigAssetPaths.Level05),
                LevelDatabase = AssetDatabase.LoadAssetAtPath<LevelDatabase>(ConfigAssetPaths.LevelDatabase)
            };

            if (refs.Weapon != null)
            {
                refs.Weapon.fireInterval = 0.16f;
                refs.Weapon.bombDamage = 42;
                refs.Weapon.bombBossDamage = 22;
                refs.Weapon.powerLevels = new[]
                {
                    new WeaponPowerLevel
                    {
                        level = 1,
                        bulletConfig = refs.BulletPlayerBasic,
                        angles = new[] { 0f },
                        offsets = new[] { Vector2.zero }
                    },
                    new WeaponPowerLevel
                    {
                        level = 2,
                        bulletConfig = refs.BulletPlayerBasic,
                        angles = new[] { -4f, 4f },
                        offsets = new[] { new Vector2(-0.12f, 0f), new Vector2(0.12f, 0f) }
                    },
                    new WeaponPowerLevel
                    {
                        level = 3,
                        bulletConfig = refs.BulletPlayerPower,
                        angles = new[] { -10f, 0f, 10f },
                        offsets = new[] { new Vector2(-0.16f, 0f), Vector2.zero, new Vector2(0.16f, 0f) }
                    },
                    new WeaponPowerLevel
                    {
                        level = 4,
                        bulletConfig = refs.BulletPlayerPower,
                        angles = new[] { -16f, -8f, 0f, 8f, 16f },
                        offsets = new[]
                        {
                            new Vector2(-0.24f, 0f),
                            new Vector2(-0.12f, 0f),
                            Vector2.zero,
                            new Vector2(0.12f, 0f),
                            new Vector2(0.24f, 0f)
                        }
                    }
                };
                EditorUtility.SetDirty(refs.Weapon);
            }

            if (refs.Player != null)
            {
                refs.Player.maxPowerLevel = 4;
                refs.Player.weaponConfig = refs.Weapon;
                EditorUtility.SetDirty(refs.Player);
            }

            // Wire richer bullet configs for advanced enemies/bosses.
            if (refs.EnemyE05 != null)
            {
                refs.EnemyE05.bulletConfig = refs.BulletEnemyBasic;
                EditorUtility.SetDirty(refs.EnemyE05);
            }

            if (refs.EnemyE06 != null)
            {
                refs.EnemyE06.bulletConfig = refs.BulletEnemyFan;
                EditorUtility.SetDirty(refs.EnemyE06);
            }

            if (refs.EnemyE07 != null)
            {
                refs.EnemyE07.bulletConfig = refs.BulletEnemyTracking;
                EditorUtility.SetDirty(refs.EnemyE07);
            }

            if (refs.EnemyE08 != null)
            {
                refs.EnemyE08.bulletConfig = refs.BulletEnemyFan;
                EditorUtility.SetDirty(refs.EnemyE08);
            }

            var allEnemies = new[]
            {
                refs.EnemyE01, refs.EnemyE02, refs.EnemyE03, refs.EnemyE04,
                refs.EnemyE05, refs.EnemyE06, refs.EnemyE07, refs.EnemyE08
            };

            foreach (var enemy in allEnemies)
            {
                if (enemy == null)
                {
                    continue;
                }

                EnsureDropIncludesScore(enemy, refs.PickupScore, 0.08f);
            }

            return refs;
        }

        private static PrefabRefs EnsurePrefabs()
        {
            var refs = new PrefabRefs
            {
                Player = BuildPlayerPrefab(),

                PlayerBullet = BuildBulletPrefab("PlayerBullet", "Assets/Game/Art/Sprites/Bullets/SPR_Bullet_Player_Basic.png", "PlayerBullet", 10),
                EnemyBullet = BuildBulletPrefab("EnemyBullet", "Assets/Game/Art/Sprites/Bullets/SPR_Bullet_Enemy_Basic.png", "EnemyBullet", 8),
                PlayerBulletPower = BuildBulletPrefab("Bullet_PlayerPower", "Assets/Game/Art/Sprites/Bullets/SPR_Bullet_Player_Power.png", "PlayerBullet", 10),
                PlayerBulletLaser = BuildBulletPrefab("Bullet_PlayerLaser", "Assets/Game/Art/Sprites/Bullets/SPR_Bullet_Player_Laser.png", "PlayerBullet", 10),
                EnemyBulletFan = BuildBulletPrefab("Bullet_EnemyFan", "Assets/Game/Art/Sprites/Bullets/SPR_Bullet_Enemy_Fan.png", "EnemyBullet", 8),
                EnemyBulletTracking = BuildBulletPrefab("Bullet_EnemyTracking", "Assets/Game/Art/Sprites/Bullets/SPR_Bullet_Enemy_Tracking.png", "EnemyBullet", 8),
                BossBulletHeavy = BuildBulletPrefab("Bullet_BossHeavy", "Assets/Game/Art/Sprites/Bullets/SPR_Bullet_Boss_Heavy.png", "EnemyBullet", 8),

                EnemyE01 = BuildEnemyPrefab("Enemy_E01_SmallStraight", "Assets/Game/Art/Sprites/Enemies/SPR_Enemy_E01_SmallStraight.png"),
                EnemyE02 = BuildEnemyPrefab("Enemy_E02_Diagonal", "Assets/Game/Art/Sprites/Enemies/SPR_Enemy_E02_Diagonal.png"),
                EnemyE03 = BuildEnemyPrefab("Enemy_E03_Shooter", "Assets/Game/Art/Sprites/Enemies/SPR_Enemy_E03_Shooter.png"),
                EnemyE04 = BuildEnemyPrefab("Enemy_E04_Assault", "Assets/Game/Art/Sprites/Enemies/SPR_Enemy_E04_Assault.png"),
                EnemyE05 = BuildEnemyPrefab("Enemy_E05_Armored", "Assets/Game/Art/Sprites/Enemies/SPR_Enemy_E05_Armored.png"),
                EnemyE06 = BuildEnemyPrefab("Enemy_E06_Spread", "Assets/Game/Art/Sprites/Enemies/SPR_Enemy_E06_Spread.png"),
                EnemyE07 = BuildEnemyPrefab("Enemy_E07_Tracking", "Assets/Game/Art/Sprites/Enemies/SPR_Enemy_E07_Tracking.png"),
                EnemyE08 = BuildEnemyPrefab("Enemy_E08_Elite", "Assets/Game/Art/Sprites/Enemies/SPR_Enemy_E08_Elite.png"),

                Boss01 = BuildBossPrefab("Boss_01_PatrolLeader", "Assets/Game/Art/Sprites/Bosses/SPR_Boss_01_PatrolLeader.png"),
                Boss02 = BuildBossPrefab("Boss_02_CloudBomber", "Assets/Game/Art/Sprites/Bosses/SPR_Boss_02_CloudBomber.png"),
                Boss03 = BuildBossPrefab("Boss_03_HeavyGunship", "Assets/Game/Art/Sprites/Bosses/SPR_Boss_03_HeavyGunboat.png"),
                Boss04 = BuildBossPrefab("Boss_04_TwinInterceptor", "Assets/Game/Art/Sprites/Bosses/SPR_Boss_04_TwinWingInterceptor.png"),
                Boss05 = BuildBossPrefab("Boss_05_StarCarrier", "Assets/Game/Art/Sprites/Bosses/SPR_Boss_05_FinalCarrier.png"),

                PickupPower = BuildPickupPrefab("Pickup_PowerUp", "Assets/Game/Art/Sprites/Pickups/SPR_Pickup_Power.png"),
                PickupHeal = BuildPickupPrefab("Pickup_Heal", "Assets/Game/Art/Sprites/Pickups/SPR_Pickup_Heal.png"),
                PickupBomb = BuildPickupPrefab("Pickup_Bomb", "Assets/Game/Art/Sprites/Pickups/SPR_Pickup_Bomb.png"),
                PickupShield = BuildPickupPrefab("Pickup_Shield", "Assets/Game/Art/Sprites/Pickups/SPR_Pickup_Shield.png"),
                PickupScore = BuildPickupPrefab("Pickup_Score", "Assets/Game/Art/Sprites/Pickups/SPR_Pickup_Score.png"),

                ExplosionSmall = BuildSimpleSpritePrefab("Explosion_Small", "Assets/Game/Art/VFX/VFX_Explosion_Small.png", "VFX", 30, false),
                GameManagers = BuildGameManagersPrefab()
            };

            return refs;
        }

        private static void BindConfigAndPresentation(ContentRefs refs, PrefabRefs prefabs)
        {
            if (refs.Player != null)
            {
                refs.Player.prefab = prefabs.Player;
                refs.Player.weaponConfig = refs.Weapon;
                EditorUtility.SetDirty(refs.Player);
            }

            BindBullet(refs.BulletPlayerBasic, prefabs.PlayerBullet);
            BindBullet(refs.BulletPlayerPower, prefabs.PlayerBulletPower);
            BindBullet(refs.BulletPlayerLaser, prefabs.PlayerBulletLaser);
            BindBullet(refs.BulletEnemyBasic, prefabs.EnemyBullet);
            BindBullet(refs.BulletEnemyFan, prefabs.EnemyBulletFan);
            BindBullet(refs.BulletEnemyTracking, prefabs.EnemyBulletTracking);
            BindBullet(refs.BulletBossHeavy, prefabs.BossBulletHeavy);

            BindEnemy(refs.EnemyE01, prefabs.EnemyE01);
            BindEnemy(refs.EnemyE02, prefabs.EnemyE02);
            BindEnemy(refs.EnemyE03, prefabs.EnemyE03);
            BindEnemy(refs.EnemyE04, prefabs.EnemyE04);
            BindEnemy(refs.EnemyE05, prefabs.EnemyE05);
            BindEnemy(refs.EnemyE06, prefabs.EnemyE06);
            BindEnemy(refs.EnemyE07, prefabs.EnemyE07);
            BindEnemy(refs.EnemyE08, prefabs.EnemyE08);

            BindBoss(refs.Boss01, prefabs.Boss01, refs.BulletEnemyBasic);
            BindBoss(refs.Boss02, prefabs.Boss02, refs.BulletEnemyFan);
            BindBoss(refs.Boss03, prefabs.Boss03, refs.BulletEnemyFan);
            BindBoss(refs.Boss04, prefabs.Boss04, refs.BulletEnemyTracking);
            BindBoss(refs.Boss05, prefabs.Boss05, refs.BulletBossHeavy);

            BindPickup(refs.PickupPower, prefabs.PickupPower, "Assets/Game/Art/UI/UI_Icon_Power.png");
            BindPickup(refs.PickupHeal, prefabs.PickupHeal, "Assets/Game/Art/UI/UI_Icon_Health.png");
            BindPickup(refs.PickupBomb, prefabs.PickupBomb, "Assets/Game/Art/UI/UI_Icon_Bomb.png");
            BindPickup(refs.PickupShield, prefabs.PickupShield, "Assets/Game/Art/Sprites/Pickups/SPR_Pickup_Shield.png");
            BindPickup(refs.PickupScore, prefabs.PickupScore, "Assets/Game/Art/UI/UI_Icon_Score.png");

            var backgroundPaths = new[]
            {
                "Assets/Game/Art/Sprites/Backgrounds/SPR_BG_TrainingAirspace.png",
                "Assets/Game/Art/Sprites/Backgrounds/SPR_BG_CloudAssault.png",
                "Assets/Game/Art/Sprites/Backgrounds/SPR_BG_FireBlockade.png",
                "Assets/Game/Art/Sprites/Backgrounds/SPR_BG_EliteIntercept.png",
                "Assets/Game/Art/Sprites/Backgrounds/SPR_BG_FinalCarrier.png"
            };
            var bosses = new[] { refs.Boss01, refs.Boss02, refs.Boss03, refs.Boss04, refs.Boss05 };
            var levels = new[] { refs.Level01, refs.Level02, refs.Level03, refs.Level04, refs.Level05 };

            for (var i = 0; i < levels.Length; i++)
            {
                var level = levels[i];
                if (level == null)
                {
                    continue;
                }

                level.backgroundSprite = LoadSprite(backgroundPaths[i]);
                level.bossConfig = bosses[i];
                EditorUtility.SetDirty(level);
            }

            if (refs.LevelDatabase != null)
            {
                refs.LevelDatabase.levels = levels;
                refs.LevelDatabase.firstLevel = refs.Level01 != null ? refs.Level01 : levels[0];
                EditorUtility.SetDirty(refs.LevelDatabase);
            }
        }

        private static void BindBullet(BulletConfig config, GameObject prefab)
        {
            if (config == null)
            {
                return;
            }

            config.prefab = prefab;
            EditorUtility.SetDirty(config);
        }

        private static void BindEnemy(EnemyConfig config, GameObject prefab)
        {
            if (config == null)
            {
                return;
            }

            config.prefab = prefab;
            EditorUtility.SetDirty(config);
        }

        private static void BindBoss(BossConfig config, GameObject prefab, BulletConfig fallbackBullet)
        {
            if (config == null)
            {
                return;
            }

            config.prefab = prefab;
            if (config.phases != null)
            {
                for (var i = 0; i < config.phases.Length; i++)
                {
                    if (config.phases[i] == null || config.phases[i].bulletConfig != null)
                    {
                        continue;
                    }

                    config.phases[i].bulletConfig = fallbackBullet;
                }
            }

            EditorUtility.SetDirty(config);
        }

        private static void BindPickup(PickupConfig config, GameObject prefab, string iconSpritePath)
        {
            if (config == null)
            {
                return;
            }

            config.prefab = prefab;
            config.icon = LoadSprite(iconSpritePath);
            EditorUtility.SetDirty(config);
        }

        private static void EnsureDropIncludesScore(EnemyConfig enemy, PickupConfig scorePickup, float chance)
        {
            if (enemy == null || scorePickup == null)
            {
                return;
            }

            if (enemy.dropTable == null || enemy.dropTable.Length == 0)
            {
                enemy.dropTable = new[]
                {
                    new PickupDropEntry { pickup = scorePickup, dropChance = Mathf.Clamp01(chance) }
                };
                EditorUtility.SetDirty(enemy);
                return;
            }

            for (var i = 0; i < enemy.dropTable.Length; i++)
            {
                if (enemy.dropTable[i].pickup == scorePickup)
                {
                    return;
                }
            }

            var newTable = new PickupDropEntry[enemy.dropTable.Length + 1];
            for (var i = 0; i < enemy.dropTable.Length; i++)
            {
                newTable[i] = enemy.dropTable[i];
            }

            newTable[newTable.Length - 1] = new PickupDropEntry
            {
                pickup = scorePickup,
                dropChance = Mathf.Clamp01(chance)
            };
            enemy.dropTable = newTable;
            EditorUtility.SetDirty(enemy);
        }

        private static GameObject BuildPlayerPrefab()
        {
            var sprite = LoadSprite("Assets/Game/Art/Sprites/Player/SPR_Player_Default.png");
            var go = new GameObject("Player");
            SetLayer(go, "Player");

            SetupSpriteRenderer(go, sprite, 20);
            SetupPhysics(go, useCircleCollider: true, colliderSize: new Vector2(0.7f, 0.7f));

            AddOrGetComponent<PlayerController>(go);
            AddOrGetComponent<PlayerHealth>(go);
            AddOrGetComponent<PlayerWeapon>(go);
            AddOrGetComponent<PlayerPickupCollector>(go);

            return SaveTempPrefab(go, $"{PrefabRoot}/Player/Player.prefab");
        }

        private static GameObject BuildEnemyPrefab(string prefabName, string spritePath)
        {
            var go = new GameObject(prefabName);
            SetLayer(go, "Enemy");
            SetupSpriteRenderer(go, LoadSprite(spritePath), 6);
            SetupPhysics(go, useCircleCollider: false, colliderSize: new Vector2(0.8f, 0.8f));

            AddOrGetComponent<EnemyBase>(go);
            AddOrGetComponent<EnemyMovement>(go);
            AddOrGetComponent<EnemyWeapon>(go);

            return SaveTempPrefab(go, $"{PrefabRoot}/Enemies/{prefabName}.prefab");
        }

        private static GameObject BuildBossPrefab(string prefabName, string spritePath)
        {
            var go = new GameObject(prefabName);
            SetLayer(go, "Enemy");
            SetupSpriteRenderer(go, LoadSprite(spritePath), 7);
            SetupPhysics(go, useCircleCollider: false, colliderSize: new Vector2(1.8f, 1f));

            AddOrGetComponent<BossController>(go);
            AddOrGetComponent<BossPhaseRunner>(go);
            AddOrGetComponent<EnemyWeapon>(go);

            return SaveTempPrefab(go, $"{PrefabRoot}/Bosses/{prefabName}.prefab");
        }

        private static GameObject BuildBulletPrefab(string prefabName, string spritePath, string layerName, int sortingOrder)
        {
            var go = new GameObject(prefabName);
            SetLayer(go, layerName);
            SetupSpriteRenderer(go, LoadSprite(spritePath), sortingOrder);
            SetupPhysics(go, useCircleCollider: true, colliderSize: new Vector2(0.18f, 0.18f));
            AddOrGetComponent<Bullet>(go);
            return SaveTempPrefab(go, $"{PrefabRoot}/Bullets/{prefabName}.prefab");
        }

        private static GameObject BuildPickupPrefab(string prefabName, string spritePath)
        {
            var go = new GameObject(prefabName);
            SetLayer(go, "Pickup");
            SetupSpriteRenderer(go, LoadSprite(spritePath), 12);
            SetupPhysics(go, useCircleCollider: true, colliderSize: new Vector2(0.5f, 0.5f));
            AddOrGetComponent<Pickup>(go);
            return SaveTempPrefab(go, $"{PrefabRoot}/Pickups/{prefabName}.prefab");
        }

        private static GameObject BuildSimpleSpritePrefab(string prefabName, string spritePath, string layerName, int sortingOrder, bool withPhysics)
        {
            var go = new GameObject(prefabName);
            SetLayer(go, layerName);
            SetupSpriteRenderer(go, LoadSprite(spritePath), sortingOrder);
            if (withPhysics)
            {
                SetupPhysics(go, useCircleCollider: true, colliderSize: new Vector2(0.8f, 0.8f));
            }

            return SaveTempPrefab(go, $"{PrefabRoot}/VFX/{prefabName}.prefab");
        }

        private static GameObject BuildGameManagersPrefab()
        {
            var go = new GameObject("GameManagers");
            AddOrGetComponent<GameManager>(go);
            return SaveTempPrefab(go, $"{PrefabRoot}/Managers/GameManagers.prefab");
        }

        private static T AddOrGetComponent<T>(GameObject go) where T : Component
        {
            var component = go.GetComponent<T>();
            if (component == null)
            {
                component = go.AddComponent<T>();
            }

            return component;
        }

        private static void SetupSpriteRenderer(GameObject go, Sprite sprite, int sortingOrder)
        {
            var renderer = AddOrGetComponent<SpriteRenderer>(go);
            renderer.sprite = sprite;
            renderer.color = Color.white;
            renderer.sortingOrder = sortingOrder;
        }

        private static void SetupPhysics(GameObject go, bool useCircleCollider, Vector2 colliderSize)
        {
            var rigidbody = AddOrGetComponent<Rigidbody2D>(go);
            rigidbody.bodyType = RigidbodyType2D.Kinematic;
            rigidbody.gravityScale = 0f;
            rigidbody.simulated = true;

            Collider2D collider;
            if (useCircleCollider)
            {
                collider = go.GetComponent<CircleCollider2D>();
                if (collider == null)
                {
                    collider = go.AddComponent<CircleCollider2D>();
                }

                ((CircleCollider2D)collider).radius = Mathf.Max(0.05f, Mathf.Max(colliderSize.x, colliderSize.y) * 0.5f);
            }
            else
            {
                collider = go.GetComponent<BoxCollider2D>();
                if (collider == null)
                {
                    collider = go.AddComponent<BoxCollider2D>();
                }

                ((BoxCollider2D)collider).size = colliderSize;
            }

            collider.isTrigger = true;
        }

        private static GameObject SaveTempPrefab(GameObject temp, string prefabPath)
        {
            EnsureDirectory(Path.GetDirectoryName(prefabPath));
            var prefab = PrefabUtility.SaveAsPrefabAsset(temp, prefabPath);
            Object.DestroyImmediate(temp);
            return prefab;
        }

        private static void EnsurePrefabDirectories()
        {
            EnsureDirectory(PrefabRoot);
            EnsureDirectory($"{PrefabRoot}/Player");
            EnsureDirectory($"{PrefabRoot}/Enemies");
            EnsureDirectory($"{PrefabRoot}/Bosses");
            EnsureDirectory($"{PrefabRoot}/Bullets");
            EnsureDirectory($"{PrefabRoot}/Pickups");
            EnsureDirectory($"{PrefabRoot}/VFX");
            EnsureDirectory($"{PrefabRoot}/UI");
            EnsureDirectory($"{PrefabRoot}/Managers");
        }

        private static void EnsureDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            var normalized = path.Replace("\\", "/");
            if (!AssetDatabase.IsValidFolder(normalized))
            {
                Directory.CreateDirectory(normalized);
                AssetDatabase.Refresh();
            }
        }

        private static void SetLayer(GameObject go, string layerName)
        {
            var layer = LayerMask.NameToLayer(layerName);
            if (layer >= 0)
            {
                go.layer = layer;
            }
        }

        private static Sprite LoadSprite(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static T CreateIfMissing<T>(string assetPath, System.Action<T> initializer) where T : ScriptableObject
        {
            EnsureDirectory(Path.GetDirectoryName(assetPath));
            var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            initializer?.Invoke(asset);
            AssetDatabase.CreateAsset(asset, assetPath);
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private sealed class ContentRefs
        {
            public PlayerConfig Player;
            public WeaponConfig Weapon;

            public BulletConfig BulletPlayerBasic;
            public BulletConfig BulletPlayerPower;
            public BulletConfig BulletPlayerLaser;
            public BulletConfig BulletEnemyBasic;
            public BulletConfig BulletEnemyFan;
            public BulletConfig BulletEnemyTracking;
            public BulletConfig BulletBossHeavy;

            public EnemyConfig EnemyE01;
            public EnemyConfig EnemyE02;
            public EnemyConfig EnemyE03;
            public EnemyConfig EnemyE04;
            public EnemyConfig EnemyE05;
            public EnemyConfig EnemyE06;
            public EnemyConfig EnemyE07;
            public EnemyConfig EnemyE08;

            public BossConfig Boss01;
            public BossConfig Boss02;
            public BossConfig Boss03;
            public BossConfig Boss04;
            public BossConfig Boss05;

            public PickupConfig PickupPower;
            public PickupConfig PickupHeal;
            public PickupConfig PickupBomb;
            public PickupConfig PickupShield;
            public PickupConfig PickupScore;

            public LevelConfig Level01;
            public LevelConfig Level02;
            public LevelConfig Level03;
            public LevelConfig Level04;
            public LevelConfig Level05;
            public LevelDatabase LevelDatabase;
        }

        private sealed class PrefabRefs
        {
            public GameObject Player;

            public GameObject PlayerBullet;
            public GameObject EnemyBullet;
            public GameObject PlayerBulletPower;
            public GameObject PlayerBulletLaser;
            public GameObject EnemyBulletFan;
            public GameObject EnemyBulletTracking;
            public GameObject BossBulletHeavy;

            public GameObject EnemyE01;
            public GameObject EnemyE02;
            public GameObject EnemyE03;
            public GameObject EnemyE04;
            public GameObject EnemyE05;
            public GameObject EnemyE06;
            public GameObject EnemyE07;
            public GameObject EnemyE08;

            public GameObject Boss01;
            public GameObject Boss02;
            public GameObject Boss03;
            public GameObject Boss04;
            public GameObject Boss05;

            public GameObject PickupPower;
            public GameObject PickupHeal;
            public GameObject PickupBomb;
            public GameObject PickupShield;
            public GameObject PickupScore;

            public GameObject ExplosionSmall;
            public GameObject GameManagers;
        }
    }
}
