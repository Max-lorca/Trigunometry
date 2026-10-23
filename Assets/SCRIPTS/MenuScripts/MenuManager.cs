using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [Header("Tiempos")]
    [SerializeField] private float impulseDelay = 5f;
    [SerializeField] private float zoomTime = 3f;
    [SerializeField] private float animationTime = 1.5f;

    [Header("Referencias")]
    [SerializeField] private CinemachineCamera cam;
    [SerializeField] private CinemachineImpulseSource blackHoleImpulse;
    [SerializeField] private ParticleSystemForceField forceField;
    [SerializeField] private CanvasGroup menuCanvas;
    [SerializeField] private Animator titleAnimator;

    private Animator _cameraAnimator;
    private bool _isPlaying;

    private void Start()
    {
        _cameraAnimator = cam.GetComponent<Animator>();

        forceField.endRange = 0f;
        cam.Lens.FieldOfView = 60f;
    }

    public void PlaySceneLoad(string sceneName)
    {
        if (_isPlaying) return;
        StartCoroutine(PlaySequence(sceneName));
    }

    private IEnumerator PlaySequence(string sceneName)
    {
        _isPlaying = true;
        menuCanvas.interactable = false;

        if (blackHoleImpulse != null)
            blackHoleImpulse.GenerateImpulse();
    
        if (titleAnimator != null)
            titleAnimator.SetTrigger("Play");

        yield return new WaitForSeconds(impulseDelay);

        // Zoom + force field + fade del menú
        float elapsed = 0f;
        while (elapsed < zoomTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / zoomTime);

            forceField.endRange = Mathf.Lerp(1f, 20f, t);
            cam.Lens.FieldOfView = Mathf.Lerp(60f, 35f, t);
            menuCanvas.alpha = 1f - t;

            yield return null;
        }

        cam.Lens.FieldOfView = 35f;
        forceField.endRange = 20f;
        menuCanvas.alpha = 0f;

        _cameraAnimator.SetTrigger("Start");
        yield return new WaitForSeconds(animationTime);

        // A partir de aquí manda la pantalla de carga
        GameManager.Instance.LoadScene(sceneName);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}