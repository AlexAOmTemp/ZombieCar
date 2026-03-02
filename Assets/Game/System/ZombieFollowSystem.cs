using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[BurstCompile]
public partial struct ZombieFollowSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // 1. Быстрый поиск игрока через Singleton
        if (!SystemAPI.HasSingleton<VehicleComponent>()) return;
        
        Entity playerEntity = SystemAPI.GetSingletonEntity<VehicleComponent>();
        float3 playerPos = SystemAPI.GetComponent<LocalTransform>(playerEntity).Position;
        float dt = SystemAPI.Time.DeltaTime;

        // 2. Запуск многопоточного Job
        // .WithNone<DeadTag>() — ключевой момент, мертвые не ходят!
        new ZombieFollowJob
        {
            PlayerPos = playerPos,
            DeltaTime = dt
        }.ScheduleParallel(); 
    }
}

[BurstCompile]
[WithNone(typeof(DeadTag))] 
[WithDisabled(typeof(KnockbackState))]
public partial struct ZombieFollowJob : IJobEntity
{
    public float3 PlayerPos;
    public float DeltaTime;

    // Убрали DeadTag из аргументов совсем
    void Execute(ref PhysicsVelocity velocity, ref LocalTransform transform, in ZombieSpeed speed)
    {
        float3 toPlayer = PlayerPos - transform.Position;
        if (math.lengthsq(toPlayer) < 0.1f) return;

        float3 direction = math.normalize(toPlayer);
        direction.y = 0;

        velocity.Linear = direction * speed.Value + new float3(0, velocity.Linear.y, 0);

        if (math.lengthsq(direction) > 0.001f)
        {
            quaternion targetRot = quaternion.LookRotationSafe(direction, math.up());
            transform.Rotation = math.slerp(transform.Rotation, targetRot, DeltaTime * 5f);
        }
    }
}
