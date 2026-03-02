using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(Unity.Physics.Systems.PhysicsSystemGroup))]
public partial struct VehicleMovementSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.HasSingleton<PhysicsWorldSingleton>())
            return;

        float dt = SystemAPI.Time.DeltaTime;
        var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;

        foreach (var (velocity, mass, transform, input, vehicle, entity) in
                 SystemAPI.Query<
                         RefRW<PhysicsVelocity>,
                         RefRO<PhysicsMass>,
                         RefRO<LocalTransform>,
                         RefRO<VehicleInput>,
                         RefRO<VehicleComponent>>()
                     .WithEntityAccess())
        {
            float speedMultiplier = 1f;

            if (SystemAPI.HasComponent<SpeedDebuff>(entity))
            {
                speedMultiplier =
                    SystemAPI.GetComponent<SpeedDebuff>(entity).Multiplier;
            }

            HandleMovement(ref velocity.ValueRW,
                mass.ValueRO,
                transform.ValueRO,
                input.ValueRO,
                vehicle.ValueRO,
                dt,
                speedMultiplier);

            HandleTurning(ref velocity.ValueRW,
                input.ValueRO,
                vehicle.ValueRO,
                dt);

            HandleSuspension(ref velocity.ValueRW,
                mass.ValueRO,
                transform.ValueRO,
                vehicle.ValueRO,
                collisionWorld,
                dt);

            HandleGrip(ref velocity.ValueRW,
                transform.ValueRO,
                vehicle.ValueRO);

            LockRotation(ref velocity.ValueRW);
        }
    }

    // =============================
    //            METHODS
    // =============================
    private static void HandleMovement(
        ref PhysicsVelocity velocity,
        PhysicsMass mass,
        LocalTransform transform,
        VehicleInput input,
        VehicleComponent vehicle,
        float dt,
        float speedMultiplier)
    {
        float3 forward = math.mul(transform.Rotation, new float3(0, 0, 1));
        float3 currentVel = velocity.Linear;

        float horizontalSpeed = math.length(new float2(currentVel.x, currentVel.z));
        float2 moveInput = input.Movement;

        if (math.abs(moveInput.y) > 0.05f)
        {
            float limit = moveInput.y > 0
                ? vehicle.MaxSpeed
                : vehicle.MaxSpeed * vehicle.ReverseSpeedFactor;

            if (horizontalSpeed < limit)
            {
                float3 impulse = forward *
                                 moveInput.y *
                                 vehicle.Acceleration *
                                 dt *
                                 speedMultiplier;

                velocity.Linear += impulse * mass.InverseMass;
            }
        }

        // Торможение
        if (math.abs(moveInput.y) < 0.05f && horizontalSpeed > 0.1f)
        {
            float3 brakeDir = -math.normalize(new float3(currentVel.x, 0, currentVel.z));
            velocity.Linear += brakeDir * 5f * dt;
        }
    }

    private static void HandleTurning(
        ref PhysicsVelocity velocity,
        VehicleInput input,
        VehicleComponent vehicle,
        float dt)
    {
        if (math.abs(input.Movement.x) > 0.05f)
        {
            float targetAngularY = input.Movement.x * vehicle.TurnSpeed;
            velocity.Angular.y =
                math.lerp(velocity.Angular.y, targetAngularY, dt * 5f);
        }
    }

    private static void HandleSuspension(
        ref PhysicsVelocity velocity,
        PhysicsMass mass,
        LocalTransform transform,
        VehicleComponent vehicle,
        CollisionWorld collisionWorld,
        float dt)
    {
        float3 position = transform.Position;
        float3 up = math.mul(transform.Rotation, new float3(0, 1, 0));

        float targetHeight = vehicle.RideHeight;
        float rayDistance = targetHeight * 1.5f;

        var rayInput = new RaycastInput
        {
            Start = position,
            End = position - (up * rayDistance),
            Filter = new CollisionFilter
            {
                BelongsTo = 1 << 7,
                CollidesWith = 1 << 10
            }
        };

        if (!collisionWorld.CastRay(rayInput, out var hit))
            return;

        float currentDist = hit.Fraction * rayDistance;

        if (currentDist >= targetHeight)
            return;

        float carMass = 1f / mass.InverseMass;
        float compression = (targetHeight - currentDist) / targetHeight;

        float springForce =
            carMass * 9.81f *
            vehicle.SpringStiffness *
            compression;

        float currentUpSpeed = math.dot(velocity.Linear, up);
        float damperForce =
            currentUpSpeed *
            vehicle.DamperFactor *
            carMass;

        float3 totalForce = up * (springForce - damperForce);
        velocity.Linear += totalForce * mass.InverseMass * dt;
    }

    private static void HandleGrip(
        ref PhysicsVelocity velocity,
        LocalTransform transform,
        VehicleComponent vehicle)
    {
        float3 right = math.mul(transform.Rotation, new float3(1, 0, 0));
        float sideSpeed = math.dot(velocity.Linear, right);

        velocity.Linear -= right * sideSpeed * vehicle.GripForce;
    }

    private static void LockRotation(ref PhysicsVelocity velocity)
    {
        float3 ang = velocity.Angular;
        ang.x = 0;
        ang.z = 0;
        velocity.Angular = ang;
    }
}