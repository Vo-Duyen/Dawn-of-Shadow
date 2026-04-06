using System;
using UnityEngine;

namespace LongNC.Script.Interface
{
    public interface IHealth
    {
        float CurrentHP {get; }
        float MaxHP {get; }
        bool IsDead {get; }
        void TakeDamage(float amount, GameObject source = null);
        void Heal(float amount);
        event Action<float, float> OnHealthChange; // (currentHP, maxHP)
        event Action OnDie;
    }
}