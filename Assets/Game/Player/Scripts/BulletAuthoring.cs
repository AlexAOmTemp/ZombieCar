using Unity.Entities;
using Unity.Physics; // Обязательно!
using UnityEngine;

public class BulletAuthoring : MonoBehaviour
{
    public float TimeToLive = 2f;

    public class Baker : Baker<BulletAuthoring>
    {
        public override void Bake(BulletAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            
            // Нам ОБЯЗАТЕЛЬНО нужны эти компоненты, чтобы система могла задать скорость
            AddComponent(entity, new BulletTag());
            AddComponent(entity, new LifeTime { Value = authoring.TimeToLive });
            
            // Добавляем пустые компоненты физики, которые заполнит TurretSystem
            AddComponent(entity, new PhysicsVelocity());
            
            // Чтобы пуля не "зависала" и физика знала, что она легкая
            AddComponent(entity, PhysicsMass.CreateDynamic(MassProperties.UnitSphere, 0.1f));
        }
    }
}

public struct BulletTag : IComponentData { }

public struct LifeTime : IComponentData
{
    public float Value;
}