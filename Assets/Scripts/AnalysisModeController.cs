using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class AnalysisModeController : MonoBehaviour
{
    [Header("Detección de enemigos")]
    [SerializeField] private LayerMask capaEnemigos;
    [SerializeField] private Camera camaraPrincipal;

    [Header("UI de análisis")]
    [SerializeField] private GameObject panelAnalisis;
    [SerializeField] private TMP_Text textoPregunta;
    [SerializeField] private TMP_InputField inputRespuesta;
    [SerializeField] private Image barraTiempo;

    [Header("Configuración")]
    [SerializeField] private float tiempoLimiteRespuesta = 5f;
    [SerializeField] private float toleranciaError = 0.05f;
    [SerializeField] private float multiplicadorDanoBonus = 2.5f;

    [Header("Referencias")]
    [SerializeField] private TimeStopManager timeStopManager;
    [SerializeField] private WeaponShoot weaponShoot;

    private IAnalizable enemigoActual;
    private Coroutine corrutinaTimer;

    private void Awake()
    {
        if (camaraPrincipal == null)
            camaraPrincipal = Camera.main;

        // ✅ NULL CHECKS: solo desactivar si existe
        if (panelAnalisis != null)
            panelAnalisis.SetActive(false);

        if (inputRespuesta != null)
            inputRespuesta.onSubmit.AddListener(_ => ValidarRespuesta());
    }

    private void Update()
    {
        if (timeStopManager == null) return;
        if (!timeStopManager.IsAnalysisActive) return;
        if (enemigoActual != null) return;

        if (Input.GetMouseButtonDown(0))
        {
            IntentarSeleccionar();
        }
    }

    private void IntentarSeleccionar()
    {
        if (camaraPrincipal == null) return;

        Vector2 mundoPos = camaraPrincipal.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mundoPos, capaEnemigos);

        if (hit != null && hit.TryGetComponent<IAnalizable>(out var analizable))
        {
            SeleccionarEnemigo(analizable);
        }
    }

    private void SeleccionarEnemigo(IAnalizable enemigo)
    {
        enemigoActual = enemigo;
        enemigo.OnSeleccionado();

        if (timeStopManager != null)
            timeStopManager.SetEnSeleccion(true);

        if (textoPregunta != null)
            textoPregunta.text = $"{enemigo.FuncionTrigonometrica}({enemigo.AnguloGrados}°) = ?";

        if (inputRespuesta != null)
        {
            inputRespuesta.text = "";
            inputRespuesta.Select();
            inputRespuesta.ActivateInputField();
        }

        if (panelAnalisis != null)
            panelAnalisis.SetActive(true);

        if (barraTiempo != null)
            barraTiempo.fillAmount = 1f;

        corrutinaTimer = StartCoroutine(TimerRespuesta());
    }

    private IEnumerator TimerRespuesta()
    {
        float t = tiempoLimiteRespuesta;
        while (t > 0f)
        {
            t -= Time.unscaledDeltaTime;
            if (barraTiempo != null)
                barraTiempo.fillAmount = t / tiempoLimiteRespuesta;
            yield return null;
        }
        Fallar();
    }

    private void ValidarRespuesta()
    {
        if (enemigoActual == null) return;

        if (inputRespuesta == null) return;

        if (!float.TryParse(inputRespuesta.text, out float valorIngresado))
            return;

        if (Mathf.Abs(valorIngresado - enemigoActual.ValorCorrecto) <= toleranciaError)
        {
            Acertar();
        }
        else
        {
            Fallar();
        }
    }

    private void Acertar()
    {
        if (corrutinaTimer != null) StopCoroutine(corrutinaTimer);

        if (enemigoActual != null)
        {
            enemigoActual.OnAnalisisExitoso(multiplicadorDanoBonus);
        }

        if (weaponShoot != null && weaponShoot.currentWeapon != null)
        {
            weaponShoot.currentWeapon.DispararAnalisisExitoso(enemigoActual, multiplicadorDanoBonus);
        }

        FinalizarSeleccion();
    }

    private void Fallar()
    {
        if (corrutinaTimer != null) StopCoroutine(corrutinaTimer);

        if (enemigoActual != null)
        {
            try
            {
                enemigoActual.OnAnalisisFallido();
            }
            catch { }
        }

        FinalizarSeleccion();
    }

    private void FinalizarSeleccion()
    {
        if (panelAnalisis != null)
            panelAnalisis.SetActive(false);

        if (enemigoActual != null)
        {
            try
            {
                enemigoActual.OnDeseleccionado();
            }
            catch { }
            enemigoActual = null;
        }

        if (timeStopManager != null)
            timeStopManager.SetEnSeleccion(false);
    }

    public void LimpiarEnemigo()
    {
        if (corrutinaTimer != null)
        {
            StopCoroutine(corrutinaTimer);
            corrutinaTimer = null;
        }

        if (enemigoActual != null)
        {
            try
            {
                enemigoActual.OnDeseleccionado();
            }
            catch { }
            enemigoActual = null;
        }

        if (panelAnalisis != null)
            panelAnalisis.SetActive(false);

        if (inputRespuesta != null)
            inputRespuesta.text = "";

        if (timeStopManager != null)
            timeStopManager.SetEnSeleccion(false);

        Debug.Log("🧹 Análisis limpiado completamente");
    }
}