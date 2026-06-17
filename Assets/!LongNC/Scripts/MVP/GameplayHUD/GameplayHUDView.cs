using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DawnOfShadow.MVP.GameplayHUD
{
    public class GameplayHUDView : MonoBehaviour
    {
        [Header("Player HP Visuals")]
        [SerializeField] private Slider hpSlider;
        [SerializeField] private TextMeshProUGUI hpText;

        [Header("Action Buttons")]
        [SerializeField] private Button attackButton;
        [SerializeField] private Button dodgeButton;
        [SerializeField] private Button skill2Button;
        [SerializeField] private Button skill3Button;
        [SerializeField] private Button healButton;

        [Header("Cooldown Visual Overlays (Radial Fill Images)")]
        [SerializeField] private Image dodgeCooldownOverlay;
        [SerializeField] private Image skill2CooldownOverlay;
        [SerializeField] private Image skill3CooldownOverlay;

        [Header("Cooldown Numeric Texts")]
        [SerializeField] private TextMeshProUGUI dodgeCooldownText;
        [SerializeField] private TextMeshProUGUI skill2CooldownText;
        [SerializeField] private TextMeshProUGUI skill3CooldownText;

        [Header("Item Indicators")]
        [SerializeField] private TextMeshProUGUI potionCountText;

        [Header("Pause & Win/Lose Panels")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private GameObject defeatPanel;

        [Header("Pause & Win/Lose Buttons")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button victoryQuitButton;
        [SerializeField] private Button defeatRestartButton;
        [SerializeField] private Button defeatQuitButton;
        [SerializeField] private TextMeshProUGUI victoryGoldText;
        [SerializeField] private TextMeshProUGUI defeatGoldText;
        [SerializeField] private Button victoryHomeButton;
        [SerializeField] private Button defeatHomeButton;

        public event Action OnAttackClicked;
        public event Action OnDodgeClicked;
        public event Action OnSkill2Clicked;
        public event Action OnSkill3Clicked;
        public event Action OnHealClicked;
        public event Action OnPauseClicked;
        public event Action OnResumeClicked;
        public event Action OnRestartClicked;
        public event Action OnQuitClicked;

        private void Start()
        {
            attackButton.onClick.AddListener(() => OnAttackClicked?.Invoke());
            dodgeButton.onClick.AddListener(() => OnDodgeClicked?.Invoke());
            skill2Button.onClick.AddListener(() => OnSkill2Clicked?.Invoke());
            skill3Button.onClick.AddListener(() => OnSkill3Clicked?.Invoke());
            healButton.onClick.AddListener(() => OnHealClicked?.Invoke());

            if (pauseButton != null) pauseButton.onClick.AddListener(() => OnPauseClicked?.Invoke());
            if (resumeButton != null) resumeButton.onClick.AddListener(() => OnResumeClicked?.Invoke());
            if (restartButton != null) restartButton.onClick.AddListener(() => OnRestartClicked?.Invoke());
            if (quitButton != null) quitButton.onClick.AddListener(() => OnQuitClicked?.Invoke());
            if (victoryQuitButton != null) victoryQuitButton.onClick.AddListener(() => OnQuitClicked?.Invoke());
            if (defeatRestartButton != null) defeatRestartButton.onClick.AddListener(() => OnRestartClicked?.Invoke());
            if (defeatQuitButton != null) defeatQuitButton.onClick.AddListener(() => OnQuitClicked?.Invoke());
            if (victoryHomeButton != null) victoryHomeButton.onClick.AddListener(() => OnQuitClicked?.Invoke());
            if (defeatHomeButton != null) defeatHomeButton.onClick.AddListener(() => OnQuitClicked?.Invoke());

            if (pausePanel != null) pausePanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (defeatPanel != null) defeatPanel.SetActive(false);

            // Initialize overlays to empty
            SetCooldown(0, 0f, 0f);
            SetCooldown(1, 0f, 0f);
            SetCooldown(2, 0f, 0f);
        }

        public void UpdateHP(int current, int max)
        {
            if (hpSlider != null)
            {
                hpSlider.maxValue = max;
                hpSlider.value = current;
            }
            if (hpText != null)
            {
                hpText.text = $"{current} / {max}";
            }
        }

        public void UpdatePotionCount(int count)
        {
            if (potionCountText != null)
            {
                potionCountText.text = $"x{count}";
            }
        }

        public void SetCooldown(int skillIndex, float fillAmount, float remainingTime)
        {
            Image overlay = null;
            TextMeshProUGUI text = null;

            if (skillIndex == 0)
            {
                overlay = dodgeCooldownOverlay;
                text = dodgeCooldownText;
            }
            else if (skillIndex == 1)
            {
                overlay = skill2CooldownOverlay;
                text = skill2CooldownText;
            }
            else if (skillIndex == 2)
            {
                overlay = skill3CooldownOverlay;
                text = skill3CooldownText;
            }

            if (overlay != null)
            {
                overlay.fillAmount = fillAmount;
            }

            if (text != null)
            {
                if (remainingTime > 0.05f)
                {
                    text.gameObject.SetActive(true);
                    text.text = $"{remainingTime:F1}s";
                }
                else
                {
                    text.gameObject.SetActive(false);
                }
            }
        }

        public void ShowVictoryPanel()
        {
            if (victoryPanel != null) victoryPanel.SetActive(true);
        }

        public void ShowDefeatPanel()
        {
            if (defeatPanel != null) defeatPanel.SetActive(true);
        }

        public void TogglePausePanel(bool show)
        {
            if (pausePanel != null) pausePanel.SetActive(show);
        }

        public void SetGoldReward(int goldAmount)
        {
            if (victoryGoldText != null)
            {
                victoryGoldText.text = $"+{goldAmount}";
            }
            if (defeatGoldText != null)
            {
                defeatGoldText.text = $"+{goldAmount}";
            }
        }
    }
}
