using Unity.Entities;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct RamCooldownSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;

        var ecb = SystemAPI
            .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (cooldown, entity) in
                 SystemAPI.Query<RefRW<RamCooldown>>()
                     .WithEntityAccess())
        {
            cooldown.ValueRW.TimeLeft -= dt;

            if (cooldown.ValueRO.TimeLeft <= 0f)
            {
                ecb.RemoveComponent<RamCooldown>(entity);
            }
        }
    }
}