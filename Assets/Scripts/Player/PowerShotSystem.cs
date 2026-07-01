using UnityEngine;
using TMPro;
using System.Collections;

public class PowerShotSystem : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private TimeStopManager timeStopManager;
    [SerializeField] private AnalysisModeController analysisModeController;
    [SerializeField] private Camera cam;
    [SerializeField] private WeaponShoot weaponShoot;
    [SerializeField] private SatoruTriangleVisualizer triangleVisualizer;
    [SerializeField] private SatoruAngleSlider angleSlider;

    [Header("UI de Datos")]
    [SerializeField] private GameObject analysisUI;
    [SerializeField] private TMP_Text distanceText;
    [SerializeField] private TMP_Text heightText;
    [SerializeField] private TMP_Text functionHintText;
    [SerializeField] private TMP_Text enemyNameText;
    [SerializeField] private TMP_Text feedbackText;

    [Header("Configuración")]
    [SerializeField] private float maxAimDistance = 20f;
    [SerializeField] private float rayoDuration = 0.5f;

    private Transform currentTarget;
    private IAnalizable currentAnalizableTarget;
    private float currentAngle = 0f;
    private bool isInitialized = false;

    private WeaponData GetCurrentWeapon()
    {
        if (weaponShoot != null && weaponShoot.currentWeapon != null)
        {
            return weaponShoot.currentWeapon;
        }

        WeaponData weapon = GetComponentInChildren<WeaponData>();
        if (weapon != null)
        {
            return weapon;
        }

        weapon = FindFirstObjectByType<WeaponData>();
        return weapon;
    }

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

        if (weaponShoot == null)
            weaponShoot = GetComponent<WeaponShoot>();

        if (cam == null)
            cam = Camera.main;

        if (triangleVisualizer == null)
            triangleVisualizer = GetComponent<SatoruTriangleVisualizer>();

        if (angleSlider == null)
            angleSlider = FindFirstObjectByType<SatoruAngleSlider>();

        if (analysisUI != null)
            analysisUI.SetActive(false);

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
                OcultarTodo();
                return;
            }
        }
        catch
        {
            OcultarTodo();
            return;
        }

        DetectarEnemigoCercano();
    }

    private void DetectarEnemigoCercano()
    {
        Collider2D[] enemigos = Physics2D.OverlapCircleAll(transform.position, maxAimDistance);
        Transform enemigoMasCercano = null;
        float distanciaMinima = float.MaxValue;

        foreach (Collider2D col in enemigos)
        {
            if (col.CompareTag("MeleeEnemy") || col.CompareTag("DistanceEnemy"))
            {
                float dist = Vector2.Distance(transform.position, col.transform.position);
                if (dist < distanciaMinima)
                {
                    distanciaMinima = dist;
                    enemigoMasCercano = col.transform;
                    currentAnalizableTarget = col.GetComponent<IAnalizable>();
                }
            }
        }

        if (enemigoMasCercano != null)
        {
            currentTarget = enemigoMasCercano;
            float dx = Mathf.Abs(currentTarget.position.x - transform.position.x);
            float dy = Mathf.Abs(currentTarget.position.y - transform.position.y);

            if (distanceText != null)
                distanceText.text = $"Distancia X: {dx:F2}";
            if (heightText != null)
                heightText.text = $"Altura Y: {dy:F2}";
            if (enemyNameText != null)
                enemyNameText.text = $"Objetivo: {currentTarget.gameObject.name}";

            string funcion = ObtenerFuncion();
            if (functionHintText != null)
                functionHintText.text = $"Usa: {funcion}";

            if (triangleVisualizer != null)
            {
                Vector2 jugadorPos = transform.position;
                Vector2 enemigoPos = currentTarget.position;
                WeaponData weapon = GetCurrentWeapon();
                string armaNombre = weapon != null ? weapon.gameObject.name : "";
                triangleVisualizer.DibujarTriangulo(jugadorPos, enemigoPos, funcion, armaNombre);
            }

            if (angleSlider != null && !angleSlider.gameObject.activeSelf)
                angleSlider.MostrarSlider();

            if (analysisUI != null && !analysisUI.activeSelf)
                analysisUI.SetActive(true);
        }
        else
        {
            currentTarget = null;
            currentAnalizableTarget = null;
            OcultarTodo();
        }
    }

    private string ObtenerFuncion()
    {
        WeaponData weapon = GetCurrentWeapon();
        if (weapon == null) return "sin(θ) = Y/H";

        string nombreArma = weapon.gameObject.name.ToLower();
        if (nombreArma.Contains("seno") || nombreArma.Contains("sin"))
            return "sin(θ) = Y / H";
        else if (nombreArma.Contains("coseno") || nombreArma.Contains("cos"))
            return "cos(θ) = X / H";
        else if (nombreArma.Contains("tangente") || nombreArma.Contains("tan"))
            return "tan(θ) = Y / X";

        return "sin(θ) = Y / H";
    }

    public void SetCurrentAngle(float angle)
    {
        currentAngle = angle;
        Debug.Log($"📐 Ángulo actualizado: {currentAngle}°");
    }

    public bool TryPowerShot()
    {
        Debug.Log($"🔫 TryPowerShot() - Ángulo actual: {currentAngle}°");

        if (timeStopManager == null || !timeStopManager.IsAnalysisActive)
        {
            Debug.LogWarning("❌ Modo Satoru no está activo");
            return false;
        }

        if (currentTarget == null)
        {
            Debug.LogWarning("❌ No hay objetivo");
            return false;
        }

        WeaponData weapon = GetCurrentWeapon();
        if (weapon == null)
        {
            Debug.LogWarning("❌ No hay arma equipada");
            return false;
        }

        float anguloCorrecto = CalcularAnguloCorrecto(weapon);
        Debug.Log($"🎯 Ángulo seleccionado: {currentAngle}° | Ángulo correcto: {anguloCorrecto}° | Margen: ±5°");

        if (Mathf.Abs(currentAngle - anguloCorrecto) <= 5f)
        {
            Debug.Log($"💥 Ángulo DENTRO DEL MARGEN! PowerShot activado.");

            if (feedbackText != null)
                feedbackText.text = $"¡ACERTÓ! 🎯 ({currentAngle:F1}°)";

            // RAYO VISUAL
            if (currentTarget != null)
            {
                StartCoroutine(DibujarRayo(currentTarget.position));
            }

            // ✅ MUERTE DIRECTA (sin depender de IAnalizable)
            if (currentTarget != null)
            {
                MeleeEnemyController meleeEnemy = currentTarget.GetComponent<MeleeEnemyController>();
                DistanceEnemyController distEnemy = currentTarget.GetComponent<DistanceEnemyController>();

                if (meleeEnemy != null)
                {
                    meleeEnemy.TomarDaño(99999f);
                    Debug.Log($"💀 MeleeEnemy {currentTarget.name} destruido!");
                }
                else if (distEnemy != null)
                {
                    distEnemy.TomarDaño(99999f);
                    Debug.Log($"💀 DistanceEnemy {currentTarget.name} destruido!");
                }
                else
                {
                    // Fallback: intentar con IAnalizable
                    if (currentAnalizableTarget != null && analysisModeController != null)
                    {
                        float dañoFinal = 99999f;
                        weapon.DispararAnalisisExitoso(currentAnalizableTarget, dañoFinal);
                    }
                }
            }

            // Limpiar referencias
            if (analysisModeController != null)
            {
                analysisModeController.LimpiarEnemigo();
            }

            currentTarget = null;
            currentAnalizableTarget = null;

            StartCoroutine(SalirDelModo());
            return true;
        }
        else
        {
            Debug.Log($"❌ Ángulo FUERA DEL MARGEN. Debería ser {anguloCorrecto}° (±5°)");

            if (feedbackText != null)
                feedbackText.text = $"FALLÓ ❌ (Debe ser {anguloCorrecto:F1}° ±5°)";

            return false;
        }
    }

    private float CalcularAnguloCorrecto(WeaponData weapon)
    {
        if (currentTarget == null) return 0f;

        Vector2 jugadorPos = transform.position;
        Vector2 enemigoPos = currentTarget.position;

        float dx = enemigoPos.x - jugadorPos.x;
        float dy = enemigoPos.y - jugadorPos.y;
        float hipotenusa = Mathf.Sqrt(dx * dx + dy * dy);

        string nombreArma = weapon.gameObject.name.ToLower();
        float angulo = 0f;

        if (nombreArma.Contains("seno") || nombreArma.Contains("sin"))
        {
            angulo = Mathf.Asin(Mathf.Clamp(dy / hipotenusa, -1f, 1f)) * Mathf.Rad2Deg;
        }
        else if (nombreArma.Contains("coseno") || nombreArma.Contains("cos"))
        {
            angulo = Mathf.Acos(Mathf.Clamp(dx / hipotenusa, -1f, 1f)) * Mathf.Rad2Deg;
        }
        else if (nombreArma.Contains("tangente") || nombreArma.Contains("tan"))
        {
            angulo = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
        }
        else
        {
            angulo = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
        }

        if (angulo < 0) angulo += 360f;

        return angulo;
    }

    private float ObtenerMultiplicadorArma(IAnalizable enemigo)
    {
        EnemyAnalizable enemyAnalizable = enemigo as EnemyAnalizable;
        if (enemyAnalizable == null) return 1f;

        WeaponData weapon = GetCurrentWeapon();
        if (weapon == null) return 1f;

        string nombreArma = weapon.gameObject.name.ToLower();

        if (nombreArma.Contains("seno") || nombreArma.Contains("sin"))
            return enemyAnalizable.multiplicadorSeno;
        else if (nombreArma.Contains("coseno") || nombreArma.Contains("cos"))
            return enemyAnalizable.multiplicadorCoseno;
        else if (nombreArma.Contains("tangente") || nombreArma.Contains("tan"))
            return enemyAnalizable.multiplicadorTangente;

        return 1f;
    }

    private void OcultarTodo()
    {
        if (analysisUI != null && analysisUI.activeSelf)
            analysisUI.SetActive(false);

        if (triangleVisualizer != null)
            triangleVisualizer.OcultarTriangulo();

        if (angleSlider != null)
            angleSlider.OcultarSlider();

        if (feedbackText != null)
            feedbackText.text = "";
    }

    private IEnumerator DibujarRayo(Vector3 objetivo)
    {
        Debug.Log("⚡ Dibujando rayo...");

        GameObject rayoObj = new GameObject("Rayo");
        rayoObj.transform.position = transform.position;

        LineRenderer rayo = rayoObj.AddComponent<LineRenderer>();
        rayo.startColor = Color.cyan;
        rayo.endColor = Color.white;
        rayo.startWidth = 0.3f;
        rayo.endWidth = 0.1f;
        rayo.positionCount = 2;
        rayo.SetPosition(0, transform.position + new Vector3(0, 0.5f, 0));
        rayo.SetPosition(1, objetivo);
        rayo.material = new Material(Shader.Find("Sprites/Default"));

        float tiempo = rayoDuration;
        while (tiempo > 0)
        {
            tiempo -= Time.unscaledDeltaTime;
            rayo.startWidth = Random.Range(0.1f, 0.6f);
            yield return null;
        }

        Destroy(rayoObj);
    }

    private IEnumerator SalirDelModo()
    {
        yield return new WaitForSecondsRealtime(0.8f);
        if (timeStopManager != null)
            timeStopManager.TryTimeStop();
    }
}