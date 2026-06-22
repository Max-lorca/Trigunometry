using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;

public class TimeStopManager : MonoBehaviour
{
    [Header("UI y Efectos")]
    [SerializeField] private CanvasGroup canvasGroupEfect;
    [SerializeField] private CanvasGroup menuCanvasGroup;
    [SerializeField] private ParticleSystem mathSymbolsParticle;
    [System.Serializable]
    public class TimeStopConfig
    {
        public float effectTime;
        public float transitionTime;
    }
    [SerializeField] private TimeStopConfig analisis;
    [SerializeField] private TimeStopConfig menu;
    private const string MODO_MENU = "menu";
    private const string MODO_ANALISIS = "analisis";
    private Transform player;
    
    private bool menuActivo = false;
    private bool isAnalysisActive = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        var main = mathSymbolsParticle.main;
        main.useUnscaledTime = true;
    }
    void Update()
    {
        Transform particlePosition = mathSymbolsParticle.GetComponent<Transform>();
        particlePosition.position = player.position;
    }
    public void TryTimeStop(string nombreModo)
    {
        switch (nombreModo)
        {
            case MODO_MENU:
                StartCoroutine(ActivarMenu());
            break;

            case MODO_ANALISIS:
                if(!isAnalysisActive) StartCoroutine(ActivarModoAnalisis());
            break;

            default:
                Debug.LogWarning("Modo de TimeStop desconocido:" + nombreModo);
            break;
        }
    }
    private IEnumerator ActivarModoAnalisis()
    {
        isAnalysisActive = true;

        GameManager.Instance.OnHighLights(true);

        Time.timeScale = 0f;

        mathSymbolsParticle.transform.position = player.position;
        mathSymbolsParticle.Play();

        while(canvasGroupEfect.alpha < 1f)
        {
            canvasGroupEfect.alpha += Time.unscaledDeltaTime * analisis.transitionTime;
            yield return null;
        }

        canvasGroupEfect.alpha = 1f;

        yield return new WaitForSecondsRealtime(analisis.effectTime);

        while(canvasGroupEfect.alpha > 0f)
        {
            canvasGroupEfect.alpha -= Time.unscaledDeltaTime * analisis.transitionTime;
            yield return null;
        }

        canvasGroupEfect.alpha = 0f;

        mathSymbolsParticle.Stop();

        GameManager.Instance.OnHighLights(false);

        Time.timeScale = 1f;
        isAnalysisActive = false;
    }
    private IEnumerator ActivarMenu()
    {
        menuActivo = !menuActivo;


        if(menuActivo)
        {
            Time.timeScale = 0f;

            menuCanvasGroup.interactable = true;
            menuCanvasGroup.blocksRaycasts = true;


            while(menuCanvasGroup.alpha < 1f)
            {
                menuCanvasGroup.alpha += Time.unscaledDeltaTime * menu.transitionTime;
                yield return null;
            }

            menuCanvasGroup.alpha = 1f;
        }
        else
        {

            while(menuCanvasGroup.alpha > 0f)
            {
                menuCanvasGroup.alpha -= Time.unscaledDeltaTime * menu.transitionTime;
                yield return null;
            }

            menuCanvasGroup.alpha = 0f;

            menuCanvasGroup.interactable = false;
            menuCanvasGroup.blocksRaycasts = false;

            Time.timeScale = 1f;
        }
    }
}
