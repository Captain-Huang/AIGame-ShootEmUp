using AIGame.ShootEmUp.Core;
using AIGame.ShootEmUp.Player;
using UnityEngine;

namespace AIGame.ShootEmUp.Enemies
{
    public class EnemyBase : MonoBehaviour
    {
        private int _currentHealth = 1;
        private int _contactDamage = 1;
        private int _scoreValue = 100;
        private bool _isDead;

        public void Configure(int maxHealth, int contactDamage, int scoreValue)
        {
            _currentHealth = Mathf.Max(1, maxHealth);
            _contactDamage = Mathf.Max(1, contactDamage);
            _scoreValue = Mathf.Max(1, scoreValue);
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
            GameEvents.RaiseEnemyKilled(_scoreValue);
            Destroy(gameObject);
        }
    }
}
