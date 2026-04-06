using UnityEngine;

using LongNC.Script.Interface;

namespace LongNC.Script.Base
{

    public abstract class BaseEntity : MonoBehaviour
    {
        protected IHealth Health { get; private set; }
        protected IMovable Movement { get; private set; }
        protected IAttackable Attack { get; private set; }

        protected virtual void Awake()
        {
            Health = GetComponent<IHealth>();
            Movement = GetComponent<IMovable>();
            Attack = GetComponent<IAttackable>();

            if (Health == null)
                Debug.LogError($"[{name}] Thiếu component implement IHealth", this);
        }

        protected bool IsAlive => Health != null && !Health.IsDead;
    }
}
