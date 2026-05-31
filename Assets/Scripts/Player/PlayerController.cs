using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //Valores
    [SerializeField] private float velocityMovement;
    [SerializeField] private float jumpForce;
    [SerializeField] private int life;

    private bool canJump = true;
    
    //Referencias
    private Animator animador;
    private Rigidbody2D rbPlayer;
    private PlayerInput playerInput;
    private SpriteRenderer spritePlayer;

    //Vectores
    private Vector2 input;


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

        animador.SetFloat("movement", Mathf.Abs(input.x));

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
        if (ctx.performed && canJump)
        {
            canJump = false;
            rbPlayer.linearVelocity = new Vector2(rbPlayer.linearVelocity.x, jumpForce);
            animador.SetBool("jump", true);
        }
    }

    public void TakeDamage(int damage)
    {
        this.life -= damage;
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