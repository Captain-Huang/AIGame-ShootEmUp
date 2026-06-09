using AIGame.ShootEmUp.Audio;
using AIGame.ShootEmUp.Bosses;
using AIGame.ShootEmUp.Bullets;
using AIGame.ShootEmUp.Configs;
using AIGame.ShootEmUp.Enemies;
using AIGame.ShootEmUp.Levels;
using AIGame.ShootEmUp.Pickups;
using AIGame.ShootEmUp.Player;
using AIGame.ShootEmUp.Save;
using AIGame.ShootEmUp.UI;
using AIGame.ShootEmUp.Utilities;
using UnityEngine;

namespace AIGame.ShootEmUp.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.Boot;
        public int CurrentScore => _score;
        public int BestScore => _bestScore;
        public int MaxUnlockedLevel => _progressStore != null ? _progressStore.GetMaxUnlockedLevel() : 1;
        public bool IsFullscreen => Screen.fullScreen;

        private EnemySpawner _enemySpawner;
        private LevelManager _levelManager;
        private HudView _hudView;
        private GameConfigProvider _configProvider;
        private AudioManager _audioManager;
        private ProgressStore _progressStore;
        private int _score;
        private int _bestScore;
        private bool _hasLoggedConfigFallback;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            TimeController.Resume();
        }

        private void Start()
        {
            EnsureMainCamera();
            ConfigureCollisionMatrixRuntime();

            _progressStore = new ProgressStore();
            Screen.fullScreen = _progressStore.IsFullscreen();

            _configProvider = GameConfigProvider.Load();
            LogConfigFallbackWarning();
            _audioManager = AudioManager.CreateOrFind();

            _hudView = HudView.CreateOrFind();
            _enemySpawner = EnemySpawner.CreateOrFind();
            _levelManager = LevelManager.CreateOrFind();
            _levelManager.Initialize(_configProvider, _enemySpawner);

            GameEvents.EnemyKilled += OnEnemyKilled;
            GameEvents.PlayerDied += OnPlayerDied;
            GameEvents.LevelCleared += OnLevelCleared;
            GameEvents.LevelStarted += OnLevelStarted;
            GameEvents.BossSpawned += OnBossSpawned;
            GameEvents.RunCompleted += OnRunCompleted;

            _bestScore = _progressStore.GetBestScore();
            GameEvents.RaiseScoreChanged(0);
            GameEvents.RaiseBestScoreChanged(_bestScore);
            GameEvents.RaiseMaxUnlockedLevelChanged(MaxUnlockedLevel);

            _audioManager.StopBgm();
            SetState(GameState.MainMenu);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            GameEvents.EnemyKilled -= OnEnemyKilled;
            GameEvents.PlayerDied -= OnPlayerDied;
            GameEvents.LevelCleared -= OnLevelCleared;
            GameEvents.LevelStarted -= OnLevelStarted;
            GameEvents.BossSpawned -= OnBossSpawned;
            GameEvents.RunCompleted -= OnRunCompleted;
            TimeController.Resume();
        }

        private void Update()
        {
            switch (CurrentState)
            {
                case GameState.MainMenu:
                    if (Input.GetKeyDown(KeyCode.Return))
                    {
                        StartRunFromMenu();
                    }

                    break;
                case GameState.Playing:
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        PauseGame();
                    }

                    break;
                case GameState.Paused:
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        ResumeGame();
                    }

                    break;
                case GameState.GameOver:
                    if (Input.GetKeyDown(KeyCode.R))
                    {
                        RestartRun();
                    }

                    break;
                case GameState.LevelCleared:
                    if (Input.GetKeyDown(KeyCode.Return))
                    {
                        ContinueToNextLevel();
                    }

                    break;
                case GameState.GameCompleted:
                    if (Input.GetKeyDown(KeyCode.Return))
                    {
                        StartRunFromMenu();
                    }

                    break;
            }
        }

        public void StartRunFromMenu()
        {
            if (CurrentState == GameState.Playing || CurrentState == GameState.Paused)
            {
                return;
            }

            StartRun();
        }

        public void ContinueToNextLevel()
        {
            if (CurrentState != GameState.LevelCleared)
            {
                return;
            }

            StartNextLevel();
        }

        public void RestartRun()
        {
            StartRun();
        }

        public void PauseGame()
        {
            if (CurrentState != GameState.Playing)
            {
                return;
            }

            SetState(GameState.Paused);
        }

        public void ResumeGame()
        {
            if (CurrentState != GameState.Paused)
            {
                return;
            }

            SetState(GameState.Playing);
        }

        public void ReturnToMainMenu()
        {
            if (CurrentState == GameState.MainMenu)
            {
                return;
            }

            CommitRunResult();
            _levelManager?.StopRun();
            ClearRuntimeActors(includePlayer: true);
            _audioManager?.StopBgm();
            SetState(GameState.MainMenu);
        }

        public void ToggleFullscreenSetting()
        {
            var next = !Screen.fullScreen;
            Screen.fullScreen = next;
            _progressStore?.SetFullscreen(next);
        }

        public void AddScore(int scoreValue)
        {
            if (scoreValue <= 0)
            {
                return;
            }

            _score += scoreValue;
            GameEvents.RaiseScoreChanged(_score);
        }

        private void StartRun()
        {
            if (_configProvider == null)
            {
                Debug.LogError("GameConfigProvider is missing, cannot start run.");
                return;
            }

            if (_configProvider.HasFallbackData)
            {
                LogConfigFallbackWarning();
                return;
            }

            ClearRuntimeActors(includePlayer: true);
            if (!CreatePlayer())
            {
                SetState(GameState.MainMenu);
                return;
            }

            _score = 0;
            GameEvents.RaiseScoreChanged(_score);

            var started = _levelManager != null && _levelManager.StartRun();
            SetState(started ? GameState.Playing : GameState.GameCompleted);
        }

        private void StartNextLevel()
        {
            if (_levelManager == null)
            {
                return;
            }

            ClearRuntimeActors(includePlayer: false);
            var started = _levelManager.StartNextLevel();
            SetState(started ? GameState.Playing : GameState.GameCompleted);
        }

        private bool CreatePlayer()
        {
            var playerConfig = _configProvider?.PlayerConfig;
            if (playerConfig == null)
            {
                Debug.LogError("PlayerConfig is missing, cannot create player.");
                return false;
            }

            if (playerConfig.prefab == null)
            {
                Debug.LogError("PlayerConfig.prefab is missing, cannot create player.");
                return false;
            }

            var player = Instantiate(playerConfig.prefab);
            if (player == null)
            {
                Debug.LogError("Failed to instantiate Player prefab.");
                return false;
            }

            var bounds = CameraBounds.GetWorldBounds(Camera.main, 0.8f);
            player.name = "Player";

            player.transform.position = new Vector3(0f, bounds.MinY + 1.25f, 0f);
            if (!TryGetRequiredComponent(player, "Player", out PlayerController playerController) ||
                !TryGetRequiredComponent(player, "Player", out PlayerHealth playerHealth) ||
                !TryGetRequiredComponent(player, "Player", out PlayerWeapon playerWeapon) ||
                !TryGetRequiredComponent(player, "Player", out PlayerPickupCollector _))
            {
                Destroy(player);
                return false;
            }

            playerController.Configure(playerConfig.moveSpeed);
            playerHealth.Configure(
                maxHp: playerConfig.maxHealth,
                initialHp: playerConfig.initialHealth,
                invincibleSeconds: playerConfig.invincibleDuration);

            playerWeapon.Configure(
                playerConfig.weaponConfig,
                autoShoot: true,
                initialBombs: playerConfig.initialBombs,
                maxBombs: playerConfig.maxBombs,
                maxPowerLevel: playerConfig.maxPowerLevel);
            return true;
        }

        private void OnEnemyKilled(int scoreValue)
        {
            if (CurrentState != GameState.Playing)
            {
                return;
            }

            AddScore(scoreValue);
        }

        private void OnPlayerDied()
        {
            _levelManager?.StopRun();
            CommitRunResult();
            SetState(GameState.GameOver);
        }

        private void OnLevelCleared(int _)
        {
            GameEvents.RaiseMaxUnlockedLevelChanged(MaxUnlockedLevel);
            if (_levelManager != null && _levelManager.HasNextLevel)
            {
                SetState(GameState.LevelCleared);
            }
        }

        private void OnRunCompleted()
        {
            CommitRunResult();
            SetState(GameState.GameCompleted);
        }

        private void OnLevelStarted(int _, string __)
        {
            if (_audioManager == null)
            {
                return;
            }

            var level = _levelManager != null ? _levelManager.CurrentLevelConfig : null;
            _audioManager.PlayBgm(level != null ? level.bgm : null);
        }

        private void OnBossSpawned(string _, int __)
        {
            if (_audioManager == null)
            {
                return;
            }

            var level = _levelManager != null ? _levelManager.CurrentLevelConfig : null;
            if (level != null && level.bossBgm != null)
            {
                _audioManager.PlayBgm(level.bossBgm);
            }
        }

        private void CommitRunResult()
        {
            if (_progressStore == null)
            {
                return;
            }

            if (_progressStore.TrySetBestScore(_score))
            {
                _bestScore = _score;
                GameEvents.RaiseBestScoreChanged(_bestScore);
                return;
            }

            _bestScore = _progressStore.GetBestScore();
        }

        private void SetState(GameState nextState)
        {
            if (CurrentState == nextState)
            {
                return;
            }

            CurrentState = nextState;
            if (CurrentState == GameState.Paused)
            {
                TimeController.Pause();
            }
            else
            {
                TimeController.Resume();
            }

            GameEvents.RaiseStateChanged(CurrentState);
        }

        private static void EnsureMainCamera()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                var cameraGo = new GameObject("Main Camera");
                cameraGo.tag = "MainCamera";
                camera = cameraGo.AddComponent<Camera>();
                cameraGo.AddComponent<AudioListener>();
            }

            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.backgroundColor = Color.black;
        }

        private static void ConfigureCollisionMatrixRuntime()
        {
            var gameplayLayers = new[]
            {
                "Player",
                "Enemy",
                "PlayerBullet",
                "EnemyBullet",
                "Pickup",
                "Boundary"
            };

            for (var i = 0; i < gameplayLayers.Length; i++)
            {
                for (var j = i; j < gameplayLayers.Length; j++)
                {
                    SetLayerCollision(gameplayLayers[i], gameplayLayers[j], shouldCollide: false);
                }
            }

            SetLayerCollision("Player", "Enemy", shouldCollide: true);
            SetLayerCollision("Player", "EnemyBullet", shouldCollide: true);
            SetLayerCollision("Player", "Pickup", shouldCollide: true);
            SetLayerCollision("Enemy", "PlayerBullet", shouldCollide: true);
            SetLayerCollision("Boundary", "Enemy", shouldCollide: true);
            SetLayerCollision("Boundary", "PlayerBullet", shouldCollide: true);
            SetLayerCollision("Boundary", "EnemyBullet", shouldCollide: true);
            SetLayerCollision("Boundary", "Pickup", shouldCollide: true);
        }

        private static void SetLayerCollision(string layerNameA, string layerNameB, bool shouldCollide)
        {
            var layerA = LayerMask.NameToLayer(layerNameA);
            var layerB = LayerMask.NameToLayer(layerNameB);
            if (layerA < 0 || layerB < 0)
            {
                return;
            }

            Physics2D.IgnoreLayerCollision(layerA, layerB, !shouldCollide);
        }

        private static void ClearRuntimeActors(bool includePlayer)
        {
            if (includePlayer)
            {
                foreach (var player in FindObjectsOfType<PlayerHealth>())
                {
                    Destroy(player.gameObject);
                }
            }

            foreach (var enemy in FindObjectsOfType<EnemyBase>())
            {
                Destroy(enemy.gameObject);
            }

            foreach (var boss in FindObjectsOfType<BossController>())
            {
                Destroy(boss.gameObject);
            }

            foreach (var bullet in FindObjectsOfType<Bullet>())
            {
                Destroy(bullet.gameObject);
            }

            foreach (var pickup in FindObjectsOfType<Pickup>())
            {
                Destroy(pickup.gameObject);
            }
        }

        private void LogConfigFallbackWarning()
        {
            if (_hasLoggedConfigFallback || _configProvider == null || !_configProvider.HasFallbackData)
            {
                return;
            }

            _hasLoggedConfigFallback = true;
            Debug.LogError(
                "[GameManager] Planned config assets are incomplete; gameplay is blocked in strict config mode. " +
                "Run Tools/ShootEmUp/Generate Planned Configs And Prefabs.\n" +
                _configProvider.MissingConfigSummary);
        }

        private static bool TryGetRequiredComponent<T>(GameObject gameObject, string context, out T component) where T : Component
        {
            component = gameObject.GetComponent<T>();
            if (component != null)
            {
                return true;
            }

            Debug.LogError($"{context} prefab is missing required component {typeof(T).Name}.");
            return false;
        }
    }
}
