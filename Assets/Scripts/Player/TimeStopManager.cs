using UnityEngine;
using System.Collections;

public class TimeStopManager : MonoBehaviour
{
    [Header("Configuración modo Analisis")]
    [SerializeField] private float transitionTime;
    [SerializeField] private float effectTime;

    [Header("UI y Efectos")]
    [SerializeField] private CanvasGroup canvasGroupEfect;
    [SerializeField] private ParticleSystem mathSymbolsParticle;

    private Transform player;
    private bool isAnalysisActive = false;

    // Mientras esto sea true, la cuenta regresiva de "effectTime" se pausa.
    // Lo controla AnalysisModeController cuando hay un enemigo seleccionado.
    private bool estaEnSeleccion = false;

    /// <summary>Permite a otros scripts (como AnalysisModeController) saber si el modo está activo.</summary>
    public bool IsAnalysisActive => isAnalysisActive;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        var main = mathSymbolsParticle.main;
        main.useUnscaledTime = true;
        canvasGroupEfect.alpha = 0f;
    }

    private void Update()
    {
        if (mathSymbolsParticle.isPlaying)
        {
            mathSymbolsParticle.transform.position = player.position;
        }
    }

    public void TryTimeStop()
    {
        if (!isAnalysisActive)
        {
            StartCoroutine(ActivarModoAnalisis());
        }
        else
        {
            // Permite cancelar el modo análisis manualmente con el mismo input.
            StartCoroutine(DesactivarModoAnalisis());
        }
    }

    /// <summary>Llamado por AnalysisModeController al seleccionar/deseleccionar un enemigo.</summary>
    public void SetEnSeleccion(bool valor)
    {
        estaEnSeleccion = valor;
    }

    private IEnumerator ActivarModoAnalisis()
    {
        isAnalysisActive = true;
        GameManager.Instance.isTimeStopped = true;
        Time.timeScale = 0f;

        mathSymbolsParticle.transform.position = player.position;
        mathSymbolsParticle.Play();

        while (canvasGroupEfect.alpha < 1f)
        {
            canvasGroupEfect.alpha += Time.unscaledDeltaTime * transitionTime;
            yield return null;
        }
        canvasGroupEfect.alpha = 1f;

        // En lugar de un WaitForSecondsRealtime fijo, el tiempo se descuenta
        // solo cuando NO hay un enemigo seleccionado/resolviéndose.
        float tiempoRestante = effectTime;
        while (tiempoRestante > 0f)
        {
            if (!estaEnSeleccion)
            {
                tiempoRestante -= Time.unscaledDeltaTime;
            }
            yield return null;
        }

        yield return StartCoroutine(DesactivarModoAnalisis());
    }

    private IEnumerator DesactivarModoAnalisis()
    {
        while (canvasGroupEfect.alpha > 0f)
        {
            canvasGroupEfect.alpha -= Time.unscaledDeltaTime * transitionTime;
            yield return null;
        }
        canvasGroupEfect.alpha = 0f;
        mathSymbolsParticle.Stop();
        GameManager.Instance.isTimeStopped = false;
        Time.timeScale = 1f;
        isAnalysisActive = false;
    }
}