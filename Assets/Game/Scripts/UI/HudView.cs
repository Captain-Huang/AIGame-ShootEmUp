using AIGame.ShootEmUp.Bosses;
using AIGame.ShootEmUp.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AIGame.ShootEmUp.UI
{
    public class HudView : MonoBehaviour
    {
        private Text _scoreText;
        private Text _healthText;
        private Text _levelText;
        private Text _stateText;
        private Text _hintText;
        private BossHealthBarBinder _bossBarBinder;

        public static HudView CreateOrFind()
        {
            var existing = FindObjectOfType<HudView>();
            if (existing != null)
            {
                return existing;
            }

            var go = new GameObject("Hud");
            return go.AddComponent<HudView>();
        }

        private void Awake()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);

            gameObject.AddComponent<GraphicRaycaster>();

            _scoreText = CreateText("ScoreText", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -20f), TextAnchor.UpperLeft, 32);
            _healthText = CreateText("HealthText", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20f, -20f), TextAnchor.UpperRight, 32);
            _levelText = CreateText("LevelText", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -20f), TextAnchor.UpperCenter, 30);
            _stateText = CreateText("StateText", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 80f), TextAnchor.MiddleCenter, 68);
            _hintText = CreateText("HintText", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -20f), TextAnchor.MiddleCenter, 36);

            _scoreText.text = "Score: 0";
            _healthText.text = "HP: 3 / 3";
            _levelText.text = "Level 1";
            _bossBarBinder = CreateBossBarBinder();
        }

        private void OnEnable()
        {
            GameEvents.ScoreChanged += OnScoreChanged;
            GameEvents.PlayerHealthChanged += OnPlayerHealthChanged;
            GameEvents.LevelStarted += OnLevelStarted;
            GameEvents.StateChanged += OnStateChanged;
        }

        private void OnDisable()
        {
            GameEvents.ScoreChanged -= OnScoreChanged;
            GameEvents.PlayerHealthChanged -= OnPlayerHealthChanged;
            GameEvents.LevelStarted -= OnLevelStarted;
            GameEvents.StateChanged -= OnStateChanged;
        }

        private void OnScoreChanged(int score)
        {
            _scoreText.text = $"Score: {score}";
        }

        private void OnPlayerHealthChanged(int currentHealth, int maxHealth)
        {
            _healthText.text = $"HP: {currentHealth} / {maxHealth}";
        }

        private void OnLevelStarted(int levelId, string displayName)
        {
            _levelText.text = string.IsNullOrWhiteSpace(displayName) ? $"Level {levelId}" : displayName;
        }

        private void OnStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.MainMenu:
                    _stateText.text = "Shoot 'Em Up";
                    _hintText.text = "Press Enter to Start";
                    break;
                case GameState.Playing:
                    _stateText.text = string.Empty;
                    _hintText.text = string.Empty;
                    break;
                case GameState.Paused:
                    _stateText.text = "Paused";
                    _hintText.text = "Press Esc to Resume";
                    break;
                case GameState.LevelCleared:
                    _stateText.text = "Level Clear";
                    _hintText.text = "Press Enter for Next Level";
                    break;
                case GameState.GameOver:
                    _stateText.text = "Game Over";
                    _hintText.text = "Press R to Restart";
                    break;
                case GameState.GameCompleted:
                    _stateText.text = "Run Completed";
                    _hintText.text = "Press Enter to Restart";
                    break;
                default:
                    _stateText.text = state.ToString();
                    _hintText.text = string.Empty;
                    break;
            }
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
            backgroundImage.color = new Color(0f, 0f, 0f, 0.65f);

            var fill = new GameObject("BossBarFill");
            fill.transform.SetParent(background.transform, false);
            var fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(1f, 1f);
            fillRect.offsetMin = new Vector2(2f, 2f);
            fillRect.offsetMax = new Vector2(-2f, -2f);
            var fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0.95f, 0.2f, 0.25f, 0.95f);
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

            var binder = gameObject.AddComponent<BossHealthBarBinder>();
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

            // Fallback for older editor/runtime variants.
            return Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
    }
}
