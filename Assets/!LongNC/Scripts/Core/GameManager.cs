using UnityEngine;

namespace DawnOfShadow.Core
{
    public enum GameState
    {
        Lobby,
        Loading,
        Playing,
        Paused,
        Victory,
        Defeat
    }

    public class GameManager : SingletonBase<GameManager>
    {
        public GameState CurrentState { get; private set; }

        private void Start()
        {
            // Subscribe to relevant events
            GameEventSystem.Subscribe("OnGameWon", HandleGameWon);
            GameEventSystem.Subscribe("OnPlayerDied", HandleGameLost);
            GameEventSystem.Subscribe("OnGamePaused", HandleGamePaused);
            GameEventSystem.Subscribe("OnGameResumed", HandleGameResumed);
        }

        private void OnDestroy()
        {
            GameEventSystem.Unsubscribe("OnGameWon", HandleGameWon);
            GameEventSystem.Unsubscribe("OnPlayerDied", HandleGameLost);
            GameEventSystem.Unsubscribe("OnGamePaused", HandleGamePaused);
            GameEventSystem.Unsubscribe("OnGameResumed", HandleGameResumed);
        }

        public void ChangeState(GameState newState)
        {
            CurrentState = newState;
            GameEventSystem.Publish("OnGameStateChanged", newState);
        }

        private void HandleGameWon(object data = null)
        {
            ChangeState(GameState.Victory);
        }

        private void HandleGameLost(object data = null)
        {
            ChangeState(GameState.Defeat);
        }

        private void HandleGamePaused(object data = null)
        {
            if (CurrentState == GameState.Playing)
            {
                ChangeState(GameState.Paused);
                Time.timeScale = 0f;
            }
        }

        private void HandleGameResumed(object data = null)
        {
            if (CurrentState == GameState.Paused)
            {
                ChangeState(GameState.Playing);
                Time.timeScale = 1f;
            }
        }
    }
}
