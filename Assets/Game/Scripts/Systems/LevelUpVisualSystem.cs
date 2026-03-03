using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial struct LevelUpVisualSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;
        foreach (var (transform, progress) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<PlayerProgress>>())
        {
            if (progress.ValueRO.LevelUpEffectTimer > 0)
            {
                progress.ValueRW.LevelUpEffectTimer -= dt;
                // Пока таймер идет, машина чуть больше (пульсирует)
                transform.ValueRW.Scale = 1.0f + math.sin(progress.ValueRO.LevelUpEffectTimer * 10f) * 0.2f;
            }
            else
            {
                transform.ValueRW.Scale = 1.0f; // Возвращаем обычный размер
            }
        }
    }
}