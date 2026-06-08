using UnityEngine;

namespace AIGame.ShootEmUp.Levels
{
    public sealed class LevelUnlockService
    {
        private const string MaxUnlockedLevelKey = "shootemup.max_unlocked_level";

        public int GetMaxUnlockedLevel()
        {
            return Mathf.Max(1, PlayerPrefs.GetInt(MaxUnlockedLevelKey, 1));
        }

        public void UnlockLevel(int levelId)
        {
            var current = GetMaxUnlockedLevel();
            if (levelId <= current)
            {
                return;
            }

            PlayerPrefs.SetInt(MaxUnlockedLevelKey, levelId);
            PlayerPrefs.Save();
        }
    }
}
