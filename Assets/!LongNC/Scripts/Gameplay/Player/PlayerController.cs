using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DawnOfShadow.Gameplay.Input;
using DawnOfShadow.Gameplay.Skills;
using DawnOfShadow.Gameplay.Enemy;
using DawnOfShadow.Data;
using DawnOfShadow.Core;
using System.Linq;
using UnityEngine.Playables;
using UnityEngine.Animations;
using DG.Tweening;

namespace DawnOfShadow.Gameplay.Player
{
    public class PlayerController : EntityBase
    {
        [Header("Controls & Input")]
        [SerializeField] private JoystickController movementJoystick;
        [SerializeField] private float autoAimRange = 12f;

        [Header("Character Selection Data")]
        [SerializeField] private CharacterData characterData; // Holds character metadata

        [Header("Combo Combat Animations")]
        [SerializeField] private List<AnimationClip> comboAttackClips;
        [SerializeField] private AnimationClip idleClip;
        [SerializeField] private AnimationClip runClip;
        [SerializeField] private AnimationClip dodgeClip;
        [SerializeField] private float comboResetTime = 1.2f;
        [SerializeField] private float forwardNudgeForce = 1.0f;

        [Header("Additional Character Animations")]
        [SerializeField] private AnimationClip skill2Clip; // For Heavy Cleave (Skill 2)
        [SerializeField] private AnimationClip skill3Clip; // For Bladestorm (Skill 3)
        [SerializeField] private AnimationClip healClip;   // For Heal
        [SerializeField] private AnimationClip hitClip;    // For Get Hit
        [SerializeField] private AnimationClip deathClip;  // For Death

        private IDodgeBehavior _dodgeBehavior;
        private bool _isDodging = false;
        private bool _isAttacking = false;
        private bool _isHitStunned = false;
        private Vector3 _moveDirection = Vector3.zero;

        // PlayableGraph animation variables
        private PlayableGraph _playableGraph;
        private AnimationPlayableOutput _playableOutput;
        private AnimationClip _currentClip;

        // Combo system variables
        private int _currentComboIndex = 0;
        private float _lastAttackTime = 0f;

        // Skill cooldown states
        private float[] _skillCooldownTimers = new float[3]; // [0] = Dodge, [1] = Skill 2 (Cleave), [2] = Skill 3 (Bladestorm)
        private const float DODGE_COOLDOWN = 1.5f;
        private const float HEAVY_CLEAVE_COOLDOWN = 4.0f;
        private const float BLADESTORM_COOLDOWN = 8.0f;

        private int _potionCount = 3;

        private Animator _animator;

        // Coroutine references
        private Coroutine _crossFadeCoroutine;
        private Coroutine _attackRecoveryCoroutine;
        private Coroutine _hitStunCoroutine;
        private Coroutine _skillCastCoroutine;
        private Coroutine _healRecoveryCoroutine;

        public CharacterData CharacterConfig => characterData;
        public int PotionCount => _potionCount;

        protected override void Start()
        {
            base.Start();

            _animator = GetComponentInChildren<Animator>();

            // Setup character stats including permanent upgrades
            if (characterData != null)
            {
                var save = SaveManager.Instance.Data;
                maxHp = characterData.hp + (save.hpUpgradeLevel * 15);
                currentHp = maxHp;
                moveSpeed = characterData.speed;
                Debug.Log($"Player initialized. Base HP: {characterData.hp}, Upgrades HP bonus: {save.hpUpgradeLevel * 15}, Total HP: {maxHp}");
            }

            // Always RollDodge for melee swordfighter
            _dodgeBehavior = new RollDodge();

            // Play Idle initially
            PlayAnimation(idleClip, 0f);
        }

        public void InitializeJoystick(JoystickController joystick)
        {
            movementJoystick = joystick;
        }

        private void Update()
        {
            // Cooldown ticks
            for (int i = 0; i < _skillCooldownTimers.Length; i++)
            {
                if (_skillCooldownTimers[i] > 0)
                {
                    _skillCooldownTimers[i] -= Time.deltaTime;
                }
            }

            // Reset combo if elapsed time is too long
            if (_isAttacking && Time.time - _lastAttackTime > comboResetTime)
            {
                _currentComboIndex = 0;
            }

            if (_isDodging) return;

            ProcessMovement();
        }

