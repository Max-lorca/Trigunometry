using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class WeaponData : MonoBehaviour
{
    [SerializeField] private ProyectileController projectilePrefab;
    [SerializeField] private Transform projectileSpawn;
    [SerializeField] private GameObject particleShoot;
    [SerializeField] private AudioClip shootAudioClip;
    [SerializeField] private float shootCooldown = 0.5f;

    private AudioSource audioSource;
    private ObjectPool<ProyectileController> _pool;
    private bool canShoot = true;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        _pool = new ObjectPool<ProyectileController>(
            createFunc: () =>
            {
                ProyectileController p = Instantiate(projectilePrefab);
                p.SetWeaponData(this);
                return p;
            },
            actionOnGet: (proj) => proj.gameObject.SetActive(true),
            actionOnRelease: (proj) => proj.gameObject.SetActive(false),
            actionOnDestroy: (proj) => Destroy(proj.gameObject),
            defaultCapacity: 15,
            maxSize: 30
        );
    }

    public void TryShoot()
    {
        if (canShoot)
            StartCoroutine(Shoot());
    }

    private IEnumerator Shoot()
    {
        canShoot = false;
        Instantiate(particleShoot, projectileSpawn.position, projectileSpawn.rotation);
        audioSource.PlayOneShot(shootAudioClip);

        ProyectileController proj = _pool.Get();
        proj.transform.position = projectileSpawn.position;
        proj.ResetProyectile(projectileSpawn.position);
        proj.SetDirection(projectileSpawn.right);

        yield return new WaitForSeconds(shootCooldown);
        canShoot = true;
    }

    public void ReleaseProjectile(GameObject proj)
    {
        ProyectileController pc = proj.GetComponent<ProyectileController>();
        if (pc != null)
            _pool.Release(pc);
        else
            Destroy(proj);
    }
}