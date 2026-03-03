using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public class ResourceAuthoring : MonoBehaviour
{
    public float VisualScale = 0.3f;

    public class Baker : Baker<ResourceAuthoring>
    {
        public override void Bake(ResourceAuthoring authoring)
        {
            // Передаем флаг Dynamic и просим Unity сразу применить Scale
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            // ВАЖНО: В Unity 6 масштаб префаба лучше задавать так:
            AddComponent(entity, new ResourceTag());
            //AddComponent(entity, new PhysicsVelocity());

            // Вместо SetComponent(LocalTransform), который вызывает ошибку:
            // Мы просто добавляем настройки массы
            /*var mass = PhysicsMass.CreateDynamic(MassProperties.UnitSphere, 0.1f);
            AddComponent(entity, mass);*/
        }
    }
}