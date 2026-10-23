using UnityEngine;
using TMPro;
using System.Collections;
using Unity.VisualScripting;

public class ExpText : MonoBehaviour
{
    [SerializeField] private TextMeshPro texto;

    [SerializeField] private float TiempoDesaparicion;

    [SerializeField] private float floatTime = 1f;
    [SerializeField] private float distanceOffsetFloat = 1f;

    private bool imFloating = false;
    void Start()
    {
        Color c = texto.color;
        c.a = 0f;
        texto.color = c;
    }
    void Update()
    {
        if(!imFloating)
            StartCoroutine(Float());
    }
    private IEnumerator ShowText(TextMeshPro t, float targetAlpha)
    {
        Color actualColor = t.color;
        float alpha = actualColor.a;

        float elapsed = 0f;

        while (elapsed < TiempoDesaparicion)
        {
            elapsed += Time.deltaTime;

            actualColor.a = Mathf.Lerp(
                alpha,
                targetAlpha,
                elapsed / TiempoDesaparicion
            );

            t.color = actualColor;

            yield return null;
        }

        actualColor.a = targetAlpha;
        t.color = actualColor;
    }
    private IEnumerator Float()
    {
        imFloating = true;
        float elapsed = 0f;
        Vector2 originalPosition = transform.position;

        Vector2 targetPosition = originalPosition + Vector2.up * distanceOffsetFloat;

        while(elapsed < floatTime)
        {
            elapsed += Time.deltaTime;

            transform.position = Vector2.Lerp(originalPosition, targetPosition, elapsed / floatTime);
            yield return null;
        }
        transform.position = targetPosition;
        elapsed = 0f;

        while(elapsed < floatTime)
        {
            elapsed += Time.deltaTime;

            transform.position = Vector2.Lerp(targetPosition, originalPosition, elapsed / floatTime);
            yield return null;
        }
        imFloating = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.tag == "Player")
        {
            StartCoroutine(ShowText(texto, 1f));
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.gameObject.tag == "Player")
        {
            StartCoroutine(ShowText(texto, 0f));
        }
    }
}
