// Сами данные в ECS

using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

public struct VehicleComponent : IComponentData
{
    public float MaxSpeed; // Максимальная скорость вперед
    public float ReverseSpeedFactor; // Множитель скорости для заднего хода (например, 0.5)
    public float Acceleration; // Сила разгона (для 3000кг нужно около 20000-40000)
    public float TurnSpeed; // Скорость поворота
    public float GripForce; // Насколько сильно гасится занос (0.0 - 1.0)
    public float RideHeight; // Желаемая высота (например, 0.2 или 2.0)
    public float SpringStiffness; // Жесткость (попробуй начать с 50.0)
    public float DamperFactor; // Затухание (попробуй 10.0)
}

public struct VehicleInput : IComponentData
{
    public float2 Movement;
}

public struct SmokeTag : IComponentData
{
}

public struct PlayerProgress : IComponentData
{
    public int Level;
    public float Experience;
    public float NextLevelXP;
    public float LevelUpEffectTimer; // Таймер эффекта
}

public struct RammingData : IComponentData
{
    public float BaseDamageMultiplier; // Общий множитель урона машины
    public float MinSpeedToRam; // Минимальная скорость
}

public struct RamCooldown : IComponentData
{
    public float TimeLeft;
}

[InternalBufferCapacity(2)]
public struct BumperInfoElement : IBufferElementData
{
    public ColliderKey Key;
    public float BumperEfficiency;
}

//Для торможения при таране зомби
[InternalBufferCapacity(8)]
public struct ResistanceBufferElement : IBufferElementData
{
    public float AddedMass; // Масса зомби (например, 90f)
}

