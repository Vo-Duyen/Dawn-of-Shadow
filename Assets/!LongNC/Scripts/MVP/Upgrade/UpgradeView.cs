using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace DawnOfShadow.MVP.Upgrade
{
    public class UpgradeView : MonoBehaviour
    {
        [Header("HP UI Elements")]
        [SerializeField] private TextMeshProUGUI hpLevelText;
        [SerializeField] private TextMeshProUGUI hpCostText;
        [SerializeField] private Button hpUpgradeButton;

        [Header("DEF UI Elements")]
        [SerializeField] private TextMeshProUGUI defLevelText;
        [SerializeField] private TextMeshProUGUI defCostText;
        [SerializeField] private Button defUpgradeButton;

        [Header("Mana UI Elements")]
        [SerializeField] private TextMeshProUGUI manaLevelText;
        [SerializeField] private TextMeshProUGUI manaCostText;
        [SerializeField] private Button manaUpgradeButton;

        [Header("Panel Close Button")]
        [SerializeField] private Button closeButton;

        public event Action OnUpgradeHpRequested;
        public event Action OnUpgradeDefRequested;
        public event Action OnUpgradeManaRequested;

        private void Start()
        {
            hpUpgradeButton.onClick.AddListener(() => OnUpgradeHpRequested?.Invoke());
            defUpgradeButton.onClick.AddListener(() => OnUpgradeDefRequested?.Invoke());
            manaUpgradeButton.onClick.AddListener(() => OnUpgradeManaRequested?.Invoke());
            closeButton.onClick.AddListener(() => gameObject.SetActive(false));
        }

        public void DisplayStats(
            int hpLvl, int hpCost,
            int defLvl, int defCost,
            int manaLvl, int manaCost,
            int maxLevel)
        {
            // Update HP Upgrade UI
            UpdateStatUI(hpLevelText, hpCostText, hpUpgradeButton, hpLvl, hpCost, maxLevel);

            // Update DEF Upgrade UI
            UpdateStatUI(defLevelText, defCostText, defUpgradeButton, defLvl, defCost, maxLevel);

            // Update Mana Upgrade UI
            UpdateStatUI(manaLevelText, manaCostText, manaUpgradeButton, manaLvl, manaCost, maxLevel);
        }

        private void UpdateStatUI(
            TextMeshProUGUI lvlText, 
            TextMeshProUGUI costText, 
            Button button, 
            int level, 
            int cost, 
            int maxLevel)
        {
            if (lvlText != null) lvlText.text = $"Level: {level}/{maxLevel}";

            if (level >= maxLevel)
            {
                if (costText != null) costText.text = "MAXED OUT";
                if (button != null) button.interactable = false;
            }
            else
            {
                if (costText != null) costText.text = $"{cost} G";
                if (button != null) button.interactable = true;
            }

            // Punch button scale slightly on display update for micro-interactions
            if (button != null)
            {
                button.transform.DOPunchScale(Vector3.one * 0.05f, 0.2f, 5, 0.5f).SetUpdate(true);
            }
        }
    }
}
