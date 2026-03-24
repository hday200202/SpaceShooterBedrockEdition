using UnityEngine;

public class Bullet : MonoBehaviour {
    [Header("Audio")]
        public AudioClip sfxFire;
        public AudioClip sfxHit;

    [Header("Settings")]
        public float knockBack = 10;
        public float maxLifetime = 5f;

    private static AudioSource sharedAudio;

    private Vector2 direction;
    private float speed;
    private GameObject parent;
    private Collider2D col;
    private Rigidbody2D rb;
    private bool bounce;
    private int bouncesRemaining;
    private float spawnTime;

    public void Launch(
        Vector2 start, 
        Vector2 dir, 
        float speed, 
        Color color, 
        GameObject parent,
        bool bounce = false
    ) {
        this.parent = parent;
        this.bounce = bounce;
        this.bouncesRemaining = 2;
        transform.position = start;
        direction = dir.normalized;
        this.speed = speed;
        spawnTime = Time.time;

        col = GetComponent<Collider2D>();

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.linearVelocity = direction * speed;

        PlaySound(sfxFire);

        if (TryGetComponent<SpriteRenderer>(out var spriteRenderer))
            spriteRenderer.color = color;
    }

    void FixedUpdate() {
        if (Time.time - spawnTime > maxLifetime) {
            Destroy(gameObject);
            return;
        }

        if (rb.linearVelocity.magnitude > 0.1f)
            direction = rb.linearVelocity.normalized;

        float dist = rb.linearVelocity.magnitude * Time.fixedDeltaTime;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, dist);

        if (hit.collider != null
            && hit.collider != col
            && hit.collider.GetComponent<Player>() == null
            && hit.collider.GetComponent<Enemy>() == null
            && hit.collider.GetComponent<Boss>() == null
            && hit.collider.GetComponent<Bullet>() == null
            && hit.collider.GetComponent<PlayerShield>() == null) {

            if (bounce && bouncesRemaining > 0) {
                bouncesRemaining--;
                direction = Vector2.Reflect(direction, hit.normal);
                rb.linearVelocity = direction * speed;
                transform.position = hit.point + hit.normal * 0.1f;
            } else {
                PlaySound(sfxHit);
                Destroy(gameObject);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (Time.time - spawnTime < 0.05f) return;
        if (other.gameObject == parent) return;
        if (other.GetComponent<Bullet>() != null) return;

        bool parentIsEnemy = parent != null && (parent.GetComponent<Enemy>() != null || parent.GetComponent<Boss>() != null);
        if (parentIsEnemy && (other.GetComponent<Enemy>() != null || other.GetComponent<Boss>() != null)) return;

        if (HandleShieldHit(other)) return;
        if (HandlePlayerHit(other)) return;
        if (HandleEnemyHit(other)) return;
        if (HandleBossHit(other)) return;

        if (bounce && bouncesRemaining > 0) {
            bouncesRemaining--;
            Vector2 closestPoint = other.ClosestPoint(transform.position);
            Vector2 normal = ((Vector2)transform.position - closestPoint).normalized;
            direction = Vector2.Reflect(direction, normal);
            rb.linearVelocity = direction * speed;
            transform.position += (Vector3)(normal * 0.1f);
            return;
        }
        PlaySound(sfxHit);
        Destroy(gameObject);
    }

    bool HandleShieldHit(Collider2D other) {
        var playerShield = other.GetComponent<PlayerShield>();
        if (playerShield == null) return false;
        if (parent != null && parent.GetComponent<Player>() != null) return false;

        PlayerShield.hitCount++;

        Vector2 normal = ((Vector2)(transform.position - other.transform.position)).normalized;
        direction = Vector2.Reflect(direction, normal);
        rb.linearVelocity = direction * speed;
        transform.position += (Vector3)(normal * 0.1f);
        parent = other.gameObject;
        return true;
    }

    bool HandlePlayerHit(Collider2D other) {
        var player = other.GetComponent<Player>();
        if (player == null) return false;
        if (player.invincible) return true;

        player.TakeDamage(1);
        player.velocity -= direction * -knockBack;
        PlaySound(sfxHit);
        Destroy(gameObject);
        return true;
    }

    bool HandleEnemyHit(Collider2D other) {
        var enemy = other.GetComponent<Enemy>();
        if (enemy == null) return false;

        enemy.TakeDamage(1);
        enemy.velocity -= direction * -knockBack;
        PlaySound(sfxHit);
        Destroy(gameObject);
        return true;
    }

    bool HandleBossHit(Collider2D other) {
        var boss = other.GetComponent<Boss>();
        if (boss == null) return false;

        boss.TakeDamage(1);
        PlaySound(sfxHit);
        Destroy(gameObject);
        return true;
    }

    static void PlaySound(AudioClip clip) {
        if (clip == null) return;
        if (sharedAudio == null) {
            var go = new GameObject("BulletAudio");
            DontDestroyOnLoad(go);
            sharedAudio = go.AddComponent<AudioSource>();
            sharedAudio.spatialBlend = 0f;
            sharedAudio.volume = 0.3f;
        }
        sharedAudio.clip = clip;
        sharedAudio.Play();
    }
}
