using System;
using UnityEngine;
using UnityEngine.UI;

namespace DawnOfShadow.MVP.HomeMenu
{
    public class HomeMenuView : MonoBehaviour
    {
        [Header("Main Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button shopButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button upgradeButton;

        [Header("Overlay Panels")]
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject charSelectPanel;
        [SerializeField] private GameObject upgradePanel;

        [Header("Currency display")]
        [SerializeField] private TMPro.TextMeshProUGUI goldText;
        [SerializeField] private TMPro.TextMeshProUGUI heartsText;
        [SerializeField] private TMPro.TextMeshProUGUI heartsCooldownText;

        public event Action OnPlayClicked;
        public event Action OnUpdate;
        public event Action OnContinueClicked;
        public event Action OnShopClicked;
        public event Action OnSettingsClicked;
        public event Action OnUpgradeClicked;

        private void Start()
        {
            playButton.onClick.AddListener(() => OnPlayClicked?.Invoke());
            continueButton.onClick.AddListener(() => OnContinueClicked?.Invoke());
            shopButton.onClick.AddListener(() => OnShopClicked?.Invoke());
            settingsButton.onClick.AddListener(() => OnSettingsClicked?.Invoke());
            if (upgradeButton != null) upgradeButton.onClick.AddListener(() => OnUpgradeClicked?.Invoke());
        }

        public void SetContinueButtonActive(bool active)
        {
            continueButton.interactable = active;
        }

        public void ToggleShopPanel(bool show)
        {
            if (shopPanel != null) shopPanel.SetActive(show);
        }

        public void ToggleSettingsPanel(bool show)
        {
            if (settingsPanel != null) settingsPanel.SetActive(show);
        }

        public void ToggleCharSelectPanel(bool show)
        {
            if (charSelectPanel != null) charSelectPanel.SetActive(show);
        }

        public void ToggleUpgradePanel(bool show)
        {
            if (upgradePanel != null) upgradePanel.SetActive(show);
        }

        public void DisplayGold(int amount)
        {
            if (goldText != null)
            {
                goldText.text = $"{amount} G";
            }
        }

        private void Update()
        {
            OnUpdate?.Invoke();
        }

        public void DisplayHearts(int currentHearts, string cooldownStr)
        {
            if (heartsText != null)
            {
                heartsText.text = currentHearts.ToString();
            }

            if (heartsCooldownText != null)
            {
                if (currentHearts >= 5)
                {
                    heartsCooldownText.text = "Full";
                }
                else
                {
                    heartsCooldownText.text = cooldownStr;
                }
            }
        }

        public void SetPlayButtonState(bool hasHearts)
        {
            if (playButton != null)
            {
                var btnImage = playButton.GetComponent<Image>();
                if (btnImage != null)
                {
                    btnImage.color = hasHearts ? Color.white : new Color(1f, 0.4f, 0.4f);
                }
            }
        }
    }
}
