using System.IO;
using AIGame.ShootEmUp.Core;
using UnityEditor;
using UnityEngine;

namespace AIGame.ShootEmUp.Configs.Editor
{
    internal static class ConfigAssetBootstrapper
    {
        // [MenuItem("Tools/ShootEmUp/Generate Default Config Assets")]
        public static void GenerateDefaultAssets()
        {
            EnsureDefaultsExist();
            AssetDatabase.Refresh();
            Debug.Log("ShootEmUp default config assets are ready.");
        }

        public static void EnsureDefaultsExist()
        {
            EnsureDirectories();

            var playerBullet = CreateIfMissing<BulletConfig>(ConfigAssetPaths.BulletPlayerBasic, asset =>
            {
                asset.bulletId = "Bullet_PlayerBasic";
                asset.speed = 12f;
                asset.damage = 1;
                asset.lifetime = 2.5f;
                asset.radius = 0.12f;
                asset.tint = new Color(0.35f, 0.9f, 1f, 1f);
                asset.size = new Vector2(0.16f, 0.36f);
            });

            var enemyBullet = CreateIfMissing<BulletConfig>(ConfigAssetPaths.BulletEnemyBasic, asset =>
            {
                asset.bulletId = "Bullet_EnemyBasic";
                asset.speed = 6f;
                asset.damage = 1;
                asset.lifetime = 4f;
                asset.radius = 0.13f;
                asset.tint = new Color(1f, 0.4f, 0.45f, 1f);
                asset.size = new Vector2(0.18f, 0.26f);
            });

            var weapon = CreateIfMissing<WeaponConfig>(ConfigAssetPaths.WeaponPlayerDefault, asset =>
            {
                asset.fireInterval = 0.18f;
                asset.powerLevels = new[]
                {
                    new WeaponPowerLevel
                    {
                        level = 1,
                        bulletConfig = playerBullet,
                        angles = new[] { 0f },
                        offsets = new[] { Vector2.zero }
                    },
                    new WeaponPowerLevel
                    {
                        level = 2,
                        bulletConfig = playerBullet,
                        angles = new[] { -4f, 4f },
                        offsets = new[] { new Vector2(-0.12f, 0f), new Vector2(0.12f, 0f) }
                    },
                    new WeaponPowerLevel
                    {
                        level = 3,
                        bulletConfig = playerBullet,
                        angles = new[] { -10f, 0f, 10f },
                        offsets = new[] { new Vector2(-0.16f, 0f), Vector2.zero, new Vector2(0.16f, 0f) }
                    }
                };
            });

            CreateIfMissing<PlayerConfig>(ConfigAssetPaths.PlayerDefault, asset =>
            {
                asset.initialHealth = 3;
                asset.maxHealth = 5;
                asset.moveSpeed = 6f;
                asset.invincibleDuration = 1.5f;
                asset.initialBombs = 1;
                asset.maxBombs = 3;
                asset.maxPowerLevel = 4;
                asset.weaponConfig = weapon;
            });

            var enemyE01 = CreateIfMissing<EnemyConfig>(ConfigAssetPaths.EnemyE01, asset =>
            {
                asset.enemyId = "E01";
                asset.displayName = "Small Straight";
                asset.maxHealth = 1;
                asset.moveSpeed = 2.3f;
                asset.contactDamage = 1;
                asset.score = 100;
                asset.spawnWeight = 55;
                asset.movementPattern = MovementPattern.StraightDown;
                asset.firePattern = FirePattern.None;
                asset.tint = new Color(1f, 0.35f, 0.35f, 1f);
                asset.size = new Vector2(0.6f, 0.6f);
            });

            var enemyE02 = CreateIfMissing<EnemyConfig>(ConfigAssetPaths.EnemyE02, asset =>
            {
                asset.enemyId = "E02";
                asset.displayName = "Diagonal";
                asset.maxHealth = 1;
                asset.moveSpeed = 2.6f;
                asset.contactDamage = 1;
                asset.score = 130;
                asset.spawnWeight = 27;
                asset.movementPattern = MovementPattern.DiagonalLeft;
                asset.firePattern = FirePattern.None;
                asset.tint = new Color(1f, 0.62f, 0.22f, 1f);
                asset.size = new Vector2(0.58f, 0.58f);
            });

            var enemyE03 = CreateIfMissing<EnemyConfig>(ConfigAssetPaths.EnemyE03, asset =>
            {
                asset.enemyId = "E03";
                asset.displayName = "Shooter";
                asset.maxHealth = 3;
                asset.moveSpeed = 1.8f;
                asset.contactDamage = 1;
                asset.score = 300;
                asset.spawnWeight = 18;
                asset.movementPattern = MovementPattern.StopAndShoot;
                asset.firePattern = FirePattern.SingleForward;
                asset.fireInterval = 1.25f;
                asset.bulletConfig = enemyBullet;
                asset.tint = new Color(0.95f, 0.25f, 0.6f, 1f);
                asset.size = new Vector2(0.75f, 0.75f);
            });

            var pickupPower = CreateIfMissing<PickupConfig>(ConfigAssetPaths.PickupPowerUp, asset =>
            {
                asset.pickupId = "Pickup_PowerUp";
                asset.type = PickupType.PowerUp;
                asset.value = 1;
                asset.moveSpeed = 1.6f;
            });
            var pickupHeal = CreateIfMissing<PickupConfig>(ConfigAssetPaths.PickupHeal, asset =>
            {
                asset.pickupId = "Pickup_Heal";
                asset.type = PickupType.Heal;
                asset.value = 1;
                asset.moveSpeed = 1.5f;
            });
            var pickupBomb = CreateIfMissing<PickupConfig>(ConfigAssetPaths.PickupBomb, asset =>
            {
                asset.pickupId = "Pickup_Bomb";
                asset.type = PickupType.Bomb;
                asset.value = 1;
                asset.moveSpeed = 1.45f;
            });
            var pickupShield = CreateIfMissing<PickupConfig>(ConfigAssetPaths.PickupShield, asset =>
            {
                asset.pickupId = "Pickup_Shield";
                asset.type = PickupType.Shield;
                asset.duration = 10f;
                asset.moveSpeed = 1.55f;
            });

            if (enemyE01.dropTable == null || enemyE01.dropTable.Length == 0)
            {
                enemyE01.dropTable = new[]
                {
                    new PickupDropEntry { pickup = pickupPower, dropChance = 0.08f },
                    new PickupDropEntry { pickup = pickupHeal, dropChance = 0.06f }
                };
                EditorUtility.SetDirty(enemyE01);
            }

            if (enemyE02.dropTable == null || enemyE02.dropTable.Length == 0)
            {
                enemyE02.dropTable = new[]
                {
                    new PickupDropEntry { pickup = pickupHeal, dropChance = 0.1f },
                    new PickupDropEntry { pickup = pickupBomb, dropChance = 0.07f }
                };
                EditorUtility.SetDirty(enemyE02);
            }

            if (enemyE03.dropTable == null || enemyE03.dropTable.Length == 0)
            {
                enemyE03.dropTable = new[]
                {
                    new PickupDropEntry { pickup = pickupPower, dropChance = 0.18f },
                    new PickupDropEntry { pickup = pickupShield, dropChance = 0.08f },
                    new PickupDropEntry { pickup = pickupBomb, dropChance = 0.07f }
                };
                EditorUtility.SetDirty(enemyE03);
            }

            var boss01 = EnsureBoss(1, ConfigAssetPaths.Boss01, enemyBullet, enemyE01, enemyE02, enemyE03, pickupPower, pickupHeal, pickupBomb);
            var boss02 = EnsureBoss(2, ConfigAssetPaths.Boss02, enemyBullet, enemyE01, enemyE02, enemyE03, pickupPower, pickupHeal, pickupBomb);
            var boss03 = EnsureBoss(3, ConfigAssetPaths.Boss03, enemyBullet, enemyE01, enemyE02, enemyE03, pickupPower, pickupHeal, pickupBomb);
            var boss04 = EnsureBoss(4, ConfigAssetPaths.Boss04, enemyBullet, enemyE01, enemyE02, enemyE03, pickupPower, pickupHeal, pickupBomb);
            var boss05 = EnsureBoss(5, ConfigAssetPaths.Boss05, enemyBullet, enemyE01, enemyE02, enemyE03, pickupPower, pickupHeal, pickupBomb);

            var levels = new[]
            {
                EnsureLevel(1, ConfigAssetPaths.Level01, enemyE01, enemyE02, enemyE03, boss01),
                EnsureLevel(2, ConfigAssetPaths.Level02, enemyE01, enemyE02, enemyE03, boss02),
                EnsureLevel(3, ConfigAssetPaths.Level03, enemyE01, enemyE02, enemyE03, boss03),
                EnsureLevel(4, ConfigAssetPaths.Level04, enemyE01, enemyE02, enemyE03, boss04),
                EnsureLevel(5, ConfigAssetPaths.Level05, enemyE01, enemyE02, enemyE03, boss05)
            };

            var levelDatabase = CreateIfMissing<LevelDatabase>(ConfigAssetPaths.LevelDatabase, _ => { });
            var needsDbUpdate = levelDatabase.firstLevel == null || levelDatabase.levels == null || levelDatabase.levels.Length < 5;
            if (needsDbUpdate)
            {
                levelDatabase.firstLevel = levels[0];
                levelDatabase.levels = levels;
                EditorUtility.SetDirty(levelDatabase);
            }

            AssetDatabase.SaveAssets();
        }

