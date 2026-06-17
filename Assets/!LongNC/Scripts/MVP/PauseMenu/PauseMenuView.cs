using System;
using UnityEngine;
using UnityEngine.UI;

namespace DawnOfShadow.MVP.PauseMenu
{
    public class PauseMenuView : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button quitButton;
        
        public event Action OnResumeClicked;
        public event Action OnRestartClicked;
        public event Action OnQuitClicked;

        private void Awake()
        {
            if (resumeButton != null)
                resumeButton.onClick.AddListener(() => OnResumeClicked?.Invoke());
                
            if (restartButton != null)
                restartButton.onClick.AddListener(() => OnRestartClicked?.Invoke());
                
            if (quitButton != null)
                quitButton.onClick.AddListener(() => OnQuitClicked?.Invoke());
        }

        public void Show()
        {
            if (pausePanel != null)
                pausePanel.SetActive(true);
        }

        public void Hide()
        {
            if (pausePanel != null)
                pausePanel.SetActive(false);
        }
    }
}
