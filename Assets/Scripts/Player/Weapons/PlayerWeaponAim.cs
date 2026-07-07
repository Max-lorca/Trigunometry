using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeaponAim : MonoBehaviour
{
    
    [SerializeField] private Camera cam;
    [SerializeField] private TimeStopManager timeStopManager;
    private SpriteRenderer _spriteRenderer;
    private PlayerInput _playerInput;
    private float _currentAngle;
    private bool _isGamepadActive;

    void Awake()
    {
        if (cam == null)
            cam = Camera.main;
        if (timeStopManager != null)
            timeStopManager = FindFirstObjectByType<TimeStopManager>();

        _playerInput = GetComponentInParent<PlayerInput>();
    }

    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (timeStopManager != null && timeStopManager.IsAnalysisActive)
            return;

        // Detecta si el último dispositivo usado fue un gamepad
        _isGamepadActive = _playerInput.currentControlScheme == "Gamepad";

        if (_isGamepadActive)
            RotateTowardsStick();
        else
            RotateTowardsMouse();
    }

    private void RotateTowardsStick()
    {
        Vector2 stickInput = _playerInput.actions["Look"].ReadValue<Vector2>();

        // Solo rota si el stick está siendo usado (evita que vuelva a 0 al soltar)
        if (stickInput.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(stickInput.y, stickInput.x) * Mathf.Rad2Deg;
            _currentAngle = angle;
        }

        transform.rotation = Quaternion.Euler(0, 0, _currentAngle);
        _spriteRenderer.flipY = _currentAngle >= 90 && _currentAngle <= 270;
    }

    private void RotateTowardsMouse()
    {
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        Vector3 direction = mouseWorld - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        _currentAngle = angle;

        transform.rotation = Quaternion.Euler(0, 0, _currentAngle);
        _spriteRenderer.flipY = _currentAngle >= 90 && _currentAngle <= 270;
    }
}