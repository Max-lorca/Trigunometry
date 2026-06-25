using UnityEngine;
using System.Collections;

public class TimeStopManager : MonoBehaviour
{
    [Header("UI y Efectos")]
    [SerializeField] private CanvasGroup canvasGroupEfect;
    [SerializeField] private ParticleSystem mathSymbolsParticle;
    [SerializeField] private float transitionTime;
    [SerializeField] private float effectTime;
    private Transform player;
    private bool isAnalysisActive = false;

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

        yield return new WaitForSecondsRealtime(effectTime);

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