using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxSpeed = 5.0f;

    public Vector2 acceleration = new(40f, 40f);
    private Vector2 velocity;

    private float dodgeDelay = 0.5f;
    private float dodgeTimer = 0.5f;
    private float shootDelay = 0.1f;
    private float shootTimer = 0.1f;

    private SpaceShooterInputActions.StandardActions input;


    void Awake()
    {
        // Initialize Input
        var inputActions = new SpaceShooterInputActions();
        input = inputActions.Standard;
        inputActions.Enable();
    }


    void Update()
    {
        HandleMovement();
        LookAt(GetMouseWorldPos());
    }


    /*
        HandleMovement():
        - Get input values for this frame
        - Handle Acceleration
        - Handle Dodge
        - Handle Shoot
        - Move the player
    */
    void HandleMovement()
    {
        Vector2 inputDir = new(
            input.HorizontalMovement.ReadValue<float>(),
            input.VerticalMovement.ReadValue<float>()
        );

        // Dodge / Shoot
        bool shoot = input.Shoot.WasPressedThisFrame();
        bool dodge = input.Dodge.WasPressedThisFrame();

        dodgeTimer += Time.deltaTime;
        shootTimer += Time.deltaTime;

        HandleAccel(inputDir);

        if (dodge) HandleDodge();
        if (shoot) HandleShoot();

        // Move Player
        transform.position += (Vector3)velocity * Time.deltaTime;
    }


    /*
        GetMouseWorldPos()
        - Get Mouse Position in World Coordinates
    */
    Vector3 GetMouseWorldPos()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        return Camera.main.ScreenToWorldPoint(new(mouseScreenPos.x, mouseScreenPos.y, 10f));
    }


    /*
        LookAt():
        - Get normalized vector from player to mouse.
        - Get the direction vector's angle.
        - Set player rotation to that angle.
    */
    void LookAt(Vector3 pos)
    {
        var direction = GetPlayerToPosVector(pos).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }


    /*
        GetPlayerToPosVector(Vector3)
        - Get the vector from the player's 
          position to a world position.
    */
    Vector3 GetPlayerToPosVector(Vector3 pos)
    { return pos - transform.position; }


    /*
        HandleDodge()
        - Handle the player's dodge functionality
    */
    void HandleDodge()
    {   
        if (dodgeTimer >= dodgeDelay)
        {
            // Backwards dodge if not moving
            if (velocity.magnitude == 0) 
                velocity = -GetPlayerToPosVector(GetMouseWorldPos());

            velocity = 2.0f * maxSpeed * velocity.normalized;
            dodgeTimer = 0.0f;
        }
    }


    /*
        HandleShoot()
        - Handle the player's shoot functionality
    */
    void HandleShoot()
    {
        if (shootTimer >= shootDelay)
        {
            shootTimer = 0.0f;
            Vector2 direction = GetPlayerToPosVector(GetMouseWorldPos()).normalized;
        }
    }


    /*
        HandleAccel()
        - Handle player acceleration / decceleration
    */
    void HandleAccel(Vector2 inputDir) {
        // Accel / Deccel Logic
        if (inputDir.magnitude > 0.01f)
        {
            velocity.x += inputDir.x * acceleration.x * Time.deltaTime;
            velocity.y += inputDir.y * acceleration.y * Time.deltaTime;

            // Deccelerate to maxSpeed if currently moving faster than maxSpeed
            if (velocity.magnitude > maxSpeed)
            {
                float newSpeed = Mathf.MoveTowards(velocity.magnitude, maxSpeed, acceleration.magnitude * Time.deltaTime);
                velocity = velocity.normalized * newSpeed;
            }
        }
        else
        {
            float decelX = acceleration.x * Time.deltaTime;
            float decelY = acceleration.y * Time.deltaTime;
            velocity.x = Mathf.MoveTowards(velocity.x, 0f, decelX);
            velocity.y = Mathf.MoveTowards(velocity.y, 0f, decelY);
        }
    }
}