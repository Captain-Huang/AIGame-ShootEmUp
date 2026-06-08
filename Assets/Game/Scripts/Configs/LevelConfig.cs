using UnityEngine;

namespace AIGame.ShootEmUp.Configs
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "ShootEmUp/Configs/Level")]
    public class LevelConfig : ScriptableObject
    {
        [Header("Identity")]
        public int levelId = 1;
        public string displayName = "Level 1";
        public int difficulty = 1;
        public float estimatedDuration = 90f;

        [Header("Presentation")]
        public Sprite backgroundSprite;
        public AudioClip bgm;
        public AudioClip bossBgm;

        [Header("Gameplay")]
        public WaveConfig[] waves;
        public BossConfig bossConfig;
        public WaveConfig preBossSupplyWave;
        public float enemyHealthMultiplier = 1f;
        public float enemyFireRateMultiplier = 1f;
        public int scoreBonus = 1000;
    }
}