        private void ProcessMovement()
        {
            if (_isAttacking || _isHitStunned) return; // Prevent movement during attack or hit stun

            float horizontal = movementJoystick != null ? movementJoystick.Horizontal : UnityEngine.Input.GetAxis("Horizontal");
            float vertical = movementJoystick != null ? movementJoystick.Vertical : UnityEngine.Input.GetAxis("Vertical");

            _moveDirection = new Vector3(horizontal, 0f, vertical).normalized;

            if (_moveDirection.magnitude > 0.1f)
            {
                transform.Translate(_moveDirection * moveSpeed * Time.deltaTime, Space.World);
                transform.forward = Vector3.Slerp(transform.forward, _moveDirection, Time.deltaTime * 10f);
                PlayAnimation(runClip, 0.1f);
            }
            else
            {
                PlayAnimation(idleClip, 0.15f);
            }
        }

        private void PlayAnimation(AnimationClip clip, float transitionTime = 0.15f)
        {
            if (clip == null || _animator == null) return;
            if (!_playableGraph.IsValid())
            {
                _playableGraph = PlayableGraph.Create(gameObject.name + "_AnimGraph");
                _playableOutput = AnimationPlayableOutput.Create(_playableGraph, "Animation", _animator);
                _playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            }

            if (_currentClip == clip && clip.isLooping) return;

            _currentClip = clip;

            var clipPlayable = AnimationClipPlayable.Create(_playableGraph, clip);
            
            if (transitionTime <= 0f)
            {
                _playableOutput.SetSourcePlayable(clipPlayable);
            }
            else
            {
                Playable oldPlayable = _playableOutput.GetSourcePlayable();
                if (oldPlayable.IsValid())
                {
                    var mixer = AnimationMixerPlayable.Create(_playableGraph, 2);
                    _playableOutput.SetSourcePlayable(mixer);
                    
                    _playableGraph.Connect(oldPlayable, 0, mixer, 0);
                    _playableGraph.Connect(clipPlayable, 0, mixer, 1);
                    
                    mixer.SetInputWeight(0, 1.0f);
                    mixer.SetInputWeight(1, 0.0f);
                    
                    if (_crossFadeCoroutine != null)
                    {
                        StopCoroutine(_crossFadeCoroutine);
                    }
                    _crossFadeCoroutine = StartCoroutine(CrossFadeCoroutine(mixer, transitionTime, oldPlayable));
                }
                else
                {
                    _playableOutput.SetSourcePlayable(clipPlayable);
                }
            }

            if (!_playableGraph.IsPlaying())
            {
                _playableGraph.Play();
            }
        }

        private IEnumerator CrossFadeCoroutine(AnimationMixerPlayable mixer, float duration, Playable oldPlayable)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (!mixer.IsValid()) yield break;
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                mixer.SetInputWeight(0, 1f - t);
                mixer.SetInputWeight(1, t);
                yield return null;
            }

