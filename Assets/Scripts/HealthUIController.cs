using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthUIController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Image circleFillImage;
    [SerializeField] private TMP_Text radianText;

    [Header("Colores")]
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color mediumHealthColor = Color.yellow;
    [SerializeField] private Color lowHealthColor = Color.red;

    private int currentHealth;
    private int maxHealth;

    void Start()
    {
        if (circleFillImage == null)
            circleFillImage = GetComponent<Image>();

        if (circleFillImage != null)
        {
            circleFillImage.type = Image.Type.Filled;
            circleFillImage.fillMethod = Image.FillMethod.Radial360;
            circleFillImage.fillOrigin = (int)Image.Origin360.Right;
        }
    }

    public void UpdateHealth(int current, int max)
    {
        currentHealth = current;
        maxHealth = max;

        float fillAmount = (float)currentHealth / (float)maxHealth;

        if (circleFillImage != null)
            circleFillImage.fillAmount = fillAmount;

        // Convertir a fracción de π
        string radianTextValue = GetRadianFraction(current, max);

        if (radianText != null)
            radianText.text = radianTextValue;

        // Cambiar color según la vida
        if (circleFillImage != null)
        {
            if (fillAmount > 0.66f)
                circleFillImage.color = fullHealthColor;
            else if (fillAmount > 0.33f)
                circleFillImage.color = mediumHealthColor;
            else
                circleFillImage.color = lowHealthColor;
        }
    }

    private string GetRadianFraction(int current, int max)
    {
        // Simplificar la fracción current/max
        int numerador = current;
        int denominador = max;

        // Si la vida es 0
        if (current == 0)
            return "0";

        // Si la vida está llena
        if (current == max)
            return "2π";

        // Fracción de π: (current/max) * 2π
        // = (2 * current / max) π

        int numeradorFinal = 2 * current;
        int denominadorFinal = max;

        // Simplificar la fracción
        int gcd = GetGCD(numeradorFinal, denominadorFinal);
        numeradorFinal /= gcd;
        denominadorFinal /= gcd;

        // Casos especiales
        if (numeradorFinal == 1 && denominadorFinal == 1)
            return "π";

        if (denominadorFinal == 1)
            return $"{numeradorFinal}π";

        return $"{numeradorFinal}/{denominadorFinal}π";
    }

    private int GetGCD(int a, int b)
    {
        while (b != 0)
        {
            int temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }
}