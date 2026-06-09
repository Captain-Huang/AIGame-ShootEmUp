using AIGame.ShootEmUp.Configs;
using AIGame.ShootEmUp.Player;
using AIGame.ShootEmUp.Utilities;
using UnityEngine;

namespace AIGame.ShootEmUp.Pickups
{
    public class Pickup : MonoBehaviour
    {
        private PickupConfig _config;
        private float _moveSpeed = 1.5f;
        private float _remainingLifetime = 14f;

        public static Pickup Spawn(PickupConfig config, Vector3 position)
        {
            if (config == null)
            {
                return null;
            }

            if (config.prefab == null)
            {
                Debug.LogError($"PickupConfig {config.pickupId} has no prefab assigned.");
                return null;
            }

            var pickupGo = Object.Instantiate(config.prefab);
            if (pickupGo == null)
            {
                Debug.LogError($"Failed to instantiate pickup prefab for {config.pickupId}.");
                return null;
            }

            pickupGo.name = string.IsNullOrWhiteSpace(config.pickupId) ? "Pickup" : config.pickupId;
            pickupGo.transform.position = position;
            var pickup = pickupGo.GetComponent<Pickup>();
            if (pickup == null)
            {
                Debug.LogError($"Pickup prefab {config.prefab.name} is missing Pickup component.");
                Object.Destroy(pickupGo);
                return null;
            }

            pickup.Configure(config);
            return pickup;
        }

        public void Configure(PickupConfig config)
        {
            _config = config;
            _moveSpeed = Mathf.Max(0.2f, config.moveSpeed);
        }

        private void Update()
        {
            transform.position += Vector3.down * (_moveSpeed * Time.deltaTime);

            _remainingLifetime -= Time.deltaTime;
            if (_remainingLifetime <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            var bounds = CameraBounds.GetWorldBounds(Camera.main, 1.1f);
            if (transform.position.y < bounds.MinY)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_config == null)
            {
                return;
            }

            if (!other.TryGetComponent(out PlayerPickupCollector collector))
            {
                return;
            }

            if (!collector.TryCollect(_config))
            {
                return;
            }

            Destroy(gameObject);
        }
    }
}
