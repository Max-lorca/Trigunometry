using UnityEngine;
using System.Collections;

public class TimeStopManager : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Segundos que tarda en aparecer el efecto de entrada. El reloj del modo NO corre durante este tiempo (es el momento que tiene el jugador para pensar). 0 = el reloj empieza de inmediato.")]
    [SerializeField] private float fadeInTime = 3.3f;
    [Tooltip("Segundos que tarda en desaparecer el efecto al salir del modo.")]
    [SerializeField] private float fadeOutTime = 0.4f;
    [SerializeField] private float satoruDuration = 4f;

    [Header("UI y Efectos")]
    [SerializeField] private CanvasGroup canvasGroupEfect;
    [SerializeField] private ParticleSystem mathSymbolsParticle;

    [Header("Referencias")]
    [SerializeField] private SatoruChargeSystem chargeSystem;
    [SerializeField] private ShockWaveManager shockWaveManager;

    private Transform player;
    private bool isAnalysisActive = false;
    private bool isClosing = false;
    private bool disparoEnCurso = false;
    private float tiempoRestante = 0f;
    private Coroutine rutinaActivacion;

    public bool IsAnalysisActive => isAnalysisActive;
    // True desde el instante en que el modo empieza a cerrarse (tiempo agotado,
    // fallo, acierto o salida manual) hasta que termina de cerrarse.
    // Mientras sea true NO se debe aceptar ningún intento del jugador.
    public bool IsClosing => isClosing;
    public float TiempoRestante => tiempoRestante;
    public float DuracionTotal => satoruDuration;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (chargeSystem == null)
            chargeSystem = FindFirstObjectByType<SatoruChargeSystem>();
    }

    void Update()
    {
        if (mathSymbolsParticle != null && mathSymbolsParticle.isPlaying)
            mathSymbolsParticle.transform.position = player.position;
    }

    // Alterna el modo: si está activo lo cierra, si no, intenta activarlo
    public void TryTimeStop()
    {
        // Se ignora si ya se está cerrando o si hay un disparo en vuelo
        // (el modo debe terminar recién cuando el disparo impacta)
        if (isClosing || disparoEnCurso) return;

        if (isAnalysisActive)
        {
            CerrarModoAnalisis();
            return;
        }

        if (chargeSystem == null || !chargeSystem.IsReady)
        {
            Debug.Log("Modo Satoru no disponible. Carga insuficiente.");
            return;
        }

        rutinaActivacion = StartCoroutine(ActivarModoAnalisis());
    }

    // Marca que hay un disparo en vuelo: el reloj del modo se congela y no se puede
    // salir manualmente. Llamar a FinalizarDisparo() + CerrarModoAnalisis() al impactar.
    public void IniciarDisparo()
    {
        disparoEnCurso = true;
    }

    public void FinalizarDisparo()
    {
        disparoEnCurso = false;
    }

    // Solo CIERRA el modo (nunca lo activa). Úsalo cuando quieras salir con seguridad,
    // por ejemplo desde un retraso: si el modo ya se cerró por otro motivo, no hace nada.
    public void CerrarModoAnalisis()
    {
        if (!isAnalysisActive || isClosing) return;

        isClosing = true; // desde este instante se bloquean los intentos

        GetComponent<P_AnimationController>().SetOnAnalisisModeAnimation(false);

        if (rutinaActivacion != null)
        {
            StopCoroutine(rutinaActivacion);
            rutinaActivacion = null;
        }

        StartCoroutine(DesactivarModoAnalisis());
    }

    private IEnumerator ActivarModoAnalisis()
    {
        isAnalysisActive = true;
        disparoEnCurso = false;
        tiempoRestante = satoruDuration;
        Time.timeScale = 0f;

        GetComponent<P_AnimationController>().SetAnalisisModeAnimation();

        if (shockWaveManager != null)
            shockWaveManager.CallShockWave();

        if (chargeSystem != null)
            chargeSystem.ConsumeCharge();

        if (mathSymbolsParticle != null)
        {
            mathSymbolsParticle.transform.position = player.position;
            mathSymbolsParticle.Play();
        }

        GetComponent<P_AnimationController>().SetOnAnalisisModeAnimation(true);

        // Entrada del efecto: el reloj todavía no corre
        yield return StartCoroutine(Fade(1f, fadeInTime));

        tiempoRestante = satoruDuration;
        while (tiempoRestante > 0f)
        {
            // Con un disparo en vuelo el reloj se congela
            if (!disparoEnCurso)
                tiempoRestante -= Time.unscaledDeltaTime;
            yield return null;
        }

        tiempoRestante = 0f;

        // Se acabó el tiempo: el modo pasa a "cerrándose" en este mismo frame
        rutinaActivacion = null; // para no detenerse a sí misma en CerrarModoAnalisis
        CerrarModoAnalisis();
    }

    private IEnumerator DesactivarModoAnalisis()
    {
        if (shockWaveManager != null)
            shockWaveManager.StopShockWave();

        yield return StartCoroutine(Fade(0f, fadeOutTime));

        if (mathSymbolsParticle != null)
            mathSymbolsParticle.Stop();

        Time.timeScale = 1f;
        isAnalysisActive = false;
        isClosing = false;
        disparoEnCurso = false;
    }

    // Fundido lineal del CanvasGroup, en tiempo real (funciona con timeScale = 0)
    private IEnumerator Fade(float objetivo, float duracion)
    {
        float inicio = canvasGroupEfect != null ? canvasGroupEfect.alpha : 0f;
        float t = 0f;

        while (t < duracion)
        {
            t += Time.unscaledDeltaTime;
            if (canvasGroupEfect != null)
                canvasGroupEfect.alpha = Mathf.Lerp(inicio, objetivo, t / duracion);
            yield return null;
        }

        if (canvasGroupEfect != null)
            canvasGroupEfect.alpha = objetivo;
    }
}