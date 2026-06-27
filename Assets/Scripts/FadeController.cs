using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
public class FadeController : MonoBehaviour
{
    public IEnumerator Desvanecimiento(SpriteRenderer renderer, float alphaFinal, float duracion)
    {
        Color c = renderer.color;
        float alphaInicial = c.a;

        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            c.a = Mathf.Lerp(alphaInicial, alphaFinal, tiempo / duracion);
            renderer.color = c;

            yield return null;
        }

        c.a = alphaFinal;
        renderer.color = c;
    }
    public IEnumerator DesvanecimientoTexto(TextMeshPro texto, float alphaFinal, float duracion)
    {
        Color c = texto.color;
        float alphaInicial = c.a;

        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            c.a = Mathf.Lerp(alphaInicial, alphaFinal, tiempo / duracion);
            texto.color = c;

            yield return null;            
        }

        c.a = alphaFinal;
        texto.color = c;        
    }
    
}
