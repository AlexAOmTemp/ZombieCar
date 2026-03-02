using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public partial struct VehicleVFXSystem : ISystem
{
    private Unity.Mathematics.Random _random;

    public void OnCreate(ref SystemState state) => _random = new Unity.Mathematics.Random(123);

    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (health, transform) in SystemAPI.Query<RefRO<Health>, RefRO<LocalToWorld>>()
                     .WithAll<VehicleComponent>())
        {
            // Если здоровья меньше 30%
            if (health.ValueRO.Current / health.ValueRO.Max < 0.3f)
            {
                // Спавним "дым" (серый кубик)
                // Для простоты можно использовать префаб пули, но покрасить его в серый
                // Или просто спавнить пустую сущность с мешем
                Debug.Log("МАШИНА ДЫМИТСЯ!");
            }
        }
    }
}