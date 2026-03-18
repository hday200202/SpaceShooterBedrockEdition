using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public float speed = 5.0f;
    private SpaceShooterInputActions.StandardActions input;
    private Camera mainCamera;

    void Start()
    {
        // Initialize Input
        var inputActions = new SpaceShooterInputActions();
        input = inputActions.Standard;
        inputActions.Enable();

        mainCamera = Camera.main;

        LookAt(new Vector3(0f, 0f, 0f));
    }

    void Update() 
    {
        HandleMovement();
        LookAt(GetMouseWorldPos());
    }

    void HandleMovement()
    {
        Vector3 pos = transform.position;
        float yMovement = input.VerticalMovement.ReadValue<float>();
        float xMovement = input.HorizontalMovement.ReadValue<float>();
        pos.y += (speed * yMovement) * Time.deltaTime;
        pos.x += (speed * xMovement) * Time.deltaTime;
        transform.position = pos;
    }

    Vector3 GetMouseWorldPos()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        return Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, 10f));
    }
    
    void LookAt(Vector3 pos)
    {
        Vector3 direction = (pos - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