        private static LevelConfig EnsureLevel(int levelId, string levelPath, EnemyConfig e01, EnemyConfig e02, EnemyConfig e03, BossConfig bossConfig)
        {
            var level = CreateIfMissing<LevelConfig>(levelPath, asset =>
            {
                asset.levelId = levelId;
                asset.displayName = $"Level {levelId}";
                asset.difficulty = levelId;
                asset.estimatedDuration = 90f + levelId * 15f;
                asset.backgroundScrollSpeed = 1.4f + levelId * 0.15f;
                asset.enemyHealthMultiplier = 1f + (levelId - 1) * 0.12f;
                asset.enemyFireRateMultiplier = 1f + (levelId - 1) * 0.08f;
                asset.scoreBonus = 1000 + levelId * 300;
            });

            if (level.waves == null || level.waves.Length == 0)
            {
                level.waves = new[]
                {
                    EnsureWave(levelId, 1, e01, 5 + levelId, SpawnPattern.HorizontalLine, MovementPattern.StraightDown, 0.28f, false, 8f),
                    EnsureWave(levelId, 2, e02, 4 + levelId, SpawnPattern.LeftRightAlternating, levelId % 2 == 0 ? MovementPattern.DiagonalRight : MovementPattern.DiagonalLeft, 0.32f, false, 10f),
                    EnsureWave(levelId, 3, e01, 6 + levelId, SpawnPattern.SinglePoint, MovementPattern.Sine, 0.22f, false, 10f),
                    EnsureWave(levelId, 4, e03, 2 + levelId / 2, SpawnPattern.HorizontalLine, MovementPattern.StopAndShoot, 0.8f, true, 35f)
                };
                EditorUtility.SetDirty(level);
            }

            if (level.bossConfig == null)
            {
                level.bossConfig = bossConfig;
                EditorUtility.SetDirty(level);
            }

            return level;
        }

