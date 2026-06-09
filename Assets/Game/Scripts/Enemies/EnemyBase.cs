using AIGame.ShootEmUp.Configs;
using AIGame.ShootEmUp.Core;
using AIGame.ShootEmUp.Pickups;
using AIGame.ShootEmUp.Player;
using UnityEngine;

namespace AIGame.ShootEmUp.Enemies
{
    public class EnemyBase : MonoBehaviour
    {
        private int _currentHealth = 1;
        private int _contactDamage = 1;
        private int _scoreValue = 100;
        private PickupDropEntry[] _dropTable;
        private bool _isDead;

        public void Configure(int maxHealth, int contactDamage, int scoreValue, PickupDropEntry[] dropTable)
        {
            _currentHealth = Mathf.Max(1, maxHealth);
            _contactDamage = Mathf.Max(1, contactDamage);
            _scoreValue = Mathf.Max(1, scoreValue);
            _dropTable = dropTable;
        }

        public void TakeDamage(int damage)
        {
            if (damage <= 0 || _isDead)
            {
                return;
            }

            _currentHealth -= damage;
            if (_currentHealth <= 0)
            {
                Die();
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
                Die();
            }
        }

        private void OnDestroy()
        {
            GameEvents.RaiseEnemyDespawned();
        }

        private void Die()
        {
            if (_isDead)
            {
                return;
            }

            _isDead = true;
            TryDropPickup();
            GameEvents.RaiseEnemyKilled(_scoreValue);
            Destroy(gameObject);
        }

        private void TryDropPickup()
        {
            if (_dropTable == null || _dropTable.Length == 0)
            {
                return;
            }

            var randomValue = Random.value;
            var cumulative = 0f;
            for (var i = 0; i < _dropTable.Length; i++)
            {
                var entry = _dropTable[i];
                if (entry.pickup == null || entry.dropChance <= 0f)
                {
                    continue;
                }

                cumulative += Mathf.Clamp01(entry.dropChance);
                if (randomValue > cumulative)
                {
                    continue;
                }

                Pickup.Spawn(entry.pickup, transform.position);
                return;
            }
        }
    }
}
