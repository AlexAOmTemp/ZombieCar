using Unity.Entities;
using UnityEngine;

namespace Game.System
{
    public partial struct LogAndUiSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (log, entity) in SystemAPI.Query<DamageLogEvent>().WithEntityAccess())
            {
                if (log.Target == DamageTargetType.Vehicle)
                {
                    Debug.Log($"<color=red>МАШИНА: -{log.Amount} HP (Осталось: {log.CurrentHp})</color>");
                }
                else
                {
                    Debug.Log($"Зомби получил {log.Amount} урона");
                }

                ecb.DestroyEntity(entity);
            }
        }
    }
}

