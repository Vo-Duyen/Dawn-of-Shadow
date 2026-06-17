using System;
using System.Collections.Generic;

namespace DawnOfShadow.Data
{
    [Serializable]
    public class GameData
    {
        public int gold = 1000; // Starter gold
        public int totalMonstersDefeated = 0;
        public string selectedCharacterId = "hero_01";
        
        // List of unlocked characters
        public List<string> unlockedCharacterIds = new List<string> { "hero_01" };

        // Character Stat Upgrades (Max 10 levels each)
        public int hpUpgradeLevel = 0;
        public int defUpgradeLevel = 0;
        public int manaUpgradeLevel = 0;

        // Continue game progress state
        public bool hasInProgressGame = false;
        public int currentStage = 1;
        public int currentSubLevel = 1;
        public string currentDifficulty = "Medium";

        // Settings
        public float musicVolume = 0.75f;
        public float sfxVolume = 0.75f;
        public float lastMusicVolume = 0.75f;
        public float lastSfxVolume = 0.75f;
        public int graphicQuality = 2; // 0: Low, 1: Medium, 2: High

        // Heart system
        public int hearts = 5;
        public long lastHeartRegenTimeTicks = 0;
    }
}
