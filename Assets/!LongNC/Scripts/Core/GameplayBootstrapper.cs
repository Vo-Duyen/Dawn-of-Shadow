using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DawnOfShadow.Data;
using DawnOfShadow.Gameplay;
using DawnOfShadow.Gameplay.Player;
using DawnOfShadow.Gameplay.Input;
using DawnOfShadow.MVP.GameplayHUD;
using DawnOfShadow.MVP.Settings;

namespace DawnOfShadow.Core
{
    public class GameplayBootstrapper : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private JoystickController gameplayJoystick;

        [Header("HUD Visual View")]
        [SerializeField] private GameplayHUDView hudView;

        [Header("Character Database")]
        [SerializeField] private List<CharacterData> allCharacters;

        [Header("Camera Settings")]
        [SerializeField] private Transform cameraRig; // Ex: A simple camera follow transform

        [Header("Settings View")]
        [SerializeField] private SettingsView settingsView;

        private GameplayHUDPresenter _hudPresenter;
        private SettingsPresenter _settingsPresenter;

        private void Start()
        {
            SpawnPlayer();

            // Khởi tạo Settings MVP trong Gameplay
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
            else
            {
                Debug.LogWarning("GameplayBootstrapper: 'settingsView' is not assigned in the Inspector and could not be found in the scene! The Settings and Give Up features will not function.");
            }
        }

        private void Update()
        {
            _hudPresenter?.Tick();
        }

        private void OnDestroy()
        {
            _hudPresenter?.Destroy();
        }

        private void SpawnPlayer()
        {
            var save = SaveManager.Instance.Data;
            string selectedId = save.selectedCharacterId;

            // Find selected character data using LINQ
            CharacterData data = allCharacters.FirstOrDefault(c => c.characterId == selectedId);

            if (data == null && allCharacters.Count > 0)
            {
                data = allCharacters[0]; // Fallback to first character
            }

            if (data != null && data.prefab != null)
            {
                Vector3 spawnPos = playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;
                Quaternion spawnRot = playerSpawnPoint != null ? playerSpawnPoint.rotation : Quaternion.identity;

                // If WorldChunkManager exists in the scene, use its custom spawn point
                var chunkManager = FindObjectOfType<WorldChunkManager>();
                if (chunkManager != null)
                {
                    spawnPos = chunkManager.PlayerSpawnPoint;
                }

                // Instantiate player character
                GameObject playerObj = Instantiate(data.prefab, spawnPos, spawnRot);
                playerObj.tag = "Player"; // Important: So enemies can find this player

                // Add or get PlayerController
                PlayerController controller = playerObj.GetComponent<PlayerController>();
                if (controller == null)
                {
                    controller = playerObj.AddComponent<PlayerController>();
                }

                // Bind the virtual joystick
                if (gameplayJoystick != null)
                {
                    controller.InitializeJoystick(gameplayJoystick);
                }

                // Initialize HUD MVP
                if (hudView != null)
                {
                    _hudPresenter = new GameplayHUDPresenter(hudView, controller);
                }

                // Camera follow setup (simple smooth look at / follow)
                if (cameraRig != null)
                {
                    // You can assign player to custom follow target scripts or Cinemachine target group
                    var followScript = cameraRig.GetComponent<SimpleCameraFollow>();
                    if (followScript != null)
                    {
                        followScript.SetTarget(playerObj.transform);
                    }
                }

                Debug.Log($"Successfully spawned player: {data.characterName}");
            }
            else
            {
                Debug.LogError("Failed to spawn player: Missing character prefab or spawn point!");
            }
        }
    }
}
