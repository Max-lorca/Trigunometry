using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeleeEnemyAttack : MonoBehaviour
{

    [SerializeField] private int damage;
    [SerializeField] private float cooldown;
    [SerializeField] private float radAttack = 1.5f;

    [SerializeField] public Transform puntoAtaque;
    [SerializeField] private LayerMask capaJugador; // Para ignorar todo lo que no sea un enemigo con el raycast

    private List<Collider2D> hits = new List<Collider2D>();

    [HideInInspector] public bool canAttack = true;
    [HideInInspector] public bool estaEnFrameDeAtaque = false;
    private MeleeEnemyController _controller;

    private void Start()
    {
        _controller.GetComponent<MeleeEnemyController>();
    }
    public IEnumerator AttackPerformance()
    {
        canAttack = false;
        
        estaEnFrameDeAtaque = true;

        ContactFilter2D filtro = new ContactFilter2D();

        filtro.SetLayerMask(capaJugador);
        filtro.useLayerMask = true;

        int hitsValue = Physics2D.OverlapCircle(
            puntoAtaque.position, radAttack, filtro,  hits);

        for(int i = 0; i < hitsValue; i++)
        {
            if(hits[i].gameObject.CompareTag("Player"))
            {
                PlayerController player = hits[i].GetComponent<PlayerController>();
                player.TakeDamage(this.damage, (Vector2)transform.position);
            }
        }

        estaEnFrameDeAtaque = false;
        yield return new WaitForSeconds(cooldown);
        canAttack = true;
    }

    public void InterrumpirPorParry()
    {
        StopAllCoroutines();
        estaEnFrameDeAtaque = false;

        if (_controller != null)
        {
            _controller.TomarDaño(0);
        }

        StartCoroutine(ResetCooldownDespuesDeParry());
    }

    private IEnumerator ResetCooldownDespuesDeParry()
    {
        canAttack = false;
        yield return new WaitForSeconds(cooldown);
        canAttack = true;
    }
}