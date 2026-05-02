using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //Referencias
    private Rigidbody2D rbPlayer;
    private PlayerInput playerInput;
    //Vectores
    private Vector2 input;
    //Valores
    [SerializeField] private float velocityMovement;
    [SerializeField] private float jumpForce;
    //Funciones principales (Start, Update, FixedUpdate, etc)
    void Start()
    {
        rbPlayer = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
    }
    void Update()
    {
        input = playerInput.actions["Move"].ReadValue<Vector2>();
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
}