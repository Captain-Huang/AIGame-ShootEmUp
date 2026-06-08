using AIGame.ShootEmUp.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AIGame.ShootEmUp.Bosses
{
    public class BossHealthBarBinder : MonoBehaviour
    {
        [SerializeField] private GameObject rootObject;
        [SerializeField] private Image fillImage;
        [SerializeField] private Text bossNameText;
        [SerializeField] private Text phaseText;

        private int _maxHealth = 1;

        public void Setup(GameObject root, Image fill, Text name, Text phase)
        {
            rootObject = root;
            fillImage = fill;
            bossNameText = name;
            phaseText = phase;
            SetVisible(false);
        }

        private void OnEnable()
        {
            GameEvents.BossSpawned += OnBossSpawned;
            GameEvents.BossHealthChanged += OnBossHealthChanged;
            GameEvents.BossPhaseChanged += OnBossPhaseChanged;
            GameEvents.BossDefeated += OnBossDefeated;
            GameEvents.StateChanged += OnStateChanged;
        }

        private void OnDisable()
        {
            GameEvents.BossSpawned -= OnBossSpawned;
            GameEvents.BossHealthChanged -= OnBossHealthChanged;
            GameEvents.BossPhaseChanged -= OnBossPhaseChanged;
            GameEvents.BossDefeated -= OnBossDefeated;
            GameEvents.StateChanged -= OnStateChanged;
        }

        private void OnBossSpawned(string bossName, int maxHealth)
        {
            _maxHealth = Mathf.Max(1, maxHealth);
            if (bossNameText != null)
            {
                bossNameText.text = string.IsNullOrWhiteSpace(bossName) ? "Boss" : bossName;
            }

            if (phaseText != null)
            {
                phaseText.text = "Phase 1";
            }

            UpdateFill(_maxHealth);
            SetVisible(true);
        }

        private void OnBossHealthChanged(int currentHealth, int maxHealth)
        {
            if (maxHealth > 0)
            {
                _maxHealth = maxHealth;
            }

            UpdateFill(currentHealth);
        }

        private void OnBossPhaseChanged(string phaseName)
        {
            if (phaseText == null)
            {
                return;
            }

            phaseText.text = string.IsNullOrWhiteSpace(phaseName) ? phaseText.text : phaseName;
        }

        private void OnBossDefeated()
        {
            SetVisible(false);
        }

        private void OnStateChanged(GameState state)
        {
            if (state == GameState.MainMenu || state == GameState.GameOver || state == GameState.GameCompleted)
            {
                SetVisible(false);
            }
        }

        private void SetVisible(bool visible)
        {
            if (rootObject != null)
            {
                rootObject.SetActive(visible);
            }
        }

        private void UpdateFill(int currentHealth)
        {
            if (fillImage == null)
            {
                return;
            }

            fillImage.fillAmount = Mathf.Clamp01((float)Mathf.Max(0, currentHealth) / Mathf.Max(1, _maxHealth));
        }
    }
}
