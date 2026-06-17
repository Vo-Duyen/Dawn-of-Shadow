using UnityEngine;
using System;
using DawnOfShadow.Core;
using DawnOfShadow.Data;

namespace DawnOfShadow.Gameplay.Level
{
    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }

    public class LevelManager : SingletonBase<LevelManager>
    {
        public event Action OnLevelCompleted;
        public event Action OnGameWon;

        private int _currentStage = 1;
        private int _currentSubLevel = 1;
        private Difficulty _difficulty = Difficulty.Medium;

        public int CurrentStage => _currentStage;
        public int CurrentSubLevel => _currentSubLevel;
        public Difficulty CurrentDifficulty => _difficulty;

        protected override void Awake()
        {
            base.Awake();
            LoadProgress();
        }

        public void LoadProgress()
        {
            var save = SaveManager.Instance.Data;
            _currentStage = save.currentStage;
            _currentSubLevel = save.currentSubLevel;
            
            if (Enum.TryParse(save.currentDifficulty, out Difficulty diff))
            {
                _difficulty = diff;
            }
        }

        public void SetDifficulty(Difficulty difficulty)
        {
            _difficulty = difficulty;
            SaveManager.Instance.Data.currentDifficulty = difficulty.ToString();
            SaveManager.Instance.Save();
        }

        public void CompleteCurrentLevel()
        {
            Debug.Log($"Completed level {_currentStage}.{_currentSubLevel}!");

            if (_currentSubLevel < 5)
            {
                // Advance to next sub-level
                _currentSubLevel++;
                SaveManager.Instance.Data.currentSubLevel = _currentSubLevel;
                SaveManager.Instance.Data.hasInProgressGame = true;
                SaveManager.Instance.Save();
                
                OnLevelCompleted?.Invoke();
            }
            else
            {
                // Finished the stage (sub-level 5 contains the boss)
                if (_currentStage < 5)
                {
                    _currentStage++;
                    _currentSubLevel = 1;
                    
                    SaveManager.Instance.Data.currentStage = _currentStage;
                    SaveManager.Instance.Data.currentSubLevel = _currentSubLevel;
                    SaveManager.Instance.Data.hasInProgressGame = true;
                    SaveManager.Instance.Save();

                    OnLevelCompleted?.Invoke();
                }
                else
                {
                    // Finished Stage 5 - Win the entire game!
                    SaveManager.Instance.Data.hasInProgressGame = false; // Reset progress
                    SaveManager.Instance.Data.currentStage = 1;
                    SaveManager.Instance.Data.currentSubLevel = 1;
                    SaveManager.Instance.Save();

                    OnGameWon?.Invoke();
                    Debug.Log("Congratulations! You have completed all stages and saved the Dawn of Shadow!");
                }
            }
        }

        public float GetEnemyStatMultiplier()
        {
            switch (_difficulty)
            {
                case Difficulty.Easy:
                    return 0.7f;
                case Difficulty.Hard:
                    return 1.5f;
                case Difficulty.Medium:
                default:
                    return 1.0f;
            }
        }
    }
}
