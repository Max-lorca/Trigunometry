using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private Vector3 _originalPos;
    private Coroutine _currentShakeCoroutine;
    
    // 🔥 El booleano para saber si se está moviendo o para detenerlo
    public bool isShaking { get; private set; }

    private void OnEnable()
    {
        _originalPos = transform.localPosition;
    }

    public void Shake(float duration, float magnitude)
    {
        // Si ya se estaba ejecutando un shake, lo detenemos primero para que no se encimen
        if (_currentShakeCoroutine != null)
        {
            StopCoroutine(_currentShakeCoroutine);
        }

        _currentShakeCoroutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    // 🔥 Nuevo método público para apagar la vibración a la fuerza desde otro script
    public void StopShake()
    {
        if (_currentShakeCoroutine != null)
        {
            StopCoroutine(_currentShakeCoroutine);
            isShaking = false;
            transform.localPosition = _originalPos; // Devolvemos la cámara a su sitio salvo
        }
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        isShaking = true; // Activamos el estado
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(x, y, _originalPos.z);

            elapsed += Time.deltaTime;
            yield return null; 
        }

        transform.localPosition = _originalPos;
        isShaking = false; // Desactivamos el estado al terminar de forma natural
    }
}