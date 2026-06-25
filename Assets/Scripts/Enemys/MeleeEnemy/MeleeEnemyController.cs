using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MeleeEnemyController : MonoBehaviour
{
    //Valores
    [SerializeField] private float velocityMovement;
    [SerializeField] private float rotationAttackVelocity;
    [SerializeField] private float vida;
    [SerializeField] private float minDistAttack;
    [SerializeField] private float minDistSpawn;
    [SerializeField] private float distanciaDeteccion;
    [SerializeField] private float dirTimeChange;
    private int patrolDir = 1;
    private float patrolTimer;
    private float distOfPlayer;
    private bool isDead = false;
    private bool isSpawn = false;
    private bool spawning = false;
    //Referencias
    [SerializeField] private GameObject spawnParticlePrefab;
    private Rigidbody2D rb;
    private Transform player;
    private MeleeEnemyAttack attack;
    private Animator animator;
    private SpriteRenderer spriteEnemy;
    //Logica del enemigo
    private enum estados{ oculto, moverse, atacar, buscar, aparecer}
    private estados estadoActual = estados.oculto;

    [Header("Drop")]
    [SerializeField] private GameObject healingItemPrefab;
    [SerializeField][Range(0f, 1f)] private float dropChance = 0.5f;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        attack = GetComponent<MeleeEnemyAttack>();
        animator = GetComponent<Animator>();
        spriteEnemy = GetComponent<SpriteRenderer>();

        Color c = spriteEnemy.color;
        c.a = 0f;
        spriteEnemy.color = c;
    }

    void Update()
    {
        Vector3 dir = (player.transform.position - attack.puntoAtaque.transform.position).normalized;
        Quaternion rot = Quaternion.LookRotation(Vector3.forward, dir);
        attack.puntoAtaque.transform.rotation = Quaternion.Lerp(attack.puntoAtaque.transform.rotation, rot, Time.deltaTime*rotationAttackVelocity);

        distOfPlayer = Vector2.Distance(transform.position, player.position);

        if (vida <= 0 && !isDead)
        {
            StartCoroutine(DeadSecuence());
            return;
        }
        if(isDead) return;

        SwitchEstados();

        switch (estadoActual)
        {
            case estados.oculto:
                rb.linearVelocity = Vector2.zero;
            break;
            case estados.aparecer:
                if (!spawning)
                {
                    StartCoroutine(SpawnSecuence());
                }
            break;
            case estados.buscar:
                BuscarPlayer();
            break;
            case estados.moverse:
                MoveToPlayer();
            break;
            case estados.atacar:
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                animator.SetFloat("movement", 0f);

                if (attack.canAttack)
                {
                    animator.SetTrigger("isAttacking");
                    StartCoroutine(attack.AttackPerformance());        
                }
             break;
        }
    }
    private void SwitchEstados()
    {

        if (!isSpawn)
        {
            if(distOfPlayer <= minDistSpawn)
            {
                estadoActual = estados.aparecer;
            }
            else
            {
                estadoActual = estados.oculto;
            }
            
        }
        else
        {
            if(distOfPlayer <= minDistAttack)
            {
                estadoActual = estados.atacar;
            }
            else if(distOfPlayer <= distanciaDeteccion)
            {
                estadoActual = estados.moverse;
            }
            else
            {
                estadoActual = estados.buscar;
            }
        }
    }
    // Patruya sin necesitadad de waypoints, lo mas probable es que el personaje se caiga si se coloca mucho tiempo de transición para cambiar de dirección
    // Claramente no es la mejor manera de hacer que un personaje patruye sin embargo sirve por el momento
    private void BuscarPlayer() // 
    {
        patrolTimer += Time.deltaTime;

        if(patrolTimer >= dirTimeChange)
        {
            patrolDir *= -1;
            patrolTimer = 0;
        }

        spriteEnemy.flipX = patrolDir < 0;
        rb.linearVelocity = new Vector2(
            patrolDir*velocityMovement,
             rb.linearVelocity.y
        );
        animator.SetFloat("movement", 1f);
        
    }
    private IEnumerator SpawnSecuence()
    {
        spawning = true;
        rb.linearVelocity = Vector2.zero;
        
        Instantiate(spawnParticlePrefab, transform.position, Quaternion.identity);
        
        float t = 0f;
        float duration = 1f;

        while( t < duration)
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

    private void MoveToPlayer()
    {
        Vector2 dir = (player.position - rb.transform.position).normalized;
        float movement = (dir*velocityMovement).x;
        if(movement < -0.1f)
        {
            spriteEnemy.flipX = true;
        }
        else if(movement > 0.1f)
        {
            spriteEnemy.flipX = false;
        }
        rb.linearVelocity = new Vector2(movement, rb.linearVelocity.y);
        animator.SetFloat("movement", Mathf.Abs(movement));

    }
    public void TomarDaño(float daño)
    {
        animator.SetTrigger("takeDamage");
        this.vida -= daño;
    }
    private void TryDropHealingItem()
    {
        if (healingItemPrefab != null && Random.value <= dropChance)
        {
            Instantiate(healingItemPrefab, transform.position, Quaternion.identity);
        }
    }

    private IEnumerator DeadSecuence()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        animator.SetBool("isDead", true);

        yield return new WaitForEndOfFrame();

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        float tiempoAnimacion = stateInfo.length;

        yield return new WaitForSeconds(tiempoAnimacion);

        TryDropHealingItem();
        Destroy(this.gameObject);
    }
}
