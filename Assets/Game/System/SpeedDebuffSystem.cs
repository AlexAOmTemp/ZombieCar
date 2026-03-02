using Unity.Entities;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct SpeedDebuffSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;

        var ecbSingleton =
            SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();

        var ecb =
            ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (debuff, entity) in
                 SystemAPI.Query<RefRW<SpeedDebuff>>()
                     .WithEntityAccess())
        {
            debuff.ValueRW.TimeLeft -= dt;

            if (debuff.ValueRO.TimeLeft <= 0f)
            {
                ecb.RemoveComponent<SpeedDebuff>(entity);
            }
        }
    }
}