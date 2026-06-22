using UnityEngine;
using UnityEngine.Pool;

public class ProyectileController : MonoBehaviour
{
    [Header("Variables")]
    [HideInInspector] public Vector2 direction;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float proyectileSpeed = 8f;
    [SerializeField] private float frecuency = 10f;
    [SerializeField] private float amplitude = 0.5f;

    private Vector3 startPosition;
    private float time;

    // Referencias
    [Header("Particulas")]
    [SerializeField] private GameObject explotionParticle;
    private ObjectPool<ProyectileController> _poolOrigen;

    public void ConfigurarPool(ObjectPool<ProyectileController> pool)
    {
        _poolOrigen = pool;
    }
    public void ResetProyectile(Vector3 nuevaPosicionInicial)
    {
        startPosition = nuevaPosicionInicial;
        time = 0f;
    }
    public void SetDirection(Vector2 dir)
    {
        direction = dir;
    }
    private void CrearExplosion()
    {
        Instantiate(explotionParticle , transform.position, Quaternion.identity);
    }
    private void DevolverAlPool()
    {
        if (_poolOrigen != null)
        {
            _poolOrigen.Release(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Impacto()
    {
        CrearExplosion();
        DevolverAlPool();
    }
    void Update()
    {
        time += Time.deltaTime;

        Vector3 basePosition = startPosition + (Vector3)(direction * proyectileSpeed * time);

        float wave = Mathf.Sin(time * frecuency) * amplitude;

        Vector2 perpendicular = new Vector2(-direction.y, direction.x).normalized;

        Vector3 offset = (Vector3)(perpendicular * wave);

        transform.position = offset + basePosition;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        string tag = collision.gameObject.tag;

       switch(tag)
       {
        case "MeleeEnemy":
            collision.GetComponent<MeleeEnemyController>()?.TomarDaño(damage);
            Impacto();
        break;
        case "DistanceEnemy":
            collision.GetComponent<DistanceEnemyController>()?.TomarDaño(damage);
            Impacto();
        break;
        case "Ground":
            Impacto();
        break;
        case "Pared":
            Impacto();
        break;
        }
    }

    
}