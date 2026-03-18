using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
        public float maxSpeed = 10.0f;
        public float acceleration = 20.0f;
        public float deceleration = 20.0f;

    private Vector2 velocity;
    private SpaceShooterInputActions.StandardActions input;

    void Awake() {
        // Initialize Input
        var inputActions = new SpaceShooterInputActions();
        input = inputActions.Standard;
        inputActions.Enable();
    }

    void Update() {
        HandleMovement();
        LookAt(GetMouseWorldPos());
    }

    void HandleMovement() {
        // Input Vector
        float xInput = input.HorizontalMovement.ReadValue<float>();
        float yInput = input.VerticalMovement.ReadValue<float>();
        Vector2 inputDir = new(xInput, yInput);

        // Accel / Deccel
        if (inputDir.magnitude > 0)
            velocity += acceleration * Time.deltaTime * inputDir;
        else
            velocity = Vector2.MoveTowards(velocity, Vector2.zero, deceleration * Time.deltaTime);

        // Move Player
        velocity = Vector2.ClampMagnitude(velocity, maxSpeed);
        transform.position += (Vector3)velocity * Time.deltaTime;
    }

    Vector3 GetMouseWorldPos() {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        return Camera.main.ScreenToWorldPoint(new(mouseScreenPos.x, mouseScreenPos.y, 10f));
    }
    
    void LookAt(Vector3 pos) {
        // Get normalized vector from player to mouse.
        // Get the direction vector's angle.
        // Set player rotation to that angle.
        Vector3 direction = (pos - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}