using LongNC.Script.Base;

namespace LongNC.Script.Interface
{
    public interface IStatusEffect
    {
        string EffectId { get; }
        float Duration { get; }
        bool IsExpired { get; }
        void OnApply(BaseEntity target);
        void OnTick(float dt);
        void OnRemove();
    }
}