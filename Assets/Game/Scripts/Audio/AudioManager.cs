using UnityEngine;

namespace AIGame.ShootEmUp.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        public float MasterVolume { get; private set; } = 1f;
        public float BgmVolume { get; private set; } = 0.8f;
        public float SfxVolume { get; private set; } = 0.9f;

        private readonly AudioSettingsStore _settingsStore = new AudioSettingsStore();
        private AudioSource _bgmSource;
        private AudioSource _sfxSource;

        public static AudioManager CreateOrFind()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var existing = FindObjectOfType<AudioManager>();
            if (existing != null)
            {
                return existing;
            }

            var go = new GameObject("AudioManager");
            return go.AddComponent<AudioManager>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureSources();
            LoadSettings();
            ApplyVolumes();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void SetVolumes(float master, float bgm, float sfx)
        {
            MasterVolume = Mathf.Clamp01(master);
            BgmVolume = Mathf.Clamp01(bgm);
            SfxVolume = Mathf.Clamp01(sfx);
            ApplyVolumes();
            _settingsStore.Save(MasterVolume, BgmVolume, SfxVolume);
        }

        public void PlayBgm(AudioClip clip)
        {
            EnsureSources();
            if (clip == null)
            {
                _bgmSource.Stop();
                _bgmSource.clip = null;
                return;
            }

            if (_bgmSource.clip == clip && _bgmSource.isPlaying)
            {
                return;
            }

            _bgmSource.clip = clip;
            _bgmSource.loop = true;
            _bgmSource.Play();
        }

        public void StopBgm()
        {
            if (_bgmSource == null)
            {
                return;
            }

            _bgmSource.Stop();
            _bgmSource.clip = null;
        }

        public void PlaySfx(AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }

            EnsureSources();
            _sfxSource.PlayOneShot(clip);
        }

        private void EnsureSources()
        {
            if (_bgmSource == null)
            {
                _bgmSource = gameObject.AddComponent<AudioSource>();
                _bgmSource.loop = true;
                _bgmSource.playOnAwake = false;
            }

            if (_sfxSource == null)
            {
                _sfxSource = gameObject.AddComponent<AudioSource>();
                _sfxSource.loop = false;
                _sfxSource.playOnAwake = false;
            }
        }

        private void LoadSettings()
        {
            MasterVolume = _settingsStore.MasterVolume;
            BgmVolume = _settingsStore.BgmVolume;
            SfxVolume = _settingsStore.SfxVolume;
        }

        private void ApplyVolumes()
        {
            EnsureSources();
            _bgmSource.volume = MasterVolume * BgmVolume;
            _sfxSource.volume = MasterVolume * SfxVolume;
            AudioListener.volume = MasterVolume;
        }
    }
}
