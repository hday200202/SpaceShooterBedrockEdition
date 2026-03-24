using UnityEngine;

public class BossRoomTrigger : MonoBehaviour {
    public Boss boss;
    public GameObject bossHealthBar;  // UI element, hidden by default
    public AudioClip bossMusic = null;
    [Range(0f, 1f)] public float musicVolume = 0.5f;

    void OnTriggerEnter2D(Collider2D other) {
        if (other.GetComponent<Player>() == null) return;
        
        boss.Activate();
        bossHealthBar.SetActive(true);        
        GetComponent<Collider2D>().enabled = false;

        if (bossMusic != null) {
            AudioSource music = gameObject.AddComponent<AudioSource>();
            music.clip = bossMusic;
            music.loop = true;
            music.spatialBlend = 0f;
            music.volume = musicVolume;
            music.Play();
        }
    }
}