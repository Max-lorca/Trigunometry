using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //Valores
    [Header("Stats")]
    [SerializeField] private float velocityMovement;
    [SerializeField] private float jumpForce;
    [SerializeField] private int maxLife = 4;
    private int currentLife;
    private bool canJump = true;
    private bool isMoving = false;

    //Referencias
    [SerializeField] private ParticleSystem walkParticle;
    private Transform walkParticleTransform;
    [SerializeField] private ParticleSystem lifeParticle;
    private Animator animador;
    private Rigidbody2D rbPlayer;
    private PlayerInput playerInput;
    private SpriteRenderer spritePlayer;
    private HealthUIController healthUI;
    private TimeStopManager timeStopController;

    //Vectores
    private Vector2 input;

    void Start()
    {
        rbPlayer = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        animador = GetComponent<Animator>();
        spritePlayer = GetComponent<SpriteRenderer>();
        timeStopController = GetComponent<TimeStopManager>();

        walkParticleTransform = walkParticle.GetComponent<Transform>();


        currentLife = maxLife;

        healthUI = FindFirstObjectByType<HealthUIController>();
        if (healthUI != null)
        {
            healthUI.UpdateHealth(currentLife, maxLife);
        }
            
    }

    void Update()
    {
        input = playerInput.actions["Move"].ReadValue<Vector2>();

        animador.SetFloat("movement", Mathf.Abs(input.x));

        if (input.x < 0)
        {
            isMoving = true;
            walkParticleTransform.localScale = new Vector3(-1,1,1);
            spritePlayer.flipX = true;
        }
        else if (input.x > 0)
        {
            isMoving = true;
            walkParticleTransform.localScale = new Vector3(1,1,1);
            spritePlayer.flipX = false;
        }
        else
        {
            isMoving = false;
        }
        if (isMoving)
        {
            walkParticle.Play();
        }
        else
        {
            walkParticle.Stop();
        }

        if (currentLife <= 0)
        {
            Die();
        }
    }

    private void FixedUpdate()
    {
        rbPlayer.linearVelocity = new Vector2(input.x * velocityMovement, rbPlayer.linearVelocity.y);
    }

    //Funciones Mecanicas
    public void TakeDamage(int damage)
    {
        currentLife -= damage;
        Debug.Log($"Player da�o: {currentLife}/{maxLife}");

        if (healthUI != null)
            healthUI.UpdateHealth(currentLife, maxLife);

        if (currentLife <= 0)
            Die();
    }

    public void Heal(int amount)
    {
        currentLife = Mathf.Min(currentLife + amount, maxLife);
        lifeParticle.Play();
        Debug.Log($"Player curado: {currentLife}/{maxLife}");

        if (healthUI != null)
            healthUI.UpdateHealth(currentLife, maxLife);
    }

    private void Die()
    {
        Debug.Log("Player muri�!");
        gameObject.SetActive(false);
    }

    //Funciones Input
    public void JumpAction(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && canJump)
        {
            canJump = false;
            rbPlayer.linearVelocity = new Vector2(rbPlayer.linearVelocity.x, jumpForce);
            animador.SetBool("jump", true);
        }
    }
    public void AnalisisTimeStop(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            timeStopController.TryTimeStop("analisis");
        }
    }
    public void MenuTimeStop(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            timeStopController.TryTimeStop("menu");
        }
    }

    private void OnCollisionEnter2D(Collision2D collider)
    {
        switch (collider.gameObject.tag)
        {
            case "Ground":
                canJump = true;
                animador.SetBool("jump", false);
                break;
        }
    }
}