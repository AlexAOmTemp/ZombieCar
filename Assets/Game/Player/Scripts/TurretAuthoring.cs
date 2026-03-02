using Game.Shared;
using Unity.Entities;
using UnityEngine;

public class TurretAuthoring : MonoBehaviour
{
    public GameObject BulletPrefab;
    public float Damage = 10f;
    public float FireRate = 0.5f;
    public float Range = 20f;

    public class Baker : Baker<TurretAuthoring>
    {
        public override void Bake(TurretAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            // Используем общую структуру AttackData
            AddComponent(entity, new AttackData {
                Damage = authoring.Damage,
                AttackRate = authoring.FireRate,
                Range = authoring.Range,
                NextAttackTime = 0
            });
            // Оставляем только ссылку на префаб в отдельном компоненте
            AddComponent(entity, new BulletPrefabReference { 
                Value = GetEntity(authoring.BulletPrefab, TransformUsageFlags.Dynamic) 
            });
        }
    }
}

// Новый вспомогательный компонент
public struct BulletPrefabReference : IComponentData { public Entity Value; }
public struct BulletDamage : IComponentData { public float Value; }