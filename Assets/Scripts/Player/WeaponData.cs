using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponData : MonoBehaviour
{

    //Referencias
    [SerializeField] private GameObject projectilePrefab;

    [SerializeField] private Transform projectileSpawn;

    //Valores
    [SerializeField] private float shootCooldown = 0.5f;

    //Booleanos
    private bool canShoot = true;

    public void TryShoot()
    {
        if (canShoot)
        {
            StartCoroutine(Shoot());
        }
    }
    private IEnumerator Shoot()
    {
        canShoot = false;

        GameObject newProyectile = Instantiate(projectilePrefab, projectileSpawn.position, projectileSpawn.rotation);

        ProyectileController proyectile = newProyectile.GetComponent<ProyectileController>();
        proyectile.SetDirection(projectileSpawn.right);

        yield return new WaitForSeconds(shootCooldown);

        canShoot = true;
    }
}
