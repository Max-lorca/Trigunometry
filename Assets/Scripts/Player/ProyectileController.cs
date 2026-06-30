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

    private float danoBase;

    // --- Disparo especial del modo Análisis ---
    private bool usarTiempoSinEscalar = false;
    private bool esDisparoEspecial = false;
    private float distanciaObjetivoEspecial = 0f;
    private bool yaImpacto = false;

    private Vector3 startPosition;
    private float time;

    // Referencias
    [Header("Particulas")]
    [SerializeField] private GameObject explotionParticle;
    private ObjectPool<ProyectileController> _poolOrigen;

    private void Awake()
    {
        danoBase = damage;
    }

    public void ConfigurarPool(ObjectPool<ProyectileController> pool)
    {
        _poolOrigen = pool;
    }
    public void ResetProyectile(Vector3 nuevaPosicionInicial)
    {
        startPosition = nuevaPosicionInicial;
        time = 0f;
        damage = danoBase;
        usarTiempoSinEscalar = false;
        esDisparoEspecial = false;
        distanciaObjetivoEspecial = 0f;
        yaImpacto = false;
    }
    public void SetDirection(Vector2 dir)
    {
        direction = dir;
    }

    /// <summary>
    /// Configura este proyectil como el disparo bonus del modo Análisis:
    /// aplica el multiplicador de daño, avanza con tiempo no escalado
    /// (porque durante el análisis Time.timeScale = 0) y se autodestruye
    /// al alcanzar la distancia del objetivo, sin depender de que el
    /// trigger de física llegue a procesarse a tiempo.
    /// </summary>
    public void ConfigurarDisparoEspecial(float multiplicadorDano, float distanciaObjetivo)
    {
        damage = danoBase * multiplicadorDano;
        usarTiempoSinEscalar = true;
        esDisparoEspecial = true;
        distanciaObjetivoEspecial = distanciaObjetivo;
    }

    public float DanoActual => damage;
    public float VelocidadActual => proyectileSpeed;

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
        // Evita doble explosión/doble release si el trigger físico y el
        // chequeo de distancia del disparo especial llegan a coincidir.
        if (yaImpacto) return;
        yaImpacto = true;

        CrearExplosion();
        DevolverAlPool();
    }
    void Update()
    {
        float dt = usarTiempoSinEscalar ? Time.unscaledDeltaTime : Time.deltaTime;
        time += dt;

        Vector3 basePosition = startPosition + (Vector3)(direction * proyectileSpeed * time);

        float wave = Mathf.Sin(time * frecuency) * amplitude;

        Vector2 perpendicular = new Vector2(-direction.y, direction.x).normalized;

        Vector3 offset = (Vector3)(perpendicular * wave);

        transform.position = offset + basePosition;

        // El disparo especial garantiza su impacto al llegar a la distancia del
        // objetivo, en vez de depender de OnTriggerEnter2D (poco confiable a timeScale 0).
        if (esDisparoEspecial && proyectileSpeed * time >= distanciaObjetivoEspecial)
        {
            Impacto();
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        string tag = collision.gameObject.tag;

       switch(tag)
       {
        case "MeleeEnemy":
            // En el disparo especial el daño ya se aplicó directamente vía
            // RecibirDanoAnalisis, así que aquí solo generamos la explosión.
            if (!esDisparoEspecial)
                collision.GetComponent<MeleeEnemyController>()?.TomarDaño(damage);
            Impacto();
        break;
        case "DistanceEnemy":
            if (!esDisparoEspecial)
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

    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    public void SetSpeed(float newSpeed)
    {
        proyectileSpeed = newSpeed;
    }

}