using AIGame.ShootEmUp.Configs;
using AIGame.ShootEmUp.Core;
using UnityEngine;

namespace AIGame.ShootEmUp.Player
{
    public class PlayerPickupCollector : MonoBehaviour
    {
        private PlayerHealth _health;
        private PlayerWeapon _weapon;

        private void Awake()
        {
            _health = GetComponent<PlayerHealth>();
            _weapon = GetComponent<PlayerWeapon>();
        }

        public bool TryCollect(PickupConfig pickup)
        {
            if (pickup == null || GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            {
                return false;
            }

            switch (pickup.type)
            {
                case PickupType.PowerUp:
                    return _weapon != null && _weapon.TryUpgradePower(Mathf.Max(1, pickup.value));
                case PickupType.Heal:
                    return _health != null && _health.Heal(Mathf.Max(1, pickup.value));
                case PickupType.Bomb:
                    return _weapon != null && _weapon.AddBomb(Mathf.Max(1, pickup.value));
                case PickupType.Shield:
                    return _health != null && _health.ActivateShield(Mathf.Max(0.2f, pickup.duration));
                case PickupType.Score:
                    GameManager.Instance.AddScore(Mathf.Max(1, pickup.value));
                    return true;
                default:
                    return false;
            }
        }
    }
}
