using DawnOfShadow.Gameplay.Player;
using DawnOfShadow.Gameplay.Level;
using DawnOfShadow.Core;
using UnityEngine;

namespace DawnOfShadow.MVP.GameplayHUD
{
    public class GameplayHUDPresenter
    {
        private readonly GameplayHUDView _view;
        private readonly PlayerController _player;
        private int _killedEnemiesCount = 0;

        public GameplayHUDPresenter(GameplayHUDView view, PlayerController player)
        {
            _view = view;
            _player = player;

            // Ensure game is not paused at start
            Time.timeScale = 1f;

            // Bind UI button events to player action methods
            _view.OnAttackClicked += _player.TriggerAttack;
            _view.OnDodgeClicked += _player.TriggerDodge;
            _view.OnSkill2Clicked += () => _player.TriggerSkill(1);
            _view.OnSkill3Clicked += () => _player.TriggerSkill(2);
            _view.OnHealClicked += _player.TriggerHeal;

            // Bind Pause, Defeat, Victory buttons
            _view.OnPauseClicked += HandlePause;
            _view.OnResumeClicked += HandleResume;
            _view.OnRestartClicked += HandleRestart;
            _view.OnQuitClicked += HandleQuit;

            // Initialize HP and Potion count
            _view.UpdateHP(_player.CurrentHp, _player.MaxHp);
            _view.UpdatePotionCount(_player.PotionCount);

            // Subscribe to player health changes & death
            _player.OnHealthChanged += _view.UpdateHP;
            _player.OnDeath += HandlePlayerDied;

            // Subscribe to LevelManager level completion / win
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelCompleted += HandleVictory;
                LevelManager.Instance.OnGameWon += HandleVictory;
            }

            // Subscribe to enemy killed event to track rewards
            GameEventSystem.Subscribe("OnEnemyKilled", HandleEnemyKilled);
        }

        private void HandlePause()
        {
            Time.timeScale = 0f;
            _view.TogglePausePanel(true);
        }

        private void HandleResume()
        {
            Time.timeScale = 1f;
            _view.TogglePausePanel(false);
        }

        private void HandleRestart()
        {
            Time.timeScale = 1f;
            SceneLoader.Instance.LoadSceneAsync("Gameplay", 0f);
        }

        private void HandleQuit()
        {
            Time.timeScale = 1f;
            SceneLoader.Instance.LoadSceneAsync("Home", 0f);
        }

        private void HandleEnemyKilled(object data)
        {
            _killedEnemiesCount++;
        }

        private void ProcessRewards()
        {
            int goldEarned = _killedEnemiesCount * 100;

            if (SaveManager.Instance != null && SaveManager.Instance.Data != null)
            {
                SaveManager.Instance.Data.gold += goldEarned;
                SaveManager.Instance.Data.totalMonstersDefeated += _killedEnemiesCount;
                SaveManager.Instance.Save();
                Debug.Log($"[GameResult] Earned {goldEarned} gold from {_killedEnemiesCount} kills. New Total Gold: {SaveManager.Instance.Data.gold}, Total Kills: {SaveManager.Instance.Data.totalMonstersDefeated}");
            }

            if (_view != null)
            {
                _view.SetGoldReward(goldEarned);
            }
        }

        private void HandlePlayerDied()
        {
            Time.timeScale = 0f;
            ProcessRewards();
            _view.ShowDefeatPanel();
        }

        private void HandleVictory()
        {
            Time.timeScale = 0f;
            ProcessRewards();
            _view.ShowVictoryPanel();
        }

        public void Tick()
        {
            if (_player == null || _view == null) return;

            // Update Cooldown overlay visual and numerical text
            _view.SetCooldown(0, _player.GetSkillCooldownProgress(0), _player.GetSkillCooldownRemaining(0));
            _view.SetCooldown(1, _player.GetSkillCooldownProgress(1), _player.GetSkillCooldownRemaining(1));
            _view.SetCooldown(2, _player.GetSkillCooldownProgress(2), _player.GetSkillCooldownRemaining(2));

            // Sync Potion Count
            _view.UpdatePotionCount(_player.PotionCount);
        }

        public void Destroy()
        {
            if (_player != null)
            {
                _player.OnHealthChanged -= _view.UpdateHP;
                _player.OnDeath -= HandlePlayerDied;
            }

            if (_view != null)
            {
                _view.OnPauseClicked -= HandlePause;
                _view.OnResumeClicked -= HandleResume;
                _view.OnRestartClicked -= HandleRestart;
                _view.OnQuitClicked -= HandleQuit;
            }

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelCompleted -= HandleVictory;
                LevelManager.Instance.OnGameWon -= HandleVictory;
            }

            GameEventSystem.Unsubscribe("OnEnemyKilled", HandleEnemyKilled);
        }
    }
}
