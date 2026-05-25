using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeleeEnemyAttack : MonoBehaviour
{

    [SerializeField] private int damage;
    [SerializeField] private float cooldown;
    [SerializeField] private float radAttack = 1.5f;

    public Transform puntoAtaque;
    public LayerMask capaEnemigos; // Para ignorar todo lo que no sea un enemigo con el raycast

    private List<Collider2D> hits = new List<Collider2D>();

    private bool canAttack = true;

    public IEnumerator AttackPerformance()
    {
        canAttack = false;

        ContactFilter2D filtro = new ContactFilter2D();

        filtro.SetLayerMask(capaEnemigos);
        filtro.useLayerMask = true;

        int hitsValue = Physics2D.OverlapCircle(
            puntoAtaque.position, radAttack, filtro,  hits);

        for(int i = 0; i < hitsValue; i++)
        {
            if(hits[i].gameObject.tag == "Player")
            {
                PlayerController player = hits[i].GetComponent<PlayerController>();
                player.TakeDamage(this.damage);
            }
        }

        yield return new WaitForSeconds(cooldown);
        canAttack = true;
    }
}