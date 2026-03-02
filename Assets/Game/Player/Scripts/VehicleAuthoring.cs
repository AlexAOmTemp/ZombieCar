using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using Unity.Physics.GraphicsIntegration;

// Этот скрипт вешается на объект в SubScene
public class VehicleAuthoring : MonoBehaviour
{
    public float Speed = 20f;
    public float MaxHP = 1000f;

    public class Baker : Baker<VehicleAuthoring>
    {
        public override void Bake(VehicleAuthoring authoring)
        {
            // Это сущность нашего ПУСТОГО ROOT
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            // 1. Все статы и логика вешаются на Root
            AddComponent(entity, new VehicleComponent
            {
                MaxSpeed = authoring.Speed,
                ReverseSpeedFactor = 0.5f,
                Acceleration = 40000f,
                TurnSpeed = 5f,
                GripForce = 0.98f,
                RideHeight = 1f,
                SpringStiffness = 50f,
                DamperFactor = 10f,
            });

            AddComponent(entity, new VehicleInput());

            // 2. ФИЗИКА (Теперь она двигает весь Root целиком)
            var mass = PhysicsMass.CreateDynamic(MassProperties.UnitSphere, 3000f);
            mass.CenterOfMass = new float3(0, -1f, 0);
            mass.InverseInertia.x = 0;
            mass.InverseInertia.z = 0;
            mass.InverseInertia.y *= 0.01f; 
            AddComponent(entity, mass);

            AddComponent(entity, new PhysicsDamping {Linear = 1f, Angular = 10f});
            AddComponent(entity, new PhysicsVelocity());

            // 3. Остальные данные
            AddComponent(entity, new Health {Current = authoring.MaxHP, Max = authoring.MaxHP});
            AddComponent(entity, new RammingData {BaseDamageMultiplier = 2.0f, MinSpeedToRam = 5.0f});

            // Буферы теперь живут на Root
            AddBuffer<DamageBufferElement>(entity);
            AddBuffer<ResistanceBufferElement>(entity);

            // Прогресс и магнетизм
            AddComponent(entity, new PlayerProgress {NextLevelXP = 100});
            AddComponent(entity, new ResourceMagnet {Radius = 3f, PullSpeed = 20f});

            // Интерполяция (чтобы модель не дергалась)
            AddComponent(entity, new PhysicsGraphicalInterpolationBuffer());
        }
    }
}

