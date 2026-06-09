using AIGame.ShootEmUp.Audio;
using AIGame.ShootEmUp.Bosses;
using AIGame.ShootEmUp.Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AIGame.ShootEmUp.UI
{
    public class HudView : MonoBehaviour
    {
        private const string HudPrefabAssetPath = "Assets/Game/Prefabs/UI/HudView.prefab";

        [Header("Top HUD")]
        [SerializeField] private Text _scoreText;
        [SerializeField] private Text _healthText;
        [SerializeField] private Text _powerText;
        [SerializeField] private Text _bombText;
        [SerializeField] private Text _shieldText;
        [SerializeField] private Text _levelText;
        [SerializeField] private BossHealthBarBinder _bossBarBinder;

        [Header("Panels")]
        [SerializeField] private GameObject _mainMenuPanel;
        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private GameObject _resultPanel;
        [SerializeField] private GameObject _failurePanel;
        [SerializeField] private GameObject _settingsPanel;

        [Header("Menu Text/Button Refs")]
        [SerializeField] private Text _bestScoreText;
        [SerializeField] private Text _unlockedLevelText;
        [SerializeField] private Text _resultTitleText;

        [Header("Main Menu Buttons")]
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _mainMenuSettingsButton;

        [Header("Pause Buttons")]
        [SerializeField] private Button _pauseResumeButton;
        [SerializeField] private Button _pauseSettingsButton;
        [SerializeField] private Button _pauseMainMenuButton;

        [Header("Result Buttons")]
        [SerializeField] private Button _nextLevelButton;
        [SerializeField] private Button _restartRunButton;
        [SerializeField] private Button _resultMainMenuButton;

        [Header("Failure Buttons")]
        [SerializeField] private Button _failureRetryButton;
        [SerializeField] private Button _failureMainMenuButton;

        [Header("Settings Buttons")]
        [SerializeField] private Button _settingsCloseButton;
        [SerializeField] private Button _masterMinusButton;
        [SerializeField] private Button _masterPlusButton;
        [SerializeField] private Button _bgmMinusButton;
        [SerializeField] private Button _bgmPlusButton;
        [SerializeField] private Button _sfxMinusButton;
        [SerializeField] private Button _sfxPlusButton;
        [SerializeField] private Button _fullscreenToggleButton;

        [Header("Settings Refs")]
        [SerializeField] private Text _masterVolumeText;
        [SerializeField] private Text _bgmVolumeText;
        [SerializeField] private Text _sfxVolumeText;
        [SerializeField] private Text _fullscreenText;

        [Header("Skin Sprites")]
        [SerializeField] private Sprite _panelSprite;
        [SerializeField] private Sprite _buttonNormalSprite;
        [SerializeField] private Sprite _buttonHoverSprite;
        [SerializeField] private Sprite _buttonPressedSprite;
        [SerializeField] private Sprite _bossBarFrameSprite;
        [SerializeField] private Sprite _bossBarFillSprite;

        private int _bestScore;
        private int _maxUnlockedLevel = 1;
        private bool _settingsVisible;

        public static HudView CreateOrFind()
        {
            var existing = FindObjectOfType<HudView>();
            if (existing != null)
            {
                return existing;
            }

            var prefab = LoadHudPrefab();
            if (prefab != null)
            {
                var instance = Instantiate(prefab);
                instance.name = "Hud";
                var hud = instance.GetComponent<HudView>();
                if (hud != null)
                {
                    return hud;
                }

                Destroy(instance);
                Debug.LogError($"HUD prefab at {HudPrefabAssetPath} has no HudView component.");
            }

            var go = new GameObject("Hud");
            return go.AddComponent<HudView>();
        }

        private static GameObject LoadHudPrefab()
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<GameObject>(HudPrefabAssetPath);
#else
            return null;
#endif
        }

        private void Awake()
        {
            EnsureCanvasComponents();
            BuildLayoutIfMissing();
            EnsureButtonReferences();
            BindButtonCallbacks();

            var manager = GameManager.Instance;
            _bestScore = manager != null ? manager.BestScore : 0;
            _maxUnlockedLevel = manager != null ? manager.MaxUnlockedLevel : 1;
            RefreshMainMenuStats();
            RefreshSettingsTexts();
            SetSettingsVisible(false);
            ShowStatePanels(mainMenu: false, pause: false, result: false, failure: false);
        }

        public void SetSkinSpritesForEditor(
            Sprite panel,
            Sprite buttonNormal,
            Sprite buttonHover,
            Sprite buttonPressed,
            Sprite bossFrame,
            Sprite bossFill)
        {
            _panelSprite = panel;
            _buttonNormalSprite = buttonNormal;
            _buttonHoverSprite = buttonHover;
            _buttonPressedSprite = buttonPressed;
            _bossBarFrameSprite = bossFrame;
            _bossBarFillSprite = bossFill;
        }

        public void RebuildLayoutForEditor()
        {
            ClearLayoutObjects();
            ResetReferences();
            EnsureCanvasComponents();
            BuildLayoutIfMissing();
        }

        private void EnsureCanvasComponents()
        {
            var canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = gameObject.AddComponent<CanvasScaler>();
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);

            if (GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        private void BuildLayoutIfMissing()
        {
            if (HasLayoutReferences())
            {
                return;
            }

            CreateTopHud();
            _bossBarBinder = CreateBossBarBinder();
            CreateMainMenuPanel();
            CreatePausePanel();
            CreateResultPanel();
            CreateFailurePanel();
            // CreateSettingsPanel();
        }

        private bool HasLayoutReferences()
        {
            return _scoreText != null &&
                   _healthText != null &&
                   _powerText != null &&
                   _bombText != null &&
                   _shieldText != null &&
                   _levelText != null &&
                   _mainMenuPanel != null &&
                   _pausePanel != null &&
                   _resultPanel != null &&
                   _failurePanel != null &&
                   // _settingsPanel != null &&
                   _bestScoreText != null &&
                   _unlockedLevelText != null &&
                   _resultTitleText != null &&
                   _nextLevelButton != null &&
                   _restartRunButton != null &&
                   _masterVolumeText != null &&
                   _bgmVolumeText != null &&
                   _sfxVolumeText != null &&
                   _fullscreenText != null &&
                   _bossBarBinder != null;
        }

        private void ResetReferences()
        {
            _scoreText = null;
            _healthText = null;
            _powerText = null;
            _bombText = null;
            _shieldText = null;
            _levelText = null;
            _bossBarBinder = null;
            _mainMenuPanel = null;
            _pausePanel = null;
            _resultPanel = null;
            _failurePanel = null;
            // _settingsPanel = null;
            _bestScoreText = null;
            _unlockedLevelText = null;
            _resultTitleText = null;
            _startButton = null;
            _mainMenuSettingsButton = null;
            _pauseResumeButton = null;
            _pauseSettingsButton = null;
            _pauseMainMenuButton = null;
            _nextLevelButton = null;
            _restartRunButton = null;
            _resultMainMenuButton = null;
            _failureRetryButton = null;
            _failureMainMenuButton = null;
            _settingsCloseButton = null;
            _masterMinusButton = null;
            _masterPlusButton = null;
            _bgmMinusButton = null;
            _bgmPlusButton = null;
            _sfxMinusButton = null;
            _sfxPlusButton = null;
            _fullscreenToggleButton = null;
            _masterVolumeText = null;
            _bgmVolumeText = null;
            _sfxVolumeText = null;
            _fullscreenText = null;
        }

        private void ClearLayoutObjects()
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i).gameObject;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    DestroyImmediate(child);
                }
                else
                {
                    Destroy(child);
                }
