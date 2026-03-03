// Новый вспомогательный компонент

using Unity.Entities;

public struct BulletPrefabReference : IComponentData { public Entity Value; }
public struct BulletDamage : IComponentData { public float Value; }