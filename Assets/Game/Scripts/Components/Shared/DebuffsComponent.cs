using Unity.Entities;

public struct SpeedDebuff : IComponentData
{
    public float Multiplier;   // например 0.6
    public float TimeLeft;
}