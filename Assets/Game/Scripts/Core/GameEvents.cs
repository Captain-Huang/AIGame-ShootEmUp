using System;

namespace AIGame.ShootEmUp.Core
{
    public static class GameEvents
    {
        public static event Action<int> EnemyKilled;
        public static event Action EnemySpawned;
        public static event Action EnemyDespawned;
        public static event Action<int> ScoreChanged;
        public static event Action<int, int> PlayerHealthChanged;
        public static event Action PlayerDied;
        public static event Action<string, int> BossSpawned;
        public static event Action<int, int> BossHealthChanged;
        public static event Action<string> BossPhaseChanged;
        public static event Action BossDefeated;
        public static event Action<int, string> LevelStarted;
        public static event Action<int> LevelCleared;
        public static event Action RunCompleted;
        public static event Action<GameState> StateChanged;

        public static void RaiseEnemyKilled(int scoreValue)
        {
            EnemyKilled?.Invoke(scoreValue);
        }

        public static void RaiseEnemySpawned()
        {
            EnemySpawned?.Invoke();
        }

        public static void RaiseEnemyDespawned()
        {
            EnemyDespawned?.Invoke();
        }

        public static void RaiseScoreChanged(int score)
        {
            ScoreChanged?.Invoke(score);
        }

        public static void RaisePlayerHealthChanged(int currentHealth, int maxHealth)
        {
            PlayerHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public static void RaisePlayerDied()
        {
            PlayerDied?.Invoke();
        }

        public static void RaiseBossSpawned(string bossName, int maxHealth)
        {
            BossSpawned?.Invoke(bossName, maxHealth);
        }

        public static void RaiseBossHealthChanged(int currentHealth, int maxHealth)
        {
            BossHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public static void RaiseBossPhaseChanged(string phaseName)
        {
            BossPhaseChanged?.Invoke(phaseName);
        }

        public static void RaiseBossDefeated()
        {
            BossDefeated?.Invoke();
        }

        public static void RaiseLevelStarted(int levelId, string displayName)
        {
            LevelStarted?.Invoke(levelId, displayName);
        }

        public static void RaiseLevelCleared(int levelId)
        {
            LevelCleared?.Invoke(levelId);
        }

        public static void RaiseRunCompleted()
        {
            RunCompleted?.Invoke();
        }

        public static void RaiseStateChanged(GameState gameState)
        {
            StateChanged?.Invoke(gameState);
        }
    }
}
