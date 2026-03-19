using UnityEngine;

public class Enemy : MonoBehaviour {
    [Header("Bullet Settings")]
        public GameObject bulletPrefab;
        public Transform bulletSpawnPoint;
        public Color bulletColor = Color.red;

    [Header("Movement Settings")]
        public float maxSpeed = 4.0f;
        public Vector2 acceleration = new(30f, 30f);
        public float rotationSpeed = 1000f;

    [Header("Shoot Settings")]
        public float shootDelay = 0.5f;

    [Header("Health")]
        public int hp = 3;
        public SpriteRenderer bodySprite;

    [Header("AI Settings")]
        public float engageRange = 8f;
        public float preferredRange = 5f;
        public float strafeChangeInterval = 1.5f;

    // Private Members
    private Vector2 velocity;
    private float shootTimer;
    private float strafeDir = 1f;
    private float strafeTimer;

    private Vector2 lookDirection = Vector2.right;

    private Rigidbody2D rigidBody;
    private SpriteRenderer[] spriteRenderers;
    private Transform target;


    void Awake() {
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        shootTimer = shootDelay;
        strafeDir = Random.value > 0.5f ? 1f : -1f;
        strafeTimer = Random.Range(0f, strafeChangeInterval);

        // Enemies start with a random hp value from 1-3
        hp = Random.Range(1, 4);

        UpdateBodyColor();
    }

    void Start() {
        var player = GameObject.FindWithTag("Player");
        if (player != null) target = player.transform;
    }


    void Update() {
        if (target == null) return;

        shootTimer += Time.deltaTime;

        UpdateLookDirection();
        ApplyLookRotation();
        HandleAI();
    }


    void FixedUpdate() {
        rigidBody.MovePosition(rigidBody.position + velocity * Time.fixedDeltaTime);
    }


    void UpdateLookDirection() {
        if (target == null) return;
        Vector2 toTarget = (Vector2)target.position - (Vector2)transform.position;
        if (toTarget.magnitude > 0.01f)
            lookDirection = toTarget.normalized;
    }


    void ApplyLookRotation() {
        float targetAngle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
        float currentAngle = rigidBody.rotation;
        float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
        rigidBody.MoveRotation(newAngle);
    }


    void HandleAI() {
        Vector2 toTarget = (Vector2)target.position - (Vector2)transform.position;
        float distToTarget = toTarget.magnitude;
        Vector2 toTargetDir = toTarget.normalized;

        // Strafe direction flips on a timer
        strafeTimer -= Time.deltaTime;
        if (strafeTimer <= 0) {
            strafeDir = -strafeDir;
            strafeTimer = strafeChangeInterval + Random.Range(-0.5f, 0.5f);
        }

        Vector2 strafeVec = new Vector2(-toTargetDir.y, toTargetDir.x) * strafeDir;

        Vector2 inputDir = Vector2.zero;
        if (distToTarget > preferredRange + 0.5f)
            inputDir = toTargetDir + strafeVec * 0.4f;

        else if (distToTarget < preferredRange - 0.5f)
            inputDir = -toTargetDir + strafeVec * 0.4f;

        else
            inputDir = strafeVec;

        HandleAccel(inputDir.normalized);

        if (distToTarget <= engageRange)
            HandleShoot();
    }


    void SetSpritesVisible(bool visible) {
        foreach (var sr in spriteRenderers)
            sr.enabled = visible;
    }


    public void TakeDamage(int damage) {
        hp -= damage;
        if (hp <= 0) Destroy(gameObject);
        else UpdateBodyColor();
    }


    void UpdateBodyColor() {
        if (bodySprite == null) return;
        bodySprite.color = hp switch {
            >= 3 => Color.green,
            2 => Color.yellow,
            _ => Color.red
        };
    }


    void HandleShoot() {
        if (shootTimer >= shootDelay) {
            shootTimer = 0.0f;
            GameObject bulletObj = Instantiate(bulletPrefab);
            Bullet bulletScript = bulletObj.GetComponent<Bullet>();
            bulletScript.Launch(
                bulletSpawnPoint.position,
                lookDirection,
                12f,
                bulletColor,
                gameObject
            );
        }
    }


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
