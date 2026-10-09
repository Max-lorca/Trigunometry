using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class WeaponData : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private ProyectileController projectilePrefab;
    [SerializeField] private Transform projectileSpawn;
    [SerializeField] private GameObject particleShoot;
    [SerializeField] private AudioClip shootAudioClip;

    [Header("Stats base")]
    [SerializeField] private WeaponType weaponType;
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float baseCooldown = 0.5f;
    [SerializeField] private float baseSpeed = 8f;
    [SerializeField] private float baseAmplitude = 0.5f;
    [SerializeField] private float baseFrequency = 5f;
    [SerializeField] private float baseProjectileCount = 1f;
    [SerializeField] private float baseSpreadAngle = 0f;

    private WeaponModifierManager modifiers;
    private AudioSource audioSource;
    private ObjectPool<ProyectileController> _pool;
    private bool canShoot = true;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        modifiers = FindFirstObjectByType<WeaponModifierManager>();

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

    private float Stat(StatType stat, float baseValue)
    {
        return modifiers != null
            ? modifiers.GetFinalValue(weaponType, stat, baseValue)
            : baseValue;
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

    // Se redondea porque un multiplicador como x1.5 daría 1.5 disparos
    int count = Mathf.Max(1, Mathf.RoundToInt(Stat(StatType.ProjectileCount, baseProjectileCount)));
    float spread = Stat(StatType.SpreadAngle, baseSpreadAngle);

    float damage = Stat(StatType.Damage, baseDamage);
    float speed = Stat(StatType.ProjectileSpeed, baseSpeed);
    float amplitude = Stat(StatType.Amplitude, baseAmplitude);
    float frequency = Stat(StatType.Frequency, baseFrequency);

    for (int i = 0; i < count; i++)
    {
        // Reparte los proyectiles en abanico, centrado en la dirección de apuntado
        float angle = count > 1 ? Mathf.Lerp(-spread / 2f, spread / 2f, i / (float)(count - 1)) : 0f;
        Vector2 dir = Quaternion.Euler(0, 0, angle) * projectileSpawn.right;

        // Desfase de la onda: con 2 disparos quedan en espejo (doble hélice)
        float phase = i * (2f * Mathf.PI / count);

        ProyectileController proj = _pool.Get();
        proj.ResetProyectile(projectileSpawn.position);
        proj.SetDirection(dir);
        proj.Configure(damage, speed, amplitude, frequency, phase);
    }

    float cooldown = Mathf.Max(0.05f, Stat(StatType.Cooldown, baseCooldown));
    yield return new WaitForSeconds(cooldown);
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