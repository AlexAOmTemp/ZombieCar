using Game.Shared;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

public partial struct TurretSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        // Собираем всех зомби
        var zombieQuery = SystemAPI.QueryBuilder().WithAll<LocalTransform, ZombieTag>().Build();
        if (zombieQuery.IsEmpty) return;
        var zombieTransforms = zombieQuery.ToComponentDataArray<LocalTransform>(state.WorldUpdateAllocator);

        // Цикл по турелям
        foreach (var (attack, prefab, ltw) in 
                 SystemAPI.Query<RefRW<AttackData>, RefRO<BulletPrefabReference>, RefRO<LocalToWorld>>())
        {
            if (SystemAPI.Time.ElapsedTime < attack.ValueRO.NextAttackTime) continue;

            float3 myWorldPos = ltw.ValueRO.Position; 
            float minDistSq = attack.ValueRO.Range * attack.ValueRO.Range;
            float3 targetDir = float3.zero;
            bool foundTarget = false;

            for (int i = 0; i < zombieTransforms.Length; i++)
            {
                float dSq = math.distancesq(myWorldPos, zombieTransforms[i].Position);
                if (dSq < minDistSq)
                {
                    minDistSq = dSq;
                    targetDir = math.normalize(zombieTransforms[i].Position - myWorldPos);
                    foundTarget = true;
                }
            }

            if (foundTarget)
            {
                Entity bullet = ecb.Instantiate(prefab.ValueRO.Value);
                var bTrans = LocalTransform.FromPositionRotation(myWorldPos + targetDir, 
                             quaternion.LookRotationSafe(targetDir, math.up()));
                bTrans.Scale = 0.2f;

                ecb.SetComponent(bullet, bTrans);
                ecb.AddComponent(bullet, new BulletDamage { Value = attack.ValueRO.Damage });
                ecb.SetComponent(bullet, new PhysicsVelocity { Linear = targetDir * 20f });
                
                // Добавляем пуле урон из AttackData турели!
                ecb.AddComponent(bullet, new DamageEvent { Amount = attack.ValueRO.Damage });

                attack.ValueRW.NextAttackTime = (float)SystemAPI.Time.ElapsedTime + attack.ValueRO.AttackRate;
            }
        }
    }
}