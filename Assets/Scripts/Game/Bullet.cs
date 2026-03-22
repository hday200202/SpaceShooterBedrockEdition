using UnityEngine;

public class Bullet : MonoBehaviour {
    [Header("Audio")]
        public AudioClip sfxFire;
        public AudioClip sfxHit;

    [Header("Settings")]
        public float knockBack = 10;

    private Vector2 direction;
    private float speed;
    private GameObject parent;
    private Collider2D col;


    public void Launch(Vector2 start, Vector2 dir, float speed, Color color, GameObject parent) {
        this.parent = parent;
        transform.position = start;
        direction = dir.normalized;
        this.speed = speed;

        col = GetComponent<Collider2D>();

        AudioSource.PlayClipAtPoint(sfxFire, transform.position);

        if (TryGetComponent<SpriteRenderer>(out var spriteRenderer))
            spriteRenderer.color = color;
    }


    void Update() {
        float moveDistance = speed * Time.deltaTime;
        if (CheckWallCollision(moveDistance)) return;

        transform.position += (Vector3)(moveDistance * direction);
    }

    bool CheckWallCollision(float moveDistance) {
        col.enabled = false;
        Collider2D parentCol = parent != null ? parent.GetComponent<Collider2D>() : null;
        if (parentCol != null) parentCol.enabled = false;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, moveDistance);

        col.enabled = true;
        if (parentCol != null) parentCol.enabled = true;

        if (hit.collider != null
            && hit.collider.GetComponent<Player>() == null
            && hit.collider.GetComponent<Enemy>() == null
            && hit.collider.GetComponent<PlayerShield>() == null) {
            AudioSource.PlayClipAtPoint(sfxHit, hit.point);
            Destroy(gameObject);
            return true;
        }

        return false;
    }


    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject == parent) return;
        if (other.GetComponent<Bullet>() != null) return;

        bool parentIsEnemy = parent != null && parent.GetComponent<Enemy>() != null;

        // Enemy bullets pass through other enemies
        if (parentIsEnemy && other.GetComponent<Enemy>() != null) return;

        if (HandleShieldHit(other)) return;
        if (HandlePlayerHit(other)) return;
        if (HandleEnemyHit(other)) return;

        AudioSource.PlayClipAtPoint(sfxHit, transform.position);
        Destroy(gameObject);
    }

    bool HandleShieldHit(Collider2D other) {
        var playerShield = other.GetComponent<PlayerShield>();
        if (playerShield == null) return false;

        PlayerShield.hitCount++;
        AudioSource.PlayClipAtPoint(sfxHit, transform.position);
        Destroy(gameObject);
        return true;
    }

    bool HandlePlayerHit(Collider2D other) {
        var player = other.GetComponent<Player>();
        if (player == null) return false;
        if (player.invincible) return true;

        player.TakeDamage(1);
        player.velocity -= direction * -knockBack;
        AudioSource.PlayClipAtPoint(sfxHit, transform.position);
        Destroy(gameObject);
        return true;
    }

    bool HandleEnemyHit(Collider2D other) {
        var enemy = other.GetComponent<Enemy>();
        if (enemy == null) return false;

        enemy.TakeDamage(1);
        enemy.velocity -= direction * -knockBack;
        AudioSource.PlayClipAtPoint(sfxHit, transform.position);
        Destroy(gameObject);
        return true;
    }
}
