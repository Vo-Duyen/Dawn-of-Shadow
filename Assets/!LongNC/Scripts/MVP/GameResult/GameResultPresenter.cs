using UnityEngine;
using DawnOfShadow.Core;

namespace DawnOfShadow.MVP.GameResult
{
    public class GameResultPresenter : MonoBehaviour
    {
        [SerializeField] private GameResultView view;
        private GameResultModel model;

        private void Awake()
        {
            model = new GameResultModel();
            
            if (view != null)
            {
                view.OnNextLevelClicked += HandleNextLevel;
                view.OnRestartClicked += HandleRestart;
                view.OnHomeClicked += HandleHome;
            }
        }

        private void Start()
        {
            GameEventSystem.Subscribe("OnGameWon", HandleVictory);
            GameEventSystem.Subscribe("OnPlayerDied", HandleDefeat);
            
            view.Hide();
        }

        private void OnDestroy()
        {
            if (view != null)
            {
                view.OnNextLevelClicked -= HandleNextLevel;
                view.OnRestartClicked -= HandleRestart;
                view.OnHomeClicked -= HandleHome;
            }
            
            GameEventSystem.Unsubscribe("OnGameWon", HandleVictory);
            GameEventSystem.Unsubscribe("OnPlayerDied", HandleDefeat);
        }

        private void HandleVictory(object data = null)
        {
            model.SetResult(true);
            model.ProcessRewards();
            view.ShowVictory();
            Time.timeScale = 0f; // Tạm dừng game khi hiện bảng kết quả
        }

        private void HandleDefeat(object data = null)
        {
            model.SetResult(false);
            view.ShowDefeat();
            Time.timeScale = 0f; // Tạm dừng game khi hiện bảng kết quả
        }

        private void HandleNextLevel()
        {
            Time.timeScale = 1f;
            // TODO: Thông báo cho LevelManager chuyển màn, hoặc load Scene kế tiếp
            // GameEventSystem.Publish("LoadNextLevel");
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadSceneAsync("Gameplay");
            }
        }

        private void HandleRestart()
        {
            Time.timeScale = 1f;
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadSceneAsync("Gameplay");
            }
        }

        private void HandleHome()
        {
            Time.timeScale = 1f;
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadSceneAsync("Home");
            }
        }
    }
}
