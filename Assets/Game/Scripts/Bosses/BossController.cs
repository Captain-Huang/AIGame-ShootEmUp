using System.Collections;
using AIGame.ShootEmUp.Configs;
using AIGame.ShootEmUp.Core;
using AIGame.ShootEmUp.Enemies;
using AIGame.ShootEmUp.Player;
using AIGame.ShootEmUp.Utilities;
using UnityEngine;

namespace AIGame.ShootEmUp.Bosses
{
    public class BossController : MonoBehaviour
    {
        private BossConfig _config;
        private BossPhaseRunner _phaseRunner;
        private int _contactDamage = 2;
        private int _scoreValue = 3000;
        private int _maxHealth = 100;
        private int _currentHealth = 100;
        private bool _isDead;

        public bool IsAlive => !_isDead;

        public static BossController Spawn(BossConfig config, EnemySpawner enemySpawner, float healthMultiplier, float fireRateMultiplier)
        {
            if (config == null)
            {
                return null;
            }

            GameObject bossGo;
            if (config.prefab != null)
            {
                bossGo = Object.Instantiate(config.prefab);
                bossGo.name = $"Boss_{config.bossId}";
            }
            else
            {
                var size = config.size.sqrMagnitude > 0f ? config.size : new Vector2(2.2f, 1.2f);
                bossGo = RuntimeFactory.CreateActor(
                    $"Boss_{config.bossId}",
                    config.tint,
                    size,
                    7,
                    "Enemy");
            }

            var bounds = CameraBounds.GetWorldBounds(Camera.main, 0.8f);
            bossGo.transform.position = new Vector3(0f, bounds.MaxY + 0.9f, 0f);

            var controller = bossGo.GetComponent<BossController>();
            if (controller == null)
            {
                controller = bossGo.AddComponent<BossController>();
            }

            controller.Configure(config, enemySpawner, healthMultiplier, fireRateMultiplier);
            return controller;
        }

        public void Configure(BossConfig config, EnemySpawner enemySpawner, float healthMultiplier, float fireRateMultiplier)
        {
            _config = config;
            _isDead = false;
            _maxHealth = Mathf.Max(1, Mathf.RoundToInt(config.maxHealth * Mathf.Max(0.1f, healthMultiplier)));
            _currentHealth = _maxHealth;
            _scoreValue = Mathf.Max(100, config.score);
            _contactDamage = Mathf.Max(1, config.contactDamage);

            EnsurePhysicsAndLayer();

            var weapon = gameObject.GetComponent<EnemyWeapon>();
            if (weapon == null)
            {
                weapon = gameObject.AddComponent<EnemyWeapon>();
            }

            _phaseRunner = gameObject.GetComponent<BossPhaseRunner>();
            if (_phaseRunner == null)
            {
                _phaseRunner = gameObject.AddComponent<BossPhaseRunner>();
            }

            _phaseRunner.Configure(config, enemySpawner, weapon, fireRateMultiplier);
            _phaseRunner.EvaluateByHealthPercent(1f);

            var bossName = string.IsNullOrWhiteSpace(config.displayName) ? "Boss" : config.displayName;
            GameEvents.RaiseBossSpawned(bossName, _maxHealth);
            GameEvents.RaiseBossHealthChanged(_currentHealth, _maxHealth);
        }

        public void TakeDamage(int damage)
        {
            if (_isDead || damage <= 0)
            {
                return;
            }

            _currentHealth = Mathf.Max(0, _currentHealth - damage);
            GameEvents.RaiseBossHealthChanged(_currentHealth, _maxHealth);
            _phaseRunner?.EvaluateByHealthPercent(_maxHealth > 0 ? (float)_currentHealth / _maxHealth : 0f);

            if (_currentHealth <= 0)
            {
                StartCoroutine(DieRoutine());
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isDead)
            {
                return;
            }

            if (other.TryGetComponent(out PlayerHealth playerHealth))
            {
                playerHealth.TakeDamage(_contactDamage);
            }
        }

        private IEnumerator DieRoutine()
        {
            if (_isDead)
            {
                yield break;
            }

            _isDead = true;
            DisableCombatComponents();

            GameEvents.RaiseEnemyKilled(_scoreValue);
            GameEvents.RaiseBossDefeated();

            var deathDuration = _config != null ? Mathf.Max(0f, _config.deathDuration) : 0f;
            if (deathDuration > 0f)
            {
                yield return new WaitForSeconds(deathDuration);
            }

            Destroy(gameObject);
        }

        private void DisableCombatComponents()
        {
            var weapon = gameObject.GetComponent<EnemyWeapon>();
            if (weapon != null)
            {
                weapon.enabled = false;
            }

            if (_phaseRunner != null)
            {
                _phaseRunner.enabled = false;
            }

            var rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
            if (rigidbody2D != null)
            {
                rigidbody2D.simulated = false;
            }

            var colliders = gameObject.GetComponents<Collider2D>();
            for (var i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
            }
        }

        private void EnsurePhysicsAndLayer()
        {
            var enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer >= 0)
            {
                gameObject.layer = enemyLayer;
            }

            var rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
            if (rigidbody2D == null)
            {
                rigidbody2D = gameObject.AddComponent<Rigidbody2D>();
            }

            rigidbody2D.isKinematic = true;
            rigidbody2D.gravityScale = 0f;

            var collider2D = gameObject.GetComponent<Collider2D>();
            if (collider2D == null)
            {
                collider2D = gameObject.AddComponent<BoxCollider2D>();
            }

            collider2D.isTrigger = true;
        }
    }
}
