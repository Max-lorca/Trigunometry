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
    
    private enum Estados {moverse = 0, disparar = 1}
    private Estados estadoActual = Estados.moverse;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        distOfPlayer = Vector2.Distance(transform.position, player.position);

        CambiarEstados();

        switch(estadoActual)
        {
            case Estados.moverse:   
            Movimiento();
                break;
            case Estados.disparar:
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
        rb.linearVelocity = new Vector2(dir.x, 0f) * velocidadMovimiento * Time.deltaTime;
    }
    public void TomarDaño(float damage)
    {
        this.vida -= damage;
    }
}
