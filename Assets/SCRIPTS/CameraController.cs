using UnityEngine;
using Unity.Cinemachine; // En Cinemachine 3.x se usa este namespace

public class CameraController : MonoBehaviour
{
    // Public
    [Header("Camera Zoom Settings")]
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float zoomAmount = 20f;

    // Private
    [HideInInspector] public CameraShake cameraShake;
    private Transform _target;
    private bool _isZooming = false;
    private Transform _dummyTarget;

    private CinemachineCamera _cinemachineCamera;

    private float _defaultFOV;

    private void Start()
    {
        cameraShake = GetComponent<CameraShake>();
        _cinemachineCamera = GetComponent<CinemachineCamera>();

        _dummyTarget = new GameObject("CameraFocus").transform;
        if (GameManager.Instance != null && GameManager.Instance.Player != null)
            _dummyTarget.position = GameManager.Instance.Player.position;

        if (_cinemachineCamera != null)
        {
            _cinemachineCamera.Follow = _dummyTarget;  // <-- esto faltaba, controla la POSICIÓN
            _cinemachineCamera.LookAt = _dummyTarget;  // esto controla la ROTACIÓN
            _defaultFOV = _cinemachineCamera.Lens.FieldOfView;
        }
    }

    private void Update()
    {
        ZoomIn();
    }

    private void ZoomIn()
    {
        if (_isZooming && _target != null)
        {
            // Zoom suave
            _cinemachineCamera.Lens.FieldOfView = Mathf.Lerp(
                _cinemachineCamera.Lens.FieldOfView,
                zoomAmount,
                Time.unscaledDeltaTime * zoomSpeed
            );

            // Calcular punto medio real en coordenadas del mundo entre jugador y objetivo
            Vector3 puntoMedio = (GameManager.Instance.Player.position + _target.position) * 0.5f;
            // Mover solo el dummy target para que la cámara mire al punto medio sin modificar al enemigo
            if (_dummyTarget != null)
                _dummyTarget.position = puntoMedio;
        }
        else
        {
            // Volver al FOV original
            _cinemachineCamera.Lens.FieldOfView = Mathf.Lerp(
                _cinemachineCamera.Lens.FieldOfView,
                _defaultFOV,
                Time.unscaledDeltaTime * zoomSpeed
            );
            // Volver a mirar al jugador moviendo el dummy (no cambiamos la posición de otros transform)
            if (GameManager.Instance.Player != null && _dummyTarget != null)
                _dummyTarget.position = GameManager.Instance.Player.position;
        }
    }

    public void ZoomCamTo(Transform target)
    {
        _target = target;
        _isZooming = true;
    }

    public void ResetZoom()
    {
        _isZooming = false;
        _target = null;
    }
}