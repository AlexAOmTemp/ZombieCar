using Unity.Entities;
using Unity.Mathematics;

public struct KnockbackState : IComponentData,IEnableableComponent
{
    public float TimeLeft;
    public float GroundedTime;
    public byte Phase; 
    // 0 = Flying
    // 1 = Grounded
}

public struct KnockbackEvent : IComponentData
{
    public float3 Direction;
    public float Force;
    public float Duration;
}

