using UnityEngine;
using System.Collections;

public class TimeStopManager : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float transitionTime = 0.3f;
    [SerializeField] private float satoruDuration = 4f;

    [Header("UI y Efectos")]
    [SerializeField] private CanvasGroup canvasGroupEfect;
    [SerializeField] private ParticleSystem mathSymbolsParticle;

    [Header("Referencias")]
    [SerializeField] private SatoruChargeSystem chargeSystem;

    private Transform player;
    private bool isAnalysisActive = false;
    private float tiempoRestante = 0f;

    public bool IsAnalysisActive => isAnalysisActive;
    public float TiempoRestante => tiempoRestante;

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

    public void TryTimeStop()
    {
        if (isAnalysisActive)
        {
            StartCoroutine(DesactivarModoAnalisis());
            return;
        }

        if (chargeSystem == null || !chargeSystem.IsReady)
        {
            Debug.Log("Modo Satoru no disponible. Carga insuficiente.");
            return;
        }

        StartCoroutine(ActivarModoAnalisis());
    }

    private IEnumerator ActivarModoAnalisis()
    {
        isAnalysisActive = true;
        Time.timeScale = 0f;

        if (chargeSystem != null)
            chargeSystem.ConsumeCharge();

        if (mathSymbolsParticle != null)
        {
            mathSymbolsParticle.transform.position = player.position;
            mathSymbolsParticle.Play();
        }

        if (canvasGroupEfect != null)
        {
            while (canvasGroupEfect.alpha < 1f)
            {
                canvasGroupEfect.alpha += Time.unscaledDeltaTime * transitionTime;
                yield return null;
            }
            canvasGroupEfect.alpha = 1f;
        }

        tiempoRestante = satoruDuration;
        while (tiempoRestante > 0f && isAnalysisActive)
        {
            tiempoRestante -= Time.unscaledDeltaTime;
            yield return null;
        }

        if (isAnalysisActive)
            yield return StartCoroutine(DesactivarModoAnalisis());
    }

    private IEnumerator DesactivarModoAnalisis()
    {
        if (canvasGroupEfect != null)
        {
            while (canvasGroupEfect.alpha > 0f)
            {
                canvasGroupEfect.alpha -= Time.unscaledDeltaTime * transitionTime;
                yield return null;
            }
            canvasGroupEfect.alpha = 0f;
        }

        if (mathSymbolsParticle != null)
            mathSymbolsParticle.Stop();

        Time.timeScale = 1f;
        isAnalysisActive = false;
    }
}