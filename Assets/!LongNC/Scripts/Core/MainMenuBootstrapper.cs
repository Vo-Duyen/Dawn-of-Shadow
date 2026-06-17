using UnityEngine;
using System.Collections.Generic;
using DawnOfShadow.Data;
using DawnOfShadow.MVP.CharacterSelection;
using DawnOfShadow.MVP.Shop;
using DawnOfShadow.MVP.Settings;
using DawnOfShadow.MVP.HomeMenu;
using DawnOfShadow.MVP.Upgrade;
using DawnOfShadow.Core;

namespace DawnOfShadow.Core
{
    public class MainMenuBootstrapper : MonoBehaviour
    {
        [Header("Views")]
        [SerializeField] private HomeMenuView homeMenuView;
        [SerializeField] private CharacterSelectionView charSelectView;
        [SerializeField] private ShopView shopView;
        [SerializeField] private SettingsView settingsView;
        [SerializeField] private UpgradeView upgradeView;

        [Header("ScriptableObject Databases")]
        [SerializeField] private List<CharacterData> allCharacters;

        private HomeMenuPresenter _homeMenuPresenter;
        private CharacterSelectionPresenter _charSelectPresenter;
        private SettingsPresenter _settingsPresenter;
        private UpgradePresenter _upgradePresenter;

        private void Start()
        {
            // Initialize Home Menu Presenter
            if (homeMenuView != null)
            {
                _homeMenuPresenter = new HomeMenuPresenter(homeMenuView);
            }

            // Initialize Character Selection MVP
            if (charSelectView != null)
            {
                var charModel = new CharacterSelectionModel(allCharacters, SaveManager.Instance.Data.selectedCharacterId);
                _charSelectPresenter = new CharacterSelectionPresenter(charModel, charSelectView);
            }



            // Initialize Settings MVP
            if (settingsView == null)
            {
                settingsView = FindObjectOfType<SettingsView>(true);
            }

            if (settingsView != null)
            {
                var currentData = SaveManager.Instance.Data;
                var settingsModel = new SettingsModel(currentData.musicVolume, currentData.sfxVolume, currentData.lastMusicVolume, currentData.lastSfxVolume);
                _settingsPresenter = new SettingsPresenter(settingsModel, settingsView);
            }

            // Initialize Upgrade MVP
            if (upgradeView != null)
            {
                var upgradeModel = new UpgradeModel();
                _upgradePresenter = new UpgradePresenter(upgradeModel, upgradeView);
                
                // When upgrades occur, reload the Home menu gold text display
                if (_homeMenuPresenter != null)
                {
                    _upgradePresenter.OnGoldChanged += _homeMenuPresenter.UpdateGoldDisplay;
                }
            }

            // Bắt đầu tải ngầm (Preload) trước Scene Gameplay ở chế độ nền của Home
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.PreloadScene("Gameplay");
            }
        }
    }
}
