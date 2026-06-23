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
    [SerializeField] private float dirTimeChange;
    private float distOfPlayer;
    private bool isDead = false;
    private bool isSpawn = false;
    //Referencias
    [SerializeField] private GameObject spawnParticlePrefab;
    private Rigidbody2D rb;
    private Transform player;
    private MeleeEnemyAttack attack;
    private Animator animator;
    private SpriteRenderer spriteEnemy;
    //Logica del enemigo
    private enum estados{ moverse = 0, atacar = 1, buscar = 2, aparecer = 3}
    private estados estadoActual = estados.moverse;

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
            case estados.aparecer:
                SpawnSecuence();
            break;
            case estados.buscar:

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
        if(distOfPlayer < minDistSpawn && !isSpawn)
        {
            estadoActual = estados.aparecer;
            isSpawn = true;
        }
        else if(distOfPlayer < minDistSpawn && isSpawn)
        {
            estadoActual = estados.buscar;
        }
        else if(distOfPlayer >= minDistAttack)
        {
            estadoActual = estados.moverse;
        }
        else if(distOfPlayer <= minDistAttack)
        {
            estadoActual = estados.atacar;
        }

    }
    // Terminar logica de la función para la patruya sin waypoints
    private IEnumerator BuscarPlayer()
    {
        Vector2 dir = Vector2.right;
        rb.linearVelocity += dir*velocityMovement;
        yield return new WaitForSeconds(dirTimeChange);
        dir = Vector2.right * -1;
        
    }
    private void SpawnSecuence()
    {
        Color c = spriteEnemy.color;
        c.a = 1f;
        spriteEnemy.color = c;
        Instantiate(spawnParticlePrefab, transform.position, Quaternion.identity);
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
