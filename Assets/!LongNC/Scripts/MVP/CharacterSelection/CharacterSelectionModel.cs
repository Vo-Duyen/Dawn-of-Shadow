using System;
using System.Collections.Generic;
using System.Linq;
using DawnOfShadow.Data;

namespace DawnOfShadow.MVP.CharacterSelection
{
    public class CharacterSelectionModel
    {
        private List<CharacterData> _allCharacters;
        private string _selectedCharacterId;

        public event Action<string> OnCharacterSelected;

        public CharacterSelectionModel(List<CharacterData> allCharacters, string initialSelectedId)
        {
            _allCharacters = allCharacters;
            _selectedCharacterId = initialSelectedId;
        }

        public string SelectedCharacterId
        {
            get => _selectedCharacterId;
            set
            {
                if (_selectedCharacterId != value)
                {
                    _selectedCharacterId = value;
                    OnCharacterSelected?.Invoke(_selectedCharacterId);
                }
            }
        }

        public CharacterData GetSelectedCharacter()
        {
            return _allCharacters.FirstOrDefault(c => c.characterId == _selectedCharacterId);
        }

        public List<CharacterData> GetAllCharacters()
        {
            return _allCharacters;
        }
    }
}
