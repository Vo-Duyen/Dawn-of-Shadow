using UnityEngine;
using UnityEngine.EventSystems;

namespace DawnOfShadow.Gameplay.Input
{
    public class JoystickController : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform container;
        [SerializeField] private RectTransform handle;
        [SerializeField] private float dragRange = 100f;

        [Header("Highlights (4 Corners of a Square)")]
        [SerializeField] private GameObject topLeftHighlight;
        [SerializeField] private GameObject topRightHighlight;
        [SerializeField] private GameObject bottomLeftHighlight;
        [SerializeField] private GameObject bottomRightHighlight;
        [SerializeField] private float highlightThreshold = 0.2f;

        private Vector2 _inputVector = Vector2.zero;

        public Vector2 Direction => _inputVector;
        public float Horizontal => _inputVector.x;
        public float Vertical => _inputVector.y;

        private void Start()
        {
            UpdateHighlights();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(container, eventData.position, eventData.pressEventCamera, out Vector2 position))
            {
                // Constrain handle movement within dragRange
                position.x = (position.x / container.sizeDelta.x) * 2;
                position.y = (position.y / container.sizeDelta.y) * 2;

                _inputVector = new Vector2(position.x, position.y);
                _inputVector = (_inputVector.magnitude > 1.0f) ? _inputVector.normalized : _inputVector;

                handle.anchoredPosition = new Vector2(_inputVector.x * (container.sizeDelta.x / 2) * (dragRange / 100f), _inputVector.y * (container.sizeDelta.y / 2) * (dragRange / 100f));
                
                UpdateHighlights();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _inputVector = Vector2.zero;
            handle.anchoredPosition = Vector2.zero;
            UpdateHighlights();
        }

        private void UpdateHighlights()
        {
            bool hasInput = _inputVector.magnitude > highlightThreshold;

            bool isTopLeft = hasInput && _inputVector.x < 0f && _inputVector.y >= 0f;
            bool isTopRight = hasInput && _inputVector.x >= 0f && _inputVector.y >= 0f;
            bool isBottomLeft = hasInput && _inputVector.x < 0f && _inputVector.y < 0f;
            bool isBottomRight = hasInput && _inputVector.x >= 0f && _inputVector.y < 0f;

            if (topLeftHighlight != null) topLeftHighlight.SetActive(isTopLeft);
            if (topRightHighlight != null) topRightHighlight.SetActive(isTopRight);
            if (bottomLeftHighlight != null) bottomLeftHighlight.SetActive(isBottomLeft);
            if (bottomRightHighlight != null) bottomRightHighlight.SetActive(isBottomRight);
        }
    }
}
