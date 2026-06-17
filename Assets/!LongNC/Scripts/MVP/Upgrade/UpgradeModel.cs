using System;

namespace DawnOfShadow.MVP.Upgrade
{
    public class UpgradeModel
    {
        public const int MAX_UPGRADE_LEVEL = 10;

        // Base costs
        public const int HP_BASE_COST = 100;
        public const int DEF_BASE_COST = 150;
        public const int MANA_BASE_COST = 120;

        // Bonuses per upgrade level
        public const int HP_BONUS_PER_LEVEL = 15;
        public const int DEF_BONUS_PER_LEVEL = 3;
        public const int MANA_BONUS_PER_LEVEL = 10;

        public int GetUpgradeCost(string statType, int currentLevel)
        {
            if (currentLevel >= MAX_UPGRADE_LEVEL) return -1; // Maxed out

            int baseCost = HP_BASE_COST;
            if (statType == "DEF") baseCost = DEF_BASE_COST;
            if (statType == "MANA") baseCost = MANA_BASE_COST;

            // Price increases: base cost * (current level + 1)
            return baseCost * (currentLevel + 1);
        }
    }
}
