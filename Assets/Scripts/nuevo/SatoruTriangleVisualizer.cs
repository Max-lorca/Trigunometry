using UnityEngine;
using TMPro;

public class SatoruTriangleVisualizer : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private LineRenderer triangleLine;
    [SerializeField] private GameObject labelPrefab;

    private TMP_Text labelX;
    private TMP_Text labelY;
    private TMP_Text labelH;
    private TMP_Text labelAngle;

    private int ladosSeleccionados = 0;
    private bool labelsCreados = false;
    private bool necesitaReset = true; // ✅ NUEVO

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
        necesitaReset = false; // ✅ YA NO NECESITA RESET
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
        if (text != null) text.fontSize = 1.2f;
        return text;
    }

    public int DibujarTriangulo(Vector2 jugador, Vector2 enemigo)
    {
        if (!labelsCreados) CrearLabels();

        // ✅ SI NECESITA RESET, HACERLO ANTES DE DIBUJAR
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

        // ✅ OCULTAR TODOS
        if (labelX != null) labelX.gameObject.SetActive(false);
        if (labelY != null) labelY.gameObject.SetActive(false);
        if (labelH != null) labelH.gameObject.SetActive(false);
        if (labelAngle != null) labelAngle.gameObject.SetActive(false);

        // ✅ MOSTRAR SEGÚN ladosSeleccionados
        switch (ladosSeleccionados)
        {
            case 0: // CA + CO → Tangente
                if (labelX != null)
                {
                    labelX.text = $"CA: {Mathf.Abs(dx):F2}";
                    labelX.transform.position = (jugador + proyeccion) / 2 + new Vector2(0, -0.5f);
                    labelX.color = Color.blue;
                    labelX.fontSize = 1.2f;
                    labelX.gameObject.SetActive(true);
                }
                if (labelY != null)
                {
                    labelY.text = $"CO: {Mathf.Abs(dy):F2}";
                    labelY.transform.position = (enemigo + proyeccion) / 2 + new Vector2(0.5f, 0);
                    labelY.color = Color.red;
                    labelY.fontSize = 1.2f;
                    labelY.gameObject.SetActive(true);
                }
                break;
            case 1: // CO + H → Seno
                if (labelY != null)
                {
                    labelY.text = $"CO: {Mathf.Abs(dy):F2}";
                    labelY.transform.position = (enemigo + proyeccion) / 2 + new Vector2(0.5f, 0);
                    labelY.color = Color.red;
                    labelY.fontSize = 1.2f;
                    labelY.gameObject.SetActive(true);
                }
                if (labelH != null)
                {
                    Vector2 posH1 = (jugador + enemigo) / 2;
                    Vector2 dirH1 = (enemigo - jugador).normalized;
                    posH1 += new Vector2(-dirH1.y, dirH1.x) * 0.5f;
                    labelH.text = $"H: {hipotenusa:F2}";
                    labelH.transform.position = posH1;
                    labelH.color = Color.green;
                    labelH.fontSize = 1.2f;
                    labelH.gameObject.SetActive(true);
                }
                break;
            case 2: // CA + H → Coseno
                if (labelX != null)
                {
                    labelX.text = $"CA: {Mathf.Abs(dx):F2}";
                    labelX.transform.position = (jugador + proyeccion) / 2 + new Vector2(0, -0.5f);
                    labelX.color = Color.blue;
                    labelX.fontSize = 1.2f;
                    labelX.gameObject.SetActive(true);
                }
                if (labelH != null)
                {
                    Vector2 posH2 = (jugador + enemigo) / 2;
                    Vector2 dirH2 = (enemigo - jugador).normalized;
                    posH2 += new Vector2(-dirH2.y, dirH2.x) * 0.5f;
                    labelH.text = $"H: {hipotenusa:F2}";
                    labelH.transform.position = posH2;
                    labelH.color = Color.green;
                    labelH.fontSize = 1.2f;
                    labelH.gameObject.SetActive(true);
                }
                break;
        }

        // Ángulo θ
        if (labelAngle != null)
        {
            labelAngle.text = "θ";
            labelAngle.transform.position = jugador + new Vector2(0.8f, 0.8f);
            labelAngle.color = Color.white;
            labelAngle.fontSize = 1.8f;
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
        necesitaReset = true; // ✅ CUANDO SE OCULTA, RESETEAR EN EL PRÓXIMO DIBUJO
    }
}