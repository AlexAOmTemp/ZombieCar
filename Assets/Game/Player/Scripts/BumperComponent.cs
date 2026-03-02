using Unity.Entities;
using Unity.Mathematics;

public struct BumperTag : IComponentData
{
    public Entity ParentVehicle; // Сама машина
    public float BumperEfficiency; // 1.0 для переднего, 0.5 для заднего
}


// Временный компонент для процесса запекания
public struct BumperBakingTag : IComponentData
{
    public float Efficiency;
}

