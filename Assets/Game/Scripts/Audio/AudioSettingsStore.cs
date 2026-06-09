using UnityEngine;

namespace AIGame.ShootEmUp.Audio
{
    public sealed class AudioSettingsStore
    {
        private const string MasterVolumeKey = "shootemup.audio.master";
        private const string BgmVolumeKey = "shootemup.audio.bgm";
        private const string SfxVolumeKey = "shootemup.audio.sfx";

        public float MasterVolume => LoadVolume(MasterVolumeKey, 1f);
        public float BgmVolume => LoadVolume(BgmVolumeKey, 0.8f);
        public float SfxVolume => LoadVolume(SfxVolumeKey, 0.9f);

        public void Save(float master, float bgm, float sfx)
        {
            PlayerPrefs.SetFloat(MasterVolumeKey, Mathf.Clamp01(master));
            PlayerPrefs.SetFloat(BgmVolumeKey, Mathf.Clamp01(bgm));
            PlayerPrefs.SetFloat(SfxVolumeKey, Mathf.Clamp01(sfx));
            PlayerPrefs.Save();
        }

        private static float LoadVolume(string key, float defaultValue)
        {
            return Mathf.Clamp01(PlayerPrefs.GetFloat(key, defaultValue));
        }
    }
}
