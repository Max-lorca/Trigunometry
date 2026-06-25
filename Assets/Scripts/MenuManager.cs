using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float transitionTime = 0.5f;

    [Header("Animators")]
    [SerializeField] private List<Animator> textAnimators = new();
    [SerializeField] private List<Animator> buttonAnimators = new();

    private CanvasGroup menuCanvasGroup;

    public bool menuActivo { get; private set; }

    private Coroutine currentTransition;

    private void Start()
    {
        menuCanvasGroup = GetComponent<CanvasGroup>();

        menuCanvasGroup.alpha = 0f;
        menuCanvasGroup.interactable = false;
        menuCanvasGroup.blocksRaycasts = false;
    }

    public void ToggleMenu()
    {
        if (currentTransition != null)
            StopCoroutine(currentTransition);

        currentTransition = StartCoroutine(MenuRoutine());
    }

    private IEnumerator MenuRoutine()
    {
        menuActivo = !menuActivo;

        if (menuActivo)
        {
            // Pausar juego
            Time.timeScale = 0f;

            // Activar interacción
            menuCanvasGroup.interactable = true;
            menuCanvasGroup.blocksRaycasts = true;

            // Animaciones de entrada
            foreach (Animator anim in textAnimators)
            {
                anim.ResetTrigger("exit");
                anim.SetTrigger("enter");
            }

            foreach (Animator anim in buttonAnimators)
            {
                anim.ResetTrigger("exit");
                anim.SetTrigger("enter");
            }

            // Fade In
            while (menuCanvasGroup.alpha < 1f)
            {
                menuCanvasGroup.alpha += Time.unscaledDeltaTime / transitionTime;
                yield return null;
            }

            menuCanvasGroup.alpha = 1f;
        }
        else
        {
            // Animaciones de salida
            foreach (Animator anim in textAnimators)
            {
                anim.ResetTrigger("enter");
                anim.SetTrigger("exit");
            }

            foreach (Animator anim in buttonAnimators)
            {
                anim.ResetTrigger("enter");
                anim.SetTrigger("exit");
            }

            // Fade Out
            while (menuCanvasGroup.alpha > 0f)
            {
                menuCanvasGroup.alpha -= Time.unscaledDeltaTime / transitionTime;
                yield return null;
            }

            menuCanvasGroup.alpha = 0f;

            menuCanvasGroup.interactable = false;
            menuCanvasGroup.blocksRaycasts = false;

            // Reanudar juego
            Time.timeScale = 1f;
        }

        currentTransition = null;
    }
}