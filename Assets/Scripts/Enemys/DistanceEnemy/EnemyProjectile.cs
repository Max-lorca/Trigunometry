using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float speed = 5f;

    private Vector2 direction;

    public void SetDamage(float dmg)
    {
        damage = dmg;
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    public void SetSpeed(float spd)
    {
        speed = spd;
    }

    void Start()
    {
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed) * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Golpea al player
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage((int)damage, transform.position);
            }
            Destroy(gameObject);
            return;
        }

        // Ignorar a cualquier enemigo (detecta por componente, no por tag:
        // así funciona con todos los hijos de EnemyBase sin importar su tag)
        if (other.GetComponentInParent<EnemyBase>() != null) return;

        // Suelo / plataformas
        if (other.CompareTag("Ground"))
        {
            Destroy(gameObject);
            return;
        }

        // Cualquier otro sólido; los triggers (zonas, pickups, etc.) se ignoran
        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}