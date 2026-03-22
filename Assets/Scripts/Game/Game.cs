using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour {
    public Canvas playerHUD;

    [Header("Health")]
        public Image health1;
        public Image health2;
        public Image health3;

    [Header("Stamina")]
        public Image stamina1;
        public Image stamina2;
        public Image stamina3;

    private Player player;

    void Start() {
        player = GetComponentInChildren<Player>();
    }

    void Update() {
        int hp = player != null ? player.health : 0;
        int sp = player != null ? player.stamina : 0;

        health1.enabled = hp >= 1;
        health2.enabled = hp >= 2;
        health3.enabled = hp >= 3;

        stamina1.enabled = sp >= 1;
        stamina2.enabled = sp >= 2;
        stamina3.enabled = sp >= 3;
    }
}
