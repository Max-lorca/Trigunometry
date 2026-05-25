using UnityEngine;

public class MeleeEnemyController : MonoBehaviour
{
    //Valores
    [SerializeField] private float velocityMovement;
    [SerializeField] private float jumpForce;
    [SerializeField] private float life;

    private float playerJumpOffset = 5f;
    private bool arePlataform = false;
    private GameObject plataform;

    //Referencias
    private Rigidbody2D rb;
    private Transform player;
    private MeleeEnemyAttack attack;

    //Logica del enemigo
    private enum estados{ moverse = 0, atacar = 1 }
    private estados estadoActual = estados.moverse;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if(life >= 0)
        {
            Destroy(this.gameObject);
        }
        switch (estadoActual)
        {
            case estados.moverse:
            MovementLogic();
                break;
            case estados.atacar:
            StartCoroutine(attack.AttackPerformance());
                break;
        }
    }

    private void MovementLogic()
    {
        if(player.position.z >= playerJumpOffset && arePlataform)
        {
            Jump();
        }
        else
        {
            MoveToPlayer();
        }
    }

    private void MoveToPlayer()
    {
        Vector2 dir = (player.position - rb.transform.position).normalized;
        rb.linearVelocity = dir * velocityMovement;
    }
    private void Jump()
    {
        Vector2 dir = (rb.transform.position - plataform.transform.position).normalized;
        rb.AddForce(dir*jumpForce, ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Plataform":
            plataform = collision.gameObject;
            arePlataform = true;
                break;
        }
    }
}
