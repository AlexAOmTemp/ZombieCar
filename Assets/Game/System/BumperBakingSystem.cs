using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
public partial struct BumperBakingSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // Оставляем только то, что фиксирует бампер на месте
        foreach (var (transform, gravity) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<PhysicsGravityFactor>>()
                     .WithAll<BumperTag>())
        {
            // 1. Исправляем позицию (обнуляем Y относительно машины)
            var pos = transform.ValueRO.Position;
            pos.y = 0f; 
            transform.ValueRW.Position = pos;
        }
    }
}