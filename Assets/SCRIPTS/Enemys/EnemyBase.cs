using UnityEngine;
using System.Collections;

/// <summary>
/// Clase base para todos los enemigos. Contiene la lógica compartida:
/// vida/daño/muerte, máquina de estados (oculto/aparecer/buscar/moverse/atacar),
/// drop de items y la implementación de IAnalizable.
///
/// Es "abstract" porque no tiene sentido tener un GameObject con un
/// "EnemyBase" pelado: siempre debe ser un enemigo concreto (Melee, Ranged, etc.)
/// </summary>
public abstract class EnemyBase : MonoBehaviour, IAnalizable
{
    // ---------- Valores ----------
    [Header("Vida")]
    [SerializeField] protected float vida;

    [Header("Movimiento")]
    [SerializeField] protected float velocityMovement;
    [SerializeField] protected float minDistAttack;
    [SerializeField] protected float minDistSpawn;
    [SerializeField] protected float distanciaDeteccion;
    [SerializeField] protected float dirTimeChange;

    protected int patrolDir = 1;
    protected float patrolTimer;
    protected float distOfPlayer;
    protected bool isDead = false;
    protected bool isSpawn = false;
    protected bool spawning = false;

    // ---------- Referencias ----------
    [SerializeField] protected GameObject spawnParticlePrefab;
    [SerializeField] protected GameObject deadParticlePrefab;
    protected Rigidbody2D rb;
    protected Transform playerPosition;
    protected Animator animator;
    protected SpriteRenderer spriteEnemy;

    // ---------- Estados ----------
    protected enum Estados { oculto, moverse, atacar, buscar, aparecer }
    protected Estados estadoActual = Estados.oculto;

    [Header("Drop")]
    [SerializeField] protected GameObject healingItemPrefab;
    [SerializeField][Range(0f, 1f)] protected float dropChance = 0.5f;

    [Header("Análisis Trigonométrico")]
    [SerializeField] protected float anguloGrados;
    [SerializeField] protected FuncionTrig funcionTrig;
    [SerializeField] protected Color colorSeleccion = new Color(1f, 0.85f, 0.2f);

    // Se activa cuando el jugador resuelve correctamente el análisis sobre este enemigo.
    protected bool dropGarantizadoAnalisis = false;

    // ---------- Ciclo de vida ----------

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerPosition = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
        spriteEnemy = GetComponent<SpriteRenderer>();

        spriteEnemy.material.SetFloat("_UseOutline", 0f);

