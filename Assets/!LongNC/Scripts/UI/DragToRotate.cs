using UnityEngine;
using UnityEngine.EventSystems;

namespace DawnOfShadow.UI
{
    public class DragToRotate : MonoBehaviour, IDragHandler
    {
        [Header("Target To Rotate")]
        [Tooltip("Transform sẽ bị xoay (Khuyên dùng: kéo Model Spawn Point vào đây)")]
        public Transform targetTransform;
        
        [Header("Rotation Settings")]
        [Tooltip("Tốc độ xoay (để số âm nếu muốn xoay thuận chiều vuốt tay)")]
        public float rotationSpeed = -0.5f;

        public void OnDrag(PointerEventData eventData)
        {
            if (targetTransform != null)
            {
                // Chỉ xoay quanh trục Y dựa trên khoảng cách kéo thả chuột/ngón tay theo chiều ngang (X)
                float rotX = eventData.delta.x * rotationSpeed;
                targetTransform.Rotate(Vector3.up, rotX, Space.World);
            }
        }
    }
}
