using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;


[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PhysicsSystemGroup))]
public partial struct KnockbackSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI
            .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged)
            .AsParallelWriter();

        float dt = SystemAPI.Time.DeltaTime;

        state.Dependency = new KnockbackApplyJob
        {
            Ecb = ecb
        }.ScheduleParallel(state.Dependency);

        state.Dependency = new KnockbackUpdateJob
        {
            DeltaTime = dt,
            Ecb = ecb
        }.ScheduleParallel(state.Dependency);
    }

    [BurstCompile]
    [WithDisabled(typeof(KnockbackState))]
    public partial struct KnockbackApplyJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;

        void Execute(
            [EntityIndexInQuery] int sortKey,
            Entity entity,
            ref PhysicsVelocity velocity,
            ref KnockbackState state,
            in KnockbackEvent evt,
            in KnockbackSettings settings)
        {
            float horizontalSpeed = evt.Force * 0.8f;  // ≈ 6 м/с
            float verticalSpeed = 3f;              // фиксированно

            velocity.Linear = 
                evt.Direction * horizontalSpeed +
                new float3(0, verticalSpeed, 0);
            
            float3 horizontal = evt.Direction * horizontalSpeed;
            horizontal.y = 0f;

            float3 upward = new float3(0, math.sqrt(evt.Force) * settings.UpwardMultiplier, 0);
            
            velocity.Angular = new float3(1f * settings.AngularMultiplier, 0, 1f * settings.AngularMultiplier);

            state.Phase = 0;
            state.GroundedTime = 0;

            Ecb.SetComponentEnabled<KnockbackState>(sortKey, entity, true);
            Ecb.RemoveComponent<KnockbackEvent>(sortKey, entity);
        }
    }

    [BurstCompile]
    [WithAll(typeof(KnockbackState))]
    public partial struct KnockbackUpdateJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter Ecb;

        void Execute(
            [EntityIndexInQuery] int sortKey,
            Entity entity,
            ref KnockbackState state,
            ref PhysicsVelocity velocity,
            ref LocalTransform transform)
        {
            // ===== ФАЗА ПОЛЕТА =====
            if (state.Phase == 0)
            {
                if (math.abs(velocity.Linear.y) < 0.1f &&
                    math.lengthsq(velocity.Linear) < 0.5f)
                {
                    state.Phase = 1;
                    state.GroundedTime = 1f;
                }

                return;
            }

            // ===== ФАЗА ЛЕЖАНИЯ =====
            state.GroundedTime -= DeltaTime;

            if (state.GroundedTime > 0f)
                return;

            // ===== ВОССТАНОВЛЕНИЕ =====
            velocity.Angular = float3.zero;

            float3 forward = math.forward(transform.Rotation);
            float3 flatForward =
                math.normalize(new float3(forward.x, 0f, forward.z));

            transform.Rotation =
                quaternion.LookRotationSafe(flatForward, math.up());

            // ВЫКЛЮЧАЕМ компонент через ECB
            Ecb.SetComponentEnabled<KnockbackState>(sortKey, entity, false);
        }
    }
}