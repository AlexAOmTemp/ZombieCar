using Unity.Entities;

public partial struct LifeTimeSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // Используем более раннюю фазу для удаления
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        // Используем EntityQuery для эффективности
        foreach (var (lifeTime, entity) in SystemAPI.Query<RefRW<LifeTime>>().WithEntityAccess())
        {
            lifeTime.ValueRW.Value -= SystemAPI.Time.DeltaTime;
            if (lifeTime.ValueRO.Value <= 0)
            {
                ecb.DestroyEntity(entity);
            }
        }
    }
}