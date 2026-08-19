using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [HideInInspector] public bool isTimeStopped = false;

    [HideInInspector] public Transform Player;
    private GameObject _backGroundMusic;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Nos suscribimos al evento de carga de escenas de Unity
        SceneManager.sceneLoaded += OnSceneLoaded;
        ResetReferences();
    }
    private void Update()
    {
        if(_backGroundMusic != null && isTimeStopped)
        {
            float pitch = _backGroundMusic.GetComponent<AudioSource>().pitch;
            pitch = Mathf.Lerp(pitch, 0.5f, Time.unscaledDeltaTime * 1.5f);
            _backGroundMusic.GetComponent<AudioSource>().pitch = pitch;
        }
        else
        {
            _backGroundMusic.GetComponent<AudioSource>().pitch = 1f;
        }
    }
    private void OnDestroy()
    {
        // Siempre desvincularse de los eventos al destruir el objeto para evitar fugas de memoria
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetReferences();
    }

    public void ResetScene()
    {
        Debug.Log("Reiniciando escena de forma segura...");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        // Quitamos ResetReferences() de aquí, ya que OnSceneLoaded se encargará automáticamente
    }

    private void ResetReferences()
    {
        Player = GameObject.FindWithTag("Player").transform;
        _backGroundMusic = GameObject.Find("BackgroundMusicManager");
    }
}