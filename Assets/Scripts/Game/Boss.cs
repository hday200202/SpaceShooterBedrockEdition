using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour {
    [Header("Bullets")]
        public GameObject bulletPrefab;
        public List<Transform> bulletSpawnPoints;
        public float bulletSpeed = 15f;

    [Header("Attacks")]
        public float attackCooldown = 2f;
        public float attack1Duration = 3f;
        public float attack1FireRate = 0.15f;
        public float attack2Duration = 3f;
        public float attack2FireRate = 0.3f;

    [Header("Health")]
        public int hp = 15;
        public List<GameObject> healthBlocks;
        public SpriteRenderer bodySprite;

    [Header("AI Settings")]
        public float spinSpeed = 180f;
        public float moveSpeed = 2f;
        public float preferredRange = 8f;
        public float engageRange = 15f;

    [Header("Audio")]
        public AudioClip sfxExplosion;

    [Header("Effects")]
        public GameObject explosionPrefab;

    [HideInInspector] public bool activated = false;

    private Coroutine attackRoutine;
    private Rigidbody2D rigidBody;
    private Transform target;

    void Awake() {
        rigidBody = GetComponent<Rigidbody2D>();
        if (rigidBody != null)
            rigidBody.sleepMode = RigidbodySleepMode2D.NeverSleep;
    }

    void Start() {
        var player = GameObject.FindWithTag("Player");
        if (player != null) target = player.transform;
    }

    public void Activate() {
        activated = true;
        attackRoutine = StartCoroutine(AttackLoop());
    }

    void Update() {
        if (!activated || target == null) return;

        if (rigidBody == null)
            transform.Rotate(0, 0, spinSpeed * Time.deltaTime);
    }

    void FixedUpdate() {
        if (!activated || target == null) return;

        if (rigidBody != null)
            rigidBody.MoveRotation(rigidBody.rotation + spinSpeed * Time.fixedDeltaTime);

        Vector2 toTarget = (Vector2)target.position - (Vector2)transform.position;
        float dist = toTarget.magnitude;
        Vector2 moveDir = Vector2.zero;
        if (dist > preferredRange + 1f)
            moveDir = toTarget.normalized;
        else if (dist < preferredRange - 1f)
            moveDir = -toTarget.normalized;

        if (rigidBody != null)
            rigidBody.MovePosition(rigidBody.position + moveDir * moveSpeed * Time.fixedDeltaTime);
    }

    IEnumerator AttackLoop() {
        while (true) {
            yield return StartCoroutine(Attack1());
            yield return new WaitForSeconds(attackCooldown);
            yield return StartCoroutine(Attack2());
            yield return new WaitForSeconds(attackCooldown);
        }
    }

    IEnumerator Attack1() {
        if (bulletSpawnPoints.Count == 0) yield break;

        float elapsed = 0f;
        int index = 0;

        while (elapsed < attack1Duration) {
            FireBullet(bulletSpawnPoints[index]);
            index = (index + 1) % bulletSpawnPoints.Count;
            yield return new WaitForSeconds(attack1FireRate);
            elapsed += attack1FireRate;
        }
    }

    IEnumerator Attack2() {
        if (bulletSpawnPoints.Count == 0) yield break;

        float elapsed = 0f;

        while (elapsed < attack2Duration) {
            foreach (Transform spawnPoint in bulletSpawnPoints) {
                FireBullet(spawnPoint);
            }
            yield return new WaitForSeconds(attack2FireRate);
            elapsed += attack2FireRate;
        }
    }

    void FireBullet(Transform spawnPoint) {
        GameObject bulletObj = Instantiate(bulletPrefab);
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        Vector2 direction = (spawnPoint.position - transform.position).normalized;
        bullet.Launch(spawnPoint.position, direction, bulletSpeed, Color.red, gameObject);
    }

    public void TakeDamage(int damage) {
        hp -= damage;
        UpdateHealthBar();
        if (hp <= 0) {
            if (sfxExplosion != null)
                AudioSource.PlayClipAtPoint(sfxExplosion, transform.position);
            if (explosionPrefab != null) {
                var explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                Destroy(explosion, 3f);
            }
            Destroy(gameObject);
        }
    }

    void UpdateHealthBar() {
        for (int i = 0; i < healthBlocks.Count; i++)
            healthBlocks[i].SetActive(i < hp);
    }
}
