using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    [SerializeField] private Camera cam;

    [SerializeField] private float rotationSpeed = 15f; // opcional si quieres suavizar
    private float currentAngle;

    void Awake()
    {
        if (cam == null)
            cam = Camera.main;
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        RotateTowardsMouse();
    }

    private void RotateTowardsMouse()
    {
        float targetAngle = GetMouseAngle();
        
        currentAngle = targetAngle;


        transform.rotation = Quaternion.Euler(0, 0, currentAngle);

        // Flip del sprite
        spriteRenderer.flipY = currentAngle >= 90 && currentAngle <= 270;
    }

    private float GetMouseAngle()
    {
        Vector3 mouseScreen = Input.mousePosition;

        Vector3 mouseWorld = cam.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = 0f;

        Vector3 direction = mouseWorld - transform.position;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        return angle;
    }
}