using Unity.Entities;
using Unity.Physics;
using UnityEngine;

namespace Game.Crystalls
{
    public class ResourceSpawnerAuthoring : MonoBehaviour
    {
        public GameObject CrystalPrefab;

        public class Baker : Baker<ResourceSpawnerAuthoring>
        {
            public override void Bake(ResourceSpawnerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new ResourceSpawner
                {
                    CrystalPrefab = GetEntity(authoring.CrystalPrefab, TransformUsageFlags.Dynamic)
                });
            }
        }
    }
}