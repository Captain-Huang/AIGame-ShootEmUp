using AIGame.ShootEmUp.Core;
using UnityEngine;

namespace AIGame.ShootEmUp.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 3;
        [SerializeField] private int initialHealth = 3;
        [SerializeField] private float invincibleDuration = 1.5f;

        private int _currentHealth;
        private float _invincibleTimer;
        private float _shieldTimer;
        private bool _shieldActive;
        private SpriteRenderer _renderer;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            ResetHealth();
        }

        public void Configure(int maxHp, int initialHp, float invincibleSeconds)
        {
            maxHealth = Mathf.Max(1, maxHp);
            initialHealth = Mathf.Clamp(initialHp, 1, maxHealth);
            invincibleDuration = Mathf.Max(0f, invincibleSeconds);
            ResetHealth();
        }

        private void Update()
        {
            if (_shieldActive)
            {
                _shieldTimer -= Time.deltaTime;
                if (_shieldTimer <= 0f)
                {
                    _shieldActive = false;
                    _shieldTimer = 0f;
                    GameEvents.RaisePlayerShieldChanged(false, 0f);
                }
            }

            if (_invincibleTimer > 0f)
            {
                _invincibleTimer -= Time.deltaTime;
                UpdateBlink();
                return;
            }

            if (_renderer != null)
            {
                var color = _renderer.color;
                color.a = 1f;
                _renderer.color = color;
            }
        }

        public void ResetHealth()
        {
            _currentHealth = Mathf.Clamp(initialHealth, 1, maxHealth);
            _invincibleTimer = 0f;
            _shieldActive = false;
            _shieldTimer = 0f;
            GameEvents.RaisePlayerHealthChanged(_currentHealth, maxHealth);
            GameEvents.RaisePlayerShieldChanged(false, 0f);
        }

        public void TakeDamage(int damage)
        {
            if (damage <= 0 || _invincibleTimer > 0f || GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            {
                return;
            }

            if (_shieldActive)
            {
                _shieldActive = false;
                _shieldTimer = 0f;
                _invincibleTimer = Mathf.Max(_invincibleTimer, 0.15f);
                GameEvents.RaisePlayerShieldChanged(false, 0f);
                return;
            }

            _currentHealth = Mathf.Max(0, _currentHealth - damage);
            GameEvents.RaisePlayerHealthChanged(_currentHealth, maxHealth);
            if (_currentHealth <= 0)
            {
                Die();
                return;
            }

            _invincibleTimer = invincibleDuration;
        }

        public bool Heal(int amount)
        {
            if (amount <= 0 || _currentHealth >= maxHealth)
            {
                return false;
            }

            _currentHealth = Mathf.Clamp(_currentHealth + amount, 0, maxHealth);
            GameEvents.RaisePlayerHealthChanged(_currentHealth, maxHealth);
            return true;
        }

        public bool ActivateShield(float duration)
        {
            if (duration <= 0f)
            {
                return false;
            }

            _shieldActive = true;
            _shieldTimer = Mathf.Max(_shieldTimer, duration);
            GameEvents.RaisePlayerShieldChanged(true, _shieldTimer);
            return true;
        }

        private void Die()
        {
            GameEvents.RaisePlayerDied();
            Destroy(gameObject);
        }

        private void UpdateBlink()
        {
            if (_renderer == null)
            {
                return;
            }

            var alpha = Mathf.PingPong(Time.unscaledTime * 12f, 1f) > 0.5f ? 0.25f : 1f;
            var color = _renderer.color;
            color.a = alpha;
            _renderer.color = color;
        }
    }
}
