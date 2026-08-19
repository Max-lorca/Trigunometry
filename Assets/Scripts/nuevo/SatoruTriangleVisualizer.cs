using UnityEngine;
using TMPro;

public class SatoruTriangleVisualizer : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private LineRenderer triangleLine;
    [SerializeField] private GameObject labelPrefab;

    [Header("Etiquetas")]
    [SerializeField] private float labelOffsetDistance = 0.5f;

    [SerializeField] private float fontSize = 10f;

    private TMP_Text labelX;
    private TMP_Text labelY;
    private TMP_Text labelH;
    private TMP_Text labelAngle;

    private int ladosSeleccionados = 0;
    private bool labelsCreados = false;
    private bool necesitaReset = true;

    void Start()
    {
        if (triangleLine == null)
            triangleLine = GetComponent<LineRenderer>();

        if (triangleLine != null)
        {
            triangleLine.startColor = Color.cyan;
            triangleLine.endColor = Color.cyan;
            triangleLine.startWidth = 0.1f;
            triangleLine.endWidth = 0.1f;
            triangleLine.positionCount = 4;
            triangleLine.enabled = false;
        }

        CrearLabels();
        ResetLados();
    }

    public void ResetLados()
    {
        ladosSeleccionados = Random.Range(0, 3);
        necesitaReset = false;
        Debug.Log($"🔄 ResetLados() -> Nuevos lados: {ladosSeleccionados} (0=CA+CO, 1=CO+H, 2=CA+H)");
    }

    private void CrearLabels()
    {
        if (labelsCreados) return;
        if (labelPrefab == null) return;

        labelX = CrearLabel("LabelX");
        labelY = CrearLabel("LabelY");
        labelH = CrearLabel("LabelH");
        labelAngle = CrearLabel("LabelAngle");

        labelsCreados = true;
        Debug.Log("✅ Labels creados (una sola vez)");
    }

    private TMP_Text CrearLabel(string nombre)
    {
        GameObject go = Instantiate(labelPrefab, transform);
        go.name = nombre;
        go.SetActive(false);
        TMP_Text text = go.GetComponent<TMP_Text>();
        if (text != null) text.fontSize = fontSize;
        return text;
    }

    /// <summary>
    /// Devuelve la posición ideal para la etiqueta del segmento puntoA-puntoB:
    /// el punto medio de la línea, desplazado perpendicularmente hacia AFUERA
    /// del triángulo (alejándose de puntoOpuesto, el tercer vértice).
    ///
    /// Por qué hace falta el "hacia afuera": la perpendicular de una línea
    /// tiene dos direcciones posibles (por ejemplo, arriba o abajo de una
    /// línea horizontal). Sin este chequeo, según la posición del enemigo
    /// respecto al jugador, la etiqueta podía terminar metida dentro del
    /// triángulo en vez de fuera. Comparamos con el tercer punto para
    /// elegir siempre el lado correcto, sin importar la orientación.
    /// </summary>
    private Vector2 CalcularPosicionEtiqueta(Vector2 puntoA, Vector2 puntoB, Vector2 puntoOpuesto, float distanciaOffset)
    {
        Vector2 puntoMedio = (puntoA + puntoB) / 2f;
        Vector2 direccionLinea = (puntoB - puntoA).normalized;

        // Rotar 90° para obtener una de las dos perpendiculares posibles
        Vector2 perpendicular = new Vector2(-direccionLinea.y, direccionLinea.x);

        // Si esa perpendicular apunta hacia el tercer vértice (o sea, hacia
        // adentro del triángulo), la invertimos.
        Vector2 haciaElTercero = puntoOpuesto - puntoMedio;
        if (Vector2.Dot(perpendicular, haciaElTercero) > 0f)
        {
            perpendicular = -perpendicular;
        }

        return puntoMedio + perpendicular * distanciaOffset;
    }

    public int DibujarTriangulo(Vector2 jugador, Vector2 enemigo)
    {
        if (!labelsCreados) CrearLabels();

        if (necesitaReset)
        {
            ResetLados();
        }

        float dx = enemigo.x - jugador.x;
        float dy = enemigo.y - jugador.y;
        float hipotenusa = Mathf.Sqrt(dx * dx + dy * dy);
        Vector2 proyeccion = new Vector2(enemigo.x, jugador.y);

        if (triangleLine != null)
        {
            triangleLine.enabled = true;
            triangleLine.SetPosition(0, jugador);
            triangleLine.SetPosition(1, enemigo);
            triangleLine.SetPosition(2, proyeccion);
            triangleLine.SetPosition(3, jugador);
        }

        if (labelX != null) labelX.gameObject.SetActive(false);
        if (labelY != null) labelY.gameObject.SetActive(false);
        if (labelH != null) labelH.gameObject.SetActive(false);
        if (labelAngle != null) labelAngle.gameObject.SetActive(false);

        switch (ladosSeleccionados)
        {
            case 0: // CA + CO → Tangente
                if (labelX != null)
                {
                    labelX.text = $"CA: {Mathf.Abs(dx):F2}";
                    labelX.transform.position = CalcularPosicionEtiqueta(jugador, proyeccion, enemigo, labelOffsetDistance);
                    labelX.color = Color.blue;
                    labelX.fontSize = fontSize;
                    labelX.gameObject.SetActive(true);
                }
                if (labelY != null)
                {
                    labelY.text = $"CO: {Mathf.Abs(dy):F2}";
                    labelY.transform.position = CalcularPosicionEtiqueta(proyeccion, enemigo, jugador, labelOffsetDistance);
                    labelY.color = Color.red;
                    labelY.fontSize = fontSize;
                    labelY.gameObject.SetActive(true);
                }
                break;
            case 1: // CO + H → Seno
                if (labelY != null)
                {
                    labelY.text = $"CO: {Mathf.Abs(dy):F2}";
                    labelY.transform.position = CalcularPosicionEtiqueta(proyeccion, enemigo, jugador, labelOffsetDistance);
                    labelY.color = Color.red;
                    labelY.fontSize = fontSize;
                    labelY.gameObject.SetActive(true);
                }
                if (labelH != null)
                {
                    labelH.text = $"H: {hipotenusa:F2}";
                    labelH.transform.position = CalcularPosicionEtiqueta(jugador, enemigo, proyeccion, labelOffsetDistance);
                    labelH.color = Color.green;
                    labelH.fontSize = fontSize;
                    labelH.gameObject.SetActive(true);
                }
                break;
            case 2: // CA + H → Coseno
                if (labelX != null)
                {
                    labelX.text = $"CA: {Mathf.Abs(dx):F2}";
                    labelX.transform.position = CalcularPosicionEtiqueta(jugador, proyeccion, enemigo, labelOffsetDistance);
                    labelX.color = Color.blue;
                    labelX.fontSize = fontSize;
                    labelX.gameObject.SetActive(true);
                }
                if (labelH != null)
                {
                    labelH.text = $"H: {hipotenusa:F2}";
                    labelH.transform.position = CalcularPosicionEtiqueta(jugador, enemigo, proyeccion, labelOffsetDistance);
                    labelH.color = Color.green;
                    labelH.fontSize = fontSize;
                    labelH.gameObject.SetActive(true);
                }
                break;
        }

        if (labelAngle != null)
        {
            labelAngle.text = "θ";
            labelAngle.transform.position = jugador + new Vector2(0.8f, 0.8f);
            labelAngle.color = Color.white;
            labelAngle.fontSize = fontSize;
            labelAngle.gameObject.SetActive(true);
        }

        return ladosSeleccionados;
    }

    public void OcultarTriangulo()
    {
        if (triangleLine != null) triangleLine.enabled = false;
        if (labelX != null) labelX.gameObject.SetActive(false);
        if (labelY != null) labelY.gameObject.SetActive(false);
        if (labelH != null) labelH.gameObject.SetActive(false);
        if (labelAngle != null) labelAngle.gameObject.SetActive(false);
        necesitaReset = true;
    }
}