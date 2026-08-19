using UnityEngine;
using TMPro;
using System.Collections;

public class PowerShotSystem : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private TimeStopManager timeStopManager;
    [SerializeField] private SatoruTriangleVisualizer triangleVisualizer;

    [Header("UI")]
    [SerializeField] private GameObject analysisUI;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private TMP_Text weaponSelectedText;

    [Header("Configuración")]
    [SerializeField] private float rayoDuration = 0.5f;
    [SerializeField] private float shootCooldown = 1.5f;

    [Header("Daño al acertar")]
    // Antes era un daño fijo de 99999 (mataba de un golpe).
    // Ahora es configurable desde el Inspector para poder ajustarlo por balance.
    [SerializeField] private float danoAlAcertar = 40f;
    [SerializeField] private float multiplicadorDropGarantizado = 1f; // reservado por si luego quieres escalar el bonus

    private Transform currentTarget;
    private string armaSeleccionada = "";
    private int ladosMostrados = 0;
    private bool canShoot = true;
    private bool satoruJustActivated = false;

    void Start()
    {
        if (analysisUI != null) analysisUI.SetActive(false);
        if (weaponSelectedText != null) weaponSelectedText.text = "";
    }

    public bool IsSatoruActive()
    {
        return timeStopManager != null && timeStopManager.IsAnalysisActive;
    }

    void Update()
    {
        // ✅ FIX: antes se accedía a timeStopManager.IsAnalysisActive sin
        // comprobar null primero, y podía tirar NullReferenceException.
        if (timeStopManager == null) return;

        if (timeStopManager.IsAnalysisActive && !satoruJustActivated)
        {
            satoruJustActivated = true;
            if (triangleVisualizer != null)
            {
                triangleVisualizer.ResetLados();
                Debug.Log("Modo Satoru activado - ResetLados() llamado");
            }
        }

        if (!timeStopManager.IsAnalysisActive)
        {
            satoruJustActivated = false;
            if (analysisUI != null && analysisUI.activeSelf) analysisUI.SetActive(false);
            if (triangleVisualizer != null) triangleVisualizer.OcultarTriangulo();
            if (weaponSelectedText != null) weaponSelectedText.text = "";
            return;
        }

        if (timerText != null)
            timerText.text = $"{timeStopManager.TiempoRestante:F1}s";

        if (analysisUI != null && !analysisUI.activeSelf)
            analysisUI.SetActive(true);

        Collider2D[] enemigos = Physics2D.OverlapCircleAll(transform.position, 20f);
        Transform enemigoMasCercano = null;
        float minDist = float.MaxValue;

        foreach (Collider2D col in enemigos)
        {
            if (col.CompareTag("MeleeEnemy") || col.CompareTag("DistanceEnemy"))
            {
                float d = Vector2.Distance(transform.position, col.transform.position);
                if (d < minDist)
                {
                    minDist = d;
                    enemigoMasCercano = col.transform;
                }
            }
        }

        if (enemigoMasCercano != null)
        {
            currentTarget = enemigoMasCercano;

            // 🔽 Zoom automático al enemigo más cercano
            var camController = FindObjectOfType<CameraController>();
            if (camController != null)
                camController.ZoomCamTo(currentTarget);

            if (triangleVisualizer != null)
            {
                Vector2 jugadorPos = transform.position;
                Vector2 enemigoPos = currentTarget.position;
                ladosMostrados = triangleVisualizer.DibujarTriangulo(jugadorPos, enemigoPos);
            }
        }
        else
        {
            currentTarget = null;
            if (triangleVisualizer != null) triangleVisualizer.OcultarTriangulo();

            // 🔽 Reset de la cámara si no hay enemigos
            var camController = FindObjectOfType<CameraController>();
            if (camController != null)
                camController.ResetZoom();
        }

    }

    public void SeleccionarArma(string arma)
    {
        armaSeleccionada = arma;
        if (feedbackText != null)
            feedbackText.text = $"Arma: {arma}";

        if (weaponSelectedText != null)
            weaponSelectedText.text = arma;
    }

    public bool TryPowerShot()
    {
        if (!canShoot)
        {
            if (feedbackText != null) feedbackText.text = "Espera...";
            return false;
        }

        if (timeStopManager == null || !timeStopManager.IsAnalysisActive)
            return false;

        if (currentTarget == null)
        {
            if (feedbackText != null) feedbackText.text = "Sin objetivo";
            return false;
        }

        if (string.IsNullOrEmpty(armaSeleccionada))
        {
            if (feedbackText != null) feedbackText.text = "Selecciona arma (1,2,3)";
            return false;
        }

        string armaCorrecta = ObtenerArmaCorrecta();

        if (armaSeleccionada == armaCorrecta)
        {
            if (feedbackText != null) feedbackText.text = $"¡ACERTÓ! ({armaSeleccionada})";

            StartCoroutine(DibujarRayo(currentTarget.position));

            // ✅ FIX: en vez de acoplarse a MeleeEnemyController / DistanceEnemyController
            // por separado, usamos la interfaz IAnalizable que ya implementan ambos.
            // Esto además hace que el sistema funcione automáticamente con cualquier
            // enemigo nuevo que implemente IAnalizable, sin tener que tocar este script.
            if (currentTarget.TryGetComponent<IAnalizable>(out var analizable))
            {
                // Garantiza el drop de vida al 100% (esto es lo que antes se perdía,
                // porque TomarDaño() se llamaba directo sin pasar por OnAnalisisExitoso).
                analizable.OnAnalisisExitoso(multiplicadorDropGarantizado);
                analizable.RecibirDanoAnalisis(danoAlAcertar);
            }

            if (weaponSelectedText != null) weaponSelectedText.text = "";

            StartCoroutine(ResetShootCooldown());
            StartCoroutine(SalirDelModo());
            return true;
        }
        else
        {
            if (feedbackText != null) feedbackText.text = $"Falló. Debe ser: {armaCorrecta}";

            StartCoroutine(ResetShootCooldown());
            return false;
        }
    }

    private IEnumerator ResetShootCooldown()
    {
        canShoot = false;
        yield return new WaitForSecondsRealtime(shootCooldown);
        canShoot = true;
        if (feedbackText != null) feedbackText.text = "Listo para disparar";
    }

    private string ObtenerArmaCorrecta()
    {
        switch (ladosMostrados)
        {
            case 0: return "Tangente";
            case 1: return "Seno";
            case 2: return "Coseno";
            default: return "Seno";
        }
    }

    private IEnumerator DibujarRayo(Vector3 objetivo)
    {
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
        if (timeStopManager != null) timeStopManager.TryTimeStop();
    }
}