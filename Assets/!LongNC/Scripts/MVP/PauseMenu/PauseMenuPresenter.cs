using UnityEngine;
using DawnOfShadow.Core;

namespace DawnOfShadow.MVP.PauseMenu
{
    public class PauseMenuPresenter : MonoBehaviour
    {
        [SerializeField] private PauseMenuView view;
        private PauseMenuModel model;

        private void Awake()
        {
            model = new PauseMenuModel();
            
            if (view != null)
            {
                view.OnResumeClicked += HandleResume;
                view.OnRestartClicked += HandleRestart;
                view.OnQuitClicked += HandleQuit;
            }
        }

        private void Start()
        {
            GameEventSystem.Subscribe("OnGamePaused", ShowPauseMenu);
            GameEventSystem.Subscribe("OnGameResumed", HidePauseMenu);
            
            view.Hide();
        }

        private void OnDestroy()
        {
            if (view != null)
            {
                view.OnResumeClicked -= HandleResume;
                view.OnRestartClicked -= HandleRestart;
                view.OnQuitClicked -= HandleQuit;
            }
            
            GameEventSystem.Unsubscribe("OnGamePaused", ShowPauseMenu);
            GameEventSystem.Unsubscribe("OnGameResumed", HidePauseMenu);
        }

        private void ShowPauseMenu(object data = null)
        {
            model.SetPaused(true);
            view.Show();
        }

        private void HidePauseMenu(object data = null)
        {
            model.SetPaused(false);
            view.Hide();
        }

        private void HandleResume()
        {
            GameEventSystem.Publish("OnGameResumed");
        }

        private void HandleRestart()
        {
            // Trở về Normal timeScale trước khi chuyển Scene
            Time.timeScale = 1f;
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadSceneAsync("Gameplay");
            }
        }

        private void HandleQuit()
        {
            Time.timeScale = 1f;
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadSceneAsync("Home");
            }
        }
    }
}
