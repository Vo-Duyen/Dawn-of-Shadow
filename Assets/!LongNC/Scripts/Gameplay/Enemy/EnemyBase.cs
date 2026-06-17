using UnityEngine;
using System.Collections;
using System.Linq;
using DawnOfShadow.Core;

namespace DawnOfShadow.Gameplay.Enemy
{
    public enum EnemyState
    {
        Patrol,
        Chase,
        Attack,
        Dead
    }

    public enum EnemyCombatType
    {
        Melee,
        Ranged
    }

    public class EnemyBase : EntityBase
    {
        public string SpawnId { get; set; }

        [Header("AI Configuration")]
        [SerializeField] protected EnemyCombatType combatType = EnemyCombatType.Melee;
        [SerializeField] protected float detectionRadius = 8f;
        [SerializeField] protected float attackRadius = 2f;
        [SerializeField] protected float attackCooldown = 1.5f;
        [SerializeField] protected int attackDamage = 15;
        [SerializeField] protected float attackDamageDelay = 0.4f; // Trễ gây sát thương (chờ chạy hoạt ảnh)

        [Header("Ranged Configuration")]
        [SerializeField] protected GameObject projectilePrefab;
        [SerializeField] protected Transform projectileSpawnPoint;

        [Header("Patrol Waypoints")]
        [SerializeField] protected Transform[] patrolWaypoints;
        protected int currentWaypointIndex = 0;
        [SerializeField] protected float patrolRadius = 10f; // Bán kính tuần tra ngẫu nhiên xung quanh điểm Spawn

        [Header("Animation Settings")]
        [SerializeField] protected AnimationClip idleClip;
        [SerializeField] protected AnimationClip moveClip;
        [SerializeField] protected AnimationClip attackClip;
        [SerializeField] protected AnimationClip hitClip;
        [SerializeField] protected AnimationClip deathClip;

        protected Animation _animation;
        protected bool _isAttackingAnim = false;
        protected bool _isHitAnim = false;
        protected bool _isStunned = false;

        private Coroutine _stunCoroutine;
        private Coroutine _hitRecoveryCoroutine;
        private Coroutine _attackExecutionCoroutine;

        protected Vector3 _spawnPosition;
        protected Vector3 _currentPatrolTarget;
        protected bool _hasPatrolTarget = false;
        private UnityEngine.AI.NavMeshAgent _agent;

        protected EnemyState currentState = EnemyState.Patrol;
        protected Transform targetPlayer;
        private float lastAttackTime = 0f;

        protected override void Awake()
        {
            base.Awake();
            _agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            _animation = GetComponentInChildren<Animation>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            // Re-enable all colliders when reused from Pool
            var colliders = GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                if (col != null) col.enabled = true;
            }

            // Reset enemy state
            currentState = EnemyState.Patrol;
            _isAttackingAnim = false;
            _isHitAnim = false;
            _isStunned = false;
            _hasPatrolTarget = false;
            lastAttackTime = 0f;

            if (_agent != null)
            {
                _agent.speed = moveSpeed;
                if (_agent.isOnNavMesh)
                {
                    _agent.isStopped = false;
                    _agent.ResetPath();
                }
            }

            _spawnPosition = transform.position;

            // Stop leftover coroutines
            if (_stunCoroutine != null) StopCoroutine(_stunCoroutine);
            if (_hitRecoveryCoroutine != null) StopCoroutine(_hitRecoveryCoroutine);
            if (_attackExecutionCoroutine != null) StopCoroutine(_attackExecutionCoroutine);
            _stunCoroutine = null;
            _hitRecoveryCoroutine = null;
            _attackExecutionCoroutine = null;

            FindTargetPlayer();
        }

        protected override void Start()
        {
            base.Start();
            FindTargetPlayer();

            _spawnPosition = transform.position;

            if (_animation != null)
            {
                AddClipIfMissing(idleClip);
                AddClipIfMissing(moveClip);
                AddClipIfMissing(attackClip);
                AddClipIfMissing(hitClip);
                AddClipIfMissing(deathClip);
            }

            // Auto-configure default ranges if they are at default values
            if (combatType == EnemyCombatType.Ranged && attackRadius == 2f)
            {
                attackRadius = 6f;
            }
        }

        protected virtual void Update()
        {
            if (currentState == EnemyState.Dead) return;

            if (_isStunned)
            {
                if (_agent != null && _agent.isOnNavMesh)
                {
                    _agent.isStopped = true;
                }
                UpdateMovementAnimation();
                return;
            }

            FindTargetPlayer();

            // Cập nhật trạng thái chuyển động của hoạt ảnh
            UpdateMovementAnimation();

            switch (currentState)
            {
                case EnemyState.Patrol:
                    PatrolBehavior();
                    break;
                case EnemyState.Chase:
                    ChaseBehavior();
                    break;
                case EnemyState.Attack:
                    AttackBehavior();
                    break;
            }
        }

