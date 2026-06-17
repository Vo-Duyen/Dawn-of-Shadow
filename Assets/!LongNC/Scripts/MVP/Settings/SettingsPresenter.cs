using DawnOfShadow.Core;
using UnityEngine;

namespace DawnOfShadow.MVP.Settings
{
    public class SettingsPresenter
    {
        private readonly SettingsModel _model;
        private readonly SettingsView _view;

        public SettingsPresenter(SettingsModel model, SettingsView view)
        {
            _model = model;
            _view = view;

            _view.InitializeValues(_model.MusicVolume, _model.SfxVolume, _model.LastMusicVolume, _model.LastSfxVolume);

            _view.OnSettingsChangedByUser += UpdateSettings;
            _view.OnCloseRequested += CloseSettings;
            _view.OnGiveUpConfirmed += HandleGiveUp;
            _view.OnOpened += OpenSettings;
        }

        private void UpdateSettings(float music, float sfx, float lastMusic, float lastSfx)
        {
            _model.UpdateSettings(music, sfx, lastMusic, lastSfx);

            // Save settings via SaveManager
            var save = SaveManager.Instance.Data;
            save.musicVolume = music;
            save.sfxVolume = sfx;
            save.lastMusicVolume = lastMusic;
            save.lastSfxVolume = lastSfx;
            SaveManager.Instance.Save();
            
            // Audio engine integration can happen here (e.g. updating mixer volumes)
        }

        private void OpenSettings()
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            Debug.Log($"[DEBUG] SettingsPresenter: OpenSettings called. Scene: {sceneName}, current timeScale: {Time.timeScale}");
            if (sceneName == "Gameplay")
            {
                Time.timeScale = 0f;
                Debug.Log("[DEBUG] SettingsPresenter: Time.timeScale set to 0f");
            }
        }

        private void CloseSettings()
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            Debug.Log($"[DEBUG] SettingsPresenter: CloseSettings called. Scene: {sceneName}, current timeScale: {Time.timeScale}");
            
            // Close setting panel UI
            _view.gameObject.SetActive(false);

            // Khôi phục timeScale về 1 nếu đang ở trong Gameplay để đảm bảo an toàn
            if (sceneName == "Gameplay")
            {
                Time.timeScale = 1f;
                Debug.Log("[DEBUG] SettingsPresenter: Time.timeScale set to 1f");
            }
        }

        private void HandleGiveUp()
        {
            var save = SaveManager.Instance.Data;

            // Trừ 1 tim khi đầu hàng
            if (save.hearts > 0)
            {
                if (save.hearts == 5)
                {
                    save.lastHeartRegenTimeTicks = System.DateTime.UtcNow.Ticks;
                }
                save.hearts--;
            }

            // Hủy tiến trình game dở dang
            save.hasInProgressGame = false;
            SaveManager.Instance.Save();

            // Khôi phục tốc độ chạy game bình thường và chuyển scene
            Time.timeScale = 1f;
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadSceneAsync("Home", 0f);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Home");
            }
        }
    }
}
