using UnityEngine;

public class Explode : MonoBehaviour {
    public float lifetime = 3f;

    void Start() {
        var ps = GetComponentInChildren<ParticleSystem>();
        if (ps == null) return;
        Destroy(gameObject, lifetime);
    }
}
