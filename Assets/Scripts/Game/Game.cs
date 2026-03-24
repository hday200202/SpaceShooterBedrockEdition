using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game : MonoBehaviour {
    public Canvas playerHUD;
    public Canvas deathScreen;
    public Canvas winScreen;

    [Header("Health")]
        public Image health1;
        public Image health2;
        public Image health3;

    [Header("Stamina")]
        public Image stamina1;
        public Image stamina2;
        public Image stamina3;

    private bool dead = false;
    private bool won = false;
    private bool playerFound = false;
    private bool bossActivated = false;

    private Player player;
    private Boss boss;
    private SpaceShooterInputActions playerInputActions;

    void Start() {
        player = GetComponentInChildren<Player>();
        if (player == null)
            player = FindAnyObjectByType<Player>();
        playerFound = player != null;
        if (player != null)
            playerInputActions = player.inputActions;
        boss = FindAnyObjectByType<Boss>();
    }

    void Update() {
        if (dead || won) return;

        if (playerFound && player == null) {
            dead = true;
            StartCoroutine(ShowDeathScreen());
            return;
        }

        if (boss != null && boss.activated)
            bossActivated = true;

        if (bossActivated && boss == null) {
            won = true;
            StartCoroutine(ShowWinScreen());
            return;
        }

        health1.enabled = player.health >= 1;
        health2.enabled = player.health >= 2;
        health3.enabled = player.health >= 3;

        stamina1.enabled = player.stamina >= 1;
        stamina2.enabled = player.stamina >= 2;
        stamina3.enabled = player.stamina >= 3;
    }

    IEnumerator ShowDeathScreen() {
        if (playerInputActions != null)
            playerInputActions.Disable();
        playerHUD.gameObject.SetActive(false);
        yield return new WaitForSeconds(3f);
        deathScreen.gameObject.SetActive(true);
    }

    IEnumerator ShowWinScreen() {
        if (playerInputActions != null)
            playerInputActions.Disable();
        playerHUD.gameObject.SetActive(false);
        yield return new WaitForSeconds(3f);
        winScreen.gameObject.SetActive(true);
    }

    public string scene;

    public void Restart() {
        SceneManager.LoadScene(scene);
    }

    public void QuitGame() {
        Application.Quit();
    }
}
