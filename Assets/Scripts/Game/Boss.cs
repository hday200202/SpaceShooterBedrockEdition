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

    private Coroutine attackRoutine;

    void Start() {
        attackRoutine = StartCoroutine(AttackLoop());
    }

    void Update() {
        transform.Rotate(0, 0, 1);
    }

    IEnumerator AttackLoop() {
        while (true) {
            // Attack 1: sequential fire from each spawn point
            yield return StartCoroutine(Attack1());

            yield return new WaitForSeconds(attackCooldown);

            // Attack 2: simultaneous fire from all spawn points
            yield return StartCoroutine(Attack2());

            yield return new WaitForSeconds(attackCooldown);
        }
    }

    // Fire from each spawn point one at a time, cycling through them
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

    // Fire from all spawn points simultaneously
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
}
