using UnityEngine;
using System.Collections;

public class DistanceEnemyController : MonoBehaviour
{
    // Valores
    [SerializeField] private float vida;
    [SerializeField] private float dieLagTime = 1f;
    [SerializeField] private float deadFadeAlpha = 0f;
    [SerializeField] private float deadFadeTime = 1.5f;
    [SerializeField] private float velocidadMovimiento = 3f; 

    [Header("Distancias de Control")]
    [Tooltip("Distancia máxima a la que el enemigo detecta al jugador.")]
    [SerializeField] private float minDistDetect = 12f;
    [Tooltip("Distancia ideal donde el enemigo se queda quieto y ataca.")]
    [SerializeField] private float maxDistAttack = 8f;
    [Tooltip("Si el jugador está más cerca que esto, el enemigo huye.")]
    [SerializeField] private float minDistSafe = 4f;

    private float distOfPlayer; 
    private bool isDead = false;
    private Vector2 posicionInicial; // Guardar el punto de origen

    // Referencias
    private Rigidbody2D rb;
    private Transform player;
    private DistEnemyWeapon weaponController;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private FadeController fadeController;

    // Estados
    private enum Estados { regresar, perseguir, huir, atacar, muerto }
    private Estados estadoActual = Estados.regresar; 

    [Header("Drop")]
    [SerializeField] private GameObject healingItemPrefab;
    [SerializeField][Range(0f, 1f)] private float dropChance = 0.5f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        fadeController = GetComponent<FadeController>();

        // Guardamos la posición donde spawnea el enemigo
        posicionInicial = transform.position;

        player = GameObject.FindGameObjectWithTag("Player").transform;

        Transform weaponTransform = transform.Find("Weapon");
        if(weaponTransform != null)
        {
            weaponController = weaponTransform.GetComponent<DistEnemyWeapon>();
        }
    }

    void Update()
    {
        // Corrección de la lógica de muerte
        if (vida <= 0 && !isDead)
        {
            StartCoroutine(DieSecuence());
            return; 
        }

        if (isDead) return;

        if (player != null)
        {
            distOfPlayer = Vector2.Distance(transform.position, player.position);
        }

        CambiarEstados();
        EjecutarEstado();
    }

    private void CambiarEstados()
    {
        // 1. Si el jugador está fuera del rango de detección -> Regresa a casa
        if (distOfPlayer > minDistDetect || player == null)
        {
            estadoActual = Estados.regresar;
        }
        // 2. Si está demasiado cerca -> ¡Peligro! Huir a una distancia segura
        else if (distOfPlayer < minDistSafe)
        {
            estadoActual = Estados.huir;
        }
        // 3. Si está en el rango correcto -> Atacar
        else if (distOfPlayer <= maxDistAttack && distOfPlayer >= minDistSafe)
        {
            estadoActual = Estados.atacar;
        }
        // 4. Si está detectado pero aún muy lejos para atacar -> Perseguir
        else if (distOfPlayer > maxDistAttack && distOfPlayer <= minDistDetect)
        {
            estadoActual = Estados.perseguir;
        }
    }

    private void EjecutarEstado()
    {
        switch(estadoActual)
        {
            case Estados.regresar:
                RegresarAPosicionInicial();
                break;
            case Estados.huir:
                MoverseEnDireccion((transform.position - player.position).normalized); // Dirección opuesta al jugador
                break;
            case Estados.perseguir:
                MoverseEnDireccion((player.position - transform.position).normalized); // Dirección hacia el jugador
                break;
            case Estados.atacar:
                rb.linearVelocity = Vector2.zero;
                animator.SetFloat("movement", 0f); // <-- esto faltaba
                ControlarGiro(player.position.x - transform.position.x);
                if (weaponController != null && weaponController.canShoot)
                {
                    animator.SetTrigger("ataque");
                    StartCoroutine(weaponController.Shoot());
                }
            break;
        case Estados.muerto:
                rb.linearVelocity = Vector2.zero;
                animator.SetFloat("movement", 0f); // <-- por si acaso
            break;
                }
    }

    private void MoverseEnDireccion(Vector2 direccion)
    {
        rb.linearVelocity = new Vector2(direccion.x, 0f) * velocidadMovimiento;
        ControlarGiro(direccion.x);
        animator.SetFloat("movement", Mathf.Abs(rb.linearVelocity.x));
    }

    private void RegresarAPosicionInicial()
    {
        float distAlOrigen = Vector2.Distance(transform.position, posicionInicial);

        // Si ya llegó cerca de su origen, se detiene
        if (distAlOrigen < 0.2f)
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetFloat("movement", 0f);
        }
        else
        {
            Vector2 dirAlOrigen = ((Vector2)posicionInicial - (Vector2)transform.position).normalized;
            MoverseEnDireccion(dirAlOrigen);
        }
    }

    private void ControlarGiro(float direccionX)
    {
        if (direccionX > 0.1f)      spriteRenderer.flipX = false;
        else if (direccionX < -0.1f) spriteRenderer.flipX = true;
    }

    private IEnumerator DieSecuence()
    {
        isDead = true;
        estadoActual = Estados.muerto;
        
        animator.SetBool("isDead", true);
        yield return new WaitForSeconds(dieLagTime);

        fadeController.Desvanecimiento(spriteRenderer, deadFadeAlpha, deadFadeTime);
        TryDropHealingItem();
    }

    public void TomarDaño(float damage)
    {
        this.vida -= damage;
    }
    private void TryDropHealingItem()
    {
        if(healingItemPrefab != null && Random.value <= dropChance)
        {
            Instantiate(healingItemPrefab, transform.position, Quaternion.identity);
        }
    }
}