using AIGame.ShootEmUp.Configs;
using AIGame.ShootEmUp.Core;
using AIGame.ShootEmUp.Utilities;
using UnityEngine;

namespace AIGame.ShootEmUp.Enemies
{
    public class EnemySpawner : MonoBehaviour
    {
        public static EnemySpawner CreateOrFind()
        {
            var existing = FindObjectOfType<EnemySpawner>();
            if (existing != null)
            {
                return existing;
            }

            var go = new GameObject("EnemySpawner");
            return go.AddComponent<EnemySpawner>();
        }

        public EnemyBase SpawnEnemy(EnemyConfig config, MovementPattern overridePattern = MovementPattern.StraightDown, Vector3? spawnPosition = null)
        {
            if (config == null)
            {
                return null;
            }

            var movementPattern = overridePattern != MovementPattern.StraightDown || config.movementPattern == MovementPattern.StraightDown
                ? overridePattern
                : config.movementPattern;

            if (overridePattern == MovementPattern.StraightDown && config.movementPattern != MovementPattern.StraightDown)
            {
                movementPattern = config.movementPattern;
            }

            var enemy = RuntimeFactory.CreateActor(
                $"Enemy_{config.enemyId}",
                config.tint,
                config.size.sqrMagnitude > 0f ? config.size : new Vector2(0.6f, 0.6f),
                6,
                "Enemy");

            enemy.transform.position = spawnPosition ?? GetSpawnPosition();
            var enemyBase = enemy.AddComponent<EnemyBase>();
            enemyBase.Configure(
                maxHealth: config.maxHealth,
                contactDamage: config.contactDamage,
                scoreValue: config.score);

            var movement = enemy.AddComponent<EnemyMovement>();
            movement.Configure(
                direction: GetDirection(movementPattern),
                speed: config.moveSpeed,
                movementPattern: movementPattern);

            if (config.firePattern != FirePattern.None && config.bulletConfig != null)
            {
                enemy.AddComponent<EnemyWeapon>().Configure(
                    interval: config.fireInterval,
                    bulletConfig: config.bulletConfig,
                    firePattern: config.firePattern);
            }

            GameEvents.RaiseEnemySpawned();
            return enemyBase;
        }

        private Vector3 GetSpawnPosition()
        {
            var bounds = CameraBounds.GetWorldBounds(Camera.main, 0.8f);
            var x = Random.Range(bounds.MinX, bounds.MaxX);
            var y = bounds.MaxY + 0.8f;
            return new Vector3(x, y, 0f);
        }

        private static Vector2 GetDirection(MovementPattern pattern)
        {
            switch (pattern)
            {
                case MovementPattern.DiagonalLeft:
                    return new Vector2(-0.4f, -1f).normalized;
                case MovementPattern.DiagonalRight:
                    return new Vector2(0.4f, -1f).normalized;
                case MovementPattern.TrackPlayerX:
                case MovementPattern.Sine:
                case MovementPattern.StopAndShoot:
                case MovementPattern.BossHorizontal:
                case MovementPattern.StraightDown:
                default:
                    return Vector2.down;
            }
        }
    }
}
