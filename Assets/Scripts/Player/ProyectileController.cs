using UnityEngine;

public class ProyectileController : MonoBehaviour
{
    [HideInInspector] public Vector2 direction; //Dada por la dirección del arma utilizada
    //Valores
    [SerializeField] private float damage;
    [SerializeField] private float proyectileSpeed;

    //Variables de movimiento ondulatorio
    private Vector3 startPosition;
    private Vector3 endPosition;
    private float time;

    [SerializeField] private float frecuency = 10f;
    [SerializeField] private float amplitude = 0.5f;


    //Funcion Sin(x). y = a*Sin(w*t)
    /*
     * a = amplitud
     * w = frecuencia
     */
    private void Start()
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
        switch (collision.gameObject.tag)
        {
            case "MeleeEnemy":
                MeleeEnemyController meleeEnemy = collision.gameObject.GetComponent<MeleeEnemyController>();
                meleeEnemy.TomarDaño(damage);
            break;
            case "DistanceEnemy":
                DistanceEnemyController distEnemy = collision.gameObject.GetComponent<DistanceEnemyController>();
                distEnemy.TomarDaño(damage);
            break; 
            //default:
            //  Destroy(this.gameObject);
            //break;
        }
    }
}
