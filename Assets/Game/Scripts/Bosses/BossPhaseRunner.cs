using System;
using System.Collections.Generic;
using AIGame.ShootEmUp.Configs;
using AIGame.ShootEmUp.Core;
using AIGame.ShootEmUp.Enemies;
using AIGame.ShootEmUp.Utilities;
using UnityEngine;

namespace AIGame.ShootEmUp.Bosses
{
    public class BossPhaseRunner : MonoBehaviour
    {
        private BossConfig _config;
        private EnemySpawner _enemySpawner;
        private EnemyWeapon _weapon;
        private BossPhaseConfig[] _phases;
        private BossPhaseConfig _currentPhase;
        private int _activePhaseIndex = -1;
        private float _fireRateMultiplier = 1f;
        private float _baseX;
        private float _targetY;
        private float _summonTimer;

        public void Configure(BossConfig config, EnemySpawner enemySpawner, EnemyWeapon weapon, float fireRateMultiplier)
        {
            _config = config;
            _enemySpawner = enemySpawner;
            _weapon = weapon;
            _fireRateMultiplier = Mathf.Max(0.1f, fireRateMultiplier);
            _baseX = transform.position.x;
            _targetY = CameraBounds.GetWorldBounds(Camera.main, 0.8f).MaxY - 1.35f;
            _activePhaseIndex = -1;

            _phases = NormalizePhases(config != null ? config.phases : null);
            if (_phases == null || _phases.Length == 0)
            {
                Debug.LogError($"BossConfig {(config != null ? config.bossId : "Unknown")} has no valid phases.");
                _phases = Array.Empty<BossPhaseConfig>();
                _currentPhase = null;
                if (_weapon != null)
                {
                    _weapon.enabled = false;
                }
            }
        }

        public void EvaluateByHealthPercent(float healthPercent)
        {
            if (_phases == null || _phases.Length == 0)
            {
                return;
            }

            var clampedPercent = Mathf.Clamp01(healthPercent);
            var selectedIndex = 0;
            for (var i = 0; i < _phases.Length; i++)
            {
                if (clampedPercent <= _phases[i].startHealthPercent)
                {
                    selectedIndex = i;
                }
            }

            if (selectedIndex == _activePhaseIndex)
            {
                return;
            }

            _activePhaseIndex = selectedIndex;
            ApplyPhase(_phases[_activePhaseIndex]);
        }

        private void Update()
        {
            if (_currentPhase == null)
            {
                return;
            }

            MoveByPattern(_currentPhase.movePattern);
            TrySummonEnemy();
        }

        private void ApplyPhase(BossPhaseConfig phase)
        {
            _currentPhase = phase;
            _summonTimer = 0.25f;

            if (_weapon == null)
            {
                Debug.LogError($"Boss {gameObject.name} is missing EnemyWeapon at runtime.");
                return;
            }

            if (phase.bulletConfig == null)
            {
                Debug.LogError($"Boss {gameObject.name} phase {phase.phaseName} has no bulletConfig.");
                _weapon.enabled = false;
                return;
            }

            var interval = Mathf.Max(0.08f, phase.fireInterval / _fireRateMultiplier);
            _weapon.enabled = true;
            _weapon.Configure(interval, phase.bulletConfig, phase.firePattern);

            var phaseName = string.IsNullOrWhiteSpace(phase.phaseName) ? $"Phase {_activePhaseIndex + 1}" : phase.phaseName;
            GameEvents.RaiseBossPhaseChanged(phaseName);
        }

        private void MoveByPattern(MovementPattern pattern)
        {
            var moveSpeed = Mathf.Max(0.4f, _config != null ? _config.moveSpeed : 1.6f);
            var position = transform.position;
            position.y = Mathf.MoveTowards(position.y, _targetY, moveSpeed * Time.deltaTime);

            switch (pattern)
            {
                case MovementPattern.TrackPlayerX:
                {
                    var player = FindObjectOfType<Player.PlayerHealth>();
                    if (player != null)
                    {
                        position.x = Mathf.MoveTowards(position.x, player.transform.position.x, moveSpeed * 0.85f * Time.deltaTime);
                    }
                    else
                    {
                        position.x = Mathf.Lerp(position.x, _baseX, moveSpeed * 0.4f * Time.deltaTime);
                    }

                    break;
                }
                case MovementPattern.Sine:
                    position.x = _baseX + Mathf.Sin(Time.time * 1.2f) * 2.2f;
                    break;
                case MovementPattern.DiagonalLeft:
                    position.x -= moveSpeed * 0.45f * Time.deltaTime;
                    break;
                case MovementPattern.DiagonalRight:
                    position.x += moveSpeed * 0.45f * Time.deltaTime;
                    break;
                case MovementPattern.BossHorizontal:
                    position.x = _baseX + Mathf.Sin(Time.time * 1.45f) * 2.6f;
                    break;
                case MovementPattern.StopAndShoot:
                case MovementPattern.StraightDown:
                default:
                    position.x = Mathf.Lerp(position.x, _baseX, moveSpeed * 0.2f * Time.deltaTime);
                    break;
            }

            var bounds = CameraBounds.GetWorldBounds(Camera.main, 0.4f);
            position.x = Mathf.Clamp(position.x, bounds.MinX, bounds.MaxX);
            transform.position = position;
        }

        private void TrySummonEnemy()
        {
            if (_enemySpawner == null || _currentPhase == null || _currentPhase.summonEnemy == null)
            {
                return;
            }

            _summonTimer -= Time.deltaTime;
            if (_summonTimer > 0f)
            {
                return;
            }

            var spawnPosition = transform.position + new Vector3(UnityEngine.Random.Range(-1.4f, 1.4f), -0.75f, 0f);
            _enemySpawner.SpawnEnemy(
                _currentPhase.summonEnemy,
                _currentPhase.summonEnemy.movementPattern,
                spawnPosition);

            _summonTimer = Mathf.Max(0.6f, _currentPhase.summonInterval);
        }

        private static BossPhaseConfig[] NormalizePhases(BossPhaseConfig[] phases)
        {
            if (phases == null || phases.Length == 0)
            {
                return null;
            }

            var list = new List<BossPhaseConfig>();
            for (var i = 0; i < phases.Length; i++)
            {
                if (phases[i] != null)
                {
                    list.Add(phases[i]);
                }
            }

            if (list.Count == 0)
            {
                return null;
            }

            list.Sort((a, b) => b.startHealthPercent.CompareTo(a.startHealthPercent));
            return list.ToArray();
        }
    }
}
