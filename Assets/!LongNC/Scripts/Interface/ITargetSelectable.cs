using UnityEngine;

namespace LongNC.Script.Interface
{
    public interface ITargetSelectable
    {
        Transform Transform { get; }
        IHealth Health { get; }
        bool IsValidTarget { get; }
    }
}   