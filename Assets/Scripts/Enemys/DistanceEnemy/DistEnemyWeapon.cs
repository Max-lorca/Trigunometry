using UnityEngine;
using System.Collections;

public class DistEnemyWeapon : MonoBehaviour
{
    [Header("Valores")]
    [SerializeField] private float daño = 10f;
    [SerializeField] private float cooldown = 1.5f;
    [SerializeField] private float projectileSpeed = 5f;

    [Header("Referencias")]
    [SerializeField] private Transform aimPoint;
    [SerializeField] private GameObject projectilePrefab;

    private Transform player;
    [HideInInspector] public bool canShoot = true;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Validaciones
        if (projectilePrefab == null)
            Debug.LogError("❌ ProjectilePrefab no asignado en " + gameObject.name);
        if (aimPoint == null)
            Debug.LogError("❌ AimPoint no asignado en " + gameObject.name);
    }

    private void Update()
    {
        if (player == null) return;

        // Rotar el arma hacia el player
        Vector2 dir = player.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public IEnumerator Shoot()
    {
        canShoot = false;

        if (projectilePrefab != null && aimPoint != null)
        {
            // Instanciar el proyectil
            GameObject projectile = Instantiate(projectilePrefab, aimPoint.position, aimPoint.rotation);

            // Configurar el proyectil
            EnemyProjectile enemyProjectile = projectile.GetComponent<EnemyProjectile>();
            if (enemyProjectile != null)
            {
                enemyProjectile.SetDamage(daño);
                enemyProjectile.SetDirection(aimPoint.right);
                enemyProjectile.SetSpeed(projectileSpeed);
                Debug.Log("🔫 Enemy disparó!");
            }
            else
            {
                Debug.LogError("❌ EnemyProjectile component no encontrado en el prefab!");
            }
        }

        yield return new WaitForSeconds(cooldown);
        canShoot = true;
    }
}