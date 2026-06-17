using UnityEngine;
using DawnOfShadow.Core;

namespace DawnOfShadow.Gameplay.Enemy
{
    public class EnemyProjectile : MonoBehaviour
    {
        [SerializeField] private float speed = 10f;
        [SerializeField] private float lifeTime = 3f;

        private Transform _target;
        private int _damage;
        private Vector3 _direction;
        private float _activeTimer;

        public void Initialize(Transform target, int damage)
        {
            _target = target;
            _damage = damage;
            _activeTimer = 0f;
            
            if (_target != null)
            {
                _direction = (_target.position - transform.position).normalized;
                transform.forward = _direction;
            }
            else
            {
                _direction = transform.forward;
            }
        }

        private void Update()
        {
            transform.Translate(_direction * speed * Time.deltaTime, Space.World);

            _activeTimer += Time.deltaTime;
            if (_activeTimer >= lifeTime)
            {
                PoolingManager.Instance.ReturnToPool(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                var playerEntity = other.GetComponent<EntityBase>();
                if (playerEntity != null)
                {
                    playerEntity.TakeDamage(_damage);
                }
                PoolingManager.Instance.ReturnToPool(gameObject);
            }
        }
    }
}
