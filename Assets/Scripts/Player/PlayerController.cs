using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    //Valores
    [Header("Stats")]
    [SerializeField] private float velocityMovement;
    [SerializeField] private float jumpForce;
    [SerializeField] private int maxLife = 4;
    [Header("Fade Config")]
    [SerializeField][Range(0f, 1f)] private float deadFade;
    [SerializeField][Range(0f, 1f)] private float spawnFade;
    [SerializeField][Range(0f, 10f)] private float fadeTime;

    [Header("Shake Camera Config")] 
    [SerializeField] [Range(0f, 5f)] private float duration;
    [SerializeField] [Range(0f, 2f)] private float magnitude;
    [Header("Animation Config")]
    [SerializeField] private float dieLagTime;
    [SerializeField] private float spawnLagTime;
    [Header("Audios")]
    [SerializeField] private AudioClip jumpAudio;
    [SerializeField] private AudioClip deadAudio;
    [SerializeField] private AudioClip spawnAudio;
    [SerializeField] private AudioClip damageAudio;
    [SerializeField] private AudioClip healAudio;

    private int currentLife;
    private bool canJump = true;
    private bool isMoving = false;
    private bool isDead = false;
    private bool isSpawn = false;

    //Referencias
    [SerializeField] private ParticleSystem walkParticle;
    [SerializeField] private GameObject deadParticle;
    [SerializeField] private GameObject spawnParticle;
    [SerializeField] private ParticleSystem lifeParticle;
    [SerializeField] private CanvasManager menuCanvasManager;
    [SerializeField] private CanvasManager deadCanvasManager;
    private CameraShake cameraShake;
    private CanvasGroup deadCanvasGroup;
    private CanvasGroup menuCanvasGroup;
    private Transform walkParticleTransform;
    private Animator animador;
    private Rigidbody2D rbPlayer;
    private PlayerInput playerInput;
    private SpriteRenderer spritePlayer;
    private HealthUIController healthUI;
    private TimeStopManager timeStopController;
    private FadeController fadeController;
    private KnockbackController knockbackController;
    private ParryController parryController;
    private AudioSource audioSource;

    //Vectores
    private Vector2 input;

    void Start()
    {
        rbPlayer = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        animador = GetComponent<Animator>();
        spritePlayer = GetComponent<SpriteRenderer>();
        timeStopController = GetComponent<TimeStopManager>();
        fadeController = GetComponent<FadeController>();
        menuCanvasGroup = menuCanvasManager.GetComponent<CanvasGroup>();
        deadCanvasGroup = deadCanvasManager.GetComponent<CanvasGroup>();
        cameraShake = GetComponent<CameraShake>();
        walkParticleTransform = walkParticle.transform;
        knockbackController = GetComponent<KnockbackController>();
        parryController = GetComponent<ParryController>();
        audioSource = GetComponent<AudioSource>();

        currentLife = maxLife;

        healthUI = FindFirstObjectByType<HealthUIController>();

        Color c = spritePlayer.color;
        c.a = 0f;
        spritePlayer.color = c;

        if (healthUI != null)
        {
            healthUI.UpdateHealth(currentLife, maxLife);
        }

        if (!isSpawn && !isDead)
        {
            StartCoroutine(Spawn());
        }

    }

    void Update()
    {
        if (isDead)
            return;

        input = playerInput.actions["Move"].ReadValue<Vector2>();

        animador.SetFloat("movement", Mathf.Abs(input.x));

        if (input.x < 0)
        {
            isMoving = true;
            walkParticleTransform.localScale = new Vector3(-1, 1, 1);
            spritePlayer.flipX = true;
        }
        else if (input.x > 0)
        {
            isMoving = true;
            walkParticleTransform.localScale = new Vector3(1, 1, 1);
            spritePlayer.flipX = false;
        }
        else
        {
            isMoving = false;
        }

        if (isMoving)
            walkParticle.Play();
        else
            walkParticle.Stop();
    }

    void FixedUpdate()
    {
        if (isDead)
            return;

        rbPlayer.linearVelocity = new Vector2(input.x * velocityMovement, rbPlayer.linearVelocity.y);
    }

    public void TakeDamage(int damage, Vector3 origenAtaque)
    {
        if (isDead)
            return;

        currentLife -= damage;
        audioSource.PlayOneShot(damageAudio, 6f);
        Debug.Log($"Player daño: {currentLife}/{maxLife}");
        
        cameraShake.Shake(duration, magnitude);
        knockbackController.RecibirKnockBack(origenAtaque);

        if (healthUI != null)
            healthUI.UpdateHealth(currentLife, maxLife);

        if (!isDead && currentLife <= 0)
        {
            StartCoroutine(Die());
        }
    }

    public void Heal(int amount)
    {
        if (isDead)
            return;
        

        currentLife = Mathf.Min(currentLife + amount, maxLife);
        
        audioSource.PlayOneShot(healAudio, 6f);
        lifeParticle.Play();

        Debug.Log($"Player curado: {currentLife}/{maxLife}");

        if (healthUI != null)
            healthUI.UpdateHealth(currentLife, maxLife);
    }

    private IEnumerator Spawn()
    {
        isDead = false;
        playerInput.enabled = false;
        rbPlayer.linearVelocity = Vector2.zero;

        Instantiate(spawnParticle, transform.position, Quaternion.identity);

        yield return new WaitForSeconds(spawnLagTime);

        StartCoroutine(fadeController.Desvanecimiento(spritePlayer, spawnFade, fadeTime));

        playerInput.enabled = true;
    }
    private IEnumerator Die()
    {
        isDead = true;

        playerInput.enabled = false;
        rbPlayer.linearVelocity = Vector2.zero;

        // Si tienes un Trigger llamado "Death" en el Animator
        // animador.SetTrigger("Death");

        // Espera un poco antes del fade
        yield return new WaitForSeconds(dieLagTime);

        yield return StartCoroutine(
            fadeController.Desvanecimiento(spritePlayer, deadFade, fadeTime)
        );

        Instantiate(deadParticle, transform.position, Quaternion.identity);

        menuCanvasGroup.blocksRaycasts  = false;
        menuCanvasGroup.interactable = false;
        deadCanvasManager.ToggleMenu();
    }


    public void JumpAction(InputAction.CallbackContext ctx)
    {
        if (isDead)
            return;

        if (ctx.performed && canJump)
        {
            canJump = false;

            rbPlayer.linearVelocity = new Vector2(rbPlayer.linearVelocity.x, jumpForce);

            animador.SetBool("jump", true);
        }
    }

    public void AnalisisTimeStop(InputAction.CallbackContext ctx)
    {
        if (isDead)
            return;

        if (ctx.performed)
        {
            timeStopController.TryTimeStop();
        }
    }

    public void MenuTimeStop(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            menuCanvasManager.ToggleMenu();
        }
    }

    public void ParryAction(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            Debug.Log("Parry");
            parryController.TryParry();
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