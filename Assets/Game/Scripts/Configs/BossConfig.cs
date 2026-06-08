using AIGame.ShootEmUp.Core;
using UnityEngine;

namespace AIGame.ShootEmUp.Configs
{
    [System.Serializable]
    public class BossPhaseConfig
    {
        public string phaseName = "Phase";
        [Range(0f, 1f)] public float startHealthPercent = 1f;
        public MovementPattern movePattern = MovementPattern.BossHorizontal;
        public FirePattern firePattern = FirePattern.SingleForward;
        public float fireInterval = 1f;
        public BulletConfig bulletConfig;
        public EnemyConfig summonEnemy;
        public float summonInterval = 6f;
    }

    [CreateAssetMenu(fileName = "BossConfig", menuName = "ShootEmUp/Configs/Boss")]
    public class BossConfig : ScriptableObject
    {
        [Header("Identity")]
        public string bossId = "B01";
        public string displayName = "Boss";

        [Header("Visual")]
        public GameObject prefab;
        public Color tint = new Color(0.85f, 0.28f, 0.9f, 1f);
        public Vector2 size = new Vector2(2.2f, 1.2f);

        [Header("Stats")]
        public int maxHealth = 100;
        public float moveSpeed = 1.65f;
        public int contactDamage = 1;
        public int score = 3000;

        [Header("Phases")]
        public BossPhaseConfig[] phases;

        [Header("Rewards")]
        public float deathDuration = 2.5f;
        public PickupConfig[] guaranteedDrops;
    }
}
