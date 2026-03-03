using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ZombieSpawnerAuthoring : MonoBehaviour
{
    public float SpawnRate = 2f;
    public float DifficultyIncreasePerMinute = 1f;
    
    // Список для инспектора
    public List<ZombiePrefabConfig> Prefabs;

    [System.Serializable]
    public struct ZombiePrefabConfig
    {
        public GameObject Prefab;
        public int Difficulty;
    }

    public class Baker : Baker<ZombieSpawnerAuthoring>
    {
        public override void Bake(ZombieSpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            
            AddComponent(entity, new ZombieSpawner
            {
                SpawnRate = authoring.SpawnRate,
                DifficultyFactor = authoring.DifficultyIncreasePerMinute
            });

            // Создаем буфер (список) префабов
            var buffer = AddBuffer<ZombieSpawnerPrefab>(entity);
            foreach (var item in authoring.Prefabs)
            {
                buffer.Add(new ZombieSpawnerPrefab
                {
                    Prefab = GetEntity(item.Prefab, TransformUsageFlags.Dynamic),
                    DifficultyLevel = item.Difficulty
                });
            }
        }
    }
}