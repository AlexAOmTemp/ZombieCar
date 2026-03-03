using Game.Shared;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;

namespace Game.Zombies.Scripts
{
    public class ZombieAuthoring : MonoBehaviour
    {
        public float MaxHP = 20f;
        public float DamagePerHit = 10f;
        public float Mass = 90f;
        public float Speed = 3f;
        public float HorizontalMultiplier = 1f;
        public float UpwardMultiplier = 1f;
        public float AngularMultiplier = 1f;
        public class Baker : Baker<ZombieAuthoring>
        {
            public override void Bake(ZombieAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new ZombieTag {Weight = authoring.Mass});

                // Блокируем наклон зомби, чтобы они стояли вертикально
                var mass = PhysicsMass.CreateDynamic(MassProperties.UnitSphere, authoring.Mass);
                mass.InverseInertia.x = 0f;
                mass.InverseInertia.z = 0f;
                AddComponent(entity, mass);

                AddComponent<KnockbackState>(entity);
                SetComponentEnabled<KnockbackState>(entity, false);
                // Добавляем скорость, чтобы они могли двигаться
                AddComponent(entity, new PhysicsVelocity());
                AddComponent(entity, new Health {Current = authoring.MaxHP, Max = authoring.MaxHP});
                AddComponent(entity, new AttackData {Damage = authoring.DamagePerHit, AttackRate = 1.0f, Range = 1.5f});
                AddComponent(entity, new ZombieSpeed {Value = authoring.Speed});
                AddComponent(entity, new KnockbackSettings
                {
                    HorizontalMultiplier = authoring.HorizontalMultiplier,
                    UpwardMultiplier = authoring.UpwardMultiplier,
                    AngularMultiplier = authoring.AngularMultiplier
                });
                AddBuffer<DamageBufferElement>(entity);
            }
        }
    }
}