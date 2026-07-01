using UnityEngine;
using System.Collections;

public class TimeStopManager : MonoBehaviour
{
    [Header("Configuración modo Analisis")]
    [SerializeField] private float transitionTime;
    [SerializeField] private float effectTime; // Ya no se usa, lo dejamos por si acaso

    [Header("UI y Efectos")]
    [SerializeField] private CanvasGroup canvasGroupEfect;
    [SerializeField] private ParticleSystem mathSymbolsParticle;

    private Transform player;
    private bool isAnalysisActive = false;
    private bool estaEnSeleccion = false;

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
            StartCoroutine(DesactivarModoAnalisis());
        }
    }

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

        // ✅ ELIMINADO: El timer ya no existe
        // El modo dura hasta que el jugador dispare o cancele manualmente

        // Esperar hasta que se desactive manualmente (desde TryPowerShot o TryTimeStop)
        while (isAnalysisActive)
        {
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