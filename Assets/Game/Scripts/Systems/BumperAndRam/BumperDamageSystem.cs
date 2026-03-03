using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct BumperDamageSystem : ISystem
{
    private ComponentLookup<BumperTag> _bumperLookup;
    private ComponentLookup<ZombieTag> _zombieLookup;
    private ComponentLookup<RammingData> _rammingLookup;
    private ComponentLookup<PhysicsVelocity> _velocityLookup;
    private ComponentLookup<DeadTag> _deadLookup;
    public ComponentLookup<RamCooldown> _cooldownLookup;

    private BufferLookup<DamageBufferElement> _damageBufferLookup;
    private BufferLookup<ResistanceBufferElement> _resistanceBufferLookup;

    public void OnCreate(ref SystemState state)
    {
        _bumperLookup = state.GetComponentLookup<BumperTag>(true);
        _zombieLookup = state.GetComponentLookup<ZombieTag>(true);
        _rammingLookup = state.GetComponentLookup<RammingData>(true);
        _velocityLookup = state.GetComponentLookup<PhysicsVelocity>(false);
        _deadLookup = state.GetComponentLookup<DeadTag>(true);
        _cooldownLookup = state.GetComponentLookup<RamCooldown>(true);
        _damageBufferLookup = state.GetBufferLookup<DamageBufferElement>(true);
        _resistanceBufferLookup = state.GetBufferLookup<ResistanceBufferElement>(true);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _bumperLookup.Update(ref state);
        _zombieLookup.Update(ref state);
        _rammingLookup.Update(ref state);
        _velocityLookup.Update(ref state);
        _deadLookup.Update(ref state);
        _damageBufferLookup.Update(ref state);
        _resistanceBufferLookup.Update(ref state);
        _cooldownLookup.Update(ref state);

        var ecb = SystemAPI
            .GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged)
            .AsParallelWriter();

        state.Dependency = new BumperCollisionJob
        {
            Ecb = ecb,
            BumperLookup = _bumperLookup,
            ZombieLookup = _zombieLookup,
            RammingLookup = _rammingLookup,
            VelocityLookup = _velocityLookup,
            DeadLookup = _deadLookup,
            DamageBufferLookup = _damageBufferLookup,
            ResistanceBufferLookup = _resistanceBufferLookup,
            CooldownLookup = _cooldownLookup,
        }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
    }
}

[BurstCompile]
struct BumperCollisionJob : ICollisionEventsJob
{
    public EntityCommandBuffer.ParallelWriter Ecb;

    [ReadOnly] public ComponentLookup<BumperTag> BumperLookup;
    [ReadOnly] public ComponentLookup<ZombieTag> ZombieLookup;
    [ReadOnly] public ComponentLookup<RammingData> RammingLookup;
    [ReadOnly] public ComponentLookup<DeadTag> DeadLookup;
    [ReadOnly] public ComponentLookup<RamCooldown> CooldownLookup;

    [ReadOnly] public BufferLookup<DamageBufferElement> DamageBufferLookup;
    [ReadOnly] public BufferLookup<ResistanceBufferElement> ResistanceBufferLookup;

    public ComponentLookup<PhysicsVelocity> VelocityLookup;

    public void Execute(CollisionEvent collisionEvent)
    {
        Entity a = collisionEvent.EntityA;
        Entity b = collisionEvent.EntityB;

        if (DeadLookup.HasComponent(a) || DeadLookup.HasComponent(b))
            return;

        bool bumperA = BumperLookup.HasComponent(a);
        bool zombieA = ZombieLookup.HasComponent(a);

        bool bumperB = BumperLookup.HasComponent(b);
        bool zombieB = ZombieLookup.HasComponent(b);

        if (!((bumperA && zombieB) || (bumperB && zombieA)))
            return;

        bool bumperWasEntityA = bumperA;

        Entity bumperEntity = bumperA ? a : b;
        Entity zombieEntity = zombieA ? a : b;

        if (!DamageBufferLookup.HasBuffer(zombieEntity))
            return;

        var bumper = BumperLookup[bumperEntity];
        Entity vehicle = bumper.ParentVehicle;

        if (!VelocityLookup.HasComponent(vehicle) ||
            !RammingLookup.HasComponent(vehicle))
            return;

        float3 carVelocity = VelocityLookup[vehicle].Linear;
        float speed = math.length(carVelocity);

        var ram = RammingLookup[vehicle];

        if (speed <= ram.MinSpeedToRam)
            return;

        if (CooldownLookup.HasComponent(zombieEntity))
            return;

        var zombieData = ZombieLookup[zombieEntity];

        // ===== DAMAGE =====
        float damage = speed * ram.BaseDamageMultiplier * bumper.BumperEfficiency;

        Ecb.AppendToBuffer(collisionEvent.BodyIndexA, zombieEntity,
            new DamageBufferElement {Amount = damage});

        Ecb.AddComponent(collisionEvent.BodyIndexA, zombieEntity,
            new RamCooldown {TimeLeft = 0.3f});

        // ===== SPEED DEBUFF =====
        Ecb.AddComponent(collisionEvent.BodyIndexA, vehicle, new SpeedDebuff
        {
            Multiplier = math.clamp(1f - (zombieData.Weight / 200f), 0.3f, 0.9f),
            TimeLeft = 0.5f
        });

        // ===== KNOCKBACK EVENT =====
        float3 bounceDir = math.normalize(carVelocity);
        bounceDir.y = 0f;
        
        if (bumperWasEntityA)
            bounceDir *= -1f;

        float massFactor = math.clamp(40f / zombieData.Weight, 0.2f, 1.2f);
        float force = math.clamp(speed * 0.4f, 2f, 12f) * massFactor;

        Ecb.AddComponent(collisionEvent.BodyIndexA, zombieEntity, new KnockbackEvent
        {
            Direction = bounceDir,
            Force = force,
            Duration = 1f
        });
    }
}