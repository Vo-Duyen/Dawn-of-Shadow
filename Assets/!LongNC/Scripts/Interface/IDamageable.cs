using UnityEngine;

namespace LongNC.Script.Interface
{
    public interface IDamageable
    {
        void ApplyDamage(float amount, GameObject source);
    }
}