using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace LongNC.Ecs
{
    public class EntiySpawner : MonoBehaviour
    {
        public GameObject prefab;

        private void Start()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            EntityArchetype entityArchetype = entityManager.CreateArchetype(
                typeof(LocalTransform),
                typeof(MoveSpeed),
                typeof(IsMoving)
            );

            for (var i = 0; i < 1000; ++i)
            {
                Entity entity = entityManager.CreateEntity(entityArchetype);

                entityManager.SetComponentData(entity, new LocalTransform
                {
                    Position = new float3(i * 0.1f, 0, 0),
                    Rotation = quaternion.identity,
                    Scale = 1f,
                });

                entityManager.SetComponentData(entity, new MoveSpeed { Value = 2f });
            }
        }

    }
}