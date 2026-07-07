using UnityEngine;
using System.Collections;
public class KnockbackController : MonoBehaviour
{
    [SerializeField] private float forceX = 5f;
    [SerializeField] private float forceY = 2f;
    [SerializeField] private float duration = 0.2f;

    private Rigidbody2D _rb;
    private bool _enKnockback = false;
    [HideInInspector] public bool EstaEnKnockback => _enKnockback;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    public void RecibirKnockBack(Vector2 origenDelGolpe)
    {
        if (_enKnockback) return;
        StartCoroutine(EjecutarKnockBack(origenDelGolpe));
    }

    private IEnumerator EjecutarKnockBack(Vector2 origen)
    {
        _enKnockback = true;

        Vector2 dir = ((Vector2)transform.position - origen).normalized;
        _rb.linearVelocity = Vector2.zero;
        _rb.AddForce(new Vector2(dir.x*forceX, forceY), ForceMode2D.Impulse);
        yield return new WaitForSeconds(duration);
        _enKnockback = false;
    }

}
