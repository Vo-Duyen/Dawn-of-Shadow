using LongNC.Script.Interface;

namespace LongNC.Script.Base
{
    public abstract class BaseStatusEffect : IStatusEffect
    {
        public string EffectId { get; protected set; }
        public float Duration { get; protected set; }
        public bool IsExpired => _elapsed >= Duration;

        protected BaseEntity Target;
        private float _elapsed;

        protected BaseStatusEffect(string id, float duration)
        {
            EffectId = id;
            Duration = duration;
        }

        public virtual void OnApply(BaseEntity target)
        {
            Target = target;
            _elapsed = 0f;
        }

        public virtual void OnTick(float dt) => _elapsed += dt;

        public abstract void OnRemove();
    }
}