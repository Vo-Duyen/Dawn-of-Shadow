using UnityEngine;
using DG.Tweening;
using DawnOfShadow.Data;
using DawnOfShadow.Gameplay.Player;

namespace DawnOfShadow.Gameplay.Items
{
    public enum ItemType
    {
        StatBoostHp,
        StatBoostAtk,
        PotionRefill
    }

    public class CollectibleItem : MonoBehaviour
    {
        [SerializeField] private ItemType itemType;
        [SerializeField] private int boostAmount = 10;

        private void Start()
        {
            // Creative visual floating and rotating animation using DOTween
            transform.DORotate(new Vector3(0, 360, 0), 3f, RotateMode.FastBeyond360)
                .SetLoops(-1, LoopType.Incremental)
                .SetEase(Ease.Linear);

            transform.DOMoveY(transform.position.y + 0.3f, 1.2f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        private void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                ApplyEffect(player);
                
                // Explode out and scale down before destroy
                transform.DOScale(Vector3.zero, 0.2f).OnComplete(() => Destroy(gameObject));
            }
        }

        private void ApplyEffect(PlayerController player)
        {
            switch (itemType)
            {
                case ItemType.StatBoostHp:
                    player.Heal(boostAmount);
                    Debug.Log($"Boosted player HP by {boostAmount}!");
                    break;
                case ItemType.StatBoostAtk:
                    // Perform custom player ATK increase if desired
                    Debug.Log($"Temporary ATK buff applied (+{boostAmount})!");
                    break;
                case ItemType.PotionRefill:
                    // Increase potion count
                    player.Heal(100); // Massive heal
                    Debug.Log("Health potion refilled!");
                    break;
            }
        }
    }
}
