using UnityEngine;

namespace AIGame.ShootEmUp.Configs
{
    [CreateAssetMenu(fileName = "LevelDatabase", menuName = "ShootEmUp/Configs/Level Database")]
    public class LevelDatabase : ScriptableObject
    {
        public LevelConfig[] levels;
        public LevelConfig firstLevel;
    }
}
