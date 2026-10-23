using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class FlyingTriangle : EnemyBase
{
    [Header("Configuración Disparo")]
    [SerializeField] protected float anticipationCooldownAttack = 1f;
    [SerializeField] protected float shootCooldown = 1.5f;
    [SerializeField] protected GameObject projectilPrefab;
    [Header("Movilidad")]
    [Range(0, 10)] protected float distanceFromGround = 7f;
    [SerializeField] protected float evasionDistance = 5f;
    
    protected bool canAttack = true;

    protected override void Start()
    {
        base.Start();
    }
    protected override void Update()
    {
        base.Update();
    }

    protected override void EstadoAtacar()
    {
        if (estadoActual == Estados.atacar)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetFloat("movement", 0f);

            if (canAttack)
            {
                animator.SetTrigger("isAttacking");
                StartCoroutine(Attack());
            }
        }
    }
    protected IEnumerator Attack()
    {
        canAttack = false;
        yield return new WaitForSeconds(anticipationCooldownAttack);
        Vector2 dir = (transform.position - GameManager.Instance.Player.position).normalized;
        GameObject p = Instantiate(projectilPrefab, transform.position, Quaternion.identity);
        p.GetComponent<ProyectileController>().SetDirection(dir);
        yield return new WaitForSeconds(shootCooldown);
        canAttack = true;
    }

    protected override void MoveToPlayer()
    {
        
    }
    
}
