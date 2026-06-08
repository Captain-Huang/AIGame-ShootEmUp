using AIGame.ShootEmUp.Core;
using UnityEngine;

namespace AIGame.ShootEmUp.Configs
{
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "ShootEmUp/Configs/Enemy")]
    public class EnemyConfig : ScriptableObject
    {
        [Header("Identity")]
        public string enemyId = "E00";
        public string displayName = "Enemy";

        [Header("Visual")]
        public GameObject prefab;
        public Color tint = Color.red;
        public Vector2 size = new Vector2(0.6f, 0.6f);

        [Header("Stats")]
        public int maxHealth = 1;
        public float moveSpeed = 2f;
        public int contactDamage = 1;
        public int score = 100;
        [Min(1)] public int spawnWeight = 100;
        public MovementPattern movementPattern = MovementPattern.StraightDown;

        [Header("Combat")]
        public FirePattern firePattern = FirePattern.None;
        public float fireInterval = 1f;
        public BulletConfig bulletConfig;

        [Header("Drops")]
        public PickupDropEntry[] dropTable;
    }
}
