using AIGame.ShootEmUp.Core;
using AIGame.ShootEmUp.Player;
using AIGame.ShootEmUp.Utilities;
using UnityEngine;

namespace AIGame.ShootEmUp.Enemies
{
    public class EnemyMovement : MonoBehaviour
    {
        private Vector2 _direction = Vector2.down;
        private float _speed = 2f;
        private MovementPattern _movementPattern = MovementPattern.StraightDown;
        private float _sineBaseX;
        private float _sineAmplitude = 0.8f;
        private float _sineFrequency = 2f;
        private float _stopY;

        public void Configure(Vector2 direction, float speed, MovementPattern movementPattern)
        {
            _direction = direction.normalized;
            _speed = Mathf.Max(0.2f, speed);
            _movementPattern = movementPattern;
            _sineBaseX = transform.position.x;
            _stopY = CameraBounds.GetWorldBounds(Camera.main, 0.8f).MaxY - 1.4f;
        }

        private void Update()
        {
            if (_movementPattern == MovementPattern.Sine)
            {
                var pos = transform.position;
                pos.y -= _speed * Time.deltaTime;
                pos.x = _sineBaseX + Mathf.Sin(Time.time * _sineFrequency) * _sineAmplitude;
                transform.position = pos;
            }
            else if (_movementPattern == MovementPattern.StopAndShoot)
            {
                var pos = transform.position;
                if (pos.y > _stopY)
                {
                    pos.y -= _speed * Time.deltaTime;
                    transform.position = pos;
                }
            }
            else if (_movementPattern == MovementPattern.TrackPlayerX)
            {
                var pos = transform.position;
                pos.y -= _speed * Time.deltaTime;
                var player = FindObjectOfType<Player.PlayerHealth>();
                if (player != null)
                {
                    pos.x = Mathf.MoveTowards(pos.x, player.transform.position.x, _speed * 0.6f * Time.deltaTime);
                }

                transform.position = pos;
            }
            else
            {
                transform.position += (Vector3)(_direction * (_speed * Time.deltaTime));
            }

            var bounds = CameraBounds.GetWorldBounds(Camera.main, 1.5f);
            if (transform.position.y < bounds.MinY)
            {
                TryApplyEscapePenalty();
                Destroy(gameObject);
                return;
            }

            if (transform.position.x < bounds.MinX || transform.position.x > bounds.MaxX)
            {
                Destroy(gameObject);
            }
        }

        private static void TryApplyEscapePenalty()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            {
                return;
            }

            var player = FindObjectOfType<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(1);
            }
        }
    }
}
