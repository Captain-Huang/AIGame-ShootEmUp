using AIGame.ShootEmUp.Core;
using AIGame.ShootEmUp.Enemies;
using AIGame.ShootEmUp.Bosses;
using AIGame.ShootEmUp.Player;
using AIGame.ShootEmUp.Utilities;
using UnityEngine;

namespace AIGame.ShootEmUp.Bullets
{
    public class Bullet : MonoBehaviour
    {
        private BulletOwner _owner;
        private Vector2 _direction;
        private float _speed;
        private int _damage;
        private float _remainingLife;

        public BulletOwner Owner => _owner;

        public void Initialize(BulletOwner owner, Vector2 direction, float speed, int damage, float lifetime)
        {
            _owner = owner;
            _direction = direction.normalized;
            _speed = speed;
            _damage = damage;
            _remainingLife = lifetime;
        }

        private void Update()
        {
            transform.position += (Vector3)(_direction * (_speed * Time.deltaTime));
            _remainingLife -= Time.deltaTime;
            if (_remainingLife <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            var bounds = CameraBounds.GetWorldBounds(Camera.main, 1f);
            var position = transform.position;
            if (position.x < bounds.MinX || position.x > bounds.MaxX || position.y < bounds.MinY || position.y > bounds.MaxY)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_owner == BulletOwner.Player)
            {
                if (other.TryGetComponent(out EnemyBase enemy))
                {
                    enemy.TakeDamage(_damage);
                    Destroy(gameObject);
                    return;
                }

                if (other.TryGetComponent(out BossController boss))
                {
                    boss.TakeDamage(_damage);
                    Destroy(gameObject);
                }

                return;
            }

            if (_owner == BulletOwner.Enemy && other.TryGetComponent(out PlayerHealth player))
            {
                player.TakeDamage(_damage);
                Destroy(gameObject);
            }
        }
    }
}
