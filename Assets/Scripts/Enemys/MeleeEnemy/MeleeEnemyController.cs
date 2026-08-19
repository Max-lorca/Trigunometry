using UnityEngine;

/// <summary>
/// Enemigo melee. Todo lo genérico vive en EnemyBase.
/// Aquí solo queda lo específico: rotar el punto de ataque
/// hacia el jugador y ejecutar el golpe cuerpo a cuerpo.
/// </summary>
public class MeleeEnemyController : EnemyBase
{
    [SerializeField] private float rotationAttackVelocity;

    private MeleeEnemyAttack attack;

    protected override void Start()
    {
        base.Start(); // ejecuta el Start() de EnemyBase (rb, player, animator, sprite...)
        attack = GetComponent<MeleeEnemyAttack>();
    }

    protected override void Update()
    {
        // Lo único extra que hace este enemigo respecto al base:
        // rotar su punto de ataque para mirar siempre al jugador.
        Vector3 dir = (playerPosition.position - attack.puntoAtaque.transform.position).normalized;
        Quaternion rot = Quaternion.LookRotation(Vector3.forward, dir);
        attack.puntoAtaque.transform.rotation = Quaternion.Lerp(
            attack.puntoAtaque.transform.rotation,
            rot,
            Time.deltaTime * rotationAttackVelocity
        );

        base.Update(); // ejecuta toda la lógica común (vida, estados, etc.)
    }

    protected override void EstadoAtacar()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        animator.SetFloat("movement", 0f);

        if (attack.canAttack)
        {
            animator.SetTrigger("isAttacking");
            StartCoroutine(attack.AttackPerformance());
        }
    }
}