using UnityEngine;
using TMPro;

public class SatoruTriangleVisualizer : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private LineRenderer triangleLine;
    [SerializeField] private GameObject labelsContainer;

    [Header("Prefabs de etiquetas")]
    [SerializeField] private GameObject labelPrefab; // TMP_Text con fondo

    [Header("Estilo")]
    [SerializeField] private Color lineColor = Color.cyan;
    [SerializeField] private float lineWidth = 0.1f;
    [SerializeField] private float labelOffset = 0.5f;

    private TMP_Text labelX;
    private TMP_Text labelY;
    private TMP_Text labelH;
    private TMP_Text labelAngle;

    void Start()
    {
        if (triangleLine == null)
            triangleLine = GetComponent<LineRenderer>();

        if (triangleLine != null)
        {
            triangleLine.startColor = lineColor;
            triangleLine.endColor = lineColor;
            triangleLine.startWidth = lineWidth;
            triangleLine.endWidth = lineWidth;
            triangleLine.positionCount = 4;
            triangleLine.enabled = false;
        }

        if (labelsContainer == null)
        {
            labelsContainer = new GameObject("LabelsContainer");
            labelsContainer.transform.SetParent(transform);
        }

        if (labelPrefab != null)
        {
            labelX = CrearLabel("LabelX");
            labelY = CrearLabel("LabelY");
            labelH = CrearLabel("LabelH");
            labelAngle = CrearLabel("LabelAngle");
        }
    }

    private TMP_Text CrearLabel(string nombre)
    {
        GameObject go = Instantiate(labelPrefab, labelsContainer.transform);
        go.name = nombre;
        go.SetActive(false);
        return go.GetComponent<TMP_Text>();
    }

    public void DibujarTriangulo(Vector2 jugador, Vector2 enemigo, string funcion, string armaNombre)
    {
        // Calcular datos
        float dx = enemigo.x - jugador.x;
        float dy = enemigo.y - jugador.y;
        float hipotenusa = Mathf.Sqrt(dx * dx + dy * dy);
        float angulo = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;

        // Proyección del enemigo en el suelo (misma altura que el jugador)
        Vector2 proyeccion = new Vector2(enemigo.x, jugador.y);

        // Dibujar triángulo
        if (triangleLine != null)
        {
            triangleLine.enabled = true;
            triangleLine.SetPosition(0, jugador);
            triangleLine.SetPosition(1, enemigo);
            triangleLine.SetPosition(2, proyeccion);
            triangleLine.SetPosition(3, jugador);
        }

        // Posicionar etiquetas
        if (labelX != null)
        {
            Vector2 posX = (jugador + proyeccion) / 2 + new Vector2(0, -labelOffset);
            labelX.text = $"X: {dx:F2}";
            labelX.transform.position = posX;
            labelX.gameObject.SetActive(true);
        }

        if (labelY != null)
        {
            Vector2 posY = (enemigo + proyeccion) / 2 + new Vector2(labelOffset, 0);
            labelY.text = $"Y: {dy:F2}";
            labelY.transform.position = posY;
            labelY.gameObject.SetActive(true);
        }

        if (labelH != null)
        {
            Vector2 posH = (jugador + enemigo) / 2;
            Vector2 dirH = (enemigo - jugador).normalized;
            posH += new Vector2(-dirH.y, dirH.x) * labelOffset;
            labelH.text = $"H: {hipotenusa:F2}";
            labelH.transform.position = posH;
            labelH.gameObject.SetActive(true);
        }

        if (labelAngle != null)
        {
            Vector2 posAngulo = jugador + new Vector2(1.5f, 0.5f);
            labelAngle.text = $"θ = ?\n{funcion}";
            labelAngle.transform.position = posAngulo;
            labelAngle.gameObject.SetActive(true);
        }
    }

    public void OcultarTriangulo()
    {
        if (triangleLine != null)
            triangleLine.enabled = false;

        if (labelX != null) labelX.gameObject.SetActive(false);
        if (labelY != null) labelY.gameObject.SetActive(false);
        if (labelH != null) labelH.gameObject.SetActive(false);
        if (labelAngle != null) labelAngle.gameObject.SetActive(false);
    }
}