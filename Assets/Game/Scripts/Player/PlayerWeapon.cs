using AIGame.ShootEmUp.Bullets;
using AIGame.ShootEmUp.Bosses;
using AIGame.ShootEmUp.Configs;
using AIGame.ShootEmUp.Core;
using AIGame.ShootEmUp.Enemies;
using UnityEngine;

namespace AIGame.ShootEmUp.Player
{
    public class PlayerWeapon : MonoBehaviour
    {
        [SerializeField] private bool autoFire = true;

        private WeaponConfig _weaponConfig;
        private int _currentPowerLevel = 1;
        private int _maxPowerLevel = 1;
        private int _currentBombs;
        private int _maxBombs = 3;
        private float _fireTimer;

        public void Configure(WeaponConfig weaponConfig, bool autoShoot, int initialBombs, int maxBombs, int maxPowerLevel)
        {
            _weaponConfig = weaponConfig;
            autoFire = autoShoot;
            _currentPowerLevel = 1;
            _maxPowerLevel = Mathf.Max(1, maxPowerLevel);
            _maxBombs = Mathf.Max(0, maxBombs);
            _currentBombs = Mathf.Clamp(initialBombs, 0, _maxBombs);
            RaisePowerChanged();
            GameEvents.RaisePlayerBombChanged(_currentBombs, _maxBombs);
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            {
                UseBomb();
            }

            var fireInterval = Mathf.Max(0.05f, _weaponConfig != null ? _weaponConfig.fireInterval : 0.18f);
            _fireTimer -= Time.deltaTime;
            var wantsFire = autoFire || Input.GetKey(KeyCode.Space);
            if (!wantsFire || _fireTimer > 0f)
            {
                return;
            }

            Fire();
            _fireTimer = fireInterval;
        }

        public bool TryUpgradePower(int amount = 1)
        {
            if (amount <= 0)
            {
                return false;
            }

            var cap = ResolveMaxPowerLevel();
            var next = Mathf.Clamp(_currentPowerLevel + amount, 1, cap);
            if (next == _currentPowerLevel)
            {
                return false;
            }

            _currentPowerLevel = next;
            RaisePowerChanged();
            return true;
        }

        public bool AddBomb(int amount = 1)
        {
            if (amount <= 0 || _currentBombs >= _maxBombs)
            {
                return false;
            }

            _currentBombs = Mathf.Clamp(_currentBombs + amount, 0, _maxBombs);
            GameEvents.RaisePlayerBombChanged(_currentBombs, _maxBombs);
            return true;
        }

        private void Fire()
        {
            var powerLevel = GetCurrentPowerLevelConfig();
            if (powerLevel == null || powerLevel.bulletConfig == null)
            {
                return;
            }

            var bulletConfig = powerLevel.bulletConfig;
            var angles = (powerLevel.angles != null && powerLevel.angles.Length > 0) ? powerLevel.angles : new[] { 0f };

            for (var i = 0; i < angles.Length; i++)
            {
                var offset = Vector2.zero;
                if (powerLevel.offsets != null && i < powerLevel.offsets.Length)
                {
                    offset = powerLevel.offsets[i];
                }

                var spawnPos = (Vector2)transform.position + new Vector2(offset.x, offset.y + 0.55f);
                var direction = Quaternion.Euler(0f, 0f, angles[i]) * Vector2.up;
                SpawnBullet(bulletConfig, spawnPos, direction);
            }
        }

        private void SpawnBullet(BulletConfig config, Vector2 position, Vector2 direction)
        {
            if (config == null || config.prefab == null)
            {
                Debug.LogError("Player bullet config prefab is missing.");
                return;
            }

            var bullet = Object.Instantiate(config.prefab);
            if (bullet == null)
            {
                Debug.LogError($"Failed to instantiate player bullet prefab for {config.bulletId}.");
                return;
            }

            bullet.name = string.IsNullOrWhiteSpace(config.bulletId) ? "PlayerBullet" : config.bulletId;
            bullet.transform.position = position;
            var layer = LayerMask.NameToLayer("PlayerBullet");
            if (layer >= 0)
            {
                bullet.layer = layer;
            }

            var bulletLogic = bullet.GetComponent<Bullet>();
            if (bulletLogic == null)
            {
                Debug.LogError($"Bullet prefab {config.prefab.name} is missing Bullet component.");
                Destroy(bullet);
                return;
            }

            bulletLogic.Initialize(
                BulletOwner.Player,
                direction,
                Mathf.Max(1f, config.speed),
                Mathf.Max(1, config.damage),
                Mathf.Max(0.5f, config.lifetime));
        }

        private WeaponPowerLevel GetCurrentPowerLevelConfig()
        {
            if (_weaponConfig == null || _weaponConfig.powerLevels == null || _weaponConfig.powerLevels.Length == 0)
            {
                return null;
            }

            WeaponPowerLevel selected = null;
            for (var i = 0; i < _weaponConfig.powerLevels.Length; i++)
            {
                var level = _weaponConfig.powerLevels[i];
                if (level == null || level.level > _currentPowerLevel)
                {
                    continue;
                }

                if (selected == null || level.level >= selected.level)
                {
                    selected = level;
                }
            }

            return selected;
        }

        private void UseBomb()
        {
            if (_currentBombs <= 0)
            {
                return;
            }

            _currentBombs--;
            GameEvents.RaisePlayerBombChanged(_currentBombs, _maxBombs);

            var bombDamage = _weaponConfig != null ? Mathf.Max(1, _weaponConfig.bombDamage) : 40;
            var bombBossDamage = _weaponConfig != null ? Mathf.Max(1, _weaponConfig.bombBossDamage) : 20;

            foreach (var enemy in FindObjectsOfType<EnemyBase>())
            {
                if (enemy != null)
                {
                    enemy.TakeDamage(bombDamage);
                }
            }

            foreach (var boss in FindObjectsOfType<BossController>())
            {
                if (boss != null)
                {
                    boss.TakeDamage(bombBossDamage);
                }
            }

            foreach (var bullet in FindObjectsOfType<Bullet>())
            {
                if (bullet != null && bullet.Owner == BulletOwner.Enemy)
                {
                    Destroy(bullet.gameObject);
                }
            }
        }

        private int ResolveMaxPowerLevel()
        {
            var configured = Mathf.Max(1, _maxPowerLevel);
            if (_weaponConfig == null || _weaponConfig.powerLevels == null || _weaponConfig.powerLevels.Length == 0)
            {
                return configured;
            }

            var highest = 1;
            for (var i = 0; i < _weaponConfig.powerLevels.Length; i++)
            {
                var level = _weaponConfig.powerLevels[i];
                if (level != null)
                {
                    highest = Mathf.Max(highest, level.level);
                }
            }

            return Mathf.Clamp(configured, 1, highest);
        }

        private void RaisePowerChanged()
        {
            GameEvents.RaisePlayerPowerChanged(_currentPowerLevel, ResolveMaxPowerLevel());
        }
    }
}