        private static BossConfig EnsureBoss(
            int levelId,
            string bossPath,
            BulletConfig enemyBullet,
            EnemyConfig e01,
            EnemyConfig e02,
            EnemyConfig e03,
            PickupConfig pickupPower,
            PickupConfig pickupHeal,
            PickupConfig pickupBomb)
        {
            var boss = CreateIfMissing<BossConfig>(bossPath, asset =>
            {
                asset.bossId = $"B{levelId:00}";
                asset.displayName = $"Boss {levelId}";
                asset.maxHealth = 85 + levelId * 35;
                asset.moveSpeed = 1.2f + levelId * 0.12f;
                asset.contactDamage = 1 + levelId / 3;
                asset.score = 2200 + levelId * 700;
                asset.deathDuration = 1.8f;
                asset.tint = Color.Lerp(new Color(0.7f, 0.2f, 0.95f, 1f), new Color(1f, 0.45f, 0.2f, 1f), (levelId - 1) / 4f);
                asset.size = new Vector2(2f + levelId * 0.08f, 1.15f + levelId * 0.04f);
            });

            if (boss.phases == null || boss.phases.Length == 0)
            {
                boss.phases = new[]
                {
                    new BossPhaseConfig
                    {
                        phaseName = "Phase 1",
                        startHealthPercent = 1f,
                        movePattern = MovementPattern.BossHorizontal,
                        firePattern = FirePattern.TripleFan,
                        fireInterval = Mathf.Max(0.28f, 0.95f - levelId * 0.05f),
                        bulletConfig = enemyBullet
                    },
                    new BossPhaseConfig
                    {
                        phaseName = "Phase 2",
                        startHealthPercent = 0.62f,
                        movePattern = MovementPattern.TrackPlayerX,
                        firePattern = levelId >= 3 ? FirePattern.FiveFan : FirePattern.AimedTriple,
                        fireInterval = Mathf.Max(0.2f, 0.72f - levelId * 0.04f),
                        bulletConfig = enemyBullet,
                        summonEnemy = levelId % 2 == 0 ? e02 : e01,
                        summonInterval = Mathf.Max(1.2f, 3.8f - levelId * 0.2f)
                    },
                    new BossPhaseConfig
                    {
                        phaseName = "Phase 3",
                        startHealthPercent = 0.3f,
                        movePattern = MovementPattern.Sine,
                        firePattern = FirePattern.Ring,
                        fireInterval = Mathf.Max(0.14f, 0.58f - levelId * 0.04f),
                        bulletConfig = enemyBullet,
                        summonEnemy = e03,
                        summonInterval = Mathf.Max(1.1f, 4.2f - levelId * 0.22f)
                    }
                };
                EditorUtility.SetDirty(boss);
            }

            if (boss.guaranteedDrops == null || boss.guaranteedDrops.Length == 0)
            {
                boss.guaranteedDrops = new[] { pickupPower, pickupHeal, pickupBomb };
                EditorUtility.SetDirty(boss);
            }

            return boss;
        }

