using System;
using UnityEngine;
using DG.Tweening;
using DawnOfShadow.Core;

namespace DawnOfShadow.Gameplay
{
    public abstract class EntityBase : MonoBehaviour
    {
        [Header("Entity Base Stats")]
        [SerializeField] protected int maxHp = 100;
        protected int currentHp;
        [SerializeField] protected float moveSpeed = 5f;

        [Header("Visual Feedback References")]
        [SerializeField] protected Renderer[] entityRenderers;
        [SerializeField] protected Color damageFlashColor = Color.red;

        public event Action<int, int> OnHealthChanged; // (current, max)
        public event Action OnDeath;

        public int CurrentHp => currentHp;
        public int MaxHp => maxHp;
        public float MoveSpeed => moveSpeed;

        protected Vector3 initialScale;

        protected virtual void Awake()
        {
            initialScale = transform.localScale;
        }

        protected virtual void OnEnable()
        {
            currentHp = maxHp;
            transform.localScale = initialScale;

            // Reset materials color to white
            if (entityRenderers != null)
            {
                foreach (var renderer in entityRenderers)
                {
                    if (renderer != null && renderer.material.HasProperty("_Color"))
                    {
                        renderer.material.color = Color.white;
                    }
                }
            }
            OnHealthChanged?.Invoke(currentHp, maxHp);
        }

        protected virtual void Start()
        {
            OnHealthChanged?.Invoke(currentHp, maxHp);
        }

        public virtual void TakeDamage(int damage)
        {
            if (currentHp <= 0) return;

            currentHp = Mathf.Max(0, currentHp - damage);
            OnHealthChanged?.Invoke(currentHp, maxHp);

            // Creative Visual feedback: Flash red using DOTween
            if (entityRenderers != null)
            {
                foreach (var renderer in entityRenderers)
                {
                    if (renderer != null)
                    {
                        renderer.material.DOColor(damageFlashColor, "_Color", 0.1f)
                            .SetLoops(2, LoopType.Yoyo)
                            .OnComplete(() => renderer.material.DOColor(Color.white, 0.05f));
                    }
                }
            }

            // Punch effect for impact feel
            transform.DOPunchScale(Vector3.one * 0.1f, 0.15f, 10, 1);

            if (currentHp <= 0)
            {
                Die();
            }
        }

        protected virtual void Die()
        {
            OnDeath?.Invoke();
            
            // Fade out and sink into the floor
            transform.DOMoveY(transform.position.y - 1.5f, 1f)
                .OnComplete(() => 
                {
                    if (PoolingManager.Instance != null)
                    {
                        PoolingManager.Instance.ReturnToPool(gameObject);
                    }
                    else
                    {
                        Destroy(gameObject);
                    }
                });
        }

        public virtual void Heal(int amount)
        {
            currentHp = Mathf.Min(maxHp, currentHp + amount);
            OnHealthChanged?.Invoke(currentHp, maxHp);

            // Healing pulse feedback
            transform.DOPunchScale(Vector3.one * 0.05f, 0.2f, 5, 0.5f);
        }
    }
}
