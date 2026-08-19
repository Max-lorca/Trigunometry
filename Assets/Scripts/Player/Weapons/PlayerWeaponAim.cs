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

        // Solo rota si el stick está siendo usado (evita que vuelva a 0 al soltar)
        if (stickInput.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(stickInput.y, stickInput.x) * Mathf.Rad2Deg;
            _currentAngle = angle;
        }

        transform.rotation = Quaternion.Euler(0, 0, _currentAngle);
        spriteRenderer.flipY = _currentAngle >= 90 && _currentAngle <= 270;
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
            spriteRenderer.flipY = angle >= 90 && angle <= 270;
        }
    }
}