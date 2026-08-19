using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using TMPro;

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
    private int _facingDirection = 1; // 1 derecha, -1 izquierda
    private bool canJump = true;
    private bool isDead = false;
    private bool isSpawn = false;
    private bool inAir = false;
    private bool canAnimateLand = false;

    //Referencias
    [SerializeField] private GameObject walkParticlePrefab;
    [SerializeField] private GameObject deadParticle;
    [SerializeField] private GameObject spawnParticle;
    [SerializeField] private ParticleSystem lifeParticle;
    [SerializeField] private CanvasManager menuCanvasManager;
    [SerializeField] private CanvasManager deadCanvasManager;
    [SerializeField] private CinemachineCamera MainCamera;
    private CanvasGroup _deadCanvasGroup;
    private CanvasGroup _menuCanvasGroup;
    private ParticleSystem _walkParticle;
    private Transform _walkParticleTransform;
    private P_AnimationController _animador;
    private Rigidbody2D _rbPlayer;
    [HideInInspector] public PlayerInput _playerInput;
    private List<SpriteRenderer> _spritesPlayer;
    private List<Material> _spriteMaterials = new List<Material>();
    private HealthUIController _healthUI;
    private TimeStopManager _timeStopController;
    private FadeController _fadeController;
    private KnockbackController _knockbackController;
    private AudioSource _audioSource;
    //Vectores
    private Vector2 _input;

    void Start()
    {
        _rbPlayer = GetComponent<Rigidbody2D>();
        _playerInput = GetComponent<PlayerInput>();
        _animador = GetComponent<P_AnimationController>();
        _spritesPlayer = new List<SpriteRenderer>(GetComponentsInChildren<SpriteRenderer>());
        foreach(var materials in _spritesPlayer)
        {
            _spriteMaterials.Add(materials.material);
        }
        _timeStopController = GetComponent<TimeStopManager>();
        _fadeController = GetComponent<FadeController>();
        _menuCanvasGroup = menuCanvasManager.GetComponent<CanvasGroup>();
        _deadCanvasGroup = deadCanvasManager.GetComponent<CanvasGroup>();
        _walkParticle = Instantiate(walkParticlePrefab, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
        _walkParticleTransform = _walkParticle.transform;
        _knockbackController = GetComponent<KnockbackController>();
        _audioSource = GetComponent<AudioSource>();

        currentLife = maxLife;

        _healthUI = FindFirstObjectByType<HealthUIController>();
        
        foreach(var sprite in _spritesPlayer)
        {
            _fadeController.Desvanecimiento(sprite, 0f, 0f);
        }

        if (_healthUI != null)
        {
            _healthUI.UpdateHealth(currentLife, maxLife);
        }

        if (!isSpawn && !isDead)
        {
            StartCoroutine(Spawn());
        }
    }
    private void Update()
    {
        if (isDead)
            return;

        _input = _playerInput.actions["Move"].ReadValue<Vector2>();

        _animador.WalkAnimation(Mathf.Abs(_input.x));
        // 1. Control de rotación y escala según la dirección del movimiento
        if (_input.x < 0 && _facingDirection != -1)
        {
            _facingDirection = -1;
            _walkParticleTransform.localScale = new Vector3(-1, 1, 1);
            _animador.RotatePlayer(false);
        }
        else if (_input.x > 0 && _facingDirection != 1)
        {
            _facingDirection = 1;
            _walkParticleTransform.localScale = new Vector3(1, 1, 1);
            _animador.RotatePlayer(true);
        }
        // 2. Control de emisión de partículas (¡La clave está aquí!)
        if (_input.x != 0) // Si el jugador se está moviendo a cualquier lado
        {
            // Solo le damos Play si NO se estaban reproduciendo ya
            if (!_walkParticle.isPlaying && !inAir)
            {
                _walkParticle.Play();
            }
        }
        else // Si el jugador está quieto (input.x == 0)
        {
            // Detiene la emisión suavemente sin desaparecer las partículas activas
            if (_walkParticle.isPlaying || inAir)
            {
                _walkParticle.Stop();
            }
        }
    }

    void FixedUpdate()
    {
        if (isDead)
            return;

        _rbPlayer.linearVelocity = new Vector2(_input.x * velocityMovement, _rbPlayer.linearVelocity.y);
    }

    public void TakeDamage(int damage, Vector2 origen)
    {
        if (isDead)
            return;

        currentLife -= damage;
        _audioSource.PlayOneShot(damageAudio, 6f);
        Debug.Log($"Player daño: {currentLife}/{maxLife}");
        
        _knockbackController.RecibirKnockBack(origen);
        MainCamera.GetComponent<CameraController>().cameraShake.Shake("Default");

        if (_healthUI != null)
            _healthUI.UpdateHealth(currentLife, maxLife);

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
        
        _audioSource.PlayOneShot(healAudio, 6f);
        lifeParticle.Play();

        Debug.Log($"Player curado: {currentLife}/{maxLife}");

        if (_healthUI != null)
            _healthUI.UpdateHealth(currentLife, maxLife);
    }

    private IEnumerator Spawn()
    {
        isDead = false;
        _playerInput.enabled = false;
        _rbPlayer.linearVelocity = Vector2.zero;

        Instantiate(spawnParticle, transform.position, Quaternion.identity);

        yield return new WaitForSeconds(spawnLagTime);

        StartCoroutine(_fadeController.Desvanecimiento(_spritesPlayer, spawnFade, fadeTime));

        _playerInput.enabled = true;
    }
    private IEnumerator Die()
    {
        isDead = true;

        _playerInput.enabled = false;
        _rbPlayer.linearVelocity = Vector2.zero;

        // Si tienes un Trigger llamado "Death" en el Animator
        // animador.SetTrigger("Death");

        // Espera un poco antes del fade
        yield return new WaitForSeconds(dieLagTime);

        yield return StartCoroutine(
            _fadeController.Desvanecimiento(_spritesPlayer, deadFade, fadeTime)
        );

        Instantiate(deadParticle, transform.position, Quaternion.identity);

        _menuCanvasGroup.blocksRaycasts  = false;
        _menuCanvasGroup.interactable = false;
        deadCanvasManager.ToggleMenu();
    }


    public void JumpAction(InputAction.CallbackContext ctx)
    {
        if (isDead)
            return;

        if (ctx.performed && canJump)
        {
            canJump = false;
            inAir = true;
            canAnimateLand = true;

            _rbPlayer.linearVelocity = new Vector2(_rbPlayer.linearVelocity.x, jumpForce);

            _animador.SetGrounded(false);
        }
    }

    public void AnalisisTimeStop(InputAction.CallbackContext ctx)
    {
        if (isDead)
            return;

        if (ctx.performed)
        {
            _timeStopController.TryTimeStop();
        }
    }

    public void MenuTimeStop(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            menuCanvasManager.ToggleMenu();
        }
    }
    private void OnCollisionEnter2D(Collision2D collider)
    {
        switch (collider.gameObject.tag)
        {
            case "Ground":
                canJump = true;
                inAir = false;
                _animador.SetGrounded(true);
                break;
        }
    }
}