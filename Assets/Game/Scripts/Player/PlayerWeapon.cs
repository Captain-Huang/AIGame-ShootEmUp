using AIGame.ShootEmUp.Bullets;
using AIGame.ShootEmUp.Configs;
using AIGame.ShootEmUp.Core;
using AIGame.ShootEmUp.Utilities;
using UnityEngine;

namespace AIGame.ShootEmUp.Player
{
    public class PlayerWeapon : MonoBehaviour
    {
        [SerializeField] private bool autoFire = true;

        private WeaponConfig _weaponConfig;
        private int _currentPowerLevel = 1;
        private float _fireTimer;

        public void Configure(WeaponConfig weaponConfig, bool autoShoot)
        {
            _weaponConfig = weaponConfig;
            autoFire = autoShoot;
            _currentPowerLevel = 1;
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            {
                return;
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
            var size = config.size.sqrMagnitude > 0f ? config.size : new Vector2(0.16f, 0.36f);
            var bullet = RuntimeFactory.CreateActor(
                "PlayerBullet",
                config.tint,
                size,
                10,
                "PlayerBullet");

            bullet.transform.position = position;

            var bulletLogic = bullet.AddComponent<Bullet>();
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
    }
}
