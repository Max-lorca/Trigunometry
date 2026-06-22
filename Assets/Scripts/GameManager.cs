using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [HideInInspector] 
    public List<GameObject> HighLights;


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
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshHighlights();
        OnHighLights(false);
    }


    public void RefreshHighlights()
    {
        HighLights = GameObject.FindGameObjectsWithTag("HighLight").ToList();
    }


    public void OnHighLights(bool active)
    {
        foreach(GameObject highLight in HighLights)
        {
            if(highLight != null)
                highLight.SetActive(active);
        }
    }


    public void ResetScene()
    {
        Debug.Log("Intentando reiniciar escena...");

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}