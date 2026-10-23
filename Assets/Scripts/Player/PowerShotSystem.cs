using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PowerShotSystem : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private TimeStopManager timeStopManager;
    [SerializeField] private SatoruTriangleVisualizer triangleVisualizer;
    [SerializeField] private Transform triangleBase;

    [Header("UI")]
    [SerializeField] private GameObject analysisUI;
    [SerializeField] private TMP_Text timerText;
    [Tooltip("Image con Image Type = Filled. Se vacía a medida que se acaba el tiempo del modo análisis. Ponla como hijo de Analysis UI para que solo se vea en ese modo.")]
    [SerializeField] private Image imageFill;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private TMP_Text weaponSelectedText;

    [Header("Configuración")]
    [SerializeField] private float shootCooldown = 1.5f;
    [Tooltip("Segundos que se mantiene la pantalla congelada DESPUÉS del impacto, antes de terminar el modo.")]
    [SerializeField] private float retrasoSalidaAcierto = 0.8f;
    [Tooltip("Segundos que se espera tras FALLAR antes de cerrar el modo (deja leer cuál era el arma correcta).")]
    [SerializeField] private float retrasoSalidaFallo = 1.2f;

    [Header("Efecto de disparo")]
    [Tooltip("Prefab de la carga: aparece al entrar al modo análisis y se queda cargando. Si no hay Shot Prefab, esta misma carga sale disparada hacia el enemigo.")]
    [SerializeField] private GameObject chargePrefab;
    [Tooltip("Opcional: prefab del proyectil. Si está vacío, se lanza la propia carga.")]
    [SerializeField] private GameObject shotPrefab;
    [Tooltip("Opcional: efecto que aparece en el enemigo al impactar.")]
    [SerializeField] private GameObject impactPrefab;
    [Tooltip("Dónde aparece la carga. Si está vacío se usa Triangle Base.")]
    [SerializeField] private Transform chargePoint;
    [Tooltip("Velocidad del proyectil en unidades por segundo.")]
    [SerializeField] private float shotSpeed = 25f;
    [Tooltip("Pausa corta entre acertar y que salga el disparo (la carga 'se suelta').")]
    [SerializeField] private float retrasoAntesDeDisparar = 0.2f;
    [Tooltip("La cámara sigue al proyectil mientras vuela.")]
    [SerializeField] private bool seguirProyectilConCamara = true;
    [Tooltip("Al terminar el modo, llama a ResetZoom() de la cámara.")]
    [SerializeField] private bool resetearZoomAlTerminar = true;

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
    private GameObject _cargaInstancia;

    // Se activa en cuanto el jugador resuelve su intento (acierta O falla).
    // Efectos: el objetivo queda fijo (si no, al morir el enemigo se apaga su collider
    // y el sistema saltaría al siguiente más cercano) y no se aceptan más intentos
    // mientras el modo se cierra. Se libera cuando el modo termina.
    private bool intentoUsado = false;

    void Start()
    {
        if (analysisUI != null) analysisUI.SetActive(false);
        if (weaponSelectedText != null) weaponSelectedText.text = "";
    }

    public bool IsSatoruActive()
    {
        return timeStopManager != null && timeStopManager.IsAnalysisActive;
    }

    // True si ya no se debe aceptar ninguna acción del jugador sobre el análisis
    private bool InputBloqueado()
    {
        return intentoUsado || timeStopManager == null || timeStopManager.IsClosing;
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

            IniciarCarga();
        }

        // El modo se está cerrando (tiempo agotado, fallo, acierto o salida manual):
        // se oculta todo de inmediato y no se acepta nada más.
        if (timeStopManager.IsClosing)
        {
            DetenerCarga();
            if (analysisUI != null && analysisUI.activeSelf) analysisUI.SetActive(false);
            if (triangleVisualizer != null) triangleVisualizer.OcultarTriangulo();
            if (weaponSelectedText != null) weaponSelectedText.text = "";
            return;
        }

        if (!timeStopManager.IsAnalysisActive)
        {
            satoruJustActivated = false;
            intentoUsado = false; // al salir del modo, se libera
            DetenerCarga();
            if (analysisUI != null && analysisUI.activeSelf) analysisUI.SetActive(false);
            if (triangleVisualizer != null) triangleVisualizer.OcultarTriangulo();
            if (weaponSelectedText != null) weaponSelectedText.text = "";
            return;
        }

        if (timerText != null)
            timerText.text = $"{timeStopManager.TiempoRestante:F1}s";

        ActualizarContador();

        if (analysisUI != null && !analysisUI.activeSelf)
            analysisUI.SetActive(true);

        // Tras resolver el intento, no se busca otro objetivo
        if (intentoUsado)
        {
            // Si el objetivo ya fue destruido, se oculta el triángulo
            if (currentTarget == null && triangleVisualizer != null)
                triangleVisualizer.OcultarTriangulo();
            return;
        }

        Collider2D[] enemigos = Physics2D.OverlapCircleAll(transform.position, 20f);
        Transform enemigoMasCercano = null;
        float minDist = float.MaxValue;

        foreach (Collider2D col in enemigos)
        {
            if (col.CompareTag("Enemy") || col.CompareTag("DistanceEnemy"))
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
            var camController = FindAnyObjectByType<CameraController>();
            if (camController != null)
                camController.ZoomCamTo(currentTarget);

            if (triangleVisualizer != null)
            {
                Vector2 enemigoPos = currentTarget.position;
                ladosMostrados = triangleVisualizer.DibujarTriangulo(triangleBase.position, enemigoPos);
            }
        }
        else
        {
            currentTarget = null;
            if (triangleVisualizer != null) triangleVisualizer.OcultarTriangulo();

            // 🔽 Reset de la cámara si no hay enemigos
            var camController = FindAnyObjectByType<CameraController>();
            if (camController != null)
                camController.ResetZoom();
        }

    }

    // Barra/círculo con el tiempo que le queda al modo análisis:
    // lleno al empezar, vacío cuando se acaba el tiempo.
    private void ActualizarContador()
    {
        if (imageFill == null) return;

        float total = timeStopManager.DuracionTotal;
        imageFill.fillAmount = total > 0f
            ? Mathf.Clamp01(timeStopManager.TiempoRestante / total)
            : 0f;
    }

    public void SeleccionarArma(string arma)
    {
        if (InputBloqueado()) return;

        armaSeleccionada = arma;
        if (feedbackText != null)
            feedbackText.text = $"Arma: {arma}";

        if (weaponSelectedText != null)
            weaponSelectedText.text = arma;
    }

    public bool TryPowerShot()
    {
        // Intento ya resuelto, o el modo se está cerrando (p. ej. se acabó el tiempo)
        if (InputBloqueado()) return false;

        if (!canShoot)
        {
            if (feedbackText != null) feedbackText.text = "Espera...";
            return false;
        }

        if (!timeStopManager.IsAnalysisActive)
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

        // Acierte o falle, el intento se gasta
        intentoUsado = true;

        if (armaSeleccionada == armaCorrecta)
        {
            if (feedbackText != null) feedbackText.text = $"¡ACERTÓ! ({armaSeleccionada})";
            if (weaponSelectedText != null) weaponSelectedText.text = "";

            // Congela el reloj del modo y bloquea la salida manual hasta que impacte.
            // El daño, el drop y el cierre del modo ocurren AL FINAL de DispararCoroutine.
            timeStopManager.IniciarDisparo();
            StartCoroutine(DispararCoroutine(currentTarget));

            StartCoroutine(ResetShootCooldown());
            return true;
        }
        else
        {
            if (feedbackText != null) feedbackText.text = $"Falló. Debe ser: {armaCorrecta}";

            StartCoroutine(ResetShootCooldown());
            StartCoroutine(SalirDelModoTrasFallo(retrasoSalidaFallo));
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

    // ───────────────────────── Carga y disparo ─────────────────────────

    private Transform PuntoDeCarga()
    {
        if (chargePoint != null) return chargePoint;
        if (triangleBase != null) return triangleBase;
        return transform;
    }

    // Aparece al entrar al modo análisis y se queda "cargando" hasta que se dispara o se sale
    private void IniciarCarga()
    {
        DetenerCarga();
        if (chargePrefab == null) return;

        _cargaInstancia = Instantiate(chargePrefab, PuntoDeCarga().position, Quaternion.identity);
        UsarTiempoReal(_cargaInstancia);
    }

    private void DetenerCarga()
    {
        if (_cargaInstancia == null) return;

        Destroy(_cargaInstancia);
        _cargaInstancia = null;
    }

    // El modo análisis detiene el tiempo (timeScale = 0), así que las partículas y
    // animaciones del efecto deben usar tiempo real o se quedarían congeladas.
    private void UsarTiempoReal(GameObject go)
    {
        foreach (ParticleSystem ps in go.GetComponentsInChildren<ParticleSystem>(true))
        {
            var main = ps.main;
            main.useUnscaledTime = true;
        }

        foreach (Animator anim in go.GetComponentsInChildren<Animator>(true))
            anim.updateMode = AnimatorUpdateMode.UnscaledTime;
    }

    // Secuencia completa del disparo acertado:
    // carga -> pausa -> vuelo hasta el enemigo -> impacto + daño -> pausa -> fin del modo
    private IEnumerator DispararCoroutine(Transform objetivo)
    {
        Vector3 origen = PuntoDeCarga().position;
        Vector3 ultimaPos = objetivo != null ? objetivo.position : origen;
        CameraController cam = FindAnyObjectByType<CameraController>();

        // 1) El proyectil: el Shot Prefab si existe; si no, la propia carga sale disparada
        GameObject proyectil = null;
        if (shotPrefab != null)
        {
            DetenerCarga();
            proyectil = Instantiate(shotPrefab, origen, Quaternion.identity);
            UsarTiempoReal(proyectil);
        }
        else if (_cargaInstancia != null)
        {
            proyectil = _cargaInstancia;
            _cargaInstancia = null; // ya no es "la carga", es el proyectil
        }

        // 2) La carga "se suelta"
        if (retrasoAntesDeDisparar > 0f)
            yield return new WaitForSecondsRealtime(retrasoAntesDeDisparar);

        // 3) Vuelo (en tiempo real: el juego sigue con timeScale = 0)
        float distancia = Vector3.Distance(origen, ultimaPos);
        float duracion = Mathf.Max(0.2f, distancia / Mathf.Max(0.01f, shotSpeed));
        float t = 0f;

        while (t < duracion)
        {
            t += Time.unscaledDeltaTime;
            if (objetivo != null) ultimaPos = objetivo.position;

            float k = Mathf.Clamp01(t / duracion);
            k *= k; // acelera hacia el final

            if (proyectil != null)
            {
                Vector3 anterior = proyectil.transform.position;
                Vector3 nueva = Vector3.Lerp(origen, ultimaPos, k);
                proyectil.transform.position = nueva;

                // Que el proyectil apunte hacia donde va
                Vector3 dir = nueva - anterior;
                if (dir.sqrMagnitude > 0.00001f)
                    proyectil.transform.right = dir.normalized;

                if (seguirProyectilConCamara && cam != null)
                    cam.ZoomCamTo(proyectil.transform);
            }

            yield return null;
        }

        // 4) Impacto
        if (proyectil != null) Destroy(proyectil);

        if (impactPrefab != null)
        {
            GameObject fx = Instantiate(impactPrefab, ultimaPos, Quaternion.identity);
            UsarTiempoReal(fx);
            Destroy(fx, 3f);
        }

        // ✅ En vez de acoplarse a cada tipo de enemigo, se usa la interfaz IAnalizable
        // (la implementa EnemyBase), así funciona con cualquier enemigo nuevo.
        if (objetivo != null && objetivo.TryGetComponent<IAnalizable>(out var analizable))
        {
            // Garantiza el drop de vida al 100%
            analizable.OnAnalisisExitoso(multiplicadorDropGarantizado);
            // El origen del golpe es donde salió el disparo (lado del knockback)
            analizable.RecibirDanoAnalisis(danoAlAcertar, origen);
        }

        if (seguirProyectilConCamara && cam != null && objetivo != null)
            cam.ZoomCamTo(objetivo);

        // 5) Un momento para ver el impacto, y recién entonces termina el modo
        yield return new WaitForSecondsRealtime(retrasoSalidaAcierto);

        timeStopManager.FinalizarDisparo();
        timeStopManager.CerrarModoAnalisis();

        if (resetearZoomAlTerminar && cam != null)
            cam.ResetZoom();
    }

    private IEnumerator SalirDelModoTrasFallo(float retraso)
    {
        yield return new WaitForSecondsRealtime(retraso);

        // CerrarModoAnalisis solo cierra: si el modo ya se cerró por otro motivo
        // (p. ej. se acabó el tiempo), no hace nada. Antes se usaba TryTimeStop,
        // que alterna y podía ABRIR un modo nuevo en ese caso.
        if (timeStopManager != null) timeStopManager.CerrarModoAnalisis();
    }
}