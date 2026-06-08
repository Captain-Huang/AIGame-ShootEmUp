using UnityEngine;

namespace AIGame.ShootEmUp.Configs
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "ShootEmUp/Configs/Player")]
    public class PlayerConfig : ScriptableObject
    {
        public GameObject prefab;
        public int maxHealth = 5;
        public int initialHealth = 3;
        public float moveSpeed = 6f;
        public float invincibleDuration = 1.5f;
        public int initialBombs = 1;
        public int maxBombs = 3;
        public int maxPowerLevel = 4;
        public WeaponConfig weaponConfig;
    }
}
