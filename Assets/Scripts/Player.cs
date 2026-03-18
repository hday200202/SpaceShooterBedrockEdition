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
    }

    void Update() 
    {
        // Movement
        Vector3 pos = transform.position;
        float yMovement = input.VerticalMovement.ReadValue<float>();
        float xMovement = input.HorizontalMovement.ReadValue<float>();
        pos.y += (speed * yMovement) * Time.deltaTime;
        pos.x += (speed * xMovement) * Time.deltaTime;
        transform.position = pos;

        // Object to Mouse Vector
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, 10f));
        Vector3 direction = (mouseWorldPos - transform.position).normalized;

        // Make Player look at Mouse
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
