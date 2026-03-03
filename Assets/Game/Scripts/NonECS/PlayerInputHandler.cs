using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem; 

public class PlayerInputHandler : MonoBehaviour
{
    private EntityManager _entityManager;
    private EntityQuery _inputQuery;

    private void Start()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        // Ищем сущность, у которой есть компонент VehicleInput
        _inputQuery = _entityManager.CreateEntityQuery(typeof(VehicleInput));
    }

    private void Update()
    {
        Vector2 move = Vector2.zero;
        var keyboard = Keyboard.current;

        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed) move.y += 1f;
            if (keyboard.sKey.isPressed) move.y -= 1f;
            if (keyboard.aKey.isPressed) move.x -= 1f;
            if (keyboard.dKey.isPressed) move.x += 1f;
        }

        // Если нашли сущность игрока — записываем в неё данные
        if (_inputQuery.IsEmpty) 
            return;
        
        Entity player = _inputQuery.GetSingletonEntity();
        _entityManager.SetComponentData(player, new VehicleInput { 
            Movement = new float2(move.x, move.y) 
        });
    }
}