using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Se encarga de TODO lo que pasa una vez que el tiempo ya está congelado
/// (lo activa/desactiva TimeStopManager): detectar clic sobre un enemigo,
/// mostrar la pregunta trigonométrica, validar la respuesta y disparar
/// el tiro bonus garantizado con el arma actualmente equipada.
/// </summary>
public class AnalysisModeController : MonoBehaviour
{
    [Header("Detección de enemigos")]
    [SerializeField] private LayerMask capaEnemigos;
    [SerializeField] private Camera camaraPrincipal;

    [Header("UI de análisis")]
    [SerializeField] private GameObject panelAnalisis;
    [SerializeField] private TMP_Text textoPregunta;
    [SerializeField] private TMP_InputField inputRespuesta;
    [SerializeField] private Image barraTiempo; // fillAmount = tiempo restante (opcional)

    [Header("Configuración")]
    [SerializeField] private float tiempoLimiteRespuesta = 5f;
    [SerializeField] private float toleranciaError = 0.05f;
    [SerializeField] private float multiplicadorDanoBonus = 2.5f;

    [Header("Referencias")]
    [SerializeField] private TimeStopManager timeStopManager;
    [SerializeField] private WeaponShoot weaponShoot; // expone weaponShoot.currentWeapon (arma equipada)

    private IAnalizable enemigoActual;
    private Coroutine corrutinaTimer;

    private void Awake()
    {
        if (camaraPrincipal == null) camaraPrincipal = Camera.main;
        panelAnalisis.SetActive(false);
        inputRespuesta.onSubmit.AddListener(_ => ValidarRespuesta());
    }

    private void Update()
    {
        if (!timeStopManager.IsAnalysisActive) return;

        if (enemigoActual == null && Input.GetMouseButtonDown(0))
        {
            IntentarSeleccionar();
        }
    }

    private void IntentarSeleccionar()
    {
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
        timeStopManager.SetEnSeleccion(true);

        textoPregunta.text = $"{enemigo.FuncionTrigonometrica}({enemigo.AnguloGrados}°) = ?";
        inputRespuesta.text = "";
        panelAnalisis.SetActive(true);
        inputRespuesta.Select();
        inputRespuesta.ActivateInputField();

        if (barraTiempo != null) barraTiempo.fillAmount = 1f;
        corrutinaTimer = StartCoroutine(TimerRespuesta());
    }

    private IEnumerator TimerRespuesta()
    {
        float t = tiempoLimiteRespuesta;
        while (t > 0f)
        {
            t -= Time.unscaledDeltaTime;
            if (barraTiempo != null) barraTiempo.fillAmount = t / tiempoLimiteRespuesta;
            yield return null;
        }
        Fallar();
    }

    private void ValidarRespuesta()
    {
        if (enemigoActual == null) return;

        if (!float.TryParse(inputRespuesta.text, out float valorIngresado))
            return; // entrada inválida, no cuenta como intento fallido todavía

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

        // El enemigo marca su drop garantizado de 2 vidas si este golpe lo mata.
        enemigoActual.OnAnalisisExitoso(multiplicadorDanoBonus);

        // El arma actualmente equipada dispara el tiro bonus garantizado.
        weaponShoot.currentWeapon.DispararAnalisisExitoso(enemigoActual, multiplicadorDanoBonus);

        FinalizarSeleccion();
    }

    private void Fallar()
    {
        if (corrutinaTimer != null) StopCoroutine(corrutinaTimer);
        enemigoActual?.OnAnalisisFallido();
        FinalizarSeleccion();
    }

    private void FinalizarSeleccion()
    {
        panelAnalisis.SetActive(false);
        enemigoActual?.OnDeseleccionado();
        enemigoActual = null;
        timeStopManager.SetEnSeleccion(false);
    }
}