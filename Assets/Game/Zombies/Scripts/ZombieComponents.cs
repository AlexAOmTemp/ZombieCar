using Unity.Entities;

// Сами данные спавнера
public struct ZombieSpawner : IComponentData
{
    public float SpawnRate;        // Раз в сколько секунд спавнить
    public float NextSpawnTime;    // Время следующего спавна
    public float DifficultyFactor; // Насколько быстро растет сложность
}

// Тег, чтобы отличать зомби от машины
public struct ZombieTag : IComponentData
{
    public float Weight;
}

// Описываем одного зомби в списке спавнера
public struct ZombieSpawnerPrefab : IBufferElementData
{
    public Entity Prefab;
    public int DifficultyLevel; // С какого уровня сложности он начинает спавниться
}

public struct ZombieSpeed : IComponentData
{
    public float Value;
}

public struct KnockbackSettings : IComponentData
{
    public float HorizontalMultiplier;
    public float UpwardMultiplier;
    public float AngularMultiplier;
}