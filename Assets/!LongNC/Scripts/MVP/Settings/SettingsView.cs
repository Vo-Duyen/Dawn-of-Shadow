using System;
using UnityEngine;
using UnityEngine.UI;

namespace DawnOfShadow.MVP.Settings
{
    public class SettingsView : MonoBehaviour
    {
        [Header("Volume Sliders")]
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Button closeButton;

        [Header("Sound Mute Buttons (Optional)")]
        [SerializeField] private Button musicButton;
        [SerializeField] private Button sfxButton;

        [Header("Sound Mute Sprites (Optional)")]
        [SerializeField] private Sprite musicOnSprite;
        [SerializeField] private Sprite musicOffSprite;
        [SerializeField] private Sprite sfxOnSprite;
        [SerializeField] private Sprite sfxOffSprite;

        [Header("Sound Mute Icon Images (Optional)")]
        [SerializeField] private Image musicIconImage;
        [SerializeField] private Image sfxIconImage;

        [Header("Gameplay Give Up (Optional)")]
        [SerializeField] private Button giveUpButton;
        [SerializeField] private GameObject areYouSurePanel;
        [SerializeField] private Button areYouSureCloseButton;
        [SerializeField] private Button areYouSureHomeButton;

        public event Action<float, float, float, float> OnSettingsChangedByUser;
        public event Action OnCloseRequested;
        public event Action OnGiveUpConfirmed;
        public event Action OnOpened;

        private void OnEnable()
        {
            Debug.Log($"[DEBUG] SettingsView: OnEnable called. GameObject active: {gameObject.activeSelf}");
            OnOpened?.Invoke();
        }

        private float _prevMusicVolume = 0.75f;
        private float _prevSfxVolume = 0.75f;

        private void Start()
        {
            musicSlider.onValueChanged.AddListener(OnValueUpdated);
            sfxSlider.onValueChanged.AddListener(OnValueUpdated);
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(() => 
                {
                    Debug.Log("[DEBUG] SettingsView: Close button clicked.");
                    OnCloseRequested?.Invoke();
                    gameObject.SetActive(false);
                });
            }

            if (musicButton != null)
            {
                musicButton.onClick.AddListener(ToggleMusicMute);
            }
            if (sfxButton != null)
            {
                sfxButton.onClick.AddListener(ToggleSfxMute);
            }

            if (giveUpButton != null)
            {
                giveUpButton.onClick.AddListener(ShowAreYouSurePanel);
            }
            else
            {
                Debug.LogWarning("SettingsView: 'giveUpButton' is not assigned in the Inspector! The Give Up panel will not be showable.");
            }

            if (areYouSureCloseButton != null)
            {
                areYouSureCloseButton.onClick.AddListener(HideAreYouSurePanel);
            }
            else
            {
                Debug.LogWarning("SettingsView: 'areYouSureCloseButton' is not assigned in the Inspector!");
            }

            if (areYouSureHomeButton != null)
            {
                areYouSureHomeButton.onClick.AddListener(() => OnGiveUpConfirmed?.Invoke());
            }
            else
            {
                Debug.LogWarning("SettingsView: 'areYouSureHomeButton' is not assigned in the Inspector!");
            }

            if (areYouSurePanel != null)
            {
                areYouSurePanel.SetActive(false);
            }
        }

        private void ShowAreYouSurePanel()
        {
            if (areYouSurePanel != null) areYouSurePanel.SetActive(true);
        }

        private void HideAreYouSurePanel()
        {
            if (areYouSurePanel != null) areYouSurePanel.SetActive(false);
        }

        public void InitializeValues(float music, float sfx, float lastMusic, float lastSfx)
        {
            // Tạm thời bỏ listener để tránh gọi vòng lặp vô hạn khi setup ban đầu
            musicSlider.onValueChanged.RemoveListener(OnValueUpdated);
            sfxSlider.onValueChanged.RemoveListener(OnValueUpdated);

            musicSlider.value = music;
            sfxSlider.value = sfx;

            _prevMusicVolume = lastMusic > 0f ? lastMusic : (music > 0f ? music : 0.75f);
            _prevSfxVolume = lastSfx > 0f ? lastSfx : (sfx > 0f ? sfx : 0.75f);

            UpdateVolumeIcons();

            musicSlider.onValueChanged.AddListener(OnValueUpdated);
            sfxSlider.onValueChanged.AddListener(OnValueUpdated);
        }

        private void OnValueUpdated(float value)
        {
            if (musicSlider.value > 0f) _prevMusicVolume = musicSlider.value;
            if (sfxSlider.value > 0f) _prevSfxVolume = sfxSlider.value;

            UpdateVolumeIcons();

            OnSettingsChangedByUser?.Invoke(musicSlider.value, sfxSlider.value, _prevMusicVolume, _prevSfxVolume);
        }

        private void ToggleMusicMute()
        {
            if (musicSlider.value > 0f)
            {
                _prevMusicVolume = musicSlider.value;
                musicSlider.value = 0f;
            }
            else
            {
                musicSlider.value = _prevMusicVolume > 0f ? _prevMusicVolume : 0.75f;
            }
        }

        private void ToggleSfxMute()
        {
            if (sfxSlider.value > 0f)
            {
                _prevSfxVolume = sfxSlider.value;
                sfxSlider.value = 0f;
            }
            else
            {
                sfxSlider.value = _prevSfxVolume > 0f ? _prevSfxVolume : 0.75f;
            }
        }

        private void UpdateVolumeIcons()
        {
            // Update Music Icon
            Sprite selectedMusicSprite = musicSlider.value > 0f ? musicOnSprite : musicOffSprite;
            if (musicIconImage != null)
            {
                musicIconImage.sprite = selectedMusicSprite;
            }
            else if (musicButton != null)
            {
                Image img = musicButton.GetComponent<Image>();
                if (img != null) img.sprite = selectedMusicSprite;
            }

            // Update SFX Icon
            Sprite selectedSfxSprite = sfxSlider.value > 0f ? sfxOnSprite : sfxOffSprite;
            if (sfxIconImage != null)
            {
                sfxIconImage.sprite = selectedSfxSprite;
            }
            else if (sfxButton != null)
            {
                Image img = sfxButton.GetComponent<Image>();
                if (img != null) img.sprite = selectedSfxSprite;
            }
        }
    }
}
