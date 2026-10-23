using UnityEngine;
using System.Collections;

public class KnockbackController : MonoBehaviour
{
    [Header("Fuerza")]
    [Tooltip("Velocidad horizontal (unidades/seg) que recibe el personaje, siempre hacia el lado contrario al golpe.")]
    [SerializeField] private float forceX = 7f;
    [Tooltip("Velocidad vertical (unidades/seg) hacia arriba. Es siempre la misma, venga el golpe de donde venga.")]
    [SerializeField] private float forceY = 2f;
    [Tooltip("Segundos que dura el estado de knockback (EstaEnKnockback = true).")]
    [SerializeField] private float duration = 0.6f;

    [Header("Debug")]
    [Tooltip("Imprime en consola cada vez que se llama a RecibirKnockBack.")]
    [SerializeField] private bool debugKnockback = false;

    private Rigidbody2D _rb;
    private bool _enKnockback = false;

    // Tu script de movimiento debe NO tocar la velocidad mientras esto sea true
    public bool EstaEnKnockback => _enKnockback;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    public void RecibirKnockBack(Vector3 origenDelGolpe)
    {
        if (debugKnockback)
            Debug.Log($"[Knockback] llamado | origen: {origenDelGolpe} | ya en knockback: {_enKnockback}");

        if (_enKnockback || _rb == null) return;
        StartCoroutine(EjecutarKnockBack(origenDelGolpe));
    }

    private IEnumerator EjecutarKnockBack(Vector3 origen)
    {
        _enKnockback = true;

        // Solo importa de qué lado vino el golpe (en X). Antes se usaba la dirección
        // completa normalizada, así que la fuerza cambiaba según el ángulo: golpe desde
        // arriba = empuje hacia el suelo (casi nada), golpe a la misma altura = empuje máximo.
        float lado = transform.position.x >= origen.x ? 1f : -1f;

        // Velocidad directa en vez de AddForce(Impulse): el resultado ya no depende de la masa
        // del Rigidbody (un impulso se divide entre la masa, con masa baja salías volando).
        _rb.linearVelocity = new Vector2(lado * forceX, forceY);

        if (debugKnockback)
            Debug.Log($"[Knockback] aplicado | velocidad: {_rb.linearVelocity}");

        yield return new WaitForSeconds(duration);

        _enKnockback = false;
    }

    private void OnDisable()
    {
        // Si el objeto se desactiva a mitad del knockback, la corrutina se corta y la
        // bandera se quedaría en true para siempre, ignorando todos los golpes siguientes.
        _enKnockback = false;
    }
}