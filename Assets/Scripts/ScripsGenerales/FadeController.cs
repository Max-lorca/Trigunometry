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
    public IEnumerator Desvanecimiento(List<SpriteRenderer> renderers, float alphaFinal, float duracion)
    {
        List<Color> coloresIniciales = new List<Color>();
        foreach (var renderer in renderers)
        {
            coloresIniciales.Add(renderer.color);
        }
        float tiempo = 0f;
        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            for (int i = 0; i < renderers.Count; i++)
            {
                Color c = coloresIniciales[i];
                c.a = Mathf.Lerp(c.a, alphaFinal, tiempo / duracion);
                renderers[i].color = c;
            }
            yield return null;
        }
        for (int i = 0; i < renderers.Count; i++)
        {
            Color c = coloresIniciales[i];
            c.a = alphaFinal;
            renderers[i].color = c;
        }
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
