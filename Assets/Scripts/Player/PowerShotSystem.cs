using UnityEngine;
using TMPro;

public class PowerShotSystem : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private TimeStopManager timeStopManager;
    [SerializeField] private AnalysisModeController analysisModeController;
    [SerializeField] private Camera cam;
    [SerializeField] private Transform weaponPivot;
    [SerializeField] private WeaponData weaponData;
    [SerializeField] private WeaponShoot weaponShoot;

    [Header("UI de Datos")]
    [SerializeField] private GameObject analysisUI;
    [SerializeField] private TMP_Text angleText;
    [SerializeField] private TMP_Text distanceText;
    [SerializeField] private TMP_Text enemyNameText;
    [SerializeField] private TMP_Text functionText;
    [SerializeField] private TMP_Text weaponInfoText;

    [Header("Línea de Apuntado")]
    [SerializeField] private LineRenderer aimLine;
    [SerializeField] private float maxAimDistance = 20f;

    [Header("Configuración")]
    [SerializeField] private float powerShotDamageMultiplier = 10f;

    private Transform currentTarget;
    private IAnalizable currentAnalizableTarget;
    private float currentAngle;
    private float currentHorizontalDistance;
    private bool isInitialized = false;

    void Start()
    {
        InicializarReferencias();
    }

    private void InicializarReferencias()
    {
        if (timeStopManager == null)
            timeStopManager = FindFirstObjectByType<TimeStopManager>();

        if (analysisModeController == null)
            analysisModeController = FindFirstObjectByType<AnalysisModeController>();

        if (weaponData == null)
            weaponData = GetComponent<WeaponData>();

        if (weaponShoot == null)
            weaponShoot = GetComponent<WeaponShoot>();

        if (cam == null)
            cam = Camera.main;

        if (analysisUI != null)
            analysisUI.SetActive(false);

        if (aimLine != null)
            aimLine.enabled = false;

        isInitialized = true;
        Debug.Log("✅ PowerShotSystem inicializado correctamente");
    }

    void Update()
    {
        if (!isInitialized)
        {
            InicializarReferencias();
            return;
        }

        if (timeStopManager == null)
        {
            timeStopManager = FindFirstObjectByType<TimeStopManager>();
            if (timeStopManager == null) return;
        }

        try
        {
            if (!timeStopManager.IsAnalysisActive)
            {
                OcultarUI();
                return;
            }
        }
        catch
        {
            OcultarUI();
            return;
        }

        UpdateAim();
    }

    private void OcultarUI()
    {
        if (analysisUI != null && analysisUI.activeSelf)
            analysisUI.SetActive(false);
        if (aimLine != null && aimLine.enabled)
            aimLine.enabled = false;
    }

    private void UpdateAim()
    {
        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null) return;
        }

        try
        {
            Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;

            Vector3 direction = mouseWorld - transform.position;
            currentAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            if (aimLine != null)
            {
                aimLine.enabled = true;
                aimLine.SetPosition(0, transform.position);
                aimLine.SetPosition(1, transform.position + (Vector3)(direction.normalized * maxAimDistance));
            }

            if (weaponPivot != null)
                weaponPivot.rotation = Quaternion.Euler(0, 0, currentAngle);

            if (analysisUI != null && !analysisUI.activeSelf)
                analysisUI.SetActive(true);

            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction.normalized, maxAimDistance);

            if (hit.collider != null && (hit.collider.CompareTag("MeleeEnemy") || hit.collider.CompareTag("DistanceEnemy")))
            {
                currentTarget = hit.transform;
                currentAnalizableTarget = hit.collider.GetComponent<IAnalizable>();

                if (timeStopManager != null)
                    timeStopManager.SetEnSeleccion(true);

                currentHorizontalDistance = Mathf.Abs(currentTarget.position.x - transform.position.x);

                if (angleText != null)
                    angleText.text = $"Ángulo: {currentAngle:F1}°";
                if (distanceText != null)
                    distanceText.text = $"Dist. X: {currentHorizontalDistance:F2}";
                if (enemyNameText != null)
                    enemyNameText.text = $"Objetivo: {hit.collider.gameObject.name}";

                if (functionText != null)
                {
                    string weaponName = weaponData != null ? weaponData.gameObject.name : "Desconocida";
                    functionText.text = $"Arma: {weaponName}";
                }

                if (weaponInfoText != null && currentAnalizableTarget != null)
                {
                    float multiplicador = ObtenerMultiplicadorArma(currentAnalizableTarget);
                    weaponInfoText.text = $"Multiplicador: x{multiplicador:F1}";

                    if (multiplicador >= 2f)
                        weaponInfoText.color = Color.green;
                    else if (multiplicador >= 1.5f)
                        weaponInfoText.color = Color.yellow;
                    else
                        weaponInfoText.color = Color.red;
                }
            }
            else
            {
                currentTarget = null;
                currentAnalizableTarget = null;

                if (timeStopManager != null)
                    timeStopManager.SetEnSeleccion(false);

                if (angleText != null) angleText.text = "Sin objetivo";
                if (distanceText != null) distanceText.text = "";
                if (enemyNameText != null) enemyNameText.text = "Sin objetivo";
                if (functionText != null) functionText.text = "Sin objetivo";
                if (weaponInfoText != null) weaponInfoText.text = "";
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"⚠️ Error en UpdateAim: {e.Message}");
            OcultarUI();
        }
    }

    private float ObtenerMultiplicadorArma(IAnalizable enemigo)
    {
        EnemyAnalizable enemyAnalizable = enemigo as EnemyAnalizable;
        if (enemyAnalizable == null) return 1f;

        string nombreArma = weaponData != null ? weaponData.gameObject.name.ToLower() : "";

        if (nombreArma.Contains("seno") || nombreArma.Contains("sin"))
            return enemyAnalizable.multiplicadorSeno;
        else if (nombreArma.Contains("coseno") || nombreArma.Contains("cos"))
            return enemyAnalizable.multiplicadorCoseno;
        else if (nombreArma.Contains("tangente") || nombreArma.Contains("tan"))
            return enemyAnalizable.multiplicadorTangente;

        return 1f;
    }

    public bool TryPowerShot()
    {
        if (timeStopManager == null || !timeStopManager.IsAnalysisActive) return false;
        if (currentTarget == null) return false;
        if (weaponData == null) return false;

        if (currentAnalizableTarget != null && analysisModeController != null)
        {
            Debug.Log($"💥 Disparo potente activado en modo Satoru!");

            float multiplicadorArma = ObtenerMultiplicadorArma(currentAnalizableTarget);
            float dañoFinal = powerShotDamageMultiplier * multiplicadorArma;

            weaponData.DispararAnalisisExitoso(currentAnalizableTarget, dañoFinal);

            StartCoroutine(SalirDelModo());
            return true;
        }

        return false;
    }

    private System.Collections.IEnumerator SalirDelModo()
    {
        yield return new WaitForSecondsRealtime(0.3f);
        if (timeStopManager != null)
            timeStopManager.TryTimeStop();
    }
}