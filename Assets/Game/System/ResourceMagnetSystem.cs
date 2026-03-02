using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct ResourceMagnetSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // 1. Ищем машину (игрок)
        var playerQuery = SystemAPI.QueryBuilder().WithAll<LocalToWorld, ResourceMagnet>().Build();
        if (playerQuery.IsEmpty) return;

        var playerEntity = playerQuery.GetSingletonEntity();
        var playerPos = SystemAPI.GetComponent<LocalToWorld>(playerEntity).Position;
        var magnet = SystemAPI.GetComponent<ResourceMagnet>(playerEntity);

        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        // 2. Тянем кристаллы
        foreach (var (trans, vel, entity) in 
                 SystemAPI.Query<RefRW<LocalTransform>, RefRW<PhysicsVelocity>>().WithAll<ResourceTag>().WithEntityAccess())
        {
            float3 crystalPos = trans.ValueRO.Position;
            float dist = math.distance(playerPos, crystalPos);

            if (dist < magnet.Radius)
            {
                // Вектор к машине
                float3 dir = math.normalize(playerPos - crystalPos);
                
                // Тянем! (игнорируем Y, чтобы не взлетали)
                vel.ValueRW.Linear = new float3(dir.x, 0, dir.z) * magnet.PullSpeed;

                // Если совсем близко - "съедаем"
                if (dist < 1.2f)
                {
                    ecb.DestroyEntity(entity);
              
                    var progress = SystemAPI.GetComponent<PlayerProgress>(playerEntity);
                    progress.Experience += 20f; // Даем 20 опыта за кристалл

                    if (progress.Experience >= progress.NextLevelXP)
                    {
                        progress.LevelUpEffectTimer = 1.0f;
                        progress.Level++;
                        progress.Experience = 0;
                        progress.NextLevelXP *= 1.5f; // Каждый уровень сложнее

                        // БОНУС: С каждым уровнем машина становится чуть быстрее
                        var vehicle = SystemAPI.GetComponent<VehicleComponent>(playerEntity);
                        vehicle.MaxSpeed += 1.5f;
                        SystemAPI.SetComponent(playerEntity, vehicle);
    
                        if (progress.Level % 3 == 0)
                        {
                            var ram = SystemAPI.GetComponent<RammingData>(playerEntity);
                            ram.BaseDamageMultiplier += 1.0f; // Увеличиваем силу удара
                            SystemAPI.SetComponent(playerEntity, ram);
                            Debug.Log("БАМПЕР УСИЛЕН!");
                        }
                        Debug.Log($"LEVEL UP! Current Level: {progress.Level}");
                    }
                    SystemAPI.SetComponent(playerEntity, progress);
                }
            }
        }
    }
}