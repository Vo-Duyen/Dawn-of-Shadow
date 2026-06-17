using UnityEngine;
using DawnOfShadow.Gameplay.Enemy;

namespace DawnOfShadow.UI
{
    public class UIObjectiveIndicator : MonoBehaviour
    {
        [Header("UI Reference")]
        [SerializeField] private RectTransform indicatorArrow;
        [SerializeField] private float screenPadding = 40f;

        private Transform _target;
        private Camera _mainCamera;
        private RectTransform _canvasRect;

        private void Start()
        {
            _mainCamera = Camera.main;
            
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                _canvasRect = canvas.GetComponent<RectTransform>();
            }

            FindBossTarget();
        }

        private void Update()
        {
            if (_target == null)
            {
                FindBossTarget();
                if (_target == null)
                {
                    if (indicatorArrow != null) indicatorArrow.gameObject.SetActive(false);
                    return;
                }
            }

            if (_mainCamera == null || _canvasRect == null) return;

            Vector3 screenPos = _mainCamera.WorldToScreenPoint(_target.position);
            
            // Check if off screen or behind camera
            bool isOffScreen = screenPos.x <= screenPadding || screenPos.x >= Screen.width - screenPadding ||
                               screenPos.y <= screenPadding || screenPos.y >= Screen.height - screenPadding ||
                               screenPos.z < 0;

            if (isOffScreen)
            {
                if (indicatorArrow != null)
                {
                    indicatorArrow.gameObject.SetActive(true);
                    
                    Vector3 cappedPos = screenPos;
                    if (cappedPos.z < 0) cappedPos *= -1; // Invert behind camera

                    Vector2 localPoint;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, cappedPos, null, out localPoint);

                    float limitX = (_canvasRect.sizeDelta.x / 2) - screenPadding;
                    float limitY = (_canvasRect.sizeDelta.y / 2) - screenPadding;

                    localPoint.x = Mathf.Clamp(localPoint.x, -limitX, limitX);
                    localPoint.y = Mathf.Clamp(localPoint.y, -limitY, limitY);

                    indicatorArrow.anchoredPosition = localPoint;

                    // Rotate arrow
                    Vector2 centerScreen = new Vector2(Screen.width / 2, Screen.height / 2);
                    Vector2 dir = ((Vector2)cappedPos - centerScreen).normalized;
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    indicatorArrow.localRotation = Quaternion.Euler(0, 0, angle - 90f);
                }
            }
            else
            {
                if (indicatorArrow != null) indicatorArrow.gameObject.SetActive(false);
            }
        }

        private void FindBossTarget()
        {
            var boss = FindObjectOfType<BossController>();
            if (boss != null)
            {
                _target = boss.transform;
            }
        }
    }
}
