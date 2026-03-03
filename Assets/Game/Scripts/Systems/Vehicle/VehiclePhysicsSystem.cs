using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

[UpdateInGroup(typeof(AfterPhysicsSystemGroup))] // После того как физика посчиталась
public partial struct VehiclePhysicsSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (velocity, mass, resistanceBuffer) in 
                 SystemAPI.Query<RefRW<PhysicsVelocity>, RefRO<PhysicsMass>, DynamicBuffer<ResistanceBufferElement>>())
        {
            if (resistanceBuffer.IsEmpty) continue;

            float totalAddedMass = 0;
            foreach (var element in resistanceBuffer)
                totalAddedMass += element.AddedMass;
            float invMass = mass.ValueRO.InverseMass;
            if (invMass > 0) 
            {
                // Текущая масса машины (InverseMass — это 1/M)
                float carMass = 1.0f / invMass;
                // Коэффициент замедления (Закон сохранения импульса)
                float speedFactor = carMass / (carMass + totalAddedMass);
                // Применяем замедление к линейной скорости
                velocity.ValueRW.Linear *= speedFactor;
            }
            // Очищаем буфер для следующего кадра
            resistanceBuffer.Clear();
        }
    }
}