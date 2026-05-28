using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DistEnemyWeapon : MonoBehaviour
{
    [Header("Valores")]
    [SerializeField] private float daño;
    [SerializeField] private float cooldown;

    //Referencias
    [Header("Referencias")]
    [SerializeField] private Transform aimPoint;
    [SerializeField] private GameObject projectilePrefab; // projectile ingles, proyectile español, no se burle de mi que no ve que se me confunden

    private float currentAngle;
    private Transform player;
    private bool canShoot = true;
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }
    private void Update()
    {
        Vector2 dir = player.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0,0,currentAngle);
    }

    private IEnumerator Shoot()
    {
        canShoot = false;
        GameObject projectile = Instantiate(projectilePrefab, aimPoint.position, aimPoint.rotation);
        yield return new WaitForSeconds(cooldown);
        canShoot = true;
    }
}
