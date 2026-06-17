using System.Collections.Generic;
using DawnOfShadow.Data;
using DawnOfShadow.Core;

namespace DawnOfShadow.MVP.CharacterSelection
{
    public class CharacterSelectionPresenter
    {
        private readonly CharacterSelectionModel _model;
        private readonly CharacterSelectionView _view;
        private readonly List<CharacterData> _characters;
        private int _currentIndex;

        public CharacterSelectionPresenter(CharacterSelectionModel model, CharacterSelectionView view)
        {
            _model = model;
            _view = view;
            _characters = _model.GetAllCharacters();
            
            // Find current index
            _currentIndex = _characters.FindIndex(c => c.characterId == _model.SelectedCharacterId);
            if (_currentIndex < 0) _currentIndex = 0;

            // Bind events
            _view.OnNextClicked += OnNextCharacter;
            _view.OnPrevClicked += OnPrevCharacter;
            _view.OnActionClicked += OnActionTriggered;

            UpdateView();
        }

        private void OnNextCharacter()
        {
            _currentIndex = (_currentIndex + 1) % _characters.Count;
            UpdateView();
        }

        private void OnPrevCharacter()
        {
            _currentIndex = (_currentIndex - 1 + _characters.Count) % _characters.Count;
            UpdateView();
        }

        private void OnActionTriggered()
        {
            var charData = _characters[_currentIndex];
            var save = SaveManager.Instance.Data;

            bool isUnlocked = save.unlockedCharacterIds.Contains(charData.characterId);

            if (isUnlocked)
            {
                // Select character
                _model.SelectedCharacterId = charData.characterId;
                save.selectedCharacterId = charData.characterId;
                SaveManager.Instance.Save();
                UpdateView();
            }
            else
            {
                // Try to unlock
                if (save.gold >= charData.goldCost)
                {
                    save.gold -= charData.goldCost;
                    save.unlockedCharacterIds.Add(charData.characterId);
                    
                    // Auto select after unlock
                    _model.SelectedCharacterId = charData.characterId;
                    save.selectedCharacterId = charData.characterId;
                    
                    SaveManager.Instance.Save();
                    UpdateView();
                }
                else
                {
                    // Insufficient funds feedback (could trigger UI warning)
                    UnityEngine.Debug.LogWarning("Not enough gold to unlock character!");
                }
            }
        }

        private void UpdateView()
        {
            if (_characters.Count == 0) return;

            var charData = _characters[_currentIndex];
            var save = SaveManager.Instance.Data;

            bool isUnlocked = save.unlockedCharacterIds.Contains(charData.characterId);
            bool isSelected = _model.SelectedCharacterId == charData.characterId;

            _view.DisplayCharacter(charData, isUnlocked, isSelected);
        }
    }
}
