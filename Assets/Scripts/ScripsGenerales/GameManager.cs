using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    //[Header("Referencias")]
    [Header("Background")]
    [SerializeField] private BackgroundController backgroundController;

    [SerializeField] public float backgroundHorizontalVelocity;
    [HideInInspector] public bool isTimeStopped = false;
    

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        ResetReferences();
    }
    public void ResetScene()
    {
        Debug.Log("Intentando reiniciar escena...");

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        ResetReferences();
    }
    private void ResetReferences()
    {
        backgroundController = GameObject.Find("Background").GetComponent<BackgroundController>();
        backgroundHorizontalVelocity = backgroundController.backgroundXVelocity;
    }
}