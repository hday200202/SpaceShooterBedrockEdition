using UnityEngine;

public class Explode : MonoBehaviour {
    void Awake() {
        var ps = GetComponent<ParticleSystem>();
        var main = ps.main;
        main.stopAction = ParticleSystemStopAction.Destroy;
    }
}