        private static WaveConfig EnsureWave(
            int levelId,
            int waveIndex,
            EnemyConfig enemyConfig,
            int count,
            SpawnPattern spawnPattern,
            MovementPattern movementPattern,
            float spawnInterval,
            bool waitUntilAllEnemiesDead,
            float maxDuration)
        {
            var dir = Path.Combine(ConfigAssetPaths.Root, "Waves", $"Level{levelId:00}");
            Directory.CreateDirectory(dir);
            var assetPath = $"{dir.Replace("\\", "/")}/Wave_{levelId:00}_{waveIndex:00}.asset";

            return CreateIfMissing<WaveConfig>(assetPath, asset =>
            {
                asset.waveId = $"Wave_{levelId:00}_{waveIndex:00}";
                asset.startDelay = waveIndex == 1 ? 0.6f : 1.2f;
                asset.waitUntilAllEnemiesDead = waitUntilAllEnemiesDead;
                asset.maxDuration = maxDuration;
                asset.entries = new[]
                {
                    new WaveEntry
                    {
                        enemyConfig = enemyConfig,
                        count = count,
                        spawnInterval = spawnInterval,
                        spawnPattern = spawnPattern,
                        movementPattern = movementPattern,
                        horizontalOffset = 0f,
                        customSpawnPoints = null
                    }
                };
            });
        }

        private static void EnsureDirectories()
        {
            Directory.CreateDirectory(Path.Combine(ConfigAssetPaths.Root, "Player"));
            Directory.CreateDirectory(Path.Combine(ConfigAssetPaths.Root, "Weapons"));
            Directory.CreateDirectory(Path.Combine(ConfigAssetPaths.Root, "Bullets"));
            Directory.CreateDirectory(Path.Combine(ConfigAssetPaths.Root, "Enemies"));
            Directory.CreateDirectory(Path.Combine(ConfigAssetPaths.Root, "Bosses"));
            Directory.CreateDirectory(Path.Combine(ConfigAssetPaths.Root, "Pickups"));
            Directory.CreateDirectory(Path.Combine(ConfigAssetPaths.Root, "Levels"));
            Directory.CreateDirectory(Path.Combine(ConfigAssetPaths.Root, "Waves"));
            Directory.CreateDirectory(Path.Combine(ConfigAssetPaths.Root, "Database"));
        }

        private static T CreateIfMissing<T>(string assetPath, System.Action<T> initializer)
            where T : ScriptableObject
        {
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
    }
}
