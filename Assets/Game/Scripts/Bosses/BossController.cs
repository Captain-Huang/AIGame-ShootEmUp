using System.Collections;
using AIGame.ShootEmUp.Configs;
using AIGame.ShootEmUp.Core;
using AIGame.ShootEmUp.Enemies;
using AIGame.ShootEmUp.Pickups;
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

            if (config.prefab == null)
            {
                Debug.LogError($"BossConfig {config.bossId} has no prefab assigned.");
                return null;
            }

            var bossGo = Object.Instantiate(config.prefab);
            if (bossGo == null)
            {
                Debug.LogError($"Failed to instantiate boss prefab for {config.bossId}.");
                return null;
            }

            bossGo.name = $"Boss_{config.bossId}";
            var bounds = CameraBounds.GetWorldBounds(Camera.main, 0.8f);
            bossGo.transform.position = new Vector3(0f, bounds.MaxY + 0.9f, 0f);
            bossGo.transform.rotation = Quaternion.Euler(0f, 0f, 180f);

            var controller = bossGo.GetComponent<BossController>();
            if (controller == null)
            {
                Debug.LogError($"Boss prefab {config.prefab.name} is missing BossController.");
                Object.Destroy(bossGo);
                return null;
            }

            if (!controller.Configure(config, enemySpawner, healthMultiplier, fireRateMultiplier))
            {
                Object.Destroy(bossGo);
                return null;
            }

            return controller;
        }

        public bool Configure(BossConfig config, EnemySpawner enemySpawner, float healthMultiplier, float fireRateMultiplier)
        {
            if (config == null)
            {
                return false;
            }

            _config = config;
            _isDead = false;
            _maxHealth = Mathf.Max(1, Mathf.RoundToInt(config.maxHealth * Mathf.Max(0.1f, healthMultiplier)));
            _currentHealth = _maxHealth;
            _scoreValue = Mathf.Max(100, config.score);
            _contactDamage = Mathf.Max(1, config.contactDamage);

            if (!EnsurePhysicsAndLayer())
            {
                return false;
            }

            var weapon = gameObject.GetComponent<EnemyWeapon>();
            if (weapon == null)
            {
                Debug.LogError($"Boss prefab {gameObject.name} is missing EnemyWeapon.");
                return false;
            }

            _phaseRunner = gameObject.GetComponent<BossPhaseRunner>();
            if (_phaseRunner == null)
            {
                Debug.LogError($"Boss prefab {gameObject.name} is missing BossPhaseRunner.");
                return false;
            }

            _phaseRunner.Configure(config, enemySpawner, weapon, fireRateMultiplier);
            _phaseRunner.EvaluateByHealthPercent(1f);

            var bossName = string.IsNullOrWhiteSpace(config.displayName) ? "Boss" : config.displayName;
            GameEvents.RaiseBossSpawned(bossName, _maxHealth);
            GameEvents.RaiseBossHealthChanged(_currentHealth, _maxHealth);
            return true;
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
            DropGuaranteedPickups();

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

        private bool EnsurePhysicsAndLayer()
        {
            var enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer >= 0)
            {
                gameObject.layer = enemyLayer;
            }

            var rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
            if (rigidbody2D == null)
            {
                Debug.LogError($"Boss prefab {gameObject.name} is missing Rigidbody2D.");
                return false;
            }

            rigidbody2D.isKinematic = true;
            rigidbody2D.gravityScale = 0f;

            var collider2D = gameObject.GetComponent<Collider2D>();
            if (collider2D == null)
            {
                Debug.LogError($"Boss prefab {gameObject.name} is missing Collider2D.");
                return false;
            }

            collider2D.isTrigger = true;
            return true;
        }

        private void DropGuaranteedPickups()
        {
            if (_config == null || _config.guaranteedDrops == null || _config.guaranteedDrops.Length == 0)
            {
                return;
            }

            for (var i = 0; i < _config.guaranteedDrops.Length; i++)
            {
                var pickup = _config.guaranteedDrops[i];
                if (pickup == null)
                {
                    continue;
                }

                var offset = new Vector3(Random.Range(-1f, 1f), Random.Range(-0.4f, 0.6f), 0f);
                Pickup.Spawn(pickup, transform.position + offset);
            }
        }
    }
}
