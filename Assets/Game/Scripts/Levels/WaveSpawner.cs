using System;
using System.Collections;
using AIGame.ShootEmUp.Bosses;
using AIGame.ShootEmUp.Configs;
using AIGame.ShootEmUp.Core;
using AIGame.ShootEmUp.Enemies;
using AIGame.ShootEmUp.Utilities;
using UnityEngine;

namespace AIGame.ShootEmUp.Levels
{
    public class WaveSpawner : MonoBehaviour
    {
        private EnemySpawner _enemySpawner;
        private Coroutine _runRoutine;
        private int _aliveEnemies;

        public void Initialize(EnemySpawner enemySpawner)
        {
            _enemySpawner = enemySpawner;
        }

        private void OnEnable()
        {
            GameEvents.EnemySpawned += OnEnemySpawned;
            GameEvents.EnemyDespawned += OnEnemyDespawned;
        }

        private void OnDisable()
        {
            GameEvents.EnemySpawned -= OnEnemySpawned;
            GameEvents.EnemyDespawned -= OnEnemyDespawned;
        }

        public void RunLevel(LevelConfig levelConfig, Action onCompleted)
        {
            StopRunning();
            _aliveEnemies = 0;
            _runRoutine = StartCoroutine(RunLevelRoutine(levelConfig, onCompleted));
        }

        public void StopRunning()
        {
            if (_runRoutine != null)
            {
                StopCoroutine(_runRoutine);
                _runRoutine = null;
            }
        }

        private IEnumerator RunLevelRoutine(LevelConfig levelConfig, Action onCompleted)
        {
            if (levelConfig == null)
            {
                yield return new WaitForSeconds(0.5f);
                onCompleted?.Invoke();
                yield break;
            }

            if (levelConfig.waves != null)
            {
                for (var i = 0; i < levelConfig.waves.Length; i++)
                {
                    var wave = levelConfig.waves[i];
                    if (wave == null)
                    {
                        continue;
                    }

                    yield return RunWaveRoutine(wave);
                }
            }

            if (levelConfig.preBossSupplyWave != null)
            {
                yield return RunWaveRoutine(levelConfig.preBossSupplyWave);
            }

            if (levelConfig.bossConfig != null)
            {
                yield return RunBossRoutine(levelConfig);
            }

            onCompleted?.Invoke();
        }

        private IEnumerator RunWaveRoutine(WaveConfig wave)
        {
            if (wave.startDelay > 0f)
            {
                yield return new WaitForSeconds(wave.startDelay);
            }

            if (wave.entries != null)
            {
                for (var entryIndex = 0; entryIndex < wave.entries.Length; entryIndex++)
                {
                    var entry = wave.entries[entryIndex];
                    if (entry == null || entry.enemyConfig == null || entry.count <= 0)
                    {
                        continue;
                    }

                    for (var i = 0; i < entry.count; i++)
                    {
                        var spawnPos = GetSpawnPosition(entry, i, entry.count);
                        _enemySpawner.SpawnEnemy(entry.enemyConfig, entry.movementPattern, spawnPos);

                        if (entry.spawnInterval > 0f)
                        {
                            yield return new WaitForSeconds(entry.spawnInterval);
                        }
                    }
                }
            }

            if (wave.waitUntilAllEnemiesDead)
            {
                var timer = 0f;
                var maxDuration = wave.maxDuration > 0f ? wave.maxDuration : 999f;
                while (_aliveEnemies > 0 && timer < maxDuration)
                {
                    timer += Time.deltaTime;
                    yield return null;
                }
            }
            else if (wave.maxDuration > 0f)
            {
                yield return new WaitForSeconds(wave.maxDuration);
            }
        }

        private static Vector3 GetSpawnPosition(WaveEntry entry, int index, int totalCount)
        {
            var bounds = CameraBounds.GetWorldBounds(Camera.main, 0.8f);
            var topY = bounds.MaxY + 0.8f;

            switch (entry.spawnPattern)
            {
                case SpawnPattern.HorizontalLine:
                {
                    var t = totalCount <= 1 ? 0.5f : (float)index / (totalCount - 1);
                    var x = Mathf.Lerp(bounds.MinX, bounds.MaxX, t) + entry.horizontalOffset;
                    return new Vector3(x, topY, 0f);
                }
                case SpawnPattern.LeftRightAlternating:
                {
                    var isLeft = index % 2 == 0;
                    var x = (isLeft ? bounds.MinX : bounds.MaxX) + entry.horizontalOffset;
                    return new Vector3(x, topY, 0f);
                }
                case SpawnPattern.CustomPoints:
                {
                    if (entry.customSpawnPoints != null && entry.customSpawnPoints.Length > 0)
                    {
                        var point = entry.customSpawnPoints[index % entry.customSpawnPoints.Length];
                        return new Vector3(point.x, point.y, 0f);
                    }

                    goto default;
                }
                case SpawnPattern.SinglePoint:
                default:
                {
                    var x = UnityEngine.Random.Range(bounds.MinX, bounds.MaxX) + entry.horizontalOffset;
                    return new Vector3(x, topY, 0f);
                }
            }
        }

        private IEnumerator RunBossRoutine(LevelConfig levelConfig)
        {
            var boss = BossController.Spawn(
                levelConfig.bossConfig,
                _enemySpawner,
                levelConfig.enemyHealthMultiplier,
                levelConfig.enemyFireRateMultiplier);

            if (boss == null)
            {
                yield break;
            }

            var timer = 0f;
            var maxDuration = Mathf.Max(45f, levelConfig.estimatedDuration * 1.2f);
            while (boss != null && boss.IsAlive && timer < maxDuration)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(0.4f);
        }

        private void OnEnemySpawned()
        {
            _aliveEnemies++;
        }

        private void OnEnemyDespawned()
        {
            _aliveEnemies = Mathf.Max(0, _aliveEnemies - 1);
        }
    }
}
