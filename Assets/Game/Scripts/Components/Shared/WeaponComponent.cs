using Unity.Entities;

namespace Game.Shared
{
    public struct AttackData : IComponentData
    {
        public float Damage;
        public float AttackRate;    // Раз в сколько секунд
        public float NextAttackTime; // Таймер
        public float Range;          // Дистанция (для зомби — 1.5, для турели — 20)
    }
}