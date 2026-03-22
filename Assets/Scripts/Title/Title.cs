using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour {
    public string scene;

    public void StartGame() {
        SceneManager.LoadScene(scene);
    }

    public void QuitGame() {
        Application.Quit();
    }
}
