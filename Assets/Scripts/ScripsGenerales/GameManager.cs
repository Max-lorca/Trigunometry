using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Carga de escenas")]
    [SerializeField] private float loadDelay = 1f; // espera con la pantalla cubierta

    [Header("Puntos")]
    [SerializeField] private float points = 0f;
    [SerializeField] private float larpPointsTime = 0.5f;
    [SerializeField] private TextMeshProUGUI pointsText;

    [HideInInspector] public bool isTimeStopped = false;
    [HideInInspector] public Transform Player;

    private AudioSource _musicSource;
    private GameObject _restartButton;
    private bool _isLoading;
    private float _targetPoints;
    private Coroutine _pointsRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
        ResetReferences(); // la primera escena no dispara sceneLoaded
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (_musicSource == null) return;

        _musicSource.pitch = isTimeStopped
            ? Mathf.Lerp(_musicSource.pitch, 0.5f, Time.unscaledDeltaTime * 1.5f)
            : 1f;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetReferences();
    }

    private void ResetReferences()
    {
        Player = GameObject.FindWithTag("Player")?.transform;

        GameObject music = GameObject.Find("BackgroundMusicManager");
        _musicSource = music != null ? music.GetComponent<AudioSource>() : null;

        // Buscamos el botón de reinicio en la nueva escena
        GameObject restartObj = GameObject.Find("RestartButton");
        if (restartObj != null)
        {
            _restartButton = restartObj;
            Button btnComponent = _restartButton.GetComponent<Button>();

            if (btnComponent != null)
            {
                // Limpiamos listeners anteriores para evitar acumulación
                btnComponent.onClick.RemoveAllListeners();

                // Asignamos la función ResetScene() de tu propio GameManager al botón
                btnComponent.onClick.AddListener(ResetScene);
            }
        }
        else
        {
            _restartButton = null;
        }

        // Si usas un texto de puntos en la UI que se regenera al cambiar de escena, 
        // puedes asegurarte de buscarlo o actualizarlo aquí si fuera necesario:
        // GameObject textObj = GameObject.Find("PointsText");
        // pointsText = textObj != null ? textObj.GetComponent<TextMeshProUGUI>() : null;
        // UpdatePointsText();
    }

    // ---------- Carga de escenas ----------

    public void LoadScene(string sceneName)
    {
        if (_isLoading) return;
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    public void ResetScene()
    {
        LoadScene(SceneManager.GetActiveScene().name);
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        _isLoading = true;
        Time.timeScale = 1f;

        // 1. Cubrir la pantalla
        if (PantallaCarga.Instance != null)
            yield return PantallaCarga.Instance.FadeTo(1f);

        // 2. Cargar en segundo plano y activar al estar listo
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
            yield return null;

        op.allowSceneActivation = true;
        yield return op; // OnSceneLoaded se dispara aquí

        // 3. Retraso con la pantalla aún cubierta
        yield return new WaitForSecondsRealtime(loadDelay);

        // 4. Desvanecer poco a poco
        if (PantallaCarga.Instance != null)
            yield return PantallaCarga.Instance.FadeTo(0f);

        _isLoading = false;
    }

    // ---------- Puntos ----------

    public void AddPoints(float amount)
    {
        _targetPoints += amount;

        if (_pointsRoutine != null) StopCoroutine(_pointsRoutine);
        _pointsRoutine = StartCoroutine(AnimatePoints());
    }

    public void ResetPoints()
    {
        if (_pointsRoutine != null) StopCoroutine(_pointsRoutine);

        points = 0f;
        _targetPoints = 0f;
        UpdatePointsText();
    }

    private IEnumerator AnimatePoints()
    {
        float start = points;
        float elapsed = 0f;

        while (elapsed < larpPointsTime)
        {
            elapsed += Time.unscaledDeltaTime;
            points = Mathf.Lerp(start, _targetPoints, elapsed / larpPointsTime);
            UpdatePointsText();
            yield return null;
        }

        points = _targetPoints;
        UpdatePointsText();
    }

    private void UpdatePointsText()
    {
        if (pointsText != null)
            pointsText.text = Mathf.RoundToInt(points).ToString();
    }
}