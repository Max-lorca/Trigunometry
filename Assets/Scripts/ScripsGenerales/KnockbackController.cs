using UnityEngine;
using System.Collections;
public class KnockbackController : MonoBehaviour
{
    [SerializeField] private float forceX = 10f;
    [SerializeField] private float forceY = 2f;
    [SerializeField] private float duration = 0.6f;

    private Rigidbody2D _rb;
    private bool _enKnockback = false;
    [HideInInspector] public bool EstaEnKnockback => _enKnockback;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    public void RecibirKnockBack(Vector3 origenDelGolpe)
    {
        if (_enKnockback) return;
        StartCoroutine(EjecutarKnockBack(origenDelGolpe));
    }

    private IEnumerator EjecutarKnockBack(Vector3 origen)
    {
        _enKnockback = true;

        Vector2 dir = (transform.position - origen).normalized;
        _rb.linearVelocity = Vector3.zero;
        _rb.AddForce(new Vector2(dir.x*forceX, dir.y*forceY), ForceMode2D.Impulse);
        yield return new WaitForSeconds(duration);
        _enKnockback = false;
    }

}
