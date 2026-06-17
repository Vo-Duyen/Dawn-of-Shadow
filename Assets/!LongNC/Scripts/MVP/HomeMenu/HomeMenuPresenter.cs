using DawnOfShadow.Core;
using DawnOfShadow.Gameplay.Level;
using System;

namespace DawnOfShadow.MVP.HomeMenu
{
    public class HomeMenuPresenter
    {
        private readonly HomeMenuView _view;

        public HomeMenuPresenter(HomeMenuView view)
        {
            _view = view;

            // Bind click events
            _view.OnPlayClicked += StartNewGame;
            _view.OnContinueClicked += ContinueGame;
            _view.OnShopClicked += OpenShop;
            _view.OnSettingsClicked += OpenSettings;
            _view.OnUpgradeClicked += OpenUpgrade;
            _view.OnUpdate += Tick;

            RefreshContinueButton();
            UpdateGoldDisplay();
            UpdateHeartsRegen();
            UpdateHeartsDisplay();
        }

        public void UpdateGoldDisplay()
        {
            _view.DisplayGold(SaveManager.Instance.Data.gold);
        }

        private void RefreshContinueButton()
        {
            var save = SaveManager.Instance.Data;
            _view.SetContinueButtonActive(save.hasInProgressGame);
        }

        private void StartNewGame()
        {
            var save = SaveManager.Instance.Data;

            // Kiểm tra số lượng tim trước khi bắt đầu
            if (save.hearts <= 0)
            {
                UnityEngine.Debug.LogWarning("Không đủ tim để bắt đầu game mới!");
                return;
            }
            
            // Reset gameplay level progress back to Stage 1 Level 1
            save.currentStage = 1;
            save.currentSubLevel = 1;
            save.hasInProgressGame = true; // Mark as in-progress now
            
            ConsumeHeart();

            SaveManager.Instance.Save();

            // Load LevelManager configuration
            LevelManager.Instance.LoadProgress();

            // Transition asynchronously to gameplay scene
            SceneLoader.Instance.LoadSceneAsync("Gameplay", 0f);
        }

        private void ContinueGame()
        {
            var save = SaveManager.Instance.Data;
            if (save.hasInProgressGame)
            {
                // Kiểm tra số lượng tim trước khi tiếp tục
                if (save.hearts <= 0)
                {
                    UnityEngine.Debug.LogWarning("Không đủ tim để tiếp tục lượt chơi!");
                    return;
                }

                ConsumeHeart();

                // Reload progress configs in LevelManager
                LevelManager.Instance.LoadProgress();

                // Transition asynchronously to gameplay scene
                SceneLoader.Instance.LoadSceneAsync("Gameplay", 0f);
            }
        }

        private void OpenShop()
        {
            _view.ToggleShopPanel(true);
            _view.ToggleSettingsPanel(false);
            _view.ToggleUpgradePanel(false);
        }

        private void OpenSettings()
        {
            _view.ToggleSettingsPanel(true);
            _view.ToggleShopPanel(false);
            _view.ToggleUpgradePanel(false);
        }

        private void OpenUpgrade()
        {
            _view.ToggleUpgradePanel(true);
            _view.ToggleShopPanel(false);
            _view.ToggleSettingsPanel(false);
        }

        private void Tick()
        {
            UpdateHeartsRegen();
            UpdateHeartsDisplay();
        }

        private void UpdateHeartsRegen()
        {
            var save = SaveManager.Instance.Data;
            if (save.hearts < 5)
            {
                if (save.lastHeartRegenTimeTicks == 0)
                {
                    save.lastHeartRegenTimeTicks = DateTime.UtcNow.Ticks;
                    SaveManager.Instance.Save();
                }
                else
                {
                    DateTime now = DateTime.UtcNow;
                    DateTime lastRegen = new DateTime(save.lastHeartRegenTimeTicks);
                    TimeSpan elapsed = now - lastRegen;
                    double minutes = elapsed.TotalMinutes;

                    if (minutes >= 10.0)
                    {
                        int heartsToAdd = (int)(minutes / 10.0);
                        save.hearts += heartsToAdd;

                        if (save.hearts >= 5)
                        {
                            save.hearts = 5;
                            save.lastHeartRegenTimeTicks = 0;
                        }
                        else
                        {
                            save.lastHeartRegenTimeTicks = lastRegen.AddMinutes(heartsToAdd * 10.0).Ticks;
                        }
                        SaveManager.Instance.Save();
                    }
                }
            }
            else
            {
                save.lastHeartRegenTimeTicks = 0;
            }
        }

        private void UpdateHeartsDisplay()
        {
            var save = SaveManager.Instance.Data;
            string cooldownStr = GetHeartCooldownString();
            _view.DisplayHearts(save.hearts, cooldownStr);
            _view.SetPlayButtonState(save.hearts > 0);
        }

        private string GetHeartCooldownString()
        {
            var save = SaveManager.Instance.Data;
            if (save.hearts >= 5) return "";
            if (save.lastHeartRegenTimeTicks == 0) return "10:00";

            DateTime now = DateTime.UtcNow;
            DateTime lastRegen = new DateTime(save.lastHeartRegenTimeTicks);
            TimeSpan elapsed = now - lastRegen;
            double elapsedSeconds = elapsed.TotalSeconds;

            // Tính toán tổng thời gian cần để hồi phục toàn bộ số tim đã mất
            int lostHearts = 5 - save.hearts;
            double totalRegenIntervalSeconds = lostHearts * 10.0 * 60.0; // 10 phút cho mỗi tim mất

            double remainingSeconds = totalRegenIntervalSeconds - elapsedSeconds;

            if (remainingSeconds <= 0)
            {
                return "00:00";
            }

            int minutes = (int)(remainingSeconds / 60.0);
            int seconds = (int)(remainingSeconds % 60.0);
            return $"{minutes:D2}:{seconds:D2}";
        }

        private void ConsumeHeart()
        {
            var save = SaveManager.Instance.Data;
            if (save.hearts == 5)
            {
                save.lastHeartRegenTimeTicks = DateTime.UtcNow.Ticks;
            }
            save.hearts--;
            SaveManager.Instance.Save();
        }
    }
}
