namespace LongNC.Script.Interface
{
    public interface IStatusEffectable
    {
        void ApplyEffect(IStatusEffect effect);
        void RemoveEffect(IStatusEffect effect);
        bool HasEffect<T>() where T : IStatusEffect;
    }
}