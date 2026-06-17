using UnityEngine;

namespace DawnOfShadow.Gameplay.Items
{
    public class ItemMagnet : MonoBehaviour
    {
        [Header("Magnet Configuration")]
        [SerializeField] private float pullRadius = 4f;
        [SerializeField] private float initialPullSpeed = 2f;
        [SerializeField] private float acceleration = 12f;

        private Transform _playerTransform;
        private float _currentSpeed;
        private bool _isBeingPulled = false;

        private void Start()
        {
            _currentSpeed = initialPullSpeed;
        }

        private void Update()
        {
            if (_playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    _playerTransform = player.transform;
                }
                return;
            }

            float distance = Vector3.Distance(transform.position, _playerTransform.position);

            if (_isBeingPulled)
            {
                // Accelerate pulling speed towards the player
                _currentSpeed += acceleration * Time.deltaTime;
                Vector3 targetPos = _playerTransform.position + Vector3.up * 0.5f; // Pull towards character chest/center
                transform.position = Vector3.MoveTowards(transform.position, targetPos, _currentSpeed * Time.deltaTime);
            }
            else if (distance <= pullRadius)
            {
                _isBeingPulled = true;
            }
        }
    }
}
