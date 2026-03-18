using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private GameObject parent;

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

        if (TryGetComponent<SpriteRenderer>(out var spriteRenderer)) 
            spriteRenderer.color = color;
    }

    void Update()
    {
        // Destroy bullet if it goes off screen
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
        if (viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1)
            Destroy(gameObject);

        transform.position += (Vector3)(speed * Time.deltaTime * direction);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject != parent)
            Destroy(gameObject);
    }
}
