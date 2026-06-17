using UnityEngine;
using DawnOfShadow.Data;
using DawnOfShadow.Core;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DawnOfShadow.Editor
{
    [CreateAssetMenu(fileName = "GameDataEditorTool", menuName = "Dawn of Shadow/Game Data Editor Tool")]
    public class GameDataEditorTool : ScriptableObject
    {
        [Header("Save Data Fields")]
        public int gold;
        public int hearts;
        public string selectedCharacterId;
        public List<string> unlockedCharacterIds = new List<string>();
        
        [Header("Upgrade Levels")]
        public int hpUpgradeLevel;
        public int defUpgradeLevel;
        public int manaUpgradeLevel;

        [Header("Game Progress")]
        public bool hasInProgressGame;
        public int currentStage;
        public int currentSubLevel;
        public string currentDifficulty;

        [Header("Settings")]
        public float musicVolume;
        public float sfxVolume;
        public int graphicQuality;

        // Load data from PlayerPrefs (using SaveManager)
        public void LoadData()
        {
            // Force SaveManager to reload from PlayerPrefs
            SaveManager.Instance.Load();
            var data = SaveManager.Instance.Data;

            gold = data.gold;
            hearts = data.hearts;
            selectedCharacterId = data.selectedCharacterId;
            unlockedCharacterIds = new List<string>(data.unlockedCharacterIds);
            hpUpgradeLevel = data.hpUpgradeLevel;
            defUpgradeLevel = data.defUpgradeLevel;
            manaUpgradeLevel = data.manaUpgradeLevel;
            hasInProgressGame = data.hasInProgressGame;
            currentStage = data.currentStage;
            currentSubLevel = data.currentSubLevel;
            currentDifficulty = data.currentDifficulty;
            musicVolume = data.musicVolume;
            sfxVolume = data.sfxVolume;
            graphicQuality = data.graphicQuality;

            Debug.Log("Game save data loaded successfully into Editor Tool.");
        }

        // Save data back to PlayerPrefs (using SaveManager)
        public void SaveData()
        {
            var data = SaveManager.Instance.Data;

            data.gold = gold;
            data.hearts = hearts;
            data.selectedCharacterId = selectedCharacterId;
            data.unlockedCharacterIds = new List<string>(unlockedCharacterIds);
            data.hpUpgradeLevel = hpUpgradeLevel;
            data.defUpgradeLevel = defUpgradeLevel;
            data.manaUpgradeLevel = manaUpgradeLevel;
            data.hasInProgressGame = hasInProgressGame;
            data.currentStage = currentStage;
            data.currentSubLevel = currentSubLevel;
            data.currentDifficulty = currentDifficulty;
            data.musicVolume = musicVolume;
            data.sfxVolume = sfxVolume;
            data.graphicQuality = graphicQuality;

            SaveManager.Instance.Save();
            Debug.Log("Game save data written and saved successfully from Editor Tool.");
        }

        // Reset/Clear all data
        public void ClearData()
        {
            SaveManager.Instance.ResetData();
            LoadData(); // Reload defaults
            Debug.Log("Game save data reset and cleared successfully.");
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(GameDataEditorTool))]
    public class GameDataEditorToolEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw default fields so we can edit them
            DrawDefaultInspector();

            GameDataEditorTool tool = (GameDataEditorTool)target;

            GUILayout.Space(20);
            GUILayout.Label("Actions", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Load Data", GUILayout.Height(30)))
            {
                tool.LoadData();
                // Mark ScriptableObject as dirty so the editor refreshes
                EditorUtility.SetDirty(tool);
            }

            if (GUILayout.Button("Save Data", GUILayout.Height(30)))
            {
                tool.SaveData();
            }

            if (GUILayout.Button("Clear Data", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Clear Data", "Are you sure you want to clear/reset all save data?", "Yes", "No"))
                {
                    tool.ClearData();
                    EditorUtility.SetDirty(tool);
                }
            }

            GUILayout.EndHorizontal();
        }
    }
#endif
}
