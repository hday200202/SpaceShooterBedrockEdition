using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Vector2 direction;
    private Vector2 start;
    private readonly float maxDistance = 1000f;
    private float speed;
    private GameObject parent;

    public void Launch(
        Vector2 start, 
        Vector2 dir, 
        float speed, 
        Color color, 
        GameObject parent
    ) {
        this.start = start;
        this.parent = parent;
        transform.position = start;
        direction = dir.normalized;
        this.speed = speed;

        if (TryGetComponent<SpriteRenderer>(out var spriteRenderer)) 
            spriteRenderer.color = color;
    }

    void Update()
    {
        transform.position += (Vector3)(speed * Time.deltaTime * direction);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject != parent)
            Destroy(gameObject);
    }
}
