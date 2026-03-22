using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour {
    [Header("Audio")]
        public AudioClip sfxFire;
        public AudioClip sfxHit;

    private Vector2 direction;
    private float speed;
    private GameObject parent;
    private float knockBack = 10;


    public void Launch(
        Vector2 start, 
        Vector2 dir, 
        float speed, 
        Color color, 
        GameObject parent
    ) {
        this.parent = parent;
        transform.position = start;
        direction = dir.normalized;
        this.speed = speed;

        AudioSource.PlayClipAtPoint(sfxFire, transform.position);

        if (TryGetComponent<SpriteRenderer>(out var spriteRenderer)) 
            spriteRenderer.color = color;
    }

    void Update() {
        // Destroy bullet if it goes off screen
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
        if (viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1)
            Destroy(gameObject);

        transform.position += (Vector3)(speed * Time.deltaTime * direction);
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject == parent) return;
        bool parentIsEnemy = parent != null && parent.GetComponent<Enemy>() != null;

        // Enemy bullets pass through other enemies
        if (parentIsEnemy && other.GetComponent<Enemy>() != null) return;

        var playerShield = other.GetComponent<PlayerShield>();
        if (playerShield != null) {
            PlayerShield.hitCount++;
            AudioSource.PlayClipAtPoint(sfxHit, transform.position);
            Destroy(gameObject);
            return;
        }

        var player = other.GetComponent<Player>();
        if (player != null) {
            if (player.invincible) return;
            player.TakeDamage(1);
            player.velocity -= direction * -knockBack;
            AudioSource.PlayClipAtPoint(sfxHit, transform.position);
            Destroy(gameObject);
            return; 
        }

        var enemy = other.GetComponent<Enemy>();
        if (enemy != null) { 
            enemy.TakeDamage(1); 
            enemy.velocity -= direction * -knockBack;
            AudioSource.PlayClipAtPoint(sfxHit, transform.position);
            Destroy(gameObject);
            return; 
        }

    }
}
