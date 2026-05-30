using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //Valores
    [SerializeField] private float velocityMovement;
    [SerializeField] private float jumpForce;
    [SerializeField] private int life;
    
    //Referencias
    private Animator animador;
    private Rigidbody2D rbPlayer;
    private PlayerInput playerInput;
    private SpriteRenderer spritePlayer;

    //Vectores
    private Vector2 input;

    //Funciones principales (Start, Update, FixedUpdate, etc)
    void Start()
    {
        rbPlayer = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        animador = GetComponent<Animator>();
        spritePlayer = GetComponent<SpriteRenderer>();
    }
    void Update()
    {
        input = playerInput.actions["Move"].ReadValue<Vector2>();

        animador.SetFloat("movement", Mathf.Abs(velocityMovement * input.x));

        if(input.x < 0)
        {
            spritePlayer.flipX = true;
        }
        else if(input.x > 0)
        {
            spritePlayer.flipX = false;
        }
        if(life <= 0)   Destroy(this.gameObject);
    }
    private void FixedUpdate()
    {
        rbPlayer.linearVelocity = new Vector2(input.x * velocityMovement, rbPlayer.linearVelocity.y);
    }
    //Funciones Mecanicas

    //Funciones Input
    public void JumpAction(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            rbPlayer.linearVelocity = new Vector2(rbPlayer.linearVelocity.x, jumpForce);
        }
    }

    public void TakeDamage(int damage)
    {
        this.life -= damage;
    }
}