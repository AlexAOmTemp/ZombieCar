using Unity.Burst;
using Unity.Entities;
using Unity.Physics;

[BurstCompile]
public partial struct ZombieHealthSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
        
        new HealthJob
        {
            Ecb = ecb
        }.ScheduleParallel(); 
    }
}

[BurstCompile]
// Исключаем мертвых и тех, кто уже на таймере удаления
[WithNone(typeof(DeadTag), typeof(LifeTime))] 
partial struct HealthJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter Ecb;
    
    public void Execute(
        Entity entity,
        [EntityIndexInQuery] int sortKey,
        ref Health health,
        DynamicBuffer<DamageBufferElement> damageBuffer)
    {
        float totalDamage = 0;
        for (int i = 0; i < damageBuffer.Length; i++)
        {
            totalDamage += damageBuffer[i].Amount;
        }

        if (totalDamage <= 0)
            return;

        health.Current -= totalDamage;
        damageBuffer.Clear();

        // ======================
        // СОЗДАЕМ LOG EVENT
        // ======================
        Entity log = Ecb.CreateEntity(sortKey);
        Ecb.AddComponent(sortKey, log, new DamageLogEvent
        {
            Amount = totalDamage,
            Target = DamageTargetType.Zombie,
            CurrentHp = health.Current
        });

        if (health.Current <= 0)
        {
            Ecb.AddComponent(sortKey, entity, new DeadTag());
            Ecb.AddComponent(sortKey, entity, new LifeTime { Value = 2.0f });

            var ragdollMass =
                PhysicsMass.CreateDynamic(MassProperties.UnitSphere, 90f);

            Ecb.SetComponent(sortKey, entity, ragdollMass);
        }
    }
}