            if (mixer.IsValid())
            {
                mixer.SetInputWeight(0, 0f);
                mixer.SetInputWeight(1, 1f);
                
                if (oldPlayable.IsValid())
                {
                    _playableGraph.Disconnect(mixer, 0);
                    oldPlayable.Destroy();
                }
            }
            _crossFadeCoroutine = null;
        }

        public void TriggerDodge()
        {
            if (_isDodging || _skillCooldownTimers[0] > 0) return;

            _isDodging = true;
            _isAttacking = false; // Interrupt attack on dodge
            _skillCooldownTimers[0] = DODGE_COOLDOWN;

            PlayAnimation(dodgeClip, 0.05f);

            _dodgeBehavior.PerformDodge(this, transform, _moveDirection, moveSpeed, () =>
            {
                _isDodging = false;
                PlayAnimation(idleClip, 0.15f);
            });
        }

        public override void TakeDamage(int damage)
        {
            base.TakeDamage(damage);
            if (currentHp > 0 && hitClip != null)
            {
                PlayAnimation(hitClip, 0.05f);
                if (_hitStunCoroutine != null)
                {
                    StopCoroutine(_hitStunCoroutine);
                }
                _hitStunCoroutine = StartCoroutine(HitStunCoroutine(0.25f)); // Khóa di chuyển 0.25s để hiển thị hoạt ảnh nhận sát thương
            }
            if (CameraShaker.Instance != null)
            {
                CameraShaker.Instance.Shake(0.25f, 0.5f);
            }
            // Decoupled event publish
            GameEventSystem.Publish("OnPlayerDamaged", damage);
        }

        private IEnumerator HitStunCoroutine(float duration)
        {
            _isHitStunned = true;
            yield return new WaitForSeconds(duration);
            _isHitStunned = false;
            _hitStunCoroutine = null;
        }

        public void TriggerAttack()
        {
            if (_isAttacking || _isDodging || _isHitStunned || comboAttackClips == null || comboAttackClips.Count == 0) return;

            // Tự động xoay mặt về phía kẻ địch gần nhất
            AutoFaceClosestEnemy(autoAimRange);

            // Reset combo if the time gap is too large
            if (Time.time - _lastAttackTime > comboResetTime)
            {
                _currentComboIndex = 0;
            }

            _isAttacking = true;
            _lastAttackTime = Time.time;

            AnimationClip attackClip = comboAttackClips[_currentComboIndex];
            PlayAnimation(attackClip, 0.05f);

            // Punchy forward nudge using DOTween
            transform.DOMove(transform.position + transform.forward * forwardNudgeForce, 0.15f);

            // Calculate scaling damage: combo hits deal progressively more damage
            int baseAtk = characterData != null ? characterData.atk : 15;
            int finalAtk = baseAtk + (_currentComboIndex * 5);
            Debug.Log($"Sword Swipe {_currentComboIndex + 1} deals {finalAtk} damage!");

            // Perform short range swipe checks
            float range = 2.2f;
            Collider[] hitColliders = Physics.OverlapSphere(transform.position + transform.forward * 1.0f, range);
            
            var hitEnemies = hitColliders
                .Select(c => c.GetComponentInParent<EnemyBase>())
                .Where(e => e != null)
                .Distinct();

            bool hitAny = false;
            foreach (var enemy in hitEnemies)
            {
                enemy.TakeDamage(finalAtk);
                hitAny = true;
            }

            if (hitAny && CameraShaker.Instance != null)
            {
                CameraShaker.Instance.Shake(0.1f + (_currentComboIndex * 0.05f), 0.2f + (_currentComboIndex * 0.05f));
            }

            // Advance combo steps
            _currentComboIndex = (_currentComboIndex + 1) % comboAttackClips.Count;

            if (_attackRecoveryCoroutine != null)
            {
                StopCoroutine(_attackRecoveryCoroutine);
            }
            _attackRecoveryCoroutine = StartCoroutine(AttackRecoveryCoroutine(attackClip.length));
        }

        private IEnumerator AttackRecoveryCoroutine(float duration)
        {
            yield return new WaitForSeconds(duration); // Chờ chạy hết 100% hoạt ảnh đánh thường
            _isAttacking = false;
            _attackRecoveryCoroutine = null;
        }

        public void TriggerSkill(int index)
        {
            if (_isDodging || _isHitStunned) return;

            if (index == 0)
            {
                TriggerDodge();
                return;
            }

            if (index == 1) // Heavy Cleave (Skill 2)
            {
                if (_skillCooldownTimers[1] > 0 || _isAttacking) return;

                // Tự động xoay mặt về phía kẻ địch gần nhất
                AutoFaceClosestEnemy(autoAimRange);

                _skillCooldownTimers[1] = HEAVY_CLEAVE_COOLDOWN;

                int cleaveDamage = (characterData != null ? characterData.atk : 15) * 2;
                Debug.Log($"Heavy Cleave cast! Deals {cleaveDamage} damage.");

                // Chạy hoạt ảnh kỹ năng 2
                if (skill2Clip != null)
                {
                    PlayAnimation(skill2Clip, 0.05f);
                    if (_skillCastCoroutine != null)
                    {
                        StopCoroutine(_skillCastCoroutine);
                    }
                    _skillCastCoroutine = StartCoroutine(SkillCastCoroutine(skill2Clip.length));
                }

                // Cleave cone check
                float cleaveRange = 3.5f;
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, cleaveRange);
                var hitEnemies = hitColliders
                    .Select(c => c.GetComponentInParent<EnemyBase>())
                    .Where(e => e != null)
                    .Distinct();

                foreach (var enemy in hitEnemies)
                {
                    Vector3 dirToEnemy = (enemy.transform.position - transform.position).normalized;
                    float angle = Vector3.Angle(transform.forward, dirToEnemy);

                    if (angle <= 45f) // 90 degree total angle arc
                    {
                        enemy.TakeDamage(cleaveDamage);
                    }
                }
            }
            else if (index == 2) // Bladestorm (Skill 3)
            {
                if (_skillCooldownTimers[2] > 0 || _isAttacking) return;

                // Tự động xoay mặt về phía kẻ địch gần nhất
                AutoFaceClosestEnemy(autoAimRange);

                _skillCooldownTimers[2] = BLADESTORM_COOLDOWN;

                int tickDamage = Mathf.RoundToInt((characterData != null ? characterData.atk : 15) * 1.5f);
                Debug.Log($"Bladestorm spinning! Deals {tickDamage} AOE damage.");

                // Chạy hoạt ảnh kỹ năng 3
                if (skill3Clip != null)
                {
                    PlayAnimation(skill3Clip, 0.05f);
                    if (_skillCastCoroutine != null)
                    {
                        StopCoroutine(_skillCastCoroutine);
                    }
                    _skillCastCoroutine = StartCoroutine(SkillCastCoroutine(skill3Clip.length));
                }

                // Circular AOE hits
                float bladestormRange = 4.0f;
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, bladestormRange);
                var hitEnemies = hitColliders
                    .Select(c => c.GetComponentInParent<EnemyBase>())
                    .Where(e => e != null)
                    .Distinct();

                foreach (var enemy in hitEnemies)
                {
                    enemy.TakeDamage(tickDamage);
                }
            }
        }

        private IEnumerator SkillCastCoroutine(float duration)
        {
            _isAttacking = true;
            yield return new WaitForSeconds(duration);
            _isAttacking = false;
            _skillCastCoroutine = null;
        }

        public void TriggerHeal()
        {
            if (_isDodging || _isAttacking || _isHitStunned) return;

            if (_potionCount > 0 && currentHp < maxHp)
            {
                _potionCount--;
                Heal(35);

                if (healClip != null)
                {
                    PlayAnimation(healClip, 0.1f);
                    if (_healRecoveryCoroutine != null)
                    {
                        StopCoroutine(_healRecoveryCoroutine);
                    }
                    _healRecoveryCoroutine = StartCoroutine(HealRecoveryCoroutine(healClip.length));
                }
                Debug.Log($"Healed! Remaining potions: {_potionCount}");
            }
        }

        private IEnumerator HealRecoveryCoroutine(float duration)
        {
            _isAttacking = true;
            yield return new WaitForSeconds(duration);
            _isAttacking = false;
            _healRecoveryCoroutine = null;
        }

        public float GetSkillCooldownProgress(int index)
        {
            if (index == 0) return Mathf.Clamp01(_skillCooldownTimers[0] / DODGE_COOLDOWN);
            if (index == 1) return Mathf.Clamp01(_skillCooldownTimers[1] / HEAVY_CLEAVE_COOLDOWN);
            if (index == 2) return Mathf.Clamp01(_skillCooldownTimers[2] / BLADESTORM_COOLDOWN);
            return 0f;
        }

        public float GetSkillCooldownRemaining(int index)
        {
            if (index < 0 || index >= _skillCooldownTimers.Length) return 0f;
            return Mathf.Max(0f, _skillCooldownTimers[index]);
        }

        protected override void Die()
        {
            if (deathClip != null)
            {
                PlayAnimation(deathClip, 0.1f);
            }
            base.Die();
            GameEventSystem.Publish("OnPlayerDied");
        }

        private void AutoFaceClosestEnemy(float searchRange)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, searchRange);
            EnemyBase closestEnemy = null;
            float closestDistance = Mathf.Infinity;
            HashSet<EnemyBase> processedEnemies = new HashSet<EnemyBase>();

            foreach (var col in hitColliders)
            {
                var enemy = col.GetComponentInParent<EnemyBase>();
                if (enemy != null && !processedEnemies.Contains(enemy))
                {
                    processedEnemies.Add(enemy);
                    if (enemy.CurrentHp > 0)
                    {
                        float distance = Vector3.Distance(transform.position, enemy.transform.position);
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestEnemy = enemy;
                        }
                    }
                }
            }

            if (closestEnemy != null)
            {
                Vector3 direction = (closestEnemy.transform.position - transform.position).normalized;
                direction.y = 0; // Khóa trục Y để nhân vật không bị chúc đầu/ngẩng đầu lên xuống
                if (direction != Vector3.zero)
                {
                    transform.forward = direction;
                }
            }
        }

        private void OnDestroy()
        {
            if (_playableGraph.IsValid())
            {
                _playableGraph.Destroy();
            }
        }
    }
}
