using AIGame.ShootEmUp.Core;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AIGame.ShootEmUp.Configs
{
    public sealed class GameConfigProvider
    {
        public PlayerConfig PlayerConfig { get; private set; }
        public EnemyConfig[] EnemyConfigs { get; private set; }
        public LevelDatabase LevelDatabase { get; private set; }

        public static GameConfigProvider Load()
        {
            var provider = new GameConfigProvider();

#if UNITY_EDITOR
            provider.PlayerConfig = LoadAsset<PlayerConfig>(ConfigAssetPaths.PlayerDefault);
            var e01 = LoadAsset<EnemyConfig>(ConfigAssetPaths.EnemyE01);
            var e02 = LoadAsset<EnemyConfig>(ConfigAssetPaths.EnemyE02);
            var e03 = LoadAsset<EnemyConfig>(ConfigAssetPaths.EnemyE03);
            provider.EnemyConfigs = FilterNull(e01, e02, e03);
            provider.LevelDatabase = LoadAsset<LevelDatabase>(ConfigAssetPaths.LevelDatabase);
#endif

            provider.FillFallbacksIfNeeded();
            return provider;
        }

        private void FillFallbacksIfNeeded()
        {
            if (PlayerConfig == null)
            {
                PlayerConfig = BuildFallbackPlayerConfig();
            }

            if (EnemyConfigs == null || EnemyConfigs.Length == 0)
            {
                EnemyConfigs = BuildFallbackEnemies();
            }

            if (LevelDatabase == null || LevelDatabase.levels == null || LevelDatabase.levels.Length == 0)
            {
                LevelDatabase = BuildFallbackLevelDatabase(EnemyConfigs);
            }
        }

        private static PlayerConfig BuildFallbackPlayerConfig()
        {
            var playerBullet = ScriptableObject.CreateInstance<BulletConfig>();
            playerBullet.bulletId = "Bullet_PlayerBasic_Fallback";
            playerBullet.speed = 12f;
            playerBullet.damage = 1;
            playerBullet.lifetime = 2.5f;
            playerBullet.tint = new Color(0.35f, 0.9f, 1f, 1f);
            playerBullet.size = new Vector2(0.16f, 0.36f);

            var weapon = ScriptableObject.CreateInstance<WeaponConfig>();
            weapon.fireInterval = 0.18f;
            weapon.powerLevels = new[]
            {
                new WeaponPowerLevel
                {
                    level = 1,
                    bulletConfig = playerBullet,
                    angles = new[] { 0f },
                    offsets = new[] { Vector2.zero }
                }
            };

            var player = ScriptableObject.CreateInstance<PlayerConfig>();
            player.initialHealth = 3;
            player.maxHealth = 5;
            player.invincibleDuration = 1.5f;
            player.moveSpeed = 6f;
            player.weaponConfig = weapon;
            return player;
        }

        private static EnemyConfig[] BuildFallbackEnemies()
        {
            var enemyBullet = ScriptableObject.CreateInstance<BulletConfig>();
            enemyBullet.bulletId = "Bullet_EnemyBasic_Fallback";
            enemyBullet.speed = 6f;
            enemyBullet.damage = 1;
            enemyBullet.lifetime = 4f;
            enemyBullet.tint = new Color(1f, 0.4f, 0.45f, 1f);
            enemyBullet.size = new Vector2(0.18f, 0.26f);

            var e01 = ScriptableObject.CreateInstance<EnemyConfig>();
            e01.enemyId = "E01";
            e01.displayName = "Small Straight";
            e01.maxHealth = 1;
            e01.moveSpeed = 2.3f;
            e01.score = 100;
            e01.contactDamage = 1;
            e01.spawnWeight = 55;
            e01.tint = new Color(1f, 0.35f, 0.35f, 1f);
            e01.size = new Vector2(0.6f, 0.6f);
            e01.movementPattern = MovementPattern.StraightDown;
            e01.firePattern = FirePattern.None;

            var e02 = ScriptableObject.CreateInstance<EnemyConfig>();
            e02.enemyId = "E02";
            e02.displayName = "Diagonal";
            e02.maxHealth = 1;
            e02.moveSpeed = 2.6f;
            e02.score = 130;
            e02.contactDamage = 1;
            e02.spawnWeight = 27;
            e02.tint = new Color(1f, 0.62f, 0.22f, 1f);
            e02.size = new Vector2(0.58f, 0.58f);
            e02.movementPattern = MovementPattern.DiagonalLeft;
            e02.firePattern = FirePattern.None;

            var e03 = ScriptableObject.CreateInstance<EnemyConfig>();
            e03.enemyId = "E03";
            e03.displayName = "Shooter";
            e03.maxHealth = 3;
            e03.moveSpeed = 1.8f;
            e03.score = 300;
            e03.contactDamage = 1;
            e03.spawnWeight = 18;
            e03.tint = new Color(0.95f, 0.25f, 0.6f, 1f);
            e03.size = new Vector2(0.75f, 0.75f);
            e03.movementPattern = MovementPattern.StraightDown;
            e03.firePattern = FirePattern.SingleForward;
            e03.fireInterval = 1.25f;
            e03.bulletConfig = enemyBullet;

            return new[] { e01, e02, e03 };
        }

        private static LevelDatabase BuildFallbackLevelDatabase(EnemyConfig[] enemies)
        {
            var e01 = FindEnemy(enemies, "E01");
            var e02 = FindEnemy(enemies, "E02");
            var e03 = FindEnemy(enemies, "E03");
            var bossBullet = e03 != null && e03.bulletConfig != null ? e03.bulletConfig : BuildFallbackBossBullet();

            var levels = new LevelConfig[5];
            for (var i = 0; i < levels.Length; i++)
            {
                levels[i] = BuildFallbackLevel(i + 1, e01, e02, e03, bossBullet);
            }

            var database = ScriptableObject.CreateInstance<LevelDatabase>();
            database.levels = levels;
            database.firstLevel = levels[0];
            return database;
        }

        private static LevelConfig BuildFallbackLevel(int levelId, EnemyConfig e01, EnemyConfig e02, EnemyConfig e03, BulletConfig bossBullet)
        {
            var level = ScriptableObject.CreateInstance<LevelConfig>();
            level.levelId = levelId;
            level.displayName = $"Level {levelId}";
            level.difficulty = levelId;
            level.estimatedDuration = 90f + levelId * 18f;
            level.enemyHealthMultiplier = 1f + (levelId - 1) * 0.12f;
            level.enemyFireRateMultiplier = 1f + (levelId - 1) * 0.08f;
            level.scoreBonus = 1000 + levelId * 300;

            var waves = new WaveConfig[4];
            waves[0] = CreateWave(levelId, 1, e01, 5 + levelId, SpawnPattern.HorizontalLine, MovementPattern.StraightDown, 0.28f, waitAllDead: false, maxDuration: 8f);
            waves[1] = CreateWave(levelId, 2, e02, 4 + levelId, SpawnPattern.LeftRightAlternating, levelId % 2 == 0 ? MovementPattern.DiagonalRight : MovementPattern.DiagonalLeft, 0.32f, waitAllDead: false, maxDuration: 10f);
            waves[2] = CreateWave(levelId, 3, e01, 6 + levelId, SpawnPattern.SinglePoint, MovementPattern.Sine, 0.22f, waitAllDead: false, maxDuration: 10f);
            waves[3] = CreateWave(levelId, 4, e03, 2 + levelId / 2, SpawnPattern.HorizontalLine, MovementPattern.StopAndShoot, 0.8f, waitAllDead: true, maxDuration: 35f);
            level.waves = waves;
            level.bossConfig = BuildFallbackBoss(levelId, bossBullet, e01, e02, e03);

            return level;
        }

        private static BossConfig BuildFallbackBoss(int levelId, BulletConfig bossBullet, EnemyConfig e01, EnemyConfig e02, EnemyConfig e03)
        {
            var boss = ScriptableObject.CreateInstance<BossConfig>();
            boss.bossId = $"B{levelId:00}";
            boss.displayName = $"Boss {levelId}";
            boss.maxHealth = 85 + levelId * 35;
            boss.moveSpeed = 1.2f + levelId * 0.12f;
            boss.contactDamage = 1 + levelId / 3;
            boss.score = 2200 + levelId * 700;
            boss.deathDuration = 1.8f;
            boss.tint = Color.Lerp(new Color(0.7f, 0.2f, 0.95f, 1f), new Color(1f, 0.45f, 0.2f, 1f), (levelId - 1) / 4f);
            boss.size = new Vector2(2f + levelId * 0.08f, 1.15f + levelId * 0.04f);
            boss.phases = new[]
            {
                new BossPhaseConfig
                {
                    phaseName = "Phase 1",
                    startHealthPercent = 1f,
                    movePattern = MovementPattern.BossHorizontal,
                    firePattern = FirePattern.TripleFan,
                    fireInterval = Mathf.Max(0.28f, 0.95f - levelId * 0.05f),
                    bulletConfig = bossBullet
                },
                new BossPhaseConfig
                {
                    phaseName = "Phase 2",
                    startHealthPercent = 0.62f,
                    movePattern = MovementPattern.TrackPlayerX,
                    firePattern = levelId >= 3 ? FirePattern.FiveFan : FirePattern.AimedTriple,
                    fireInterval = Mathf.Max(0.2f, 0.72f - levelId * 0.04f),
                    bulletConfig = bossBullet,
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
                    bulletConfig = bossBullet,
                    summonEnemy = e03,
                    summonInterval = Mathf.Max(1.1f, 4.2f - levelId * 0.22f)
                }
            };

            return boss;
        }

        private static BulletConfig BuildFallbackBossBullet()
        {
            var bullet = ScriptableObject.CreateInstance<BulletConfig>();
            bullet.bulletId = "Bullet_Boss_Fallback";
            bullet.speed = 6.8f;
            bullet.damage = 1;
            bullet.lifetime = 4f;
            bullet.tint = new Color(1f, 0.48f, 0.2f, 1f);
            bullet.size = new Vector2(0.22f, 0.3f);
            return bullet;
        }

        private static WaveConfig CreateWave(
            int levelId,
            int waveIndex,
            EnemyConfig enemy,
            int count,
            SpawnPattern spawnPattern,
            MovementPattern movementPattern,
            float spawnInterval,
            bool waitAllDead,
            float maxDuration)
        {
            var wave = ScriptableObject.CreateInstance<WaveConfig>();
            wave.waveId = $"Wave_{levelId:00}_{waveIndex:00}";
            wave.startDelay = waveIndex == 1 ? 0.6f : 1.2f;
            wave.waitUntilAllEnemiesDead = waitAllDead;
            wave.maxDuration = maxDuration;

            var entry = new WaveEntry
            {
                enemyConfig = enemy,
                count = count,
                spawnInterval = spawnInterval,
                spawnPattern = spawnPattern,
                movementPattern = movementPattern,
                horizontalOffset = 0f,
                customSpawnPoints = null
            };

            wave.entries = new[] { entry };
            return wave;
        }

        private static EnemyConfig FindEnemy(EnemyConfig[] enemies, string enemyId)
        {
            if (enemies == null || enemies.Length == 0)
            {
                return null;
            }

            for (var i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] != null && enemies[i].enemyId == enemyId)
                {
                    return enemies[i];
                }
            }

            return enemies[0];
        }

        private static EnemyConfig[] FilterNull(params EnemyConfig[] items)
        {
            var count = 0;
            for (var i = 0; i < items.Length; i++)
            {
                if (items[i] != null)
                {
                    count++;
                }
            }

            var result = new EnemyConfig[count];
            var cursor = 0;
            for (var i = 0; i < items.Length; i++)
            {
                if (items[i] == null)
                {
                    continue;
                }

                result[cursor++] = items[i];
            }

            return result;
        }

#if UNITY_EDITOR
        private static T LoadAsset<T>(string assetPath) where T : Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }
#endif
    }
}
