using UnityEngine;

namespace DawnOfShadow.UI
{
    public class CameraTurntable : MonoBehaviour
    {
        [Header("Target To Rotate Around")]
        [Tooltip("Nhân vật 3D mà Camera sẽ xoay quanh")]
        public Transform target;
        
        [Header("Rotation Settings")]
        [Tooltip("Tốc độ xoay của Camera")]
        public float rotationSpeed = 10f;
        
        [Tooltip("Độ cao bù thêm để Camera nhìn thẳng vào ngực/mặt nhân vật thay vì dưới chân")]
        public float lookAtHeightOffset = 1f;

        private void Update()
        {
            if (target != null)
            {
                // Xoay Camera quanh trục Y của nhân vật mục tiêu
                transform.RotateAround(target.position, Vector3.up, rotationSpeed * Time.deltaTime);
                
                // Đảm bảo Camera luôn nhìn vào vị trí mục tiêu + độ cao bù
                transform.LookAt(target.position + Vector3.up * lookAtHeightOffset); 
            }
        }
    }
}
