using System;

namespace DawnOfShadow.MVP.Settings
{
    public class SettingsModel
    {
        public float MusicVolume { get; set; }
        public float SfxVolume { get; set; }
        public float LastMusicVolume { get; set; }
        public float LastSfxVolume { get; set; }

        public event Action OnSettingsChanged;

        public SettingsModel(float music, float sfx, float lastMusic, float lastSfx)
        {
            MusicVolume = music;
            SfxVolume = sfx;
            LastMusicVolume = lastMusic;
            LastSfxVolume = lastSfx;
        }

        public void UpdateSettings(float music, float sfx, float lastMusic, float lastSfx)
        {
            MusicVolume = music;
            SfxVolume = sfx;
            LastMusicVolume = lastMusic;
            LastSfxVolume = lastSfx;
            OnSettingsChanged?.Invoke();
        }
    }
}
