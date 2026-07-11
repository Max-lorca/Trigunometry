using UnityEngine;

public class ProyectileController : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] public float damage = 10f;
    [SerializeField] private float speed = 8f;

    private Vector2 direction;
    private WeaponData weaponData;
    private float time;
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        time += Time.deltaTime;
        Vector3 basePosition = startPosition + (Vector3)(direction * speed * time);
        float wave = Mathf.Sin(time * 5f) * 0.5f;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x).normalized;
        transform.position = basePosition + (Vector3)(perpendicular * wave);
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    public void SetWeaponData(WeaponData wd)
    {
        weaponData = wd;
    }

    public void ResetProyectile(Vector3 spawnPosition)
    {
        startPosition = spawnPosition;
        transform.position = spawnPosition;
        time = 0f;
        gameObject.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("MeleeEnemy"))
        {
            MeleeEnemyController melee = other.GetComponent<MeleeEnemyController>();
            if (melee != null)
            {
                melee.TomarDaño(damage);
                NotificarImpacto();
            }
            ReleaseProjectile();
        }
        else if (other.CompareTag("DistanceEnemy"))
        {
            DistanceEnemyController dist = other.GetComponent<DistanceEnemyController>();
            if (dist != null)
            {
                dist.TomarDaño(damage);
                NotificarImpacto();
            }
            ReleaseProjectile();
        }
        else if (other.CompareTag("Ground"))
        {
            ReleaseProjectile();
        }
    }

    private void NotificarImpacto()
    {
        TimeStopManager ts = FindFirstObjectByType<TimeStopManager>();
        if (ts != null && ts.IsAnalysisActive)
            return;

        WeaponShoot ws = FindFirstObjectByType<WeaponShoot>();
        if (ws != null) ws.OnHitEnemy();
    }

    private void ReleaseProjectile()
    {
        if (weaponData != null)
            weaponData.ReleaseProjectile(gameObject);
        else
            Destroy(gameObject);
    }
}