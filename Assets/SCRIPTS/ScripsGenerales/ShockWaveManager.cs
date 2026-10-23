using System.Collections;
using UnityEngine;

public class ShockWaveManager : MonoBehaviour
{
    [SerializeField] private float shockWaveDuration = 2f;
    [SerializeField] private float alphaWave = 0.3f;

    private Coroutine _shockWaveCorutine;
    private SpriteRenderer _spriteRenderer;
    private Material _material;
    private Transform _player;
    private FadeController _fadeController;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _material = _spriteRenderer.material;
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _fadeController = GetComponent<FadeController>();
        StartCoroutine(_fadeController.Desvanecimiento(_spriteRenderer, 0, 0));
    }
    private void Update()
    {
        this.transform.position = _player.position;
    }
    public void CallShockWave()
    {
        if (_shockWaveCorutine == null)
        {
            _material.SetFloat("_Alpha", alphaWave);   // restaura la opacidad de la onda
            StartCoroutine(_fadeController.Desvanecimiento(_spriteRenderer, 0.1f, 0.67f));
            _shockWaveCorutine = StartCoroutine(ShockWaveAction(-0.1f, 5f));
        }
    }
    public void StopShockWave()
    {
        if (_shockWaveCorutine != null)
        {
            StopCoroutine(_shockWaveCorutine);
            _shockWaveCorutine = null;
            StartCoroutine(_fadeController.Desvanecimiento(_spriteRenderer, 0, 0.67f));
            _material.SetFloat("_Alpha", 0f);
        }
    }
    private IEnumerator ShockWaveAction(float startPos, float endPos)
    {
        _material.SetFloat("_WaveDistanceFromCenter", startPos);
        float elapsedTime = 0f;
        while(elapsedTime < shockWaveDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float currentPos = Mathf.Lerp(startPos, endPos, elapsedTime / shockWaveDuration);
            _material.SetFloat("_WaveDistanceFromCenter", currentPos);
            yield return null;
        }
        _material.SetFloat("_WaveDistanceFromCenter", endPos);
    }
}
