using Game.Shared;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PhysicsSystemGroup))] // Используем общую физическую группу
public partial struct VehicleDamageSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.HasSingleton<SimulationSingleton>()) return;

        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

        state.Dependency = new CollisionDamageJob
        {
            Ecb = ecb,
            ZombieLookup = SystemAPI.GetComponentLookup<ZombieTag>(true),
            VehicleLookup = SystemAPI.GetComponentLookup<VehicleComponent>(true),
            AttackLookup = SystemAPI.GetComponentLookup<AttackData>(false),
            DeadLookup = SystemAPI.GetComponentLookup<DeadTag>(true),
            CurrentTime = (float)SystemAPI.Time.ElapsedTime
        }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
    }
}

[BurstCompile]
struct CollisionDamageJob : ICollisionEventsJob
{
    public EntityCommandBuffer.ParallelWriter Ecb;
    [ReadOnly] public ComponentLookup<ZombieTag> ZombieLookup;
    [ReadOnly] public ComponentLookup<VehicleComponent> VehicleLookup;
    [ReadOnly] public ComponentLookup<DeadTag> DeadLookup;
    public ComponentLookup<AttackData> AttackLookup;
    public float CurrentTime;

    public void Execute(CollisionEvent collisionEvent)
    {
        Entity a = collisionEvent.EntityA;
        Entity b = collisionEvent.EntityB;

        bool isZombieA = ZombieLookup.HasComponent(a);
        bool isZombieB = ZombieLookup.HasComponent(b);
        bool isVehicleA = VehicleLookup.HasComponent(a);
        bool isVehicleB = VehicleLookup.HasComponent(b);

        if ((isZombieA && isVehicleB) || (isZombieB && isVehicleA))
        {
            Entity zombie = isZombieA ? a : b;
            Entity vehicle = isVehicleA ? a : b;

            //УКУСЫ ЗОМБИ
            if (DeadLookup.HasComponent(zombie)) return;
            var attack = AttackLookup[zombie];
            if (CurrentTime > attack.NextAttackTime)
            {
                // Наносим урон машине через буфер
                Ecb.AppendToBuffer(0, vehicle, new DamageBufferElement { Amount = attack.Damage });
                
                // Обновляем таймер следующей атаки
                attack.NextAttackTime = CurrentTime + attack.AttackRate;
                AttackLookup[zombie] = attack;
            }
        }
    }
}

