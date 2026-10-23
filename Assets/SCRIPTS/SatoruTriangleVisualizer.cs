using UnityEngine;
using TMPro;

public class SatoruTriangleVisualizer : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private LineRenderer triangleLine;
    [SerializeField] private GameObject labelPrefab;
    [SerializeField] private Canvas canvas;
    private RectTransform canvasRect;
    [Header("Configuración")]
    [SerializeField] private float lineWidth = 2f;
    [SerializeField] private float alturaMinimaVisual;
    [SerializeField] private float anchoMinimoVisual;

    [Header("Etiquetas")]
    [SerializeField] private float offsetCA = 0.3f;
    [SerializeField] private float offsetCO = 0.3f;
    [SerializeField] private float offsetH = 0.3f;
    [SerializeField] private float fontSize = 10f;
    private Material _material;
    private TMP_Text labelX; 
    private TMP_Text labelY;
    private TMP_Text labelH;
    private TMP_Text labelAngle;

    private int ladosSeleccionados = 0;
    private bool labelsCreados = false;
    private bool necesitaReset = true;

    void Start()
    {
        canvasRect = canvas.GetComponent<RectTransform>();
        if (triangleLine == null)
            triangleLine = GetComponent<LineRenderer>();

        if (triangleLine != null)
        {
            triangleLine.startColor = Color.cyan;
            triangleLine.endColor = Color.cyan;
            triangleLine.startWidth = lineWidth;
            triangleLine.endWidth = lineWidth;
            triangleLine.positionCount = 4;
            triangleLine.enabled = false;
            triangleLine.useWorldSpace = true;
            _material = triangleLine.material;

        }

        CrearLabels();
        ResetLados();
    }
    void Update()
    {
        if (triangleLine != null && triangleLine.enabled)
        {
            _material.SetFloat("_UnscaledTime", Time.unscaledTime);
        }   
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
        GameObject go = Instantiate(labelPrefab, canvas.transform);
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
    private Vector2 DireccionHaciaAfuera(Vector2 puntoA, Vector2 puntoB, Vector2 puntoOpuesto)
    {
        Vector2 puntoMedio = (puntoA + puntoB) / 2f;
        Vector2 dirLinea = (puntoB - puntoA).normalized;
        Vector2 perpendicular = new Vector2(-dirLinea.y, dirLinea.x);

        if (Vector2.Dot(perpendicular, puntoOpuesto - puntoMedio) > 0f)
            perpendicular = -perpendicular;

        return perpendicular;
    }

    private void ColocarLabel(TMP_Text label, string texto, Color color,
                            Vector2 puntoA, Vector2 puntoB, Vector2 puntoOpuesto, float offset)
    {
        if (label == null) return;

        Vector2 dir = DireccionHaciaAfuera(puntoA, puntoB, puntoOpuesto);
        Vector2 puntoMedio = (puntoA + puntoB) / 2f;

        label.text = texto;
        label.color = color;
        label.fontSize = fontSize;
        label.alignment = TextAlignmentOptions.Center;

        // Ajusta el tamaño del rect al texto para que el pivote caiga en el borde real
        label.rectTransform.sizeDelta = label.GetPreferredValues(texto);

        // Pivote: el borde de la etiqueta más cercano a la línea queda sobre el punto
        label.rectTransform.pivot = new Vector2(0.5f - dir.x * 0.5f, 0.5f - dir.y * 0.5f);

        PosicionarLabelUI(label, puntoMedio + dir * offset);
        label.gameObject.SetActive(true);
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

            const float z = 0f; // misma Z para todos los puntos
            triangleLine.SetPositions(new Vector3[]
            {
                new Vector3(jugador.x,    jugador.y,    z),
                new Vector3(enemigo.x,    enemigo.y,    z),
                new Vector3(proyeccion.x, proyeccion.y, z),
                new Vector3(jugador.x,    jugador.y,    z)
            });
        }

        Debug.Log($"Z punto 3: {triangleLine.GetPosition(3).z}");

        if (labelX != null) labelX.gameObject.SetActive(false);
        if (labelY != null) labelY.gameObject.SetActive(false);
        if (labelH != null) labelH.gameObject.SetActive(false);
        if (labelAngle != null) labelAngle.gameObject.SetActive(false);

        string txtCA = $"CA: {Mathf.Abs(dx):F2}";
        string txtCO = $"CO: {Mathf.Abs(dy):F2}";
        string txtH  = $"H: {hipotenusa:F2}";

        switch (ladosSeleccionados)
        {
            case 0: // CA + CO → Tangente
                ColocarLabel(labelX, txtCA, Color.blue, jugador, proyeccion, enemigo, offsetCA);
                ColocarLabel(labelY, txtCO, Color.red, proyeccion, enemigo, jugador, offsetCO);
                break;
            case 1: // CO + H → Seno
                ColocarLabel(labelY, txtCO, Color.red, proyeccion, enemigo, jugador, offsetCO);
                ColocarLabel(labelH, txtH, Color.green, jugador, enemigo, proyeccion, offsetH);
                break;
            case 2: // CA + H → Coseno
                ColocarLabel(labelX, txtCA, Color.blue, jugador, proyeccion, enemigo, offsetCA);
                ColocarLabel(labelH, txtH, Color.green, jugador, enemigo, proyeccion, offsetH);
                break;
        }

        if (labelAngle != null)
        {
            labelAngle.text = "θ";
            labelAngle.color = Color.white;
            labelAngle.fontSize = fontSize;
            labelAngle.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            PosicionarLabelUI(labelAngle, jugador + new Vector2(0.8f, 0.8f));
            labelAngle.gameObject.SetActive(true);
        }

        return ladosSeleccionados;
    }
    private void PosicionarLabelUI(TMP_Text label, Vector2 posicionMundo)
    {
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main;
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, posicionMundo);
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, cam, out Vector2 localPoint))
            label.rectTransform.anchoredPosition = localPoint;
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