using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Transforms;
using UnityEngine;

public class BumperAuthoring : MonoBehaviour
{
    public float BumperEfficiency = 1.0f;


    public class Baker : Baker<BumperAuthoring>
    {
        public override void Bake(BumperAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            var rootVehicle = authoring.GetComponentInParent<VehicleAuthoring>();
            if (rootVehicle == null) return;
            var rootEntity = GetEntity(rootVehicle, TransformUsageFlags.Dynamic);

            AddComponent(entity, new Parent { Value = rootEntity });
            AddComponent(entity, new BumperTag { 
                ParentVehicle = rootEntity, 
                BumperEfficiency = authoring.BumperEfficiency 
            });
        }
    }
}

