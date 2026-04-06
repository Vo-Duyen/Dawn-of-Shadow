namespace LongNC.Script.Interface
{
    public interface IAttackable
{
    float AttackRange { get; }
    void  PerformNormalAttack();
}
}