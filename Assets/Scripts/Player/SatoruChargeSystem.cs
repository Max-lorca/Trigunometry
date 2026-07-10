using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SatoruChargeSystem : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float maxCharge = 100f;
    [SerializeField] private float chargePerHit = 10f;
    [SerializeField] private float satoruDuration = 4f;

    [Header("UI")]
    [SerializeField] private TMP_Text chargeText;
    [SerializeField] private Image chargeBar;

    [Header("Referencias")]
    [SerializeField] private TimeStopManager timeStopManager;

    private float currentCharge = 0f;
    private bool isReady = false;

    public float CurrentCharge => currentCharge;
    public float MaxCharge => maxCharge;
    public bool IsReady => isReady;

    void Start()
    {
        if (timeStopManager == null)
            timeStopManager = GetComponent<TimeStopManager>();

        if (chargeBar != null)
            chargeBar.fillAmount = 0f;

        if (chargeText != null)
            chargeText.text = $"Cargando: 0/{maxCharge}";
    }

    void Update()
    {
        if (chargeText != null)
        {
            if (isReady)
                chargeText.text = "⚡ ¡LISTO! Presiona F";
            else
                chargeText.text = $"Cargando: {currentCharge}/{maxCharge}";
        }

        if (chargeBar != null)
            chargeBar.fillAmount = currentCharge / maxCharge;
    }

    public void AddCharge()
    {
        if (isReady) return;
        if (timeStopManager != null && timeStopManager.IsAnalysisActive) return;

        currentCharge += chargePerHit;
        if (currentCharge >= maxCharge)
        {
            currentCharge = maxCharge;
            isReady = true;
            Debug.Log("⚡ ¡Modo Satoru listo!");
        }
    }

    public void ConsumeCharge()
    {
        if (!isReady) return;

        currentCharge = 0f;
        isReady = false;
        Debug.Log("🔄 Carga consumida");
    }

    public void ResetCharge()
    {
        currentCharge = 0f;
        isReady = false;
    }
}