using Unity.Entities;

public struct Health : IComponentData
{
    public float Current;
    public float Max;
}

public struct DamageEvent : IComponentData
{
    public float Amount;
}

[InternalBufferCapacity(8)] // Оптимизация: резервируем место под 8 попаданий
public struct DamageBufferElement : IBufferElementData
{
    public float Amount;
    // Можно добавить, кто нанес урон (Entity source)
}

public struct ContactDamage : IComponentData
{
    public float Value;
}

public enum DamageTargetType
{
    Zombie,
    Vehicle
}

public struct DamageLogEvent : IComponentData
{
    public float Amount;
    public DamageTargetType Target;
    public float CurrentHp;
}

public struct VehicleHitTag : IComponentData 
{ 
    public float Amount; 
    public float CurrentHp; 
}

public struct GameOverTag : IComponentData
{
}

public struct DeadTag : IComponentData, IEnableableComponent { }