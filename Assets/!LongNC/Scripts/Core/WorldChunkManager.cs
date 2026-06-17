using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DawnOfShadow.Gameplay.Enemy;
using DawnOfShadow.Gameplay.Level;

namespace DawnOfShadow.Core
{
    public class WorldChunkManager : SingletonBase<WorldChunkManager>
    {
        [System.Serializable]
        public struct ChunkData
        {
            public Vector2Int gridPosition;
            public GameObject chunkPrefab;
            public Vector3 playerSpawnOffset; // Vị trí spawn của Player tương đối với tâm chunk này
            public GameObject[] enemyPrefabs;
            public Vector3[] enemySpawnOffsets;
        }

        [Header("Grid Configuration")]
        [SerializeField] private float chunkSize = 40f;
        [SerializeField] private List<ChunkData> chunks = new List<ChunkData>();
        [SerializeField] private float activationRadius = 80f;

        // Trả về vị trí spawn của Player ở chunk đầu tiên để giữ tương thích ngược với GameplayBootstrapper
        public Vector3 PlayerSpawnPoint
        {
            get
            {
                if (chunks != null && chunks.Count > 0)
                {
                    var firstChunk = chunks[0];
                    Vector3 chunkCenter = new Vector3(firstChunk.gridPosition.x * chunkSize, 0f, firstChunk.gridPosition.y * chunkSize);
                    return chunkCenter + firstChunk.playerSpawnOffset;
                }
                return Vector3.zero;
            }
        }

        // Lấy vị trí spawn của Player cho một chunk cụ thể
        public Vector3 GetPlayerSpawnPoint(Vector2Int gridPos)
        {
            foreach (var chunk in chunks)
            {
                if (chunk.gridPosition == gridPos)
                {
                    Vector3 chunkCenter = new Vector3(chunk.gridPosition.x * chunkSize, 0f, chunk.gridPosition.y * chunkSize);
                    return chunkCenter + chunk.playerSpawnOffset;
                }
            }
            return Vector3.zero;
        }

        private Transform _playerTransform;
        private Dictionary<Vector2Int, GameObject> _activeChunks = new Dictionary<Vector2Int, GameObject>();
        private Dictionary<Vector2Int, List<GameObject>> _spawnedEnemies = new Dictionary<Vector2Int, List<GameObject>>();

        private Queue<ChunkData> _chunksToActivate = new Queue<ChunkData>();
        private HashSet<Vector2Int> _pendingActivations = new HashSet<Vector2Int>();
        private bool _isActivatingChunk = false;

        private HashSet<string> _killedEnemySpawnIds = new HashSet<string>();

        protected override void Awake()
        {
            dontDestroyOnLoad = false;
            base.Awake();
        }

        private void Start()
        {
            GameEventSystem.Subscribe("OnEnemyKilled", HandleEnemyKilled);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GameEventSystem.Unsubscribe("OnEnemyKilled", HandleEnemyKilled);
        }

        private void HandleEnemyKilled(object data)
        {
            var enemy = data as EnemyBase;
            if (enemy != null && !string.IsNullOrEmpty(enemy.SpawnId))
            {
                _killedEnemySpawnIds.Add(enemy.SpawnId);
                
                int totalEnemies = GetTotalEnemiesInLevel();
                Debug.Log($"[LevelProgress] Enemy killed: {enemy.SpawnId}. Progress: {_killedEnemySpawnIds.Count}/{totalEnemies}");
                
                if (_killedEnemySpawnIds.Count >= totalEnemies)
                {
                    Debug.Log("[LevelProgress] All enemies killed! Completing level.");
                    if (LevelManager.Instance != null)
                    {
                        LevelManager.Instance.CompleteCurrentLevel();
                    }
                }
            }
        }

        private int GetTotalEnemiesInLevel()
        {
            int count = 0;
            foreach (var chunk in chunks)
            {
                if (chunk.enemyPrefabs != null)
                {
                    count += chunk.enemyPrefabs.Length;
                }
            }
            return count;
        }

        private void Update()
        {
            if (_playerTransform == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    _playerTransform = player.transform;
                }
                return;
            }

            Vector3 playerPos = _playerTransform.position;

