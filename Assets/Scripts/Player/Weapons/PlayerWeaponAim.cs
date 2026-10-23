using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeaponAim : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private TimeStopManager timeStopManager;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Transform rotationPoint;
    [SerializeField] private float gamepadRotationSpeed = 1000f;
    private float _targetAngle;
    private float _currentAngle;
    private bool _isGamepadActive;
    void Update()
    {
        transform.position = rotationPoint.position;
        if (timeStopManager != null && timeStopManager.IsAnalysisActive)
            return;
        _isGamepadActive = playerInput.currentControlScheme == "Gamepad";

        if (_isGamepadActive)
            RotateTowardsStick();
        else
            RotateTowardsMouse();
    }
        private void RotateTowardsStick()
    {
        Vector2 stickInput = playerInput.actions["Look"].ReadValue<Vector2>();

        // Solo cambia el objetivo si el stick se usa (así no vuelve a 0 al soltar)
        if (stickInput.magnitude > 0.1f)
            _targetAngle = Mathf.Atan2(stickInput.y, stickInput.x) * Mathf.Rad2Deg;

        // Se acerca al objetivo poco a poco, por el camino corto
        _currentAngle = Mathf.MoveTowardsAngle(_currentAngle, _targetAngle, gamepadRotationSpeed * Time.deltaTime);

        transform.rotation = Quaternion.Euler(0, 0, _currentAngle);

        // Normaliza a 0-360 para que el flip funcione con ángulos negativos
        float a = Mathf.Repeat(_currentAngle, 360f);
        if(spriteRenderer != null) spriteRenderer.flipY = a >= 90f && a <= 270f;
    }
    private void RotateTowardsMouse()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        // Plano donde está el juego (Z = 0)
        Plane plane = new Plane(Vector3.forward, Vector3.zero);

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 mouseWorld = ray.GetPoint(distance);

            Vector3 direction = mouseWorld - transform.position;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            _currentAngle = angle;

            transform.rotation = Quaternion.Euler(0, 0, angle);
            if(spriteRenderer != null)spriteRenderer.flipY = angle >= 90 && angle <= 270;
        }
    }
}