using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using DawnOfShadow.Data;
using DawnOfShadow.Gameplay.Player;

namespace DawnOfShadow.MVP.CharacterSelection
{
    public class CharacterSelectionView : MonoBehaviour
    {
        [Header("UI Text & Information")]
        [SerializeField] private TextMeshProUGUI characterNameText;
        [SerializeField] private TextMeshProUGUI storyText;

        [Header("Stats Visualizers")]
        [SerializeField] private Slider hpSlider;
        [SerializeField] private Slider atkSlider;
        [SerializeField] private Slider defSlider;
        [SerializeField] private Slider speedSlider;

        [Header("Interactive Buttons")]
        [SerializeField] private Button nextButton;
        [SerializeField] private Button prevButton;
        [SerializeField] private Button actionButton; // "Select" or "Unlock"
        [SerializeField] private TextMeshProUGUI actionButtonText;

        [Header("Character Render Spot")]
        [SerializeField] private Transform modelSpawnPoint;

        public event Action OnNextClicked;
        public event Action OnPrevClicked;
        public event Action OnActionClicked;

        private GameObject currentSpawnedModel;

        private void Awake()
        {
            nextButton.onClick.AddListener(() => OnNextClicked?.Invoke());
            prevButton.onClick.AddListener(() => OnPrevClicked?.Invoke());
            actionButton.onClick.AddListener(() => OnActionClicked?.Invoke());
        }

        public void DisplayCharacter(CharacterData data, bool isUnlocked, bool isSelected)
        {
            characterNameText.text = data.characterName;
            storyText.text = data.story;

            // Animate sliders smoothly using DOTween
            hpSlider.DOValue(data.hp, 0.4f).SetEase(Ease.OutQuad);
            atkSlider.DOValue(data.atk, 0.4f).SetEase(Ease.OutQuad);
            defSlider.DOValue(data.def, 0.4f).SetEase(Ease.OutQuad);
            speedSlider.DOValue(data.speed, 0.4f).SetEase(Ease.OutQuad);

            // Update action button text/state
            if (isSelected)
            {
                actionButtonText.text = "Selected";
                actionButton.interactable = false;
            }
            else if (isUnlocked)
            {
                actionButtonText.text = "Select";
                actionButton.interactable = true;
            }
            else
            {
                actionButtonText.text = $"Unlock ({data.goldCost} G)";
                actionButton.interactable = true;
            }

            // Spawn Character 3D Prefab
            if (currentSpawnedModel != null)
            {
                Destroy(currentSpawnedModel);
            }

            if (data.prefab != null && modelSpawnPoint != null)
            {
                currentSpawnedModel = Instantiate(data.prefab, modelSpawnPoint);
                
                // Tắt Rigidbody để tránh rơi tự do do trọng lực và các tương tác vật lý khác
                Rigidbody rb = currentSpawnedModel.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }

                // Tắt các Collider để tránh va chạm vật lý không đáng có ở Lobby
                Collider[] colliders = currentSpawnedModel.GetComponentsInChildren<Collider>();
                foreach (Collider col in colliders)
                {
                    col.enabled = false;
                }

                // Tắt script điều khiển PlayerController để tránh việc nhân vật phản hồi với phím di chuyển (WASD/Arrow) ở sảnh chính
                PlayerController playerCtrl = currentSpawnedModel.GetComponent<PlayerController>();
                if (playerCtrl != null)
                {
                    playerCtrl.enabled = false;
                }

                // Áp dụng độ cao bù từ CharacterData
                currentSpawnedModel.transform.localPosition = new Vector3(0, data.lobbyHeightOffset, 0);
                currentSpawnedModel.transform.localRotation = Quaternion.Euler(0, 180f, 0); // Quay mặt 180 độ
                
                // Fancy entrance scale animation with DOTween
                float scale = data.lobbyDisplayScale;
                Vector3 targetScale = new Vector3(scale, scale, scale);
                currentSpawnedModel.transform.localScale = Vector3.zero;
                currentSpawnedModel.transform.DOScale(targetScale, 0.5f).SetEase(Ease.OutBack);
            }
        }
    }
}
