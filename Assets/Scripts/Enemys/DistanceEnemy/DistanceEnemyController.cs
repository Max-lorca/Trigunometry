using UnityEngine;

public class DistanceEnemyController : MonoBehaviour
{
    //Valores
    [SerializeField] private float vida;
        // velocidad baja de movimiento ya que estos enemigos no necesitan moverse mucho
    [SerializeField] private float velocidadMovimiento; 
    [SerializeField] private float minDistToMovement;

    private float distOfPlayer;

    //Referencias
    private Rigidbody2D rb;
    private Transform player;
    private DistEnemyWeapon weaponController;

    //Estados
    private enum Estados {moverse = 0, disparar = 1}
    private Estados estadoActual = Estados.moverse;

    [Header("Drop")]
    [SerializeField] private GameObject healingItemPrefab;
    [SerializeField][Range(0f, 1f)] private float dropChance = 0.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Encontrar el script del arma en el objeto hijo
        Transform weaponTransform = transform.Find("Weapon");
        if(weaponTransform != null)
        {
            weaponController = weaponTransform.GetComponent<DistEnemyWeapon>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (vida <= 0)
        {
            TryDropHealingItem();
            Destroy(this.gameObject);
        }

        distOfPlayer = Vector2.Distance(transform.position, player.position);

        CambiarEstados();

        switch(estadoActual)
        {
            case Estados.moverse:   
            Movimiento();
                break;
            case Estados.disparar:
                if (weaponController.canShoot)
                {
                    StartCoroutine(weaponController.Shoot());
                }
                break;
        }
    }
    private void CambiarEstados()
    {
        if(distOfPlayer >= minDistToMovement)
        {
            estadoActual = Estados.disparar;
        }
        else if(distOfPlayer < minDistToMovement)
        {
            estadoActual = Estados.moverse;
        }
    }
    private void Movimiento()
    {
        Vector2 dir = (rb.transform.position - player.position).normalized;
        rb.linearVelocity = new Vector2(dir.x, 0f) * velocidadMovimiento;
    }
    public void TomarDaño(float daño)
    {
        this.vida -= daño;
    }

    private void TryDropHealingItem()
    {
        if (healingItemPrefab != null && Random.value <= dropChance)
        {
            Instantiate(healingItemPrefab, transform.position, Quaternion.identity);
        }
    }
}