        Color c = spriteEnemy.color;
        c.a = 0f;
        spriteEnemy.color = c;
    }

    protected virtual void Update()
    {
        distOfPlayer = Vector2.Distance(transform.position, playerPosition.position);
        spriteEnemy.material.SetFloat("_TiempoReal", Time.unscaledDeltaTime);

        if (vida <= 0 && !isDead)
        {
            StartCoroutine(DeadSecuence());
            return;
        }
        if (isDead) return;

        SwitchEstados();

        switch (estadoActual)
        {
            case Estados.oculto:
                rb.linearVelocity = Vector2.zero;
                break;
            case Estados.aparecer:
                if (!spawning)
                {
                    StartCoroutine(SpawnSecuence());
                }
                break;
            case Estados.buscar:
                BuscarPlayer();
                break;
            case Estados.moverse:
                MoveToPlayer();
                break;
            case Estados.atacar:
                EstadoAtacar();
                break;
        }
    }

    /// <summary>
    /// Cada enemigo decide qué hace en el estado "atacar".
    /// El melee rota y golpea, uno a distancia dispararía un proyectil, etc.
    /// </summary>
    protected abstract void EstadoAtacar();

    // ---------- Máquina de estados ----------

    protected virtual void SwitchEstados()
    {
        if (!isSpawn)
        {
            estadoActual = distOfPlayer <= minDistSpawn ? Estados.aparecer : Estados.oculto;
        }
        else
        {
            if (distOfPlayer <= minDistAttack)
            {
                estadoActual = Estados.atacar;
            }
            else if (distOfPlayer <= distanciaDeteccion)
            {
                estadoActual = Estados.moverse;
            }
            else
            {
                estadoActual = Estados.buscar;
            }
        }
    }

    protected virtual void BuscarPlayer()
    {
        patrolTimer += Time.deltaTime;

        if (patrolTimer >= dirTimeChange)
        {
            patrolDir *= -1;
            patrolTimer = 0;
        }

        spriteEnemy.flipX = patrolDir < 0;
        rb.linearVelocity = new Vector2(patrolDir * velocityMovement, rb.linearVelocity.y);
        animator.SetFloat("movement", 1f);
    }

    protected virtual IEnumerator SpawnSecuence()
    {
        spawning = true;
        rb.linearVelocity = Vector2.zero;

        Instantiate(spawnParticlePrefab, transform.position, Quaternion.identity);

        float t = 0f;
        float duration = 1f;

        while (t < duration)
        {
            t += Time.deltaTime;
            Color c = spriteEnemy.color;
            c.a = t / duration;
            spriteEnemy.color = c;

            yield return null;
        }

        yield return new WaitForSeconds(1f);

        isSpawn = true;
        spawning = false;
    }

    protected virtual void MoveToPlayer()
    {
        Vector2 dir = (playerPosition.position - rb.transform.position).normalized;
        float movement = (dir * velocityMovement).x;

        if (movement < -0.1f)
        {
            spriteEnemy.flipX = true;
        }
        else if (movement > 0.1f)
        {
            spriteEnemy.flipX = false;
        }

        rb.linearVelocity = new Vector2(movement, rb.linearVelocity.y);
        animator.SetFloat("movement", Mathf.Abs(movement));
    }

    // ---------- Vida / daño / muerte ----------

    public virtual void TomarDaño(float daño)
    {
        animator.SetTrigger("takeDamage");
        vida -= daño;
    }

    protected virtual void TryDropHealingItem()
    {
        if (healingItemPrefab == null) return;

        if (dropGarantizadoAnalisis)
        {
            Instantiate(healingItemPrefab, transform.position, Quaternion.identity);
            Instantiate(healingItemPrefab, transform.position + Vector3.up * 0.3f, Quaternion.identity);
            return;
        }

        if (Random.value <= dropChance)
        {
            Instantiate(healingItemPrefab, transform.position, Quaternion.identity);
        }
    }

    protected virtual IEnumerator DeadSecuence()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        animator.SetBool("isDead", true);

        yield return new WaitForEndOfFrame();

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float tiempoAnimacion = stateInfo.length;

        yield return new WaitForSeconds(tiempoAnimacion);

        Instantiate(deadParticlePrefab, transform.position, Quaternion.identity);

        TryDropHealingItem();
        Destroy(this.gameObject);
    }

    // ---------- Implementación de IAnalizable ----------

    public Transform AnalysisTransform => transform;

    public string FuncionTrigonometrica => funcionTrig.ToString().ToLower();

    public float AnguloGrados => anguloGrados;

    public float ValorCorrecto
    {
        get
        {
            float rad = anguloGrados * Mathf.Deg2Rad;
            switch (funcionTrig)
            {
                case FuncionTrig.Sin: return Mathf.Sin(rad);
                case FuncionTrig.Cos: return Mathf.Cos(rad);
                case FuncionTrig.Tan: return Mathf.Tan(rad);
                default: return 0f;
            }
        }
    }

    public virtual void OnSeleccionado()
    {
        spriteEnemy.material.SetFloat("_UseOutline", 1f);
    }

    public virtual void OnDeseleccionado()
    {
        spriteEnemy.material.SetFloat("_UseOutline", 0f);
    }

    public virtual void OnAnalisisExitoso(float multiplicadorDano)
    {
        dropGarantizadoAnalisis = true;
    }

    public virtual void OnAnalisisFallido()
    {
        // Espacio para feedback de error (sonido, flash rojo, etc.)
    }

    public virtual void RecibirDanoAnalisis(float dano)
    {
        TomarDaño(dano);
    }
}