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
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Si golpea al player
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage((int)damage);
                Debug.Log("💥 Proyectil enemigo golpeó al player!");
            }
            Destroy(gameObject);
        }
        // Si golpea el suelo, plataforma o cualquier obstáculo
        else if (other.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }

        // Si golpea cualquier otra cosa que no sea el enemigo (para que no choque con otros enemigos)

        else if (!other.CompareTag("DistanceEnemy") && !other.CompareTag("MeleeEnemy"))
        {
            Destroy(gameObject);
        }
    }
}