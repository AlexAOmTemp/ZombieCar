using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct BulletCollisionSystem : ISystem
{
    private ComponentLookup<ZombieTag> _zombieLookup;
    private ComponentLookup<BulletTag> _bulletLookup;
    private ComponentLookup<LocalTransform> _transformLookup;
    private ComponentLookup<BulletDamage> _bulletDamageLookup; // НОВОЕ

    public void OnCreate(ref SystemState state)
    {
        _zombieLookup = state.GetComponentLookup<ZombieTag>(true);
        _bulletLookup = state.GetComponentLookup<BulletTag>(true);
        _transformLookup = state.GetComponentLookup<LocalTransform>(true);
        _bulletDamageLookup = state.GetComponentLookup<BulletDamage>(true); // НОВОЕ
    }

    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.HasSingleton<ResourceSpawner>()) return;

        _zombieLookup.Update(ref state);
        _bulletLookup.Update(ref state);
        _transformLookup.Update(ref state);
        _bulletDamageLookup.Update(ref state); // ОБНОВЛЯЕМ

        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged)
            .AsParallelWriter();

        state.Dependency = new BulletTriggerJob
        {
            Ecb = ecb,
            ZombieLookup = _zombieLookup,
            BulletLookup = _bulletLookup,
            BulletDamageLookup = _bulletDamageLookup // ПЕРЕДАЕМ В JOB
        }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
    }
}

[BurstCompile]
struct BulletTriggerJob : ITriggerEventsJob
{
    // Используем ParallelWriter для безопасности в потоках
    public EntityCommandBuffer.ParallelWriter Ecb;

    [ReadOnly] public ComponentLookup<ZombieTag> ZombieLookup;
    [ReadOnly] public ComponentLookup<BulletTag> BulletLookup;
    [ReadOnly] public ComponentLookup<BulletDamage> BulletDamageLookup;
    
    public void Execute(TriggerEvent triggerEvent)
    {
        Entity entityA = triggerEvent.EntityA;
        Entity entityB = triggerEvent.EntityB;

        bool isAZombie = ZombieLookup.HasComponent(entityA);
        bool isBZombie = ZombieLookup.HasComponent(entityB);
        bool isABullet = BulletLookup.HasComponent(entityA);
        bool isBBullet = BulletLookup.HasComponent(entityB);

        Entity zombieEntity = isAZombie ? entityA : (isBZombie ? entityB : Entity.Null);
        Entity bulletEntity = isABullet ? entityA : (isBBullet ? entityB : Entity.Null);

        if (zombieEntity != Entity.Null && bulletEntity != Entity.Null)
        {
            float damageFromBullet = BulletDamageLookup[bulletEntity].Value;

            // ВАЖНО: Вместо AddComponent используем AppendToBuffer
            // 0 здесь — это упрощенный sortKey (для TriggerJob это допустимо)
            Ecb.AppendToBuffer(0, zombieEntity, new DamageBufferElement { Amount = damageFromBullet });
            
            Ecb.DestroyEntity(0, bulletEntity); 
        }
    }
}