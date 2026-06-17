using UnityEngine;
using System.Collections;
using System.Linq;
using DG.Tweening;
using DawnOfShadow.Gameplay.Level;

namespace DawnOfShadow.Gameplay.Enemy
{
    public class BossController : EnemyBase
    {
        [Header("Boss Specifics")]
        [SerializeField] private string bossName = "Shadow Lord";
        [SerializeField] private int bossPhase = 1;
        [SerializeField] private GameObject aoeIndicatorPrefab;
        [SerializeField] private float aoeRadius = 5f;
        [SerializeField] private float aoeCastTime = 2f;
        [SerializeField] private int aoeDamage = 40;

        private bool _isCastingAoe = false;

        protected override void Start()
        {
            base.Start();
            maxHp = 500; // High health
            currentHp = maxHp;
            attackRadius = 3f;
        }

        protected override void Update()
        {
            if (currentState == EnemyState.Dead || _isCastingAoe) return;

            // Health threshold triggers Phase 2
            if (bossPhase == 1 && currentHp < maxHp / 2)
            {
                TransitionToPhaseTwo();
            }

            base.Update();
        }

        private void TransitionToPhaseTwo()
        {
            bossPhase = 2;
            moveSpeed *= 1.3f; // Speed up
            attackCooldown *= 0.8f; // Attack faster
            Debug.Log($"{bossName} entered PHASE 2! Stats increased.");

            // Red roar visual punch effect
            transform.DOPunchScale(Vector3.one * 0.3f, 0.5f, 10, 1)
                .OnComplete(() => transform.localScale = Vector3.one);
        }

        protected override void AttackBehavior()
        {
            if (targetPlayer == null)
            {
                currentState = EnemyState.Patrol;
                return;
            }

            float distance = Vector3.Distance(transform.position, targetPlayer.position);
            
            // In Phase 2, randomly cast AOE skill if close
            if (bossPhase == 2 && !_isCastingAoe && Random.value < 0.35f && distance <= 6f)
            {
                StartCoroutine(CastAoeSpell());
                return;
            }

            base.AttackBehavior();
        }

        private IEnumerator CastAoeSpell()
        {
            _isCastingAoe = true;
            Debug.Log($"{bossName} is casting AOE Dark Blast!");

            // Spawn AOE Telegraph visual indicator
            GameObject indicator = null;
            if (aoeIndicatorPrefab != null)
            {
                indicator = Instantiate(aoeIndicatorPrefab, transform.position, Quaternion.identity);
                indicator.transform.localScale = Vector3.zero;
                // Animate AOE radius expanding to telegraph the incoming spell
                indicator.transform.DOScale(new Vector3(aoeRadius * 2, 0.1f, aoeRadius * 2), aoeCastTime).SetEase(Ease.Linear);
            }

            yield return new WaitForSeconds(aoeCastTime);

            // Execute blast and inflict damage
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, aoeRadius);
            var hitPlayers = hitColliders
                .Select(c => c.GetComponentInParent<Player.PlayerController>())
                .Where(p => p != null)
                .Distinct();

            foreach (var player in hitPlayers)
            {
                player.TakeDamage(aoeDamage);
                Debug.Log($"{bossName}'s Dark Blast hit player for {aoeDamage} damage!");
            }

            // Cleanup indicator
            if (indicator != null)
            {
                Destroy(indicator);
            }

            _isCastingAoe = false;
        }

        protected override void Die()
        {
            Debug.Log($"{bossName} has been slain! Achievement Unlocked: Slayer of {bossName}");
            base.Die();
        }
    }
}