#else
                Destroy(child);
#endif
            }
        }

        private void OnEnable()
        {
            GameEvents.ScoreChanged += OnScoreChanged;
            GameEvents.BestScoreChanged += OnBestScoreChanged;
            GameEvents.MaxUnlockedLevelChanged += OnMaxUnlockedLevelChanged;
            GameEvents.PlayerHealthChanged += OnPlayerHealthChanged;
            GameEvents.PlayerPowerChanged += OnPlayerPowerChanged;
            GameEvents.PlayerBombChanged += OnPlayerBombChanged;
            GameEvents.PlayerShieldChanged += OnPlayerShieldChanged;
            GameEvents.LevelStarted += OnLevelStarted;
            GameEvents.StateChanged += OnStateChanged;
        }

        private void OnDisable()
        {
            GameEvents.ScoreChanged -= OnScoreChanged;
            GameEvents.BestScoreChanged -= OnBestScoreChanged;
            GameEvents.MaxUnlockedLevelChanged -= OnMaxUnlockedLevelChanged;
            GameEvents.PlayerHealthChanged -= OnPlayerHealthChanged;
            GameEvents.PlayerPowerChanged -= OnPlayerPowerChanged;
            GameEvents.PlayerBombChanged -= OnPlayerBombChanged;
            GameEvents.PlayerShieldChanged -= OnPlayerShieldChanged;
            GameEvents.LevelStarted -= OnLevelStarted;
            GameEvents.StateChanged -= OnStateChanged;
        }

        private void OnScoreChanged(int score)
        {
            if (_scoreText != null)
            {
                _scoreText.text = $"Score: {score}";
            }
        }

        private void OnBestScoreChanged(int score)
        {
            _bestScore = score;
            RefreshMainMenuStats();
        }

        private void OnMaxUnlockedLevelChanged(int level)
        {
            _maxUnlockedLevel = Mathf.Max(1, level);
            RefreshMainMenuStats();
        }

        private void OnPlayerHealthChanged(int currentHealth, int maxHealth)
        {
            if (_healthText != null)
            {
                _healthText.text = $"HP: {currentHealth}/{maxHealth}";
            }
        }

        private void OnPlayerPowerChanged(int currentPower, int maxPower)
        {
            if (_powerText != null)
            {
                _powerText.text = $"Power: {currentPower}/{maxPower}";
            }
        }

        private void OnPlayerBombChanged(int currentBombs, int maxBombs)
        {
            if (_bombText != null)
            {
                _bombText.text = $"Bomb: {currentBombs}/{maxBombs}";
            }
        }

        private void OnPlayerShieldChanged(bool active, float _)
        {
            if (_shieldText != null)
            {
                _shieldText.text = active ? "Shield: On" : "Shield: Off";
            }
        }

        private void OnLevelStarted(int levelId, string displayName)
        {
            if (_levelText != null)
            {
                _levelText.text = string.IsNullOrWhiteSpace(displayName) ? $"Level {levelId}" : displayName;
            }
        }

        private void OnStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.MainMenu:
                    ShowStatePanels(mainMenu: true, pause: false, result: false, failure: false);
                    break;
                case GameState.Playing:
                    ShowStatePanels(mainMenu: false, pause: false, result: false, failure: false);
                    break;
                case GameState.Paused:
                    ShowStatePanels(mainMenu: false, pause: true, result: false, failure: false);
                    break;
                case GameState.LevelCleared:
                    ShowStatePanels(mainMenu: false, pause: false, result: true, failure: false);
                    if (_resultTitleText != null)
                    {
                        _resultTitleText.text = "Level Cleared";
                    }

                    SetButtonVisible(_nextLevelButton, true);
                    SetButtonVisible(_restartRunButton, false);
                    break;
                case GameState.GameOver:
                    ShowStatePanels(mainMenu: false, pause: false, result: false, failure: true);
                    break;
                case GameState.GameCompleted:
                    ShowStatePanels(mainMenu: false, pause: false, result: true, failure: false);
                    if (_resultTitleText != null)
                    {
                        _resultTitleText.text = "Run Completed";
                    }

                    SetButtonVisible(_nextLevelButton, false);
                    SetButtonVisible(_restartRunButton, true);
                    break;
            }
        }

        [ContextMenu("Auto Bind HUD Button Refs")]
        public void AutoBindReferencesByName()
        {
            EnsureButtonReferences();
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(this);
            }
