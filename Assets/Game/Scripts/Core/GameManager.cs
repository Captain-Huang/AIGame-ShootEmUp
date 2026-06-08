using AIGame.ShootEmUp.Bullets;
using AIGame.ShootEmUp.Bosses;
using AIGame.ShootEmUp.Configs;
using AIGame.ShootEmUp.Enemies;
using AIGame.ShootEmUp.Levels;
using AIGame.ShootEmUp.Player;
using AIGame.ShootEmUp.UI;
using AIGame.ShootEmUp.Utilities;
using UnityEngine;

namespace AIGame.ShootEmUp.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.Boot;

        private EnemySpawner _enemySpawner;
        private LevelManager _levelManager;
        private HudView _hudView;
        private GameConfigProvider _configProvider;
        private int _score;

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
            _configProvider = GameConfigProvider.Load();

            _hudView = HudView.CreateOrFind();
            _enemySpawner = EnemySpawner.CreateOrFind();
            _levelManager = LevelManager.CreateOrFind();
            _levelManager.Initialize(_configProvider, _enemySpawner);

            GameEvents.EnemyKilled += OnEnemyKilled;
            GameEvents.PlayerDied += OnPlayerDied;
            GameEvents.LevelCleared += OnLevelCleared;
            GameEvents.RunCompleted += OnRunCompleted;

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
                        StartRun();
                    }

                    break;
                case GameState.Playing:
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        SetState(GameState.Paused);
                    }

                    break;
                case GameState.Paused:
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        SetState(GameState.Playing);
                    }

                    break;
                case GameState.GameOver:
                    if (Input.GetKeyDown(KeyCode.R))
                    {
                        StartRun();
                    }

                    break;
                case GameState.LevelCleared:
                    if (Input.GetKeyDown(KeyCode.Return))
                    {
                        StartNextLevel();
                    }

                    break;
                case GameState.GameCompleted:
                    if (Input.GetKeyDown(KeyCode.Return))
                    {
                        StartRun();
                    }

                    break;
            }
        }

        private void StartRun()
        {
            ClearRuntimeActors(includePlayer: true);
            CreatePlayer();

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

        private void CreatePlayer()
        {
            var bounds = CameraBounds.GetWorldBounds(Camera.main, 0.8f);
            var playerConfig = _configProvider?.PlayerConfig;
            if (playerConfig == null)
            {
                Debug.LogError("PlayerConfig is missing, cannot create player.");
                return;
            }

            var player = RuntimeFactory.CreateActor(
                "Player",
                new Color(0.25f, 0.82f, 1f, 1f),
                new Vector2(0.75f, 0.75f),
                20,
                "Player");

            player.transform.position = new Vector3(0f, bounds.MinY + 1.25f, 0f);
            player.AddComponent<PlayerController>().Configure(playerConfig.moveSpeed);
            player.AddComponent<PlayerHealth>().Configure(
                maxHp: playerConfig.maxHealth,
                initialHp: playerConfig.initialHealth,
                invincibleSeconds: playerConfig.invincibleDuration);
            player.AddComponent<PlayerWeapon>().Configure(playerConfig.weaponConfig, autoShoot: true);
        }

        private void OnEnemyKilled(int scoreValue)
        {
            if (CurrentState != GameState.Playing)
            {
                return;
            }

            _score += scoreValue;
            GameEvents.RaiseScoreChanged(_score);
        }

        private void OnPlayerDied()
        {
            _levelManager?.StopRun();
            SetState(GameState.GameOver);
        }

        private void OnLevelCleared(int _)
        {
            if (_levelManager != null && _levelManager.HasNextLevel)
            {
                SetState(GameState.LevelCleared);
            }
        }

        private void OnRunCompleted()
        {
            SetState(GameState.GameCompleted);
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
        }
    }
}