        protected virtual void FindTargetPlayer()
        {
            if (targetPlayer == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    targetPlayer = playerObj.transform;
                }
            }
        }

        protected virtual void PatrolBehavior()
        {
            if (targetPlayer != null && Vector3.Distance(transform.position, targetPlayer.position) <= detectionRadius)
            {
                currentState = EnemyState.Chase;
                if (_agent != null && _agent.isOnNavMesh) _agent.ResetPath();
                _hasPatrolTarget = false;
                return;
            }

            if (_isHitAnim || _isAttackingAnim)
            {
                if (_agent != null && _agent.isOnNavMesh) _agent.isStopped = true;
                return;
            }
            else
            {
                if (_agent != null && _agent.isOnNavMesh) _agent.isStopped = false;
            }

            // Nếu tuần tra theo Waypoints được cấu hình sẵn trong Inspector
            if (patrolWaypoints != null && patrolWaypoints.Length > 0)
            {
                Transform waypoint = patrolWaypoints[currentWaypointIndex];
                
                if (_agent != null && _agent.isOnNavMesh)
                {
                    _agent.SetDestination(waypoint.position);
                    if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
                    {
                        currentWaypointIndex = (currentWaypointIndex + 1) % patrolWaypoints.Length;
                    }
                }
                else
                {
                    Vector3 direction = (waypoint.position - transform.position).normalized;
                    direction.y = 0; // Lock vertical axis

                    transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
                    if (direction != Vector3.zero)
                    {
                        transform.forward = Vector3.Slerp(transform.forward, direction, Time.deltaTime * 5f);
                    }

                    if (Vector3.Distance(transform.position, waypoint.position) < 0.5f)
                    {
                        currentWaypointIndex = (currentWaypointIndex + 1) % patrolWaypoints.Length;
                    }
                }
            }
            else // Nếu không gán Waypoints, tự động tuần tra ngẫu nhiên xung quanh điểm Spawn
            {
                if (!_hasPatrolTarget || GetRemainingDistanceToTarget() < 0.8f)
                {
                    _currentPatrolTarget = _agent != null ? GetRandomNavMeshPoint(_spawnPosition, patrolRadius) : GetRandomPatrolPoint(_spawnPosition, patrolRadius);
                    _hasPatrolTarget = true;
                    
                    if (_agent != null && _agent.isOnNavMesh)
                    {
                        _agent.SetDestination(_currentPatrolTarget);
                    }
                }

                if (_agent == null)
                {
                    Vector3 direction = (_currentPatrolTarget - transform.position).normalized;
                    direction.y = 0;

                    transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
                    if (direction != Vector3.zero)
                    {
                        transform.forward = Vector3.Slerp(transform.forward, direction, Time.deltaTime * 5f);
                    }
                }
            }
        }

        protected virtual void ChaseBehavior()
        {
            if (targetPlayer == null)
            {
                currentState = EnemyState.Patrol;
                _hasPatrolTarget = false;
                if (_agent != null && _agent.isOnNavMesh) _agent.ResetPath();
                return;
            }

            float distance = Vector3.Distance(transform.position, targetPlayer.position);
            if (distance > detectionRadius)
            {
                currentState = EnemyState.Patrol;
                _hasPatrolTarget = false;
                if (_agent != null && _agent.isOnNavMesh) _agent.ResetPath();
                return;
            }

            if (distance <= attackRadius)
            {
                currentState = EnemyState.Attack;
                if (_agent != null && _agent.isOnNavMesh) _agent.ResetPath();
                return;
            }

            if (_isHitAnim || _isAttackingAnim)
            {
                if (_agent != null && _agent.isOnNavMesh) _agent.isStopped = true;
                return;
            }
            else
            {
                if (_agent != null && _agent.isOnNavMesh) _agent.isStopped = false;
            }

            if (_agent != null && _agent.isOnNavMesh)
            {
                _agent.SetDestination(targetPlayer.position);
            }
            else
            {
                Vector3 direction = (targetPlayer.position - transform.position).normalized;
                direction.y = 0;

                transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
                if (direction != Vector3.zero)
                {
                    transform.forward = Vector3.Slerp(transform.forward, direction, Time.deltaTime * 8f);
                }
            }
        }

        protected virtual void AttackBehavior()
        {
            if (targetPlayer == null)
            {
                currentState = EnemyState.Patrol;
                _hasPatrolTarget = false;
                return;
            }

            float distance = Vector3.Distance(transform.position, targetPlayer.position);
            if (distance > attackRadius)
            {
                currentState = EnemyState.Chase;
                return;
            }

            if (_agent != null && _agent.isOnNavMesh)
            {
                _agent.isStopped = true; // Dừng di chuyển khi tấn công
            }

            // Keep facing player while attacking
            Vector3 direction = (targetPlayer.position - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                transform.forward = Vector3.Slerp(transform.forward, direction, Time.deltaTime * 10f);
            }

            // Attack on cooldown
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                lastAttackTime = Time.time;
                PerformAttack();
            }
        }

        protected virtual void PerformAttack()
        {
            if (attackClip != null)
            {
                _isAttackingAnim = true;
                _isHitAnim = false; // Ngắt hoạt ảnh bị đánh khi tấn công
                PlayEnemyAnimation(attackClip, 0.05f);
                StartCoroutine(AttackAnimRecovery(attackClip.length > 0f ? attackClip.length : 1f));
            }

            if (_attackExecutionCoroutine != null)
            {
                StopCoroutine(_attackExecutionCoroutine);
            }
            _attackExecutionCoroutine = StartCoroutine(ExecuteAttackAfterDelay(attackDamageDelay));
        }

        private IEnumerator ExecuteAttackAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            // Interrupt check: if stunned or dead, cancel attack execution
            if (currentState == EnemyState.Dead || _isStunned)
            {
                _attackExecutionCoroutine = null;
                yield break;
            }

            if (combatType == EnemyCombatType.Melee)
            {
                if (targetPlayer != null)
                {
                    float distance = Vector3.Distance(transform.position, targetPlayer.position);
                    if (distance <= attackRadius * 1.3f) // 1.3x range leniency for movement
                    {
                        var playerEntity = targetPlayer.GetComponent<EntityBase>();
                        if (playerEntity != null)
                        {
                            playerEntity.TakeDamage(attackDamage);
                            Debug.Log($"{gameObject.name} (Melee) hit player for {attackDamage} damage.");
                        }
                    }
                    else
                    {
                        Debug.Log($"{gameObject.name} (Melee) missed (player out of range).");
                    }
                }
            }
            else if (combatType == EnemyCombatType.Ranged)
            {
                if (targetPlayer != null && projectilePrefab != null)
                {
                    Transform spawnLoc = projectileSpawnPoint != null ? projectileSpawnPoint : transform;
                    
                    // Spawn projectile using PoolingManager
                    GameObject proj = PoolingManager.Instance.SpawnFromPool(projectilePrefab, spawnLoc.position, spawnLoc.rotation);
                    
                    var projectileComp = proj.GetComponent<EnemyProjectile>();
                    if (projectileComp == null)
                    {
                        projectileComp = proj.AddComponent<EnemyProjectile>();
                    }
                    projectileComp.Initialize(targetPlayer, attackDamage);
                    Debug.Log($"{gameObject.name} (Ranged) fired projectile at player.");
                }
            }

            _attackExecutionCoroutine = null;
        }

        public override void TakeDamage(int damage)
        {
            if (currentState == EnemyState.Dead || currentHp <= 0) return;

            base.TakeDamage(damage);

            if (_agent != null && _agent.isOnNavMesh)
            {
                _agent.isStopped = true; // Khóa di chuyển khi bị trúng đòn
            }

            if (currentHp > 0)
            {
                _isStunned = true;
                _isAttackingAnim = false; // Bị trúng đòn cắt ngang đòn đánh

                if (_attackExecutionCoroutine != null)
                {
                    StopCoroutine(_attackExecutionCoroutine);
                    _attackExecutionCoroutine = null;
                }

                if (_stunCoroutine != null)
                {
                    StopCoroutine(_stunCoroutine);
                }
                _stunCoroutine = StartCoroutine(StunRecovery(1.0f));

                if (hitClip != null)
                {
                    _isHitAnim = true;
                    PlayEnemyAnimation(hitClip, 0.05f);
                    float hitStunTime = hitClip.length > 0f ? Mathf.Min(hitClip.length, 0.5f) : 0.25f;

                    if (_hitRecoveryCoroutine != null)
                    {
                        StopCoroutine(_hitRecoveryCoroutine);
                    }
                    _hitRecoveryCoroutine = StartCoroutine(HitAnimRecovery(hitStunTime));
                }
            }
        }

        protected override void Die()
        {
            currentState = EnemyState.Dead;
            GameEventSystem.Publish("OnEnemyKilled", this);

            if (_attackExecutionCoroutine != null)
            {
                StopCoroutine(_attackExecutionCoroutine);
                _attackExecutionCoroutine = null;
            }

            // Tắt agent khi chết
            if (_agent != null)
            {
                _agent.enabled = false;
            }

            // Vô hiệu hóa tất cả colliders để không cản đường Player nữa
            var colliders = GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                if (col != null) col.enabled = false;
            }

            if (deathClip != null)
            {
                PlayEnemyAnimation(deathClip, 0.1f);
                StartCoroutine(DestroyAfterDeath(deathClip.length > 0f ? deathClip.length : 1.5f));
            }
            else
            {
                base.Die();
            }
        }

        private IEnumerator DestroyAfterDeath(float delay)
        {
            yield return new WaitForSeconds(delay);
            base.Die();
        }

        protected virtual void UpdateMovementAnimation()
        {
            if (_isAttackingAnim || _isHitAnim) return;

            bool isMoving = false;

            if (_agent != null && _agent.isOnNavMesh)
            {
                // Nếu dùng NavMeshAgent, kiểm tra vận tốc của agent
                isMoving = _agent.velocity.magnitude > 0.1f;
            }
            else
            {
                // Fallback tính theo khoảng cách của tuần tra / đuổi theo truyền thống
                if (currentState == EnemyState.Patrol && patrolWaypoints != null && patrolWaypoints.Length > 0)
                {
                    Transform waypoint = patrolWaypoints[currentWaypointIndex];
                    if (Vector3.Distance(transform.position, waypoint.position) >= 0.5f)
                    {
                        isMoving = true;
                    }
                }
                else if (currentState == EnemyState.Patrol && _hasPatrolTarget)
                {
                    if (Vector3.Distance(transform.position, _currentPatrolTarget) >= 0.5f)
                    {
                        isMoving = true;
                    }
                }
                else if (currentState == EnemyState.Chase && targetPlayer != null)
                {
                    float distance = Vector3.Distance(transform.position, targetPlayer.position);
                    if (distance > attackRadius && distance <= detectionRadius)
                    {
                        isMoving = true;
                    }
                }
            }

            if (isMoving && moveSpeed > 0.1f)
            {
                PlayEnemyAnimation(moveClip, 0.15f);
            }
            else
            {
                PlayEnemyAnimation(idleClip, 0.2f);
            }
        }

        protected void PlayEnemyAnimation(AnimationClip clip, float fadeTime = 0.15f)
        {
            if (clip == null || _animation == null) return;

            // Chuyển đổi clip sang chế độ Legacy động tại runtime để có thể chạy bằng component Animation
            clip.legacy = true;
            AddClipIfMissing(clip);
            _animation.CrossFade(clip.name, fadeTime);
        }

        private void AddClipIfMissing(AnimationClip clip)
        {
            if (clip != null && _animation != null && _animation[clip.name] == null)
            {
                _animation.AddClip(clip, clip.name);
            }
        }

        private IEnumerator AttackAnimRecovery(float duration)
        {
            yield return new WaitForSeconds(duration);
            _isAttackingAnim = false;
        }

        private IEnumerator HitAnimRecovery(float duration)
        {
            yield return new WaitForSeconds(duration);
            _isHitAnim = false;
        }

        private IEnumerator StunRecovery(float duration)
        {
            yield return new WaitForSeconds(duration);
            _isStunned = false;
            if (_agent != null && _agent.isOnNavMesh && currentState != EnemyState.Attack && currentState != EnemyState.Dead)
            {
                _agent.isStopped = false;
            }
        }

        private float GetRemainingDistanceToTarget()
        {
            if (_agent != null && _agent.isOnNavMesh)
            {
                if (!_agent.pathPending)
                {
                    return _agent.remainingDistance;
                }
            }
            return Vector3.Distance(transform.position, _currentPatrolTarget);
        }

        private Vector3 GetRandomNavMeshPoint(Vector3 center, float radius)
        {
            Vector3 randomDirection = Random.insideUnitSphere * radius;
            randomDirection += center;

            UnityEngine.AI.NavMeshHit navHit;
            if (UnityEngine.AI.NavMesh.SamplePosition(randomDirection, out navHit, radius, UnityEngine.AI.NavMesh.AllAreas))
            {
                return navHit.position;
            }
            return center;
        }

        private Vector3 GetRandomPatrolPoint(Vector3 center, float radius)
        {
            Vector3 randomPoint = center + new Vector3(Random.Range(-radius, radius), 0f, Random.Range(-radius, radius));
            return randomPoint;
        }
    }
}
