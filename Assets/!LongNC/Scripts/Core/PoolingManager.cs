using System.Collections.Generic;
using UnityEngine;

namespace DawnOfShadow.Core
{
    public class PoolingManager : SingletonBase<PoolingManager>
    {
        private Dictionary<GameObject, Queue<GameObject>> _pools = new Dictionary<GameObject, Queue<GameObject>>();
        private Dictionary<GameObject, GameObject> _instanceToPrefabMap = new Dictionary<GameObject, GameObject>();

        public GameObject SpawnFromPool(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return null;

            if (!_pools.ContainsKey(prefab))
            {
                _pools.Add(prefab, new Queue<GameObject>());
            }

            GameObject instance;
            if (_pools[prefab].Count > 0)
            {
                instance = _pools[prefab].Dequeue();
                
                // Cần thiết lập vị trí trước khi Active
                instance.transform.position = position;
                instance.transform.rotation = rotation;

                // Xử lý tạm ngắt NavMeshAgent để tránh lỗi trượt NavMesh khi tái kích hoạt từ Pool
                var agent = instance.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null)
                {
                    agent.enabled = false;
                }

                instance.SetActive(true);

                if (agent != null)
                {
                    // Đảm bảo NavMeshAgent được đặt lại vị trí đúng trên bản đồ trước khi kích hoạt lại
                    agent.enabled = true;
                }
            }
            else
            {
                instance = Instantiate(prefab, position, rotation);
                _instanceToPrefabMap.Add(instance, prefab);
            }

            return instance;
        }

        public void ReturnToPool(GameObject instance)
        {
            if (instance == null) return;

            if (_instanceToPrefabMap.ContainsKey(instance))
            {
                GameObject prefab = _instanceToPrefabMap[instance];
                instance.SetActive(false);
                
                // Reparent to manager to keep hierarchy clean
                instance.transform.SetParent(transform);

                if (!_pools[prefab].Contains(instance))
                {
                    _pools[prefab].Enqueue(instance);
                }
            }
            else
            {
                // Fallback: if not managed by pool, destroy
                Destroy(instance);
            }
        }
    }
}
