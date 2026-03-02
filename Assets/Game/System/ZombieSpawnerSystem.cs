using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public partial struct ZombieSpawnerSystem : ISystem
{
    private Random _random;

    public void OnCreate(ref SystemState state) => _random = new Random(123);

    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        float3 playerPos = float3.zero;
        foreach (var transform in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<VehicleComponent>())
        {
            playerPos = transform.ValueRO.Position;
        }

        double time = SystemAPI.Time.ElapsedTime;

        // Ищем спавнеры и их буферы префабов
        foreach (var (spawner, prefabs, entity) in 
                 SystemAPI.Query<RefRW<ZombieSpawner>, DynamicBuffer<ZombieSpawnerPrefab>>().WithEntityAccess())
        {
            if (time > spawner.ValueRO.NextSpawnTime)
            {
                // 1. Рассчитываем текущую сложность (растет каждую минуту)
                float currentDifficulty = (float)(time / 60.0) * spawner.ValueRO.DifficultyFactor;

                // 2. Фильтруем префабы, доступные на этой сложности
                // Собираем индексы подходящих зомби
                var availableIndices = new System.Collections.Generic.List<int>();
                for (int i = 0; i < prefabs.Length; i++)
                {
                    if (prefabs[i].DifficultyLevel <= (int)currentDifficulty)
                        availableIndices.Add(i);
                }

                if (availableIndices.Count > 0)
                {
                    // 3. Выбираем случайного из доступных
                    int randomIndex = availableIndices[_random.NextInt(0, availableIndices.Count)];
                    Entity selectedPrefab = prefabs[randomIndex].Prefab;

                    // 4. Спавним
                    Entity newZombie = ecb.Instantiate(selectedPrefab);

                    float angle = _random.NextFloat(0, math.PI * 2);
                    float distance = _random.NextFloat(18, 25);
                    float3 spawnOffset = new float3(math.cos(angle) * distance, 1, math.sin(angle) * distance);

                    ecb.SetComponent(newZombie, LocalTransform.FromPosition(playerPos + spawnOffset));
                }

                spawner.ValueRW.NextSpawnTime = (float)time + spawner.ValueRO.SpawnRate;
            }
        }
    }
}