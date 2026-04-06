using UnityEngine;

namespace LongNC.Script.Interface
{
    public interface IMovable
    {
        float BaseSpeed { get; }
        float CurrentSpeed { get; }
        bool IsStunned { get; }
        void Move(Vector2 direction);
        void Stop();
        void SetStunned(bool value);
    }
}