using System;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.U2D;

public class Player : MonoBehaviour {
    // Public Members
    [Header("Bullet Settings")]
        public GameObject bulletPrefab;
        public Transform bulletSpawnPoint;

    [Header("Movement Settings")]
        public float maxSpeed = 5.0f;
        public Vector2 acceleration = new(40f, 40f);
        public float rotationSpeed = 1500f;
        public Vector2 velocity;

    [Header("Dodge Settings")]
        public float dodgeDelay = 0.5f;
        public float dodgeDuration = 0.3f;
        public float dodgeDistance = 3.0f;
        public bool invincible = false;

    [Header("Shoot Settings")]
        public float shootDelay = 0.1f;

    [Header("Health / Stamina")]
        public int health = 3;
        public int stamina = 3;
        public SpriteRenderer bodySprite;

    [Header("Shield")]
        public SpriteShapeRenderer shieldSprite;
        public PolygonCollider2D shieldCollider;

    [Header("Audio")]
        public AudioClip sfxExplosion;

    [Header("Effects")]
        public GameObject explosionPrefab;

    // Private Members
    private float dodgeTimer = 0.5f;
    private float dodgeActiveTimer = 0.0f;
    private float shootTimer = 0.1f;
    private Vector2 preDodgeVelocity;
    private Vector2 dodgeDir;
    private Vector2 dodgeStartPos;
    private float dodgeSafeDist;

    private Vector2 lookDirection = Vector2.right;

    private SpaceShooterInputActions.StandardActions input;
    private Rigidbody2D rigidBody;
    private SpriteRenderer[] spriteRenderers;
    private PlayerShield shield;
    private AudioSource audioSource;


    void Awake() {
        var inputActions = new SpaceShooterInputActions();
        input = inputActions.Standard;
        inputActions.Enable();

        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        shield = GetComponentInChildren<PlayerShield>();

        audioSource = GetComponent<AudioSource>();
    }


    void Update() {
        HandleInput();
        UpdateLookDirection();
        ApplyLookRotation();
        UpdateDodgeVisuals();
        CheckShield();
    }

    void CheckShield() {
        if (PlayerShield.hitCount > 0) {
            stamina = Math.Max(0, stamina - PlayerShield.hitCount);
            PlayerShield.hitCount = 0;
            print($"Stamina: {stamina}");
        }
    }


    void FixedUpdate() {
        rigidBody.MovePosition(rigidBody.position + velocity * Time.fixedDeltaTime);
    }


    /*
        HandleMovement():
        - Get input values for this frame
        - Handle Acceleration
        - Handle Dodge
        - Handle Shoot
        - Move the player
    */
    void HandleInput() {
        Vector2 inputDir = new(
            input.HorizontalMovement.ReadValue<float>(),
            input.VerticalMovement.ReadValue<float>()
        );

        bool shoot = input.Shoot.WasPressedThisFrame();
        bool dodge = input.Dodge.WasPressedThisFrame();
        bool block = input.Block.IsPressed();

        dodgeTimer += Time.deltaTime;
        shootTimer += Time.deltaTime;

        HandleAccel(inputDir);

        if (block && stamina > 0) {
            shieldSprite.enabled = true;
            shieldCollider.enabled = true;
        }
        else { 
            shieldSprite.enabled = false;
            shieldCollider.enabled = false;
        }
        if (dodge) HandleDodge();
        if (shoot && !dodge && !block) HandleShoot();
    }


    /*
        UpdateLookDirection()
        - If controller stick input exists, use it as the look direction.
        - Otherwise, calculate direction from player to mouse.
    */
    void UpdateLookDirection() {
        Vector2 stickInput = input.Look.ReadValue<Vector2>();

        if (stickInput.magnitude > 0.1f)
            lookDirection = stickInput.normalized;

        else if (Mouse.current != null) {
            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new(mouseScreenPos.x, mouseScreenPos.y, 10f));
            Vector2 toMouse = mouseWorldPos - transform.position;

            if (toMouse.magnitude > 0.01f)
                lookDirection = toMouse.normalized;
        }
    }


    /*
        ApplyLookRotation()
        - Rotate the player to face the current look direction.
    */
    void ApplyLookRotation() {
        float targetAngle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
        float currentAngle = rigidBody.rotation;
        float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
        rigidBody.MoveRotation(newAngle);
    }


    void HandleBlock() { shieldSprite.enabled = true; }

    /*
        HandleDodge()
        - Handle the player's dodge functionality
    */
    void HandleDodge() {
        if (dodgeTimer >= dodgeDelay) {
            dodgeDir = velocity.magnitude > 0.01f ? velocity.normalized : -lookDirection;

            preDodgeVelocity = velocity;
            velocity = Vector2.zero;
            SetSpritesVisible(false);

            // Calculate safe dodge distance up front
            dodgeStartPos = rigidBody.position;
            var playerCollider = GetComponent<Collider2D>();
            playerCollider.enabled = false;
            RaycastHit2D hit = Physics2D.Raycast(dodgeStartPos, dodgeDir, dodgeDistance);
            playerCollider.enabled = true;
            dodgeSafeDist = hit.collider != null ? hit.distance - 0.1f : dodgeDistance;
            if (dodgeSafeDist < 0) dodgeSafeDist = 0;

            invincible = true;
            dodgeActiveTimer = dodgeDuration;
            dodgeTimer = 0.0f;
        }
    }


    /*
        UpdateDodgeVisuals()
        - Handle invincibility timer and restore state when dodge ends
    */
    void UpdateDodgeVisuals() {
        if (dodgeActiveTimer > 0) {
            dodgeActiveTimer -= Time.deltaTime;

            if (dodgeActiveTimer <= 0) {
                // Teleport player
                if (dodgeSafeDist > 0) {
                    Vector2 newPos = dodgeStartPos + dodgeDir * dodgeSafeDist;
                    rigidBody.position = newPos;
                    transform.position = newPos;
                }

                invincible = false;
                velocity = preDodgeVelocity;
                SetSpritesVisible(true);
            }
        }
    }


    void SetSpritesVisible(bool visible) {
        foreach (var sr in spriteRenderers)
            sr.enabled = visible;
    }


    public void TakeDamage(int damage) {
        if (invincible) return;
        health -= damage;
        if (health <= 0) {
            AudioSource.PlayClipAtPoint(sfxExplosion, transform.position);
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    public void UseStamina(int amount) {
        stamina -= stamina;
    }

    /*
        HandleShoot()
        - Handle the player's shoot functionality
    */
    void HandleShoot() {
        if (shootTimer >= shootDelay) {
            shootTimer = 0.0f;
            GameObject bulletObj = Instantiate(bulletPrefab);
            Bullet bulletScript = bulletObj.GetComponent<Bullet>();
            bulletScript.Launch(
                bulletSpawnPoint.position,
                lookDirection, 
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
        if (inputDir.magnitude > 0.01f) {
            velocity.x += inputDir.x * acceleration.x * Time.deltaTime;
            velocity.y += inputDir.y * acceleration.y * Time.deltaTime;

            if (velocity.magnitude > maxSpeed) {
                float newSpeed = Mathf.MoveTowards(velocity.magnitude, maxSpeed, acceleration.magnitude * Time.deltaTime);
                velocity = velocity.normalized * newSpeed;
            }
        }
        else {
            float decelX = acceleration.x * Time.deltaTime;
            float decelY = acceleration.y * Time.deltaTime;
            velocity.x = Mathf.MoveTowards(velocity.x, 0f, decelX);
            velocity.y = Mathf.MoveTowards(velocity.y, 0f, decelY);
        }
    }
}