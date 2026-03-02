using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;


public class CameraFollow : MonoBehaviour
{
    private EntityManager _entityManager;
    private EntityQuery _playerQuery;

    private void Start()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        // Используем LocalToWorld — это финальная мировая позиция после всех расчетов
        _playerQuery = _entityManager.CreateEntityQuery(typeof(VehicleComponent), typeof(LocalToWorld));
    }

    public float3 BaseOffset = new float3(0, 20, -15); // Базовый наклон
    public float LookAheadFactor = 0.5f; // Насколько сильно камера "убегает" вперед
    public float SmoothTime = 0.15f;

    private Vector3 _currentVelocity;
    private Vector3 _smoothedVelocity;
    private void LateUpdate()
    {
        if (_playerQuery.IsEmpty) return;

        Entity playerEntity = _playerQuery.GetSingletonEntity();

        // Нам нужна и позиция, и скорость машины
        LocalToWorld ltw = _entityManager.GetComponentData<LocalToWorld>(playerEntity);
        PhysicsVelocity velocity = _entityManager.GetComponentData<PhysicsVelocity>(playerEntity);
        
// Вместо прямой скорости используем отфильтрованную
// Если скорость меньше 1, считаем её нулевой (убирает дрожь на месте)
        float3 cleanVelocity = math.length(velocity.Linear) > 1f ? velocity.Linear : float3.zero;

// Плавный фильтр для самой скорости (чтобы камера не дергалась при резком разгоне)
        _smoothedVelocity = Vector3.Lerp(_smoothedVelocity, (Vector3)cleanVelocity, Time.deltaTime * 5f);

        float3 lookAhead = (float3)_smoothedVelocity * LookAheadFactor;
        Vector3 targetPos = (Vector3)(ltw.Position + BaseOffset + lookAhead);
        targetPos.y = BaseOffset.y;
        // 3. Плавное движение
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _currentVelocity, SmoothTime);

        // 4. Оставляем угол камеры статичным (настрой его один раз в инспекторе, например Rotation X: 60)
        // Чтобы камера не дергалась, LookAt лучше не использовать или нацелить его тоже с опережением
    }
}