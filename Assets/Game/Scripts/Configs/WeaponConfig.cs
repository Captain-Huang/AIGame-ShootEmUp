using UnityEngine;

namespace AIGame.ShootEmUp.Configs
{
    [System.Serializable]
    public class WeaponPowerLevel
    {
        public int level = 1;
        public BulletConfig bulletConfig;
        public float[] angles = { 0f };
        public Vector2[] offsets = { Vector2.zero };
    }

    [CreateAssetMenu(fileName = "WeaponConfig", menuName = "ShootEmUp/Configs/Weapon")]
    public class WeaponConfig : ScriptableObject
    {
        public float fireInterval = 0.18f;
        public WeaponPowerLevel[] powerLevels;
        public int bombDamage = 40;
        public int bombBossDamage = 20;
    }
}
