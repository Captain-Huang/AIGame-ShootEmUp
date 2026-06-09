using AIGame.ShootEmUp.Bullets;
using AIGame.ShootEmUp.Configs;
using AIGame.ShootEmUp.Core;
using UnityEngine;

namespace AIGame.ShootEmUp.Enemies
{
    public class EnemyWeapon : MonoBehaviour
    {
        private float _fireInterval = 1.35f;
        private BulletConfig _bulletConfig;
        private FirePattern _firePattern = FirePattern.SingleForward;
        private float _fireTimer;

        public void Configure(float interval, BulletConfig bulletConfig, FirePattern firePattern)
        {
            _fireInterval = Mathf.Max(0.1f, interval);
            _bulletConfig = bulletConfig;
            _firePattern = firePattern;
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing || _bulletConfig == null)
            {
                return;
            }

            _fireTimer -= Time.deltaTime;
            if (_fireTimer > 0f)
            {
                return;
            }

            FirePatternBurst();
            _fireTimer = _fireInterval;
        }

        private void FirePatternBurst()
        {
            switch (_firePattern)
            {
                case FirePattern.SingleForward:
                    SpawnBullet(Vector2.down);
                    break;
                case FirePattern.TripleFan:
                    SpawnByAngles(Vector2.down, new[] { -15f, 0f, 15f });
                    break;
                case FirePattern.FiveFan:
                    SpawnByAngles(Vector2.down, new[] { -25f, -12f, 0f, 12f, 25f });
                    break;
                case FirePattern.Ring:
                    SpawnRing(8);
                    break;
                case FirePattern.AimedSingle:
                    SpawnBullet(GetAimDirection());
                    break;
                case FirePattern.AimedTriple:
                    SpawnByAngles(GetAimDirection(), new[] { -10f, 0f, 10f });
                    break;
                case FirePattern.None:
                default:
                    break;
            }
        }

        private void SpawnByAngles(Vector2 baseDirection, float[] angles)
        {
            for (var i = 0; i < angles.Length; i++)
            {
                var dir = (Vector2)(Quaternion.Euler(0f, 0f, angles[i]) * baseDirection);
                SpawnBullet(dir.normalized);
            }
        }

        private void SpawnRing(int count)
        {
            var total = Mathf.Max(4, count);
            var angleStep = 360f / total;
            for (var i = 0; i < total; i++)
            {
                var dir = Quaternion.Euler(0f, 0f, i * angleStep) * Vector2.up;
                SpawnBullet(dir.normalized);
            }
        }

        private void SpawnBullet(Vector2 direction)
        {
            if (_bulletConfig == null || _bulletConfig.prefab == null)
            {
                Debug.LogError("Enemy bullet config prefab is missing.");
                return;
            }

            var bullet = Object.Instantiate(_bulletConfig.prefab);
            if (bullet == null)
            {
                Debug.LogError($"Failed to instantiate enemy bullet prefab for {_bulletConfig.bulletId}.");
                return;
            }

            bullet.name = string.IsNullOrWhiteSpace(_bulletConfig.bulletId) ? "EnemyBullet" : _bulletConfig.bulletId;
            bullet.transform.position = transform.position + (Vector3)(direction.normalized * 0.5f);
            var layer = LayerMask.NameToLayer("EnemyBullet");
            if (layer >= 0)
            {
                bullet.layer = layer;
            }

            var bulletLogic = bullet.GetComponent<Bullet>();
            if (bulletLogic == null)
            {
                Debug.LogError($"Bullet prefab {_bulletConfig.prefab.name} is missing Bullet component.");
                Destroy(bullet);
                return;
            }

            bulletLogic.Initialize(
                BulletOwner.Enemy,
                direction.normalized,
                Mathf.Max(0.5f, _bulletConfig.speed),
                Mathf.Max(1, _bulletConfig.damage),
                Mathf.Max(0.5f, _bulletConfig.lifetime));
        }

        private Vector2 GetAimDirection()
        {
            var player = FindObjectOfType<Player.PlayerHealth>();
            if (player == null)
            {
                return Vector2.down;
            }

            var toPlayer = (Vector2)(player.transform.position - transform.position);
            if (toPlayer.sqrMagnitude <= 0.001f)
            {
                return Vector2.down;
            }

            return toPlayer.normalized;
        }
    }
}
