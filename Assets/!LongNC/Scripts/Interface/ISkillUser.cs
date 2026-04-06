namespace LongNC.Script.Interface
{
    public interface ISkillUser
    {
        void UseSkill(int index);
        bool CanUseSkill(int index);
        float GetCooldownRemaining(int index);
    }
}