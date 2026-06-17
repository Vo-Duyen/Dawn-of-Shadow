using UnityEngine;

namespace DawnOfShadow.Data
{
    [CreateAssetMenu(fileName = "NewCharacter", menuName = "Dawn of Shadow/Character Data")]
    public class CharacterData : ScriptableObject
    {
        public string characterId;
        public string characterName;
        [TextArea(3, 10)]
        public string story;
        public int hp;
        public int atk;
        public int def;
        public float speed;
        public int goldCost;
        public Sprite icon;
        public GameObject prefab;
        
        [Header("Lobby Display")]
        [Tooltip("Hệ số phóng to nhân vật khi hiển thị ở sảnh chờ")]
        public float lobbyDisplayScale = 1f;
        
        [Tooltip("Độ cao bù thêm (lên/xuống) khi hiển thị ở sảnh chờ")]
        public float lobbyHeightOffset = 0f;
    }
}
