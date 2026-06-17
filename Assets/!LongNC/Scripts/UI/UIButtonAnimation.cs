using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace DawnOfShadow.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class UIButtonAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Scale Settings")]
        [SerializeField] private float pressScale = 0.92f;
        [SerializeField] private float hoverScale = 1.04f;
        [SerializeField] private float animDuration = 0.15f;
        [SerializeField] private Ease easeType = Ease.OutQuad;

        private Vector3 _originalScale;
        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _originalScale = _rectTransform.localScale;
        }

        private void OnDisable()
        {
            // Reset to original scale when disabled to prevent sticking at small size
            if (_rectTransform != null)
            {
                _rectTransform.localScale = _originalScale;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _rectTransform.DOScale(_originalScale * pressScale, animDuration)
                .SetEase(easeType)
                .SetUpdate(true); // Works even if Time.timeScale is 0 (paused)
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _rectTransform.DOScale(_originalScale, animDuration)
                .SetEase(easeType)
                .SetUpdate(true);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _rectTransform.DOScale(_originalScale * hoverScale, animDuration)
                .SetEase(easeType)
                .SetUpdate(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _rectTransform.DOScale(_originalScale, animDuration)
                .SetEase(easeType)
                .SetUpdate(true);
        }
    }
}
