using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Bullet Settings")]
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;

    [Header("Movement Settings")]
    public float maxSpeed = 5.0f;

    [Header("Dodge Settings")]
    public float dodgeDelay = 0.5f;
    public float dodgeDuration = 0.3f;
    public float flashFrequency = 0.1f; // How often to toggle visibility (seconds)
    public bool invincible = false;

    [Header("Shoot Settings")]
    public float shootDelay = 0.1f;

    private Vector2 acceleration = new(40f, 40f);
    private Vector2 velocity;

    private float dodgeTimer = 0.5f;
    private float dodgeActiveTimer = 0.0f;
    private float shootTimer = 0.1f;
    private float flashTimer = 0.0f;

    private SpaceShooterInputActions.StandardActions input;
    private Rigidbody2D rb;
    private SpriteRenderer[] spriteRenderers; // Changed to array


    void Awake()
    {
        // Initialize Input
        var inputActions = new SpaceShooterInputActions();
        input = inputActions.Standard;
        inputActions.Enable();

        rb = GetComponent<Rigidbody2D>();
        
        // Find all SpriteRenderers on this object and children
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        
        if (spriteRenderers.Length == 0)
        {
            Debug.LogError("No SpriteRenderer found on Player or children!");
        }
        else
        {
            Debug.Log($"Found {spriteRenderers.Length} SpriteRenderer(s)");
        }
    }


    void Update()
    {
        HandleInput();
        LookAt(GetMouseWorldPos());
        UpdateDodgeVisuals();
    }


    void FixedUpdate()
    {
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }


    /*
        HandleMovement():
        - Get input values for this frame
        - Handle Acceleration
        - Handle Dodge
        - Handle Shoot
        - Move the player
    */
    void HandleInput()
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
        rb.MoveRotation(angle);
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
            
            // Start invincibility and visual effect
            invincible = true;
            dodgeActiveTimer = dodgeDuration;
            flashTimer = 0.0f;
            
            Debug.Log("Dodge triggered! Invincible = " + invincible);
        }
    }


    /*
        UpdateDodgeVisuals()
        - Update flashing visibility and invincibility during dodge
    */
    void UpdateDodgeVisuals()
    {
        if (dodgeActiveTimer > 0)
        {
            dodgeActiveTimer -= Time.deltaTime;
            flashTimer += Time.deltaTime;
            
            // Toggle visibility at flash frequency for all sprite renderers
            if (spriteRenderers.Length > 0)
            {
                // Flash on/off based on timer
                bool shouldBeVisible = (Mathf.FloorToInt(flashTimer / flashFrequency) % 2) == 0;
                
                foreach (var sr in spriteRenderers)
                {
                    sr.enabled = shouldBeVisible;
                }
            }
            
            // End invincibility when dodge duration expires
            if (dodgeActiveTimer <= 0)
            {
                invincible = false;
                
                // Ensure all sprites are visible again
                foreach (var sr in spriteRenderers)
                {
                    sr.enabled = true;
                }
                
                Debug.Log("Dodge ended! Invincible = " + invincible);
            }
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
            GameObject bulletObj = Instantiate(bulletPrefab);        
            Bullet bulletScript = bulletObj.GetComponent<Bullet>();
            bulletScript.Launch(
                bulletSpawnPoint.position, 
                direction, 
                15f, 
                Color.black,
                gameObject
            );
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