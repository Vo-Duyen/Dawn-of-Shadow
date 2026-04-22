using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace LongNC.Ecs
{
    [BurstCompile]
    public partial struct MoveSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (transform, speed) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<MoveSpeed>>().WithAll<IsMoving>())
            {
                transform.ValueRW.Position += speed.ValueRO.Value * deltaTime;
            }
        }
    }
}