using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class PantallaCarga : MonoBehaviour
{
    public static PantallaCarga Instance { get; private set; }

    [SerializeField] private float fadeDuration = 1f;

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
    }

    public IEnumerator FadeTo(float targetAlpha)
    {
        _canvasGroup.blocksRaycasts = true; // bloquea clics mientras se ve

        float startAlpha = _canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / fadeDuration);
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        _canvasGroup.alpha = targetAlpha;
        _canvasGroup.blocksRaycasts = targetAlpha > 0f;
    }
}