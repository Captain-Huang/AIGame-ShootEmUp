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
        private BulletConfig _fallbackBullet;
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
                _phases = new[]
                {
                    new BossPhaseConfig
                    {
                        phaseName = "Phase 1",
                        startHealthPercent = 1f,
                        movePattern = MovementPattern.BossHorizontal,
                        firePattern = FirePattern.TripleFan,
                        fireInterval = 0.95f,
                        bulletConfig = ResolveFallbackBullet(),
                        summonEnemy = null,
                        summonInterval = 6f
                    }
                };
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

            var bullet = phase.bulletConfig != null ? phase.bulletConfig : ResolveFallbackBullet();
            var interval = Mathf.Max(0.08f, phase.fireInterval / _fireRateMultiplier);
            _weapon.Configure(interval, bullet, phase.firePattern);

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

            var spawnPosition = transform.position + new Vector3(Random.Range(-1.4f, 1.4f), -0.75f, 0f);
            _enemySpawner.SpawnEnemy(
                _currentPhase.summonEnemy,
                _currentPhase.summonEnemy.movementPattern,
                spawnPosition);

            _summonTimer = Mathf.Max(0.6f, _currentPhase.summonInterval);
        }

        private BulletConfig ResolveFallbackBullet()
        {
            if (_fallbackBullet != null)
            {
                return _fallbackBullet;
            }

            _fallbackBullet = ScriptableObject.CreateInstance<BulletConfig>();
            _fallbackBullet.bulletId = "Bullet_BossFallback";
            _fallbackBullet.speed = 6.5f;
            _fallbackBullet.damage = 1;
            _fallbackBullet.lifetime = 4.2f;
            _fallbackBullet.tint = new Color(1f, 0.45f, 0.18f, 1f);
            _fallbackBullet.size = new Vector2(0.22f, 0.3f);
            return _fallbackBullet;
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
