using UnityEngine;

public class MeleeEnemyController : MonoBehaviour
{
    //Valores
    [SerializeField] private float velocityMovement;
    [SerializeField] private float rotationAttackVelocity;
    [SerializeField] private float jumpForce;
    [SerializeField] private float vida;
    [SerializeField] private float minDistAttack;
    //Referencias
    private Rigidbody2D rb;
    private Transform player;
    private MeleeEnemyAttack attack;
    private float distOfPlayer;
    
    //Logica del enemigo
    private enum estados{ moverse = 0, atacar = 1 }
    private estados estadoActual = estados.moverse;

    [Header("Drop")]
    [SerializeField] private GameObject healingItemPrefab;
    [SerializeField][Range(0f, 1f)] private float dropChance = 0.5f;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        attack = GetComponent<MeleeEnemyAttack>();
    }

    void Update()
    {
        Vector3 dir = (player.transform.position - attack.puntoAtaque.transform.position).normalized;
        Quaternion rot = Quaternion.LookRotation(Vector3.forward, dir);
        attack.puntoAtaque.transform.rotation = Quaternion.Lerp(attack.puntoAtaque.transform.rotation, rot, Time.deltaTime*rotationAttackVelocity);

        distOfPlayer = Vector2.Distance(transform.position, player.position);

        if (vida <= 0)
        {
            TryDropHealingItem();
            Destroy(this.gameObject);
        }

        SwitchEstados();

        switch (estadoActual)
        {
            case estados.moverse:
            MoveToPlayer();
                break;
            case estados.atacar:
                if (attack.canAttack)
                {
                    StartCoroutine(attack.AttackPerformance());        
                }
                break;
        }
    }
    private void SwitchEstados()
    {
        if(distOfPlayer >= minDistAttack)
        {
            estadoActual = estados.moverse;
        }
        else if(distOfPlayer <= minDistAttack)
        {
            estadoActual = estados.atacar;
        }

    }

    private void MoveToPlayer()
    {
        Vector2 dir = (player.position - rb.transform.position).normalized;
        rb.linearVelocity = new Vector2((dir * velocityMovement).x, rb.linearVelocity.y);
    }
    public void TomarDaño(float daño)
    {
        this.vida -= daño;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        /*
        switch (collision.gameObject.tag)
        {
            case "Player":
                PlayerController player = collision.gameObject.GetComponent<PlayerController>();
                player.TakeDamage(1);
                break;
        }
        */
    }


    private void TryDropHealingItem()
    {
        if (healingItemPrefab != null && Random.value <= dropChance)
        {
            Instantiate(healingItemPrefab, transform.position, Quaternion.identity);
        }
    }
}
