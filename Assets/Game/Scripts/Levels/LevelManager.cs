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
        private LevelBackgroundScroller _backgroundScroller;
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

            if (_backgroundScroller == null)
            {
                _backgroundScroller = gameObject.GetComponent<LevelBackgroundScroller>();
                if (_backgroundScroller == null)
                {
                    _backgroundScroller = gameObject.AddComponent<LevelBackgroundScroller>();
                }
            }

            _backgroundScroller.Initialize(Camera.main);
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
            _backgroundScroller?.Clear();
        }

        private void StartLevel(LevelConfig levelConfig)
        {
            _currentLevelConfig = levelConfig;
            _backgroundScroller?.ApplyLevel(levelConfig);
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

    public class LevelBackgroundScroller : MonoBehaviour
    {
        private const int BackgroundSortingOrder = -200;
        private const float MinScrollSpeed = 0.05f;

        private Camera _mainCamera;
        private Transform _root;
        private SpriteRenderer _segmentA;
        private SpriteRenderer _segmentB;
        private float _scrollSpeed;
        private float _segmentHeight;
        private bool _active;

        public void Initialize(Camera mainCamera)
        {
            _mainCamera = mainCamera;
            EnsureRenderers();
            Clear();
        }

        public void ApplyLevel(LevelConfig levelConfig)
        {
            if (levelConfig == null || levelConfig.backgroundSprite == null)
            {
                Clear();
                return;
            }

            EnsureRenderers();
            _root.gameObject.SetActive(true);
            _active = true;
            _scrollSpeed = Mathf.Max(MinScrollSpeed, levelConfig.backgroundScrollSpeed);

            _segmentA.sprite = levelConfig.backgroundSprite;
            _segmentB.sprite = levelConfig.backgroundSprite;

            var camera = ResolveCamera();
            if (camera == null)
            {
                Clear();
                return;
            }
            var spriteWidth = Mathf.Max(0.01f, levelConfig.backgroundSprite.bounds.size.x);
            var cameraWidth = camera.orthographicSize * camera.aspect * 2f;
            var scale = cameraWidth / spriteWidth;

            var scaleVector = new Vector3(scale, scale, 1f);
            _segmentA.transform.localScale = scaleVector;
            _segmentB.transform.localScale = scaleVector;

            _segmentHeight = Mathf.Max(0.1f, levelConfig.backgroundSprite.bounds.size.y * scale);
            var center = camera.transform.position;
            _segmentA.transform.position = new Vector3(center.x, center.y, 0f);
            _segmentB.transform.position = new Vector3(center.x, center.y + _segmentHeight, 0f);
        }

        public void Clear()
        {
            _active = false;
            if (_root != null)
            {
                _root.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (!_active)
            {
                return;
            }

            var camera = ResolveCamera();
            if (camera == null)
            {
                return;
            }
            var delta = _scrollSpeed * Time.deltaTime;
            _segmentA.transform.position += Vector3.down * delta;
            _segmentB.transform.position += Vector3.down * delta;

            var recycleThreshold = camera.transform.position.y - _segmentHeight;
            if (_segmentA.transform.position.y <= recycleThreshold)
            {
                _segmentA.transform.position += Vector3.up * (_segmentHeight * 2f);
            }

            if (_segmentB.transform.position.y <= recycleThreshold)
            {
                _segmentB.transform.position += Vector3.up * (_segmentHeight * 2f);
            }

            var centerX = camera.transform.position.x;
            var positionA = _segmentA.transform.position;
            var positionB = _segmentB.transform.position;
            _segmentA.transform.position = new Vector3(centerX, positionA.y, positionA.z);
            _segmentB.transform.position = new Vector3(centerX, positionB.y, positionB.z);
        }

        private Camera ResolveCamera()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }

            return _mainCamera;
        }

        private void EnsureRenderers()
        {
            if (_root == null)
            {
                var rootObject = new GameObject("LevelBackgroundRoot");
                rootObject.transform.SetParent(transform, false);
                _root = rootObject.transform;
            }

            if (_segmentA == null)
            {
                _segmentA = CreateSegment("SegmentA");
            }

            if (_segmentB == null)
            {
                _segmentB = CreateSegment("SegmentB");
            }
        }

        private SpriteRenderer CreateSegment(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_root, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = BackgroundSortingOrder;
            return renderer;
        }
    }
}
