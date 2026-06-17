using System;
using UnityEngine;
using UnityEngine.UI;

namespace DawnOfShadow.MVP.GameResult
{
    public class GameResultView : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private GameObject defeatPanel;

        [Header("Buttons")]
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button homeButton;

        public event Action OnNextLevelClicked;
        public event Action OnRestartClicked;
        public event Action OnHomeClicked;

        private void Awake()
        {
            if (nextLevelButton != null)
                nextLevelButton.onClick.AddListener(() => OnNextLevelClicked?.Invoke());
                
            if (restartButton != null)
                restartButton.onClick.AddListener(() => OnRestartClicked?.Invoke());
                
            if (homeButton != null)
                homeButton.onClick.AddListener(() => OnHomeClicked?.Invoke());
        }

        public void ShowVictory()
        {
            if (resultPanel != null) resultPanel.SetActive(true);
            if (victoryPanel != null) victoryPanel.SetActive(true);
            if (defeatPanel != null) defeatPanel.SetActive(false);
        }

        public void ShowDefeat()
        {
            if (resultPanel != null) resultPanel.SetActive(true);
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (defeatPanel != null) defeatPanel.SetActive(true);
        }

        public void Hide()
        {
            if (resultPanel != null) resultPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (defeatPanel != null) defeatPanel.SetActive(false);
        }
    }
}
