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

            if (config.prefab == null)
            {
                Debug.LogError($"EnemyConfig {config.enemyId} has no prefab assigned.");
                return null;
            }

            var movementPattern = overridePattern != MovementPattern.StraightDown || config.movementPattern == MovementPattern.StraightDown
                ? overridePattern
                : config.movementPattern;

            if (overridePattern == MovementPattern.StraightDown && config.movementPattern != MovementPattern.StraightDown)
            {
                movementPattern = config.movementPattern;
            }

            var enemy = Object.Instantiate(config.prefab);
            if (enemy == null)
            {
                Debug.LogError($"Failed to instantiate enemy prefab for {config.enemyId}.");
                return null;
            }

            enemy.name = $"Enemy_{config.enemyId}";
            enemy.transform.position = spawnPosition ?? GetSpawnPosition();
            enemy.transform.rotation = Quaternion.Euler(0f, 0f, 180f);

            var enemyBase = enemy.GetComponent<EnemyBase>();
            var movement = enemy.GetComponent<EnemyMovement>();
            if (enemyBase == null || movement == null)
            {
                Debug.LogError($"Enemy prefab {config.prefab.name} is missing EnemyBase or EnemyMovement.");
                Object.Destroy(enemy);
                return null;
            }

            enemyBase.Configure(
                maxHealth: config.maxHealth,
                contactDamage: config.contactDamage,
                scoreValue: config.score,
                dropTable: config.dropTable);

            movement.Configure(
                direction: GetDirection(movementPattern),
                speed: config.moveSpeed,
                movementPattern: movementPattern);

            var weapon = enemy.GetComponent<EnemyWeapon>();
            if (config.firePattern == FirePattern.None)
            {
                if (weapon != null)
                {
                    weapon.enabled = false;
                }
            }
            else if (config.bulletConfig == null)
            {
                Debug.LogError($"EnemyConfig {config.enemyId} firePattern is {config.firePattern} but bulletConfig is missing.");
                if (weapon != null)
                {
                    weapon.enabled = false;
                }
            }
            else
            {
                if (weapon == null)
                {
                    Debug.LogError($"Enemy prefab {config.prefab.name} is missing EnemyWeapon.");
                    Object.Destroy(enemy);
                    return null;
                }

                weapon.enabled = true;
                weapon.Configure(
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
