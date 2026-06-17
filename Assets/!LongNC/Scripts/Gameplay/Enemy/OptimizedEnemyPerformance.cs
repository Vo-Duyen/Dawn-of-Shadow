using UnityEngine;
using UnityEngine.AI;

namespace DawnOfShadow.Gameplay.Enemy
{
    [RequireComponent(typeof(EnemyBase))]
    public class OptimizedEnemyPerformance : MonoBehaviour
    {
        [Header("Optimization Settings")]
        [SerializeField] private float cullingDistance = 25f;
        [SerializeField] private float updateInterval = 0.5f; // Check distance only twice a second

        private EnemyBase _enemyBase;
        private NavMeshAgent _navAgent;
        private Transform _playerTransform;
        private float _nextCheckTime;
        private bool _isCulled = false;

        private void Start()
        {
            _enemyBase = GetComponent<EnemyBase>();
            _navAgent = GetComponent<NavMeshAgent>();
            FindPlayer();
        }

        private void Update()
        {
            if (Time.time >= _nextCheckTime)
            {
                _nextCheckTime = Time.time + updateInterval;
                CheckCulling();
            }
        }

        private void FindPlayer()
        {
            if (_playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    _playerTransform = player.transform;
                }
            }
        }

        private void CheckCulling()
        {
            FindPlayer();

            if (_playerTransform == null) return;

            float distance = Vector3.Distance(transform.position, _playerTransform.position);

            if (distance > cullingDistance)
            {
                if (!_isCulled)
                {
                    CullEnemy(true);
                }
            }
            else
            {
                if (_isCulled)
                {
                    CullEnemy(false);
                }
            }
        }

        private void CullEnemy(bool cull)
        {
            _isCulled = cull;

            // Enable/disable AI logic script
            if (_enemyBase != null)
            {
                _enemyBase.enabled = !cull;
            }

            // Enable/disable pathfinding agent
            if (_navAgent != null)
            {
                _navAgent.enabled = !cull;
            }

            // Optional: Disable animators, colliders, or trigger floating indicators if needed
        }
    }
}
