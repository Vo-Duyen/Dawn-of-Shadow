using UnityEngine;
using DawnOfShadow.Data;

namespace DawnOfShadow.Core
{
    public class SaveManager : SingletonBase<SaveManager>
    {
        private const string SAVE_KEY = "DawnOfShadow_SaveData";
        private GameData _gameData;

        public GameData Data
        {
            get
            {
                if (_gameData == null)
                {
                    Load();
                }
                return _gameData;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            Load();
        }

        public void Save()
        {
            string json = JsonUtility.ToJson(Data);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
        }

        public void Load()
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                string json = PlayerPrefs.GetString(SAVE_KEY);
                try
                {
                    _gameData = JsonUtility.FromJson<GameData>(json);
                }
                catch
                {
                    _gameData = new GameData();
                }
            }
            else
            {
                _gameData = new GameData();
            }
        }

        public void ResetData()
        {
            _gameData = new GameData();
            Save();
        }
    }
}
