using UnityEngine;

public class FullHealthPickup : MonoBehaviour {
    public AudioClip sfxPowerUp = null;

    void OnTriggerEnter2D(Collider2D other) {
        var player = other.GetComponent<Player>();
        if (player == null) return;

        player.health = 3;
        AudioSource.PlayClipAtPoint(sfxPowerUp, transform.position);
        Destroy(gameObject);
    }
}
