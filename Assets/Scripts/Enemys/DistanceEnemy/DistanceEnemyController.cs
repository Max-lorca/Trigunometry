using UnityEngine;
using System.Collections;

/// <summary>
/// Enemigo a distancia. Hereda de EnemyBase (vida, daño, drop, debilidades,
/// IAnalizable) pero NO usa su máquina de estados, porque este enemigo no tiene
/// aparición oculta: reemplaza Update() con sus propios estados
/// (regresar / perseguir / huir / atacar).
/// </summary>
public class DistanceEnemyController : EnemyBase
{
    [Header("Muerte")]
    [SerializeField] private float dieLagTime = 1f;
    [SerializeField] private float deadFadeAlpha = 0f;
    [SerializeField] private float deadFadeTime = 1.5f;

    [Header("Distancias de Control")]
    [Tooltip("Distancia máxima a la que el enemigo detecta al jugador.")]
    [SerializeField] private float minDistDetect = 12f;
    [Tooltip("Distancia ideal donde el enemigo se queda quieto y ataca.")]
    [SerializeField] private float maxDistAttack = 8f;
    [Tooltip("Si el jugador está más cerca que esto, el enemigo huye.")]
    [SerializeField] private float minDistSafe = 4f;

    [Header("Debug")]
    [Tooltip("Imprime en consola cada cambio de estado del enemigo")]
    [SerializeField] private bool debugEstados = false;

    private Vector2 posicionInicial;
    private DistEnemyWeapon weaponController;
    private FadeController fadeController;
    private Collider2D col;

    private enum EstadoDist { regresar, perseguir, huir, atacar }
    private EstadoDist estadoDist = EstadoDist.regresar;

    protected override void Start()
    {
        // EnemyBase.Start() deja el sprite invisible (alpha 0) para su secuencia
        // de aparición. Este enemigo es visible desde el inicio, así que
        // guardamos el alpha original y lo restauramos después.
        float alphaOriginal = GetComponent<SpriteRenderer>().color.a;

        base.Start();

        Color c = spriteEnemy.color;
        c.a = alphaOriginal;
        spriteEnemy.color = c;
        isSpawn = true;

        fadeController = GetComponent<FadeController>();
        col = GetComponent<Collider2D>();
        posicionInicial = transform.position;

        // Busca el arma en cualquier hijo (ya no depende de que se llame "Weapon")
        weaponController = GetComponentInChildren<DistEnemyWeapon>();
        if (weaponController == null)
            Debug.LogWarning($"{name}: no se encontró ningún DistEnemyWeapon en sus hijos, no podrá disparar.");
    }

    protected override void Update()
    {
        spriteEnemy.material.SetFloat("_TiempoReal", Time.unscaledDeltaTime);

        if (vida <= 0 && !isDead)
        {
            StartCoroutine(DeadSecuence());
            return;
        }
        if (isDead) return;

        // Durante el knockback no se mueve ni actúa (si no, pisaría la velocidad del empuje)
        if (_knockbackController != null && _knockbackController.EstaEnKnockback)
            return;

        if (playerPosition != null)
        {
            distOfPlayer = Vector2.Distance(transform.position, playerPosition.position);
        }

        CambiarEstados();
        EjecutarEstado();
    }

    private void CambiarEstados()
    {
        EstadoDist nuevo;

        if (playerPosition == null || distOfPlayer > minDistDetect)
            nuevo = EstadoDist.regresar;        // fuera de rango -> vuelve a casa
        else if (distOfPlayer < minDistSafe)
            nuevo = EstadoDist.huir;            // demasiado cerca -> huye
        else if (distOfPlayer <= maxDistAttack)
            nuevo = EstadoDist.atacar;          // rango correcto -> ataca
        else
            nuevo = EstadoDist.perseguir;       // detectado pero lejos -> se acerca

        if (debugEstados && nuevo != estadoDist)
            Debug.Log($"{name}: {estadoDist} -> {nuevo} (distancia {distOfPlayer:F1})");

        estadoDist = nuevo;
    }

    private void EjecutarEstado()
    {
        switch (estadoDist)
        {
            case EstadoDist.regresar:
                RegresarAPosicionInicial();
                break;
            case EstadoDist.huir:
                MoverseEnDireccion((transform.position - playerPosition.position).normalized);
                break;
            case EstadoDist.perseguir:
                MoverseEnDireccion((playerPosition.position - transform.position).normalized);
                break;
            case EstadoDist.atacar:
                EstadoAtacar();
                break;
        }
    }

    // Requerido por EnemyBase (método abstracto)
    protected override void EstadoAtacar()
    {
        rb.linearVelocity = Vector2.zero;
        animator.SetFloat("movement", 0f);
        ControlarGiro(playerPosition.position.x - transform.position.x);

        if (weaponController != null && weaponController.canShoot)
        {
            animator.SetTrigger("ataque");
            StartCoroutine(weaponController.Shoot());
        }
    }

    private void MoverseEnDireccion(Vector2 direccion)
    {
        rb.linearVelocity = new Vector2(direccion.x, 0f) * velocityMovement;
        ControlarGiro(direccion.x);
        animator.SetFloat("movement", Mathf.Abs(rb.linearVelocity.x));
    }

    private void RegresarAPosicionInicial()
    {
        float distAlOrigen = Vector2.Distance(transform.position, posicionInicial);

        if (distAlOrigen < 0.2f)
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetFloat("movement", 0f);
        }
        else
        {
            Vector2 dirAlOrigen = (posicionInicial - (Vector2)transform.position).normalized;
            MoverseEnDireccion(dirAlOrigen);
        }
    }

    private void ControlarGiro(float direccionX)
    {
        if (direccionX > 0.1f) spriteEnemy.flipX = false;
        else if (direccionX < -0.1f) spriteEnemy.flipX = true;
    }

    // Secuencia de muerte propia (con fade), pero con las mismas recompensas
    // que EnemyBase: puntos, item de vida y loot de modificadores.
    protected override IEnumerator DeadSecuence()
    {
        isDead = true;

        rb.linearVelocity = Vector2.zero;
        animator.SetFloat("movement", 0f);

        // El cadáver no debe bloquear al jugador ni recibir disparos
        if (col != null) col.enabled = false;

        // Kinematic para que la física no lo siga empujando mientras se desvanece
        rb.bodyType = RigidbodyType2D.Kinematic;

        animator.SetBool("isDead", true);
        yield return new WaitForSeconds(dieLagTime);

        // Desvanecimiento es una corrutina (así la usa ShockWaveManager), hay que iniciarla
        if (fadeController != null)
            StartCoroutine(fadeController.Desvanecimiento(spriteEnemy, deadFadeAlpha, deadFadeTime));

        if (deadParticlePrefab != null)
            Instantiate(deadParticlePrefab, transform.position, Quaternion.identity);

        TryDropHealingItem();
        if (GameManager.Instance != null) GameManager.Instance.AddPoints(10f);
        GetComponent<EDropItem>()?.Drop();

        // Si tu FadeController ya destruye el GameObject al terminar,
        // borra esta espera y el Destroy para no duplicarlo.
        yield return new WaitForSeconds(deadFadeTime);

        Destroy(gameObject);
    }
}