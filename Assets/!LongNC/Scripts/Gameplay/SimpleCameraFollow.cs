using UnityEngine;

namespace DawnOfShadow.Gameplay
{
    public class SimpleCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float smoothSpeed = 5f;
        [SerializeField] private float rotationSmoothSpeed = 5f;

        [Header("Follow Settings")]
        [SerializeField] private bool useFixedOffset = true;
        [SerializeField] private Vector3 fixedOffset = new Vector3(0f, 7f, -7f);

        [Header("Rotation Settings")]
        [SerializeField] private bool useFixedRotation = true;
        [SerializeField] private Vector3 fixedRotationAngles = new Vector3(45f, 0f, 0f);

        [Header("RPG Camera Settings (Legacy)")]
        [SerializeField] private float distance = 8f;            // Khoảng cách phía sau nhân vật
        [SerializeField] private float height = 4.5f;            // Chiều cao của camera
        [SerializeField] private float targetHeightOffset = 1.2f; // Điểm nhìn trên người nhân vật (1.2m ~ ngực/vai)

        [Header("Collision/Obstacle Settings")]
        [SerializeField] private bool detectCollisions = true;
        [SerializeField] private LayerMask collisionLayers = 1;   // Layer chứa tường (Mặc định là Layer 1: Default, nên chỉnh sang Layer cụ thể như Obstacle/Wall)
        [SerializeField] private float cameraRadius = 0.2f;
        [SerializeField] private float minDistanceToPlayer = 1.5f;

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 targetPosition;
            if (useFixedOffset)
            {
                // Tính toán vị trí mục tiêu dựa trên offset cố định, không xoay theo nhân vật
                targetPosition = target.position + fixedOffset;
            }
            else
            {
                // Tính toán vị trí mục tiêu ở phía sau nhân vật (cách tính cũ)
                targetPosition = target.position - target.forward * distance + Vector3.up * height;
            }

            // Điểm nhìn trên người nhân vật (để làm gốc bắn tia kiểm tra va chạm)
            Vector3 targetLookAt = target.position + Vector3.up * targetHeightOffset;

            // Xử lý va chạm với tường/vật cản nếu được kích hoạt
            if (detectCollisions)
            {
                Vector3 direction = targetPosition - targetLookAt;
                float maxDist = direction.magnitude;

                // Sử dụng SphereCastAll để lấy tất cả va chạm trên đường đi, tránh bị chặn bởi chính Player
                RaycastHit[] hits = Physics.SphereCastAll(targetLookAt, cameraRadius, direction.normalized, maxDist, collisionLayers);
                float closestValidDistance = maxDist;

                foreach (var hit in hits)
                {
                    // Bỏ qua nếu va chạm trúng chính target (Player) hoặc các đối tượng con của nó (vũ khí, effect...)
                    if (hit.transform == target || hit.transform.IsChildOf(target))
                        continue;

                    // Bỏ qua các trigger collider (vùng phát hiện, vùng nhặt đồ...)
                    if (hit.collider.isTrigger)
                        continue;

                    if (hit.distance < closestValidDistance)
                    {
                        closestValidDistance = hit.distance;
                    }
                }

                if (closestValidDistance < maxDist)
                {
                    // Lùi camera về điểm va chạm (trừ đi một khoảng đệm nhỏ để tránh nhìn xuyên qua góc tường)
                    float adjustedDistance = Mathf.Max(closestValidDistance - 0.1f, minDistanceToPlayer);
                    targetPosition = targetLookAt + direction.normalized * adjustedDistance;
                }
            }

            // Di chuyển camera mượt mà đến vị trí mục tiêu đã tính toán
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

            // Xoay camera mượt mà
            Quaternion targetRotation;
            if (useFixedRotation)
            {
                // Góc quay cố định theo cấu hình
                targetRotation = Quaternion.Euler(fixedRotationAngles);
            }
            else
            {
                // Tính toán góc quay hướng về phía nhân vật
                targetRotation = Quaternion.LookRotation(targetLookAt - transform.position);
            }

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothSpeed * Time.deltaTime);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}
