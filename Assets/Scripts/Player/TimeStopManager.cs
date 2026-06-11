using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

public class TimeStopManager : MonoBehaviour
{
    [Header("UI y Efectos")]
    [SerializeField] private CanvasGroup canvasGroupEfect;
    [SerializeField] private float efectTime = 5f;
    [SerializeField] private float transitionTime = 4f; // Qué tan rápido aparece/desaparece el canvas
    //[SerializeField] private GameObject volumenPostProcesadoAzul;

    [Header("Configuración")]
    [SerializeField] private float timeStop = 5f;
    private bool isTimeStop = false;

    public void TryTimeStop()
    {
        if (!isTimeStop)
        {
            StartCoroutine(ActivarModoAnalisis());
        }
    }
    private IEnumerator ActivarModoAnalisis()
    {
        isTimeStop = true;
        Time.timeScale = 0f;

        while(canvasGroupEfect.alpha < 1f)
        {
            canvasGroupEfect.alpha += Time.unscaledDeltaTime*transitionTime;
            yield return null;
        }
        canvasGroupEfect.alpha = 1f;

        yield return new WaitForSecondsRealtime(efectTime);

        while(canvasGroupEfect.alpha > 0f)
        {
            canvasGroupEfect.alpha -= Time.unscaledDeltaTime*transitionTime;
            yield return null;
        }
        canvasGroupEfect.alpha = 0f;

        Time.timeScale = 1f;
        
        isTimeStop = false;
    }


}
