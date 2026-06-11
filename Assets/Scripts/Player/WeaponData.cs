using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;

public class WeaponData : MonoBehaviour
{

    //Referencias
    [SerializeField] private ProyectileController projectilePrefab;
    [SerializeField] private Transform projectileSpawn;
    private ObjectPool<ProyectileController> _pool;
    //Valores
    [SerializeField] private float shootCooldown = 0.5f;
    //Booleanos
    private bool canShoot = true;

    private void Start()
    {
        _pool = new ObjectPool<ProyectileController>(
            createFunc: () =>
            {
               ProyectileController p = Instantiate(projectilePrefab);
               p.ConfigurarPool(_pool);
               return p; 
            },
            actionOnGet: (proyectil) => proyectil.gameObject.SetActive(true),
            actionOnRelease: (proyectil) => proyectil.gameObject.SetActive(false),
            actionOnDestroy: (proyectil) => Destroy(proyectil.gameObject),
            defaultCapacity: 15,
            maxSize: 30
        );
    }
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

        ProyectileController proyectil = _pool.Get();
        proyectil.transform.position = projectileSpawn.position;
        proyectil.ResetProyectile(projectileSpawn.position);
        proyectil.SetDirection(projectileSpawn.right);
        yield return new WaitForSeconds(shootCooldown);

        canShoot = true;
    }
}