#endif
        }

        private void EnsureButtonReferences()
        {
            if (_mainMenuPanel != null)
            {
                if (_startButton == null)
                {
                    _startButton = FindButtonByName(_mainMenuPanel.transform, "StartButton");
                }

                if (_mainMenuSettingsButton == null)
                {
                    _mainMenuSettingsButton = FindButtonByName(_mainMenuPanel.transform, "SettingsButton");
                }
            }

            if (_pausePanel != null)
            {
                if (_pauseResumeButton == null)
                {
                    _pauseResumeButton = FindButtonByName(_pausePanel.transform, "ResumeButton");
                }

                if (_pauseSettingsButton == null)
                {
                    _pauseSettingsButton = FindButtonByName(_pausePanel.transform, "SettingsButton");
                }

                if (_pauseMainMenuButton == null)
                {
                    _pauseMainMenuButton = FindButtonByName(_pausePanel.transform, "Main MenuButton");
                }
            }

            if (_resultPanel != null)
            {
                if (_nextLevelButton == null)
                {
                    _nextLevelButton = FindButtonByName(_resultPanel.transform, "Next LevelButton");
                }

                if (_restartRunButton == null)
                {
                    _restartRunButton = FindButtonByName(_resultPanel.transform, "RestartButton");
                }

                if (_resultMainMenuButton == null)
                {
                    _resultMainMenuButton = FindButtonByName(_resultPanel.transform, "Main MenuButton");
                }
            }

            if (_failurePanel != null)
            {
                if (_failureRetryButton == null)
                {
                    _failureRetryButton = FindButtonByName(_failurePanel.transform, "RetryButton");
                }

                if (_failureMainMenuButton == null)
                {
                    _failureMainMenuButton = FindButtonByName(_failurePanel.transform, "Main MenuButton");
                }
            }

            if (_settingsPanel != null)
            {
                if (_settingsCloseButton == null)
                {
                    _settingsCloseButton = FindButtonByName(_settingsPanel.transform, "CloseButton");
                }

                if (_fullscreenToggleButton == null)
                {
                    _fullscreenToggleButton = FindButtonByName(_settingsPanel.transform, "ToggleButton");
                }

                AutoAssignSettingsStepperButtons();
            }
        }

        private void AutoAssignSettingsStepperButtons()
        {
            if (_settingsPanel == null)
            {
                return;
            }

            if (_masterMinusButton != null && _masterPlusButton != null &&
                _bgmMinusButton != null && _bgmPlusButton != null &&
                _sfxMinusButton != null && _sfxPlusButton != null)
            {
                return;
            }

            var minusButtons = GetNamedButtons(_settingsPanel.transform, "-Button");
            var plusButtons = GetNamedButtons(_settingsPanel.transform, "+Button");
            minusButtons.Sort((a, b) => b.GetComponent<RectTransform>().anchoredPosition.y.CompareTo(a.GetComponent<RectTransform>().anchoredPosition.y));
            plusButtons.Sort((a, b) => b.GetComponent<RectTransform>().anchoredPosition.y.CompareTo(a.GetComponent<RectTransform>().anchoredPosition.y));

            if (_masterMinusButton == null && minusButtons.Count > 0)
            {
                _masterMinusButton = minusButtons[0];
            }

            if (_bgmMinusButton == null && minusButtons.Count > 1)
            {
                _bgmMinusButton = minusButtons[1];
            }

            if (_sfxMinusButton == null && minusButtons.Count > 2)
            {
                _sfxMinusButton = minusButtons[2];
            }

            if (_masterPlusButton == null && plusButtons.Count > 0)
            {
                _masterPlusButton = plusButtons[0];
            }

            if (_bgmPlusButton == null && plusButtons.Count > 1)
            {
                _bgmPlusButton = plusButtons[1];
            }

            if (_sfxPlusButton == null && plusButtons.Count > 2)
            {
                _sfxPlusButton = plusButtons[2];
            }
        }

        private void BindButtonCallbacks()
        {
            BindButton(_startButton, () => GameManager.Instance?.StartRunFromMenu());
            // BindButton(_mainMenuSettingsButton, () => SetSettingsVisible(true));

            BindButton(_pauseResumeButton, () => GameManager.Instance?.ResumeGame());
            // BindButton(_pauseSettingsButton, () => SetSettingsVisible(true));
            BindButton(_pauseMainMenuButton, () => GameManager.Instance?.ReturnToMainMenu());

            BindButton(_nextLevelButton, () => GameManager.Instance?.ContinueToNextLevel());
            BindButton(_restartRunButton, () => GameManager.Instance?.RestartRun());
            BindButton(_resultMainMenuButton, () => GameManager.Instance?.ReturnToMainMenu());

            BindButton(_failureRetryButton, () => GameManager.Instance?.RestartRun());
            BindButton(_failureMainMenuButton, () => GameManager.Instance?.ReturnToMainMenu());

            BindButton(_settingsCloseButton, () => SetSettingsVisible(false));
            BindButton(_masterMinusButton, () => AdjustMasterVolume(-0.1f));
            BindButton(_masterPlusButton, () => AdjustMasterVolume(0.1f));
            BindButton(_bgmMinusButton, () => AdjustBgmVolume(-0.1f));
            BindButton(_bgmPlusButton, () => AdjustBgmVolume(0.1f));
            BindButton(_sfxMinusButton, () => AdjustSfxVolume(-0.1f));
            BindButton(_sfxPlusButton, () => AdjustSfxVolume(0.1f));
            BindButton(_fullscreenToggleButton, ToggleFullscreen);
        }

        private static void BindButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        private static Button FindButtonByName(Transform root, string buttonName)
        {
            if (root == null)
            {
                return null;
            }

            foreach (var button in root.GetComponentsInChildren<Button>(true))
            {
                if (button != null && button.name == buttonName)
                {
                    return button;
                }
            }

            return null;
        }

        private static List<Button> GetNamedButtons(Transform root, string buttonName)
        {
            var result = new List<Button>();
            if (root == null)
            {
                return result;
            }

            foreach (var button in root.GetComponentsInChildren<Button>(true))
            {
                if (button != null && button.name == buttonName)
                {
                    result.Add(button);
                }
            }

            return result;
        }

        private void CreateTopHud()
        {
            _scoreText = CreateText("ScoreText", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -20f), TextAnchor.UpperLeft, 32);
            _powerText = CreateText("PowerText", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -62f), TextAnchor.UpperLeft, 28);
            _bombText = CreateText("BombText", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -98f), TextAnchor.UpperLeft, 28);

            _healthText = CreateText("HealthText", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20f, -20f), TextAnchor.UpperRight, 32);
            _shieldText = CreateText("ShieldText", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20f, -62f), TextAnchor.UpperRight, 26);
            _levelText = CreateText("LevelText", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -20f), TextAnchor.UpperCenter, 30);

            _scoreText.text = "Score: 0";
            _powerText.text = "Power: 1";
            _bombText.text = "Bomb: 0";
            _healthText.text = "HP: 3/3";
            _shieldText.text = "Shield: Off";
            _levelText.text = "Level 1";
        }

        private void CreateMainMenuPanel()
        {
            _mainMenuPanel = CreatePanel("MainMenuPanel", new Vector2(560f, 460f), new Color(0f, 0f, 0f, 0.7f));

            CreateText("MainTitle", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -36f), TextAnchor.UpperCenter, 54, _mainMenuPanel.transform).text = "Shoot 'Em Up";
            _bestScoreText = CreateText("BestScoreText", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -120f), TextAnchor.UpperCenter, 28, _mainMenuPanel.transform);
            _unlockedLevelText = CreateText("UnlockedLevelText", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -160f), TextAnchor.UpperCenter, 28, _mainMenuPanel.transform);

            _startButton = CreateButton(_mainMenuPanel.transform, "Start", new Vector2(0f, -245f), new Vector2(260f, 54f), () => GameManager.Instance?.StartRunFromMenu());
            // _mainMenuSettingsButton = CreateButton(_mainMenuPanel.transform, "Settings", new Vector2(0f, -315f), new Vector2(260f, 54f), () => SetSettingsVisible(true));
        }

        private void CreatePausePanel()
        {
            _pausePanel = CreatePanel("PausePanel", new Vector2(520f, 360f), new Color(0f, 0f, 0f, 0.74f));

            CreateText("PauseTitle", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -34f), TextAnchor.UpperCenter, 52, _pausePanel.transform).text = "Paused";
            _pauseResumeButton = CreateButton(_pausePanel.transform, "Resume", new Vector2(0f, -150f), new Vector2(250f, 52f), () => GameManager.Instance?.ResumeGame());
            _pauseSettingsButton = CreateButton(_pausePanel.transform, "Settings", new Vector2(0f, -214f), new Vector2(250f, 52f), () => SetSettingsVisible(true));
            _pauseMainMenuButton = CreateButton(_pausePanel.transform, "Main Menu", new Vector2(0f, -278f), new Vector2(250f, 52f), () => GameManager.Instance?.ReturnToMainMenu());
        }

        private void CreateResultPanel()
        {
            _resultPanel = CreatePanel("ResultPanel", new Vector2(540f, 380f), new Color(0f, 0f, 0f, 0.74f));

            _resultTitleText = CreateText("ResultTitle", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -34f), TextAnchor.UpperCenter, 50, _resultPanel.transform);
            _resultTitleText.text = "Result";

            _nextLevelButton = CreateButton(_resultPanel.transform, "Next Level", new Vector2(0f, -164f), new Vector2(260f, 52f), () => GameManager.Instance?.ContinueToNextLevel());
            _restartRunButton = CreateButton(_resultPanel.transform, "Restart", new Vector2(0f, -164f), new Vector2(260f, 52f), () => GameManager.Instance?.RestartRun());
            _resultMainMenuButton = CreateButton(_resultPanel.transform, "Main Menu", new Vector2(0f, -228f), new Vector2(260f, 52f), () => GameManager.Instance?.ReturnToMainMenu());
        }

        private void CreateFailurePanel()
        {
            _failurePanel = CreatePanel("FailurePanel", new Vector2(540f, 360f), new Color(0f, 0f, 0f, 0.74f));

            CreateText("FailureTitle", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -34f), TextAnchor.UpperCenter, 50, _failurePanel.transform).text = "Game Over";
            _failureRetryButton = CreateButton(_failurePanel.transform, "Retry", new Vector2(0f, -164f), new Vector2(260f, 52f), () => GameManager.Instance?.RestartRun());
            _failureMainMenuButton = CreateButton(_failurePanel.transform, "Main Menu", new Vector2(0f, -228f), new Vector2(260f, 52f), () => GameManager.Instance?.ReturnToMainMenu());
        }

        private void CreateSettingsPanel()
        {
            _settingsPanel = CreatePanel("SettingsPanel", new Vector2(680f, 470f), new Color(0f, 0f, 0f, 0.82f));
            _settingsPanel.transform.SetAsLastSibling();

            CreateText("SettingsTitle", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), TextAnchor.UpperCenter, 44, _settingsPanel.transform).text = "Settings";
            _settingsCloseButton = CreateButton(_settingsPanel.transform, "Close", new Vector2(262f, 206f), new Vector2(100f, 42f), () => SetSettingsVisible(false));

            _masterVolumeText = CreateStepperRow(_settingsPanel.transform, "Master", 120f, AdjustMasterVolume, out _masterMinusButton, out _masterPlusButton);
            _bgmVolumeText = CreateStepperRow(_settingsPanel.transform, "BGM", 54f, AdjustBgmVolume, out _bgmMinusButton, out _bgmPlusButton);
            _sfxVolumeText = CreateStepperRow(_settingsPanel.transform, "SFX", -12f, AdjustSfxVolume, out _sfxMinusButton, out _sfxPlusButton);

            CreateText("FullscreenLabel", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(70f, -96f), TextAnchor.MiddleLeft, 30, _settingsPanel.transform).text = "Fullscreen";
            _fullscreenText = CreateText("FullscreenValue", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(120f, -96f), TextAnchor.MiddleCenter, 28, _settingsPanel.transform, new Vector2(120f, 44f));
            _fullscreenToggleButton = CreateButton(_settingsPanel.transform, "Toggle", new Vector2(260f, -96f), new Vector2(150f, 46f), ToggleFullscreen);
        }

        private Text CreateStepperRow(Transform parent, string label, float y, System.Action<float> adjustAction, out Button minusButton, out Button plusButton)
        {
            CreateText($"{label}Label", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(70f, y), TextAnchor.MiddleLeft, 30, parent).text = label;
            var valueText = CreateText($"{label}Value", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(120f, y), TextAnchor.MiddleCenter, 28, parent, new Vector2(120f, 44f));
            minusButton = CreateButton(parent, "-", new Vector2(210f, y), new Vector2(50f, 46f), () => adjustAction?.Invoke(-0.1f));
            plusButton = CreateButton(parent, "+", new Vector2(310f, y), new Vector2(50f, 46f), () => adjustAction?.Invoke(0.1f));
            return valueText;
        }

        private void AdjustMasterVolume(float delta)
        {
            var audio = AudioManager.Instance;
            if (audio == null)
            {
                return;
            }

            audio.SetVolumes(audio.MasterVolume + delta, audio.BgmVolume, audio.SfxVolume);
            RefreshSettingsTexts();
        }

        private void AdjustBgmVolume(float delta)
        {
            var audio = AudioManager.Instance;
            if (audio == null)
            {
                return;
            }

            audio.SetVolumes(audio.MasterVolume, audio.BgmVolume + delta, audio.SfxVolume);
            RefreshSettingsTexts();
        }

        private void AdjustSfxVolume(float delta)
        {
            var audio = AudioManager.Instance;
            if (audio == null)
            {
                return;
            }

            audio.SetVolumes(audio.MasterVolume, audio.BgmVolume, audio.SfxVolume + delta);
            RefreshSettingsTexts();
        }

        private void ToggleFullscreen()
        {
            GameManager.Instance?.ToggleFullscreenSetting();
            RefreshSettingsTexts();
        }

        private void RefreshMainMenuStats()
        {
            if (_bestScoreText != null)
            {
                _bestScoreText.text = $"Best Score: {_bestScore}";
            }

            if (_unlockedLevelText != null)
            {
                _unlockedLevelText.text = $"Unlocked Level: {_maxUnlockedLevel}";
            }
        }

        private void RefreshSettingsTexts()
        {
            var audio = AudioManager.Instance;
            if (audio != null)
            {
                if (_masterVolumeText != null)
                {
                    _masterVolumeText.text = $"{Mathf.RoundToInt(audio.MasterVolume * 100f)}%";
                }

                if (_bgmVolumeText != null)
                {
                    _bgmVolumeText.text = $"{Mathf.RoundToInt(audio.BgmVolume * 100f)}%";
                }

                if (_sfxVolumeText != null)
                {
                    _sfxVolumeText.text = $"{Mathf.RoundToInt(audio.SfxVolume * 100f)}%";
                }
            }

            if (_fullscreenText != null)
            {
                _fullscreenText.text = (GameManager.Instance != null && GameManager.Instance.IsFullscreen) ? "On" : "Off";
            }
        }

        private void SetSettingsVisible(bool visible)
        {
            _settingsVisible = visible;
            if (_settingsPanel != null)
            {
                _settingsPanel.SetActive(visible);
            }

            if (visible)
            {
                RefreshSettingsTexts();
            }
        }

        private void ShowStatePanels(bool mainMenu, bool pause, bool result, bool failure)
        {
            if (_mainMenuPanel != null)
            {
                _mainMenuPanel.SetActive(mainMenu);
            }

            if (_pausePanel != null)
            {
                _pausePanel.SetActive(pause);
            }

            if (_resultPanel != null)
            {
                _resultPanel.SetActive(result);
            }

            if (_failurePanel != null)
            {
                _failurePanel.SetActive(failure);
            }

            if (!mainMenu && !pause)
            {
                SetSettingsVisible(false);
            }
        }

        private static void SetButtonVisible(Button button, bool visible)
        {
            if (button != null)
            {
                button.gameObject.SetActive(visible);
            }
        }

        private GameObject CreatePanel(string name, Vector2 size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = size;

            var image = go.AddComponent<Image>();
            image.color = color;
            if (_panelSprite != null)
            {
                image.sprite = _panelSprite;
                image.type = Image.Type.Sliced;
                image.color = Color.white;
            }

            return go;
        }

        private Button CreateButton(Transform parent, string label, Vector2 anchoredPosition, Vector2 size, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject($"{label}Button");
            go.transform.SetParent(parent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            var image = go.AddComponent<Image>();
            var button = go.AddComponent<Button>();

            if (_buttonNormalSprite != null)
            {
                image.sprite = _buttonNormalSprite;
                image.type = Image.Type.Sliced;
                image.color = Color.white;

                button.transition = Selectable.Transition.SpriteSwap;
                var spriteState = button.spriteState;
                spriteState.highlightedSprite = _buttonHoverSprite != null ? _buttonHoverSprite : _buttonNormalSprite;
                spriteState.pressedSprite = _buttonPressedSprite != null ? _buttonPressedSprite : _buttonNormalSprite;
                spriteState.selectedSprite = spriteState.highlightedSprite;
                button.spriteState = spriteState;
            }
            else
            {
                image.color = new Color(0.18f, 0.22f, 0.3f, 0.94f);
                button.transition = Selectable.Transition.ColorTint;
                var colors = button.colors;
                colors.normalColor = image.color;
                colors.highlightedColor = new Color(0.27f, 0.34f, 0.48f, 1f);
                colors.pressedColor = new Color(0.16f, 0.2f, 0.28f, 1f);
                colors.selectedColor = colors.highlightedColor;
                button.colors = colors;
            }

            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }

            CreateText($"{label}Text", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, TextAnchor.MiddleCenter, 26, go.transform, size).text = label;
            return button;
        }

        private Text CreateText(
            string name,
            Vector2 anchor,
            Vector2 pivot,
            Vector2 anchoredPosition,
            TextAnchor alignment,
            int fontSize,
            Transform parent = null,
            Vector2? size = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent == null ? transform : parent, false);

            var text = go.AddComponent<Text>();
            text.font = GetBuiltinFont();
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;

            var rect = text.rectTransform;
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size ?? new Vector2(900f, 220f);
            return text;
        }

        private BossHealthBarBinder CreateBossBarBinder()
        {
            var root = new GameObject("BossBarRoot");
            root.transform.SetParent(transform, false);

            var rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 1f);
            rootRect.anchorMax = new Vector2(0.5f, 1f);
            rootRect.pivot = new Vector2(0.5f, 1f);
            rootRect.anchoredPosition = new Vector2(0f, -86f);
            rootRect.sizeDelta = new Vector2(640f, 72f);

            var background = new GameObject("BossBarBackground");
            background.transform.SetParent(root.transform, false);
            var backgroundRect = background.AddComponent<RectTransform>();
            backgroundRect.anchorMin = new Vector2(0f, 0f);
            backgroundRect.anchorMax = new Vector2(1f, 0f);
            backgroundRect.pivot = new Vector2(0.5f, 0f);
            backgroundRect.anchoredPosition = new Vector2(0f, 8f);
            backgroundRect.sizeDelta = new Vector2(0f, 18f);
            var backgroundImage = background.AddComponent<Image>();
            if (_bossBarFrameSprite != null)
            {
                backgroundImage.sprite = _bossBarFrameSprite;
                backgroundImage.type = Image.Type.Sliced;
                backgroundImage.color = Color.white;
            }
            else
            {
                backgroundImage.color = new Color(0f, 0f, 0f, 0.65f);
            }

            var fill = new GameObject("BossBarFill");
            fill.transform.SetParent(background.transform, false);
            var fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(1f, 1f);
            fillRect.offsetMin = new Vector2(2f, 2f);
            fillRect.offsetMax = new Vector2(-2f, -2f);
            var fillImage = fill.AddComponent<Image>();
            if (_bossBarFillSprite != null)
            {
                fillImage.sprite = _bossBarFillSprite;
                fillImage.color = Color.white;
            }
            else
            {
                fillImage.color = new Color(0.95f, 0.2f, 0.25f, 0.95f);
            }

            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImage.fillAmount = 1f;

            var nameText = CreateText(
                "BossNameText",
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(4f, 0f),
                TextAnchor.UpperLeft,
                22,
                root.transform,
                new Vector2(420f, 38f));
            nameText.text = "Boss";

            var phaseText = CreateText(
                "BossPhaseText",
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-4f, 0f),
                TextAnchor.UpperRight,
                20,
                root.transform,
                new Vector2(220f, 38f));
            phaseText.text = string.Empty;

            var binder = GetComponent<BossHealthBarBinder>();
            if (binder == null)
            {
                binder = gameObject.AddComponent<BossHealthBarBinder>();
            }

            binder.Setup(root, fillImage, nameText, phaseText);
            return binder;
        }

        private static Font GetBuiltinFont()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font != null)
            {
                return font;
            }

            return Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
    }
}
