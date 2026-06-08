using AIGame.ShootEmUp.Core;
using UnityEngine;

namespace AIGame.ShootEmUp.Configs
{
    [System.Serializable]
    public class WaveEntry
    {
        public EnemyConfig enemyConfig;
        public int count = 1;
        public float spawnInterval = 0.4f;
        public SpawnPattern spawnPattern = SpawnPattern.SinglePoint;
        public MovementPattern movementPattern = MovementPattern.StraightDown;
        public float horizontalOffset = 0f;
        public Vector2[] customSpawnPoints;
    }

    [CreateAssetMenu(fileName = "WaveConfig", menuName = "ShootEmUp/Configs/Wave")]
    public class WaveConfig : ScriptableObject
    {
        public string waveId = "Wave_01";
        public float startDelay = 0f;
        public WaveEntry[] entries;
        public bool waitUntilAllEnemiesDead;
        public float maxDuration = 20f;
    }
}
