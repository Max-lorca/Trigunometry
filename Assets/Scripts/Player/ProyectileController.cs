using UnityEngine;

public class ProyectileController : MonoBehaviour
{
    [HideInInspector] public Vector2 direction;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float proyectileSpeed = 8f;

    private Vector3 startPosition;
    private float time;

    [SerializeField] private float frecuency = 10f;
    [SerializeField] private float amplitude = 0.5f;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        time += Time.deltaTime;

        Vector3 basePosition = startPosition + (Vector3)(direction * proyectileSpeed * time);

        float wave = Mathf.Sin(time * frecuency) * amplitude;

        Vector2 perpendicular = new Vector2(-direction.y, direction.x).normalized;

        Vector3 offset = (Vector3)(perpendicular * wave);

        transform.position = offset + basePosition;
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        string tag = collision.gameObject.tag;

        Debug.Log($"Proyectil chocó con: {tag}");

        if (tag == "MeleeEnemy")
        {
            MeleeEnemyController meleeEnemy = collision.gameObject.GetComponent<MeleeEnemyController>();
            if (meleeEnemy != null)
            {
                meleeEnemy.TomarDaño(damage);
                Debug.Log($"✅ Daño a MeleeEnemy: {damage}");
            }
            Destroy(gameObject);
        }
        else if (tag == "DistanceEnemy")
        {
            DistanceEnemyController distEnemy = collision.gameObject.GetComponent<DistanceEnemyController>();
            if (distEnemy != null)
            {
                distEnemy.TomarDaño(damage);
                Debug.Log($"✅ Daño a DistanceEnemy: {damage}");
            }
            Destroy(gameObject);
        }
        else if (tag == "Ground")
        {
            Destroy(gameObject);
        }
    }
}