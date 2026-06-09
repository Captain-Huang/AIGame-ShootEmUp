using UnityEngine;

namespace AIGame.ShootEmUp.Save
{
    public sealed class ProgressStore
    {
        private const string MaxUnlockedLevelKey = "shootemup.max_unlocked_level";
        private const string BestScoreKey = "shootemup.best_score";
        private const string FullscreenKey = "shootemup.fullscreen";

        public int GetBestScore()
        {
            return Mathf.Max(0, PlayerPrefs.GetInt(BestScoreKey, 0));
        }

        public bool TrySetBestScore(int score)
        {
            var value = Mathf.Max(0, score);
            var current = GetBestScore();
            if (value <= current)
            {
                return false;
            }

            PlayerPrefs.SetInt(BestScoreKey, value);
            PlayerPrefs.Save();
            return true;
        }

        public int GetMaxUnlockedLevel()
        {
            return Mathf.Max(1, PlayerPrefs.GetInt(MaxUnlockedLevelKey, 1));
        }

        public bool IsFullscreen()
        {
            return PlayerPrefs.GetInt(FullscreenKey, 1) != 0;
        }

        public void SetFullscreen(bool fullscreen)
        {
            PlayerPrefs.SetInt(FullscreenKey, fullscreen ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
