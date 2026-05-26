using UnityEngine;

public class MeleeEnemyController : MonoBehaviour
{
    //Valores
    [SerializeField] private float velocityMovement;
    [SerializeField] private float rotationAttackVelocity;
    [SerializeField] private float jumpForce;
    [SerializeField] private float life;
    [SerializeField] private float minDistAttack;
    //Referencias
    private Rigidbody2D rb;
    private Transform player;
    private MeleeEnemyAttack attack;
    private float distOfPlayer;
    
    //Logica del enemigo
    private enum estados{ moverse = 0, atacar = 1 }
    private estados estadoActual = estados.moverse;


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

        if(life <= 0)
        {
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
}
