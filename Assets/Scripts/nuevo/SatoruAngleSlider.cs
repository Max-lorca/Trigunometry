using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SatoruAngleSlider : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Slider angleSlider;
    [SerializeField] private TMP_Text angleText;
    [SerializeField] private PowerShotSystem powerShotSystem;

    private float currentAngle = 0f;

    void Start()
    {
        if (angleSlider == null)
            angleSlider = GetComponent<Slider>();

        if (angleSlider != null)
        {
            angleSlider.minValue = 0f;
            angleSlider.maxValue = 360f;
            angleSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        if (powerShotSystem == null)
            powerShotSystem = FindFirstObjectByType<PowerShotSystem>();

        // Ocultar al inicio
        gameObject.SetActive(false);
    }

    private void OnSliderValueChanged(float value)
    {
        currentAngle = value;
        Debug.Log($" Slider movido a: {currentAngle}°");

        if (angleText != null)
            angleText.text = $"Ángulo: {currentAngle:F1}°";

        if (powerShotSystem != null)
        {
            //powerShotSystem.SetCurrentAngle(currentAngle);
        }
            
    }

    public void MostrarSlider()
    {
        gameObject.SetActive(true);
        if (angleSlider != null)
            angleSlider.value = 0f; // Resetear
    }

    public void OcultarSlider()
    {
        gameObject.SetActive(false);
    }

    public float GetCurrentAngle()
    {
        return currentAngle;
    }
}