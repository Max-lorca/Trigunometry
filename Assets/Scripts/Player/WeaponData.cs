using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;

public class WeaponData : MonoBehaviour
{

    //Referencias
    [SerializeField] private ProyectileController projectilePrefab;
    [SerializeField] private Transform projectileSpawn;
    [SerializeField] private GameObject particleShoot;
    private AudioSource audioSource;
    [SerializeField] private AudioClip shootAudioClip;
    private ObjectPool<ProyectileController> _pool;
    //Valores
    [SerializeField] private float shootCooldown = 0.5f;
    //Booleanos
    private bool canShoot = true;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

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
        Instantiate(particleShoot, projectileSpawn.transform.position, projectileSpawn.transform.rotation);

        audioSource.PlayOneShot(shootAudioClip);
        ProyectileController proyectil = _pool.Get();
        proyectil.transform.position = projectileSpawn.position;
        proyectil.ResetProyectile(projectileSpawn.position);
        proyectil.SetDirection(projectileSpawn.right);

        yield return new WaitForSeconds(shootCooldown);

        canShoot = true;
    }

    /// <summary>
    /// Disparo especial al acertar el análisis trigonométrico: apunta directo
    /// al enemigo seleccionado, viaja con tiempo no escalado (porque durante el
    /// modo análisis Time.timeScale = 0) y aplica el daño con bonificación
    /// directamente al llegar a destino, sin depender de la física para el impacto.
    /// No respeta el cooldown normal: es un disparo garantizado, no un tiro más.
    /// Lo llama AnalysisModeController sobre el arma actualmente equipada
    /// (weaponShoot.currentWeapon).
    /// </summary>
    public void DispararAnalisisExitoso(IAnalizable objetivo, float multiplicadorDano)
    {
        StartCoroutine(DisparoBonus(objetivo, multiplicadorDano));
    }

    private IEnumerator DisparoBonus(IAnalizable objetivo, float multiplicadorDano)
    {
        Vector3 origen = projectileSpawn.position;
        Vector3 destino = objetivo.AnalysisTransform.position;
        Vector2 direccion = ((Vector2)(destino - origen)).normalized;
        float distancia = Vector2.Distance(origen, destino);

        Instantiate(particleShoot, origen, Quaternion.LookRotation(Vector3.forward, direccion));
        audioSource.PlayOneShot(shootAudioClip);

        ProyectileController proyectil = _pool.Get();
        proyectil.transform.position = origen;
        proyectil.ResetProyectile(origen);
        proyectil.SetDirection(direccion);
        proyectil.ConfigurarDisparoEspecial(multiplicadorDano, distancia);

        // Esperamos (en tiempo real) lo que tarde en llegar visualmente al objetivo
        // antes de aplicar el daño, para que el golpe coincida con el impacto visual.
        float tiempoDeViaje = distancia / proyectil.VelocidadActual;
        yield return new WaitForSecondsRealtime(tiempoDeViaje);

        objetivo.RecibirDanoAnalisis(proyectil.DanoActual);
    }

    public float ObtenerMultiplicadorContraEnemigo(IAnalizable enemigo)
    {
        EnemyAnalizable enemyAnalizable = enemigo as EnemyAnalizable;
        if (enemyAnalizable == null) return 1f;

        string nombreArma = gameObject.name.ToLower();

        if (nombreArma.Contains("seno") || nombreArma.Contains("sin"))
            return enemyAnalizable.multiplicadorSeno;
        else if (nombreArma.Contains("coseno") || nombreArma.Contains("cos"))
            return enemyAnalizable.multiplicadorCoseno;
        else if (nombreArma.Contains("tangente") || nombreArma.Contains("tan"))
            return enemyAnalizable.multiplicadorTangente;

        return 1f;
    }
}