            foreach (var chunk in chunks)
            {
                Vector3 chunkCenter = new Vector3(chunk.gridPosition.x * chunkSize, 0f, chunk.gridPosition.y * chunkSize);
                float distance = Vector3.Distance(playerPos, chunkCenter);

                if (distance <= activationRadius)
                {
                    if (!_activeChunks.ContainsKey(chunk.gridPosition) && !_pendingActivations.Contains(chunk.gridPosition))
                    {
                        _chunksToActivate.Enqueue(chunk);
                        _pendingActivations.Add(chunk.gridPosition);
                    }
                }
                else
                {
                    if (_activeChunks.ContainsKey(chunk.gridPosition))
                    {
                        DeactivateChunk(chunk.gridPosition);
                    }
                }
            }

            if (_chunksToActivate.Count > 0 && !_isActivatingChunk)
            {
                StartCoroutine(ActivateChunkQueueCoroutine());
            }
        }

        private IEnumerator ActivateChunkQueueCoroutine()
        {
            _isActivatingChunk = true;

            while (_chunksToActivate.Count > 0)
            {
                var chunk = _chunksToActivate.Dequeue();
                _pendingActivations.Remove(chunk.gridPosition);

                if (_playerTransform != null)
                {
                    Vector3 chunkCenter = new Vector3(chunk.gridPosition.x * chunkSize, 0f, chunk.gridPosition.y * chunkSize);
                    float distance = Vector3.Distance(_playerTransform.position, chunkCenter);
                    if (distance <= activationRadius && !_activeChunks.ContainsKey(chunk.gridPosition))
                    {
                        yield return StartCoroutine(ActivateChunkCoroutine(chunk));
                    }
                }
            }

            _isActivatingChunk = false;
        }

        private IEnumerator ActivateChunkCoroutine(ChunkData chunk)
        {
            Vector3 chunkPos = new Vector3(chunk.gridPosition.x * chunkSize, 0f, chunk.gridPosition.y * chunkSize);
            
            // Spawn chunk map segment
            if (chunk.chunkPrefab != null)
            {
                GameObject chunkObj = PoolingManager.Instance.SpawnFromPool(chunk.chunkPrefab, chunkPos, Quaternion.identity);
                _activeChunks.Add(chunk.gridPosition, chunkObj);
            }

            // Chờ 1 frame để hệ thống NavMesh của Unity nhận diện và đăng ký bề mặt đi lại của chunk mới
            yield return null;

            // Spawn monsters for this chunk
            List<GameObject> enemies = new List<GameObject>();
            if (chunk.enemyPrefabs != null)
            {
                for (int i = 0; i < chunk.enemyPrefabs.Length; i++)
                {
                    if (chunk.enemyPrefabs[i] == null) continue;

                    string spawnId = $"{chunk.gridPosition.x}_{chunk.gridPosition.y}_{i}";
                    if (_killedEnemySpawnIds.Contains(spawnId))
                    {
                        continue;
                    }

                    Vector3 spawnPos = chunkPos + (chunk.enemySpawnOffsets != null && i < chunk.enemySpawnOffsets.Length ? chunk.enemySpawnOffsets[i] : Vector3.zero);
                    GameObject enemyObj = PoolingManager.Instance.SpawnFromPool(chunk.enemyPrefabs[i], spawnPos, Quaternion.identity);
                    
                    var enemyComp = enemyObj.GetComponent<EnemyBase>();
                    if (enemyComp != null)
                    {
                        enemyComp.SpawnId = spawnId;
                    }

                    enemies.Add(enemyObj);
                }
            }
            _spawnedEnemies.Add(chunk.gridPosition, enemies);
        }

        private void DeactivateChunk(Vector2Int gridPos)
        {
            _pendingActivations.Remove(gridPos);

            if (_activeChunks.ContainsKey(gridPos))
            {
                PoolingManager.Instance.ReturnToPool(_activeChunks[gridPos]);
                _activeChunks.Remove(gridPos);
            }

            if (_spawnedEnemies.ContainsKey(gridPos))
            {
                foreach (var enemy in _spawnedEnemies[gridPos])
                {
                    if (enemy != null) PoolingManager.Instance.ReturnToPool(enemy);
                }
                _spawnedEnemies.Remove(gridPos);
            }
        }
    }
}
