using DawnOfShadow.Core;
using System;

namespace DawnOfShadow.MVP.Upgrade
{
    public class UpgradePresenter
    {
        private readonly UpgradeModel _model;
        private readonly UpgradeView _view;
        
        public event Action OnGoldChanged; // Fires to notify HomeMenuPresenter to reload gold display

        public UpgradePresenter(UpgradeModel model, UpgradeView view)
        {
            _model = model;
            _view = view;

            _view.OnUpgradeHpRequested += UpgradeHP;
            _view.OnUpgradeDefRequested += UpgradeDEF;
            _view.OnUpgradeManaRequested += UpgradeMana;

            RefreshUI();
        }

        private void UpgradeHP()
        {
            var save = SaveManager.Instance.Data;
            int cost = _model.GetUpgradeCost("HP", save.hpUpgradeLevel);

            if (cost > 0 && save.gold >= cost && save.hpUpgradeLevel < UpgradeModel.MAX_UPGRADE_LEVEL)
            {
                save.gold -= cost;
                save.hpUpgradeLevel++;
                SaveManager.Instance.Save();

                OnGoldChanged?.Invoke();
                RefreshUI();
            }
        }

        private void UpgradeDEF()
        {
            var save = SaveManager.Instance.Data;
            int cost = _model.GetUpgradeCost("DEF", save.defUpgradeLevel);

            if (cost > 0 && save.gold >= cost && save.defUpgradeLevel < UpgradeModel.MAX_UPGRADE_LEVEL)
            {
                save.gold -= cost;
                save.defUpgradeLevel++;
                SaveManager.Instance.Save();

                OnGoldChanged?.Invoke();
                RefreshUI();
            }
        }

        private void UpgradeMana()
        {
            var save = SaveManager.Instance.Data;
            int cost = _model.GetUpgradeCost("MANA", save.manaUpgradeLevel);

            if (cost > 0 && save.gold >= cost && save.manaUpgradeLevel < UpgradeModel.MAX_UPGRADE_LEVEL)
            {
                save.gold -= cost;
                save.manaUpgradeLevel++;
                SaveManager.Instance.Save();

                OnGoldChanged?.Invoke();
                RefreshUI();
            }
        }

        public void RefreshUI()
        {
            var save = SaveManager.Instance.Data;

            int hpCost = _model.GetUpgradeCost("HP", save.hpUpgradeLevel);
            int defCost = _model.GetUpgradeCost("DEF", save.defUpgradeLevel);
            int manaCost = _model.GetUpgradeCost("MANA", save.manaUpgradeLevel);

            _view.DisplayStats(
                save.hpUpgradeLevel, hpCost,
                save.defUpgradeLevel, defCost,
                save.manaUpgradeLevel, manaCost,
                UpgradeModel.MAX_UPGRADE_LEVEL
            );
        }
    }
}
