using Unity.Entities;
using Unity.Mathematics;

public struct ResourceTag : IComponentData { }

public struct ResourceSpawner : IComponentData
{
    public Entity CrystalPrefab;
}

// Добавим в машину "магнит"
public struct ResourceMagnet : IComponentData
{
    public float Radius;
    public float PullSpeed;
}