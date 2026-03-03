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
            
            AddComponent(entity, new ResourceTag());
        }
    }
}