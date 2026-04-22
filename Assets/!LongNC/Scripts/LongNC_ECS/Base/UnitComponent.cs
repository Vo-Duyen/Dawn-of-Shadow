using System.ComponentModel;
using Unity.Entities;

namespace LongNC.Ecs
{
    public struct MoveSpeed : IComponentData
    {
        public float Value;
    }

    public struct IsMoving : IComponentData
    {
        
    }
}