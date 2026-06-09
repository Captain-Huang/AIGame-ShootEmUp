using AIGame.ShootEmUp.Configs;
using AIGame.ShootEmUp.Core;
using AIGame.ShootEmUp.Enemies;
using UnityEngine;

namespace AIGame.ShootEmUp.Levels
{
    public class LevelManager : MonoBehaviour
    {
        private GameConfigProvider _configProvider;
        private EnemySpawner _enemySpawner;
        private WaveSpawner _waveSpawner;
        private readonly LevelUnlockService _unlockService = new LevelUnlockService();

        private LevelConfig[] _levels;
        private LevelConfig _currentLevelConfig;
        private int _currentLevelIndex = -1;
        private bool _runActive;

        public int CurrentLevelIndex => _currentLevelIndex;
        public int CurrentLevelId => (_levels != null && _currentLevelIndex >= 0 && _currentLevelIndex < _levels.Length)
            ? _levels[_currentLevelIndex].levelId
            : 0;
        public LevelConfig CurrentLevelConfig => _currentLevelConfig;

        public bool HasNextLevel => _levels != null && _currentLevelIndex + 1 < _levels.Length;

        public static LevelManager CreateOrFind()
        {
            var existing = FindObjectOfType<LevelManager>();
            if (existing != null)
            {
                return existing;
            }

            var go = new GameObject("LevelManager");
            return go.AddComponent<LevelManager>();
        }

        public void Initialize(GameConfigProvider configProvider, EnemySpawner enemySpawner)
        {
            _configProvider = configProvider;
            _enemySpawner = enemySpawner;
            _levels = ResolveLevels(configProvider);

            if (_waveSpawner == null)
            {
                _waveSpawner = gameObject.GetComponent<WaveSpawner>();
                if (_waveSpawner == null)
                {
                    _waveSpawner = gameObject.AddComponent<WaveSpawner>();
                }
            }

            _waveSpawner.Initialize(_enemySpawner);
        }

        public bool StartRun()
        {
            _currentLevelIndex = -1;
            _currentLevelConfig = null;
            _runActive = true;
            return StartNextLevel();
        }

        public bool StartNextLevel()
        {
            if (!_runActive)
            {
                return false;
            }

            if (_levels == null || _levels.Length == 0 || !HasNextLevel && _currentLevelIndex >= 0)
            {
                return false;
            }

            var nextIndex = _currentLevelIndex + 1;
            if (nextIndex < 0 || nextIndex >= _levels.Length)
            {
                return false;
            }

            _currentLevelIndex = nextIndex;
            StartLevel(_levels[_currentLevelIndex]);
            return true;
        }

        public void StopRun()
        {
            _runActive = false;
            _currentLevelConfig = null;
            _waveSpawner?.StopRunning();
        }

        private void StartLevel(LevelConfig levelConfig)
        {
            _currentLevelConfig = levelConfig;
            if (levelConfig == null)
            {
                OnCurrentLevelCompleted();
                return;
            }

            GameEvents.RaiseLevelStarted(levelConfig.levelId, levelConfig.displayName);
            _waveSpawner.RunLevel(levelConfig, OnCurrentLevelCompleted);
        }

        private void OnCurrentLevelCompleted()
        {
            var levelId = CurrentLevelId;
            if (levelId > 0)
            {
                GameEvents.RaiseLevelCleared(levelId);
                _unlockService.UnlockLevel(levelId + 1);
            }

            if (!HasNextLevel)
            {
                _runActive = false;
                GameEvents.RaiseRunCompleted();
            }
        }

        private static LevelConfig[] ResolveLevels(GameConfigProvider configProvider)
        {
            var database = configProvider?.LevelDatabase;
            if (database != null && database.levels != null && database.levels.Length > 0)
            {
                return database.levels;
            }

            return new LevelConfig[0];
        }
    }
}
