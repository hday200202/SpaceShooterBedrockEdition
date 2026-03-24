using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.U2D;

public class Player : MonoBehaviour {
    [Header("Bullet Settings")]
        public GameObject bulletPrefab      = null;
        public Transform bulletSpawnPoint   = null;

    [Header("Movement Settings")]
        public float maxSpeed               = 5.0f;
        public Vector2 acceleration         = new(40f, 40f);
        public float rotationSpeed          = 1500f;
        public Vector2 velocity             = Vector2.zero;

    [Header("Dodge Settings")]
        public float dodgeDelay             = 0.5f;
        public float dodgeDuration          = 0.3f;
        public float dodgeDistance          = 3.0f;
        public bool invincible              = false;
        public int afterimageCount          = 6;

    [Header("Shoot Settings")]
        public float shootDelay             = 0.1f;

    [Header("Health / Stamina")]
        public int health                   = 3;
        public int stamina                  = 3;
        public float staminaRefillDelay     = 1.0f;
        public SpriteRenderer bodySprite    = null;

    [Header("Shield")]
        public SpriteShapeRenderer shieldSprite = null;
        public PolygonCollider2D shieldCollider = null;

    [Header("Audio")]
        public AudioClip sfxExplosion       = null;

    [Header("Effects")]
        public GameObject explosionPrefab   = null;

    private float dodgeTimer                = 0.5f;
    private float dodgeActiveTimer          = 0.0f;
    private float staminaRefillTimer        = 0.0f;
    private float shootTimer                = 0.1f;
    private Vector2 preDodgeVelocity        = Vector2.zero;
    private Vector2 dodgeDir                = Vector2.zero;
    private Vector2 dodgeStartPos           = Vector2.zero;
    private float dodgeSafeDist             = 0.0f;

    private Vector2 lookDirection = Vector2.right;
    private readonly System.Collections.Generic.List<GameObject> afterimages = new();

    private SpaceShooterInputActions.StandardActions input;
    private Rigidbody2D rigidBody;
    private SpriteRenderer[] spriteRenderers = null;


    void Awake() {
        var inputActions = new SpaceShooterInputActions();
        input = inputActions.Standard;
        inputActions.Enable();

        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    void Update() {
        HandleInput();
        UpdateLookDirection();
        ApplyLookRotation();
        UpdateDodge();
        CheckShield();
        RefillStamina();
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
        HandleBlock(block);

        if (dodge && stamina > 0 && !block) HandleDodge();
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

            SpawnAfterimages();

            staminaRefillTimer = 0.0f;
            stamina = Math.Max(0, stamina - 1);
        }
    }

    /*
        UpdateDodge()
        - Handle invincibility timer and restore state when dodge ends
    */
    void UpdateDodge() {
        if (dodgeActiveTimer > 0) {
            dodgeActiveTimer -= Time.deltaTime;

            if (dodgeActiveTimer <= 0) {
                DestroyAfterimages();

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


    void HandleBlock(bool block) {
        bool active = block && stamina > 0;
        shieldSprite.enabled = active;
        shieldCollider.enabled = active;
    }

    void CheckShield() {
        if (PlayerShield.hitCount > 0) {
            stamina = Math.Max(0, stamina - PlayerShield.hitCount);
            PlayerShield.hitCount = 0;
            print($"Stamina: {stamina}");
        }
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
                30f,
                Color.orange,
                gameObject
            );
        }
    }


    public void TakeDamage(int damage) {
        if (invincible) return;
        health -= damage;
        if (health <= 0) {
            AudioSource.PlayClipAtPoint(sfxExplosion, transform.position);
            var explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 3f);
            Destroy(gameObject);
        }
    }

    public void UseStamina(int amount) {
        stamina -= amount;
    }

    void RefillStamina() {
        staminaRefillTimer += Time.deltaTime;
        if (staminaRefillTimer >= staminaRefillDelay) {
            stamina = Math.Min(3, stamina + 1);
            staminaRefillTimer = 0.0f;
        }
    }

    void SetSpritesVisible(bool visible) {
        foreach (var spr in spriteRenderers)
            spr.enabled = visible;
    }

    void SpawnAfterimages() {
        if (bodySprite == null || bodySprite.sprite == null) return;

        for (int i = 1; i <= afterimageCount; i++) {
            float t = (float)i / (afterimageCount + 1);
            Vector2 pos = dodgeStartPos + dodgeDir * dodgeSafeDist * t;

            GameObject ghost = new("Afterimage");
            ghost.transform.SetPositionAndRotation(
                pos, Quaternion.Euler(0, 0, bodySprite.transform.eulerAngles.z)
            );
            ghost.transform.localScale = bodySprite.transform.lossyScale;

            var spr = ghost.AddComponent<SpriteRenderer>();
            spr.sprite = bodySprite.sprite;
            spr.material = bodySprite.material;
            spr.sortingLayerID = bodySprite.sortingLayerID;
            spr.sortingOrder = bodySprite.sortingOrder;
            Color c = bodySprite.color;

            // Fade in as afterimages approach player location after dodge
            float alpha = Mathf.Lerp(0f, 0.75f, t);
            spr.color = new Color(c.r, c.g, c.b, alpha);

            afterimages.Add(ghost);
        }
    }

    void DestroyAfterimages() {
        foreach (var ghost in afterimages)
            if (ghost != null) Destroy(ghost);
        afterimages.Clear();
    }
}