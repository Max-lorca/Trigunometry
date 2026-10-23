using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class Tan_x : MonoBehaviour
{
    [Header("Escudo")]
    [SerializeField] private float blockRadius = 2.5f;
    [Tooltip("Ancho del arco que cubre el escudo, en grados, centrado en la dirección de apuntado. 360 = protege en todas direcciones (comportamiento anterior).")]
    [SerializeField][Range(10f, 360f)] private float blockAngle = 120f;
    [SerializeField] private int maxBlocks = 2;
    [SerializeField] private float shieldDuration = 1.5f;
    [SerializeField] private float blockCooldown = 10f;
    [Header("Configuración Explosión")]
    [SerializeField][Range(0, 10)] private float explotionDamage = 1.5f;
    [Header("Configuración Visual")]
    [Tooltip("Objeto del escudo. Debe tener el SpriteRenderer en este mismo objeto (es el que se desvanece) y estar ACTIVO en la escena: se oculta con alpha 0, no con SetActive.")]
    [SerializeField] private GameObject shieldVisual;
    [Tooltip("Objeto que rota hacia donde apunta el jugador (el que lleva PlayerWeaponAim). Su eje X (derecha) es la dirección del escudo. Si está vacío se usa el Shield Visual.")]
    [SerializeField] private Transform aimTransform;
    [Tooltip("Segundos que tarda el escudo en aparecer y desaparecer (fade). 0 = instantáneo.")]
    [SerializeField] private float transitionEffectTime = 0.2f;

    [Header("Indicador de recarga (UI)")]
    [Tooltip("Raíz del indicador que sigue al puntero. Hijo directo del Canvas, con anclas y pivote al centro.")]
    [SerializeField] private RectTransform cooldownIndicator;
    [Tooltip("Image con Image Type = Filled y Fill Method = Radial 360 (necesita un sprite asignado).")]
    [SerializeField] private Image cooldownFill;
    [Tooltip("Opcional: si está vacío se busca el Canvas padre del indicador.")]
    [SerializeField] private Canvas canvas;
    [Tooltip("Desplazamiento respecto al puntero (en píxeles si el Canvas es Overlay).")]
    [SerializeField] private Vector2 indicatorOffset = new Vector2(40f, -40f);
    [Tooltip("Si está activo, muestra el círculo lleno cuando el escudo está listo. Si no, se oculta al terminar la recarga.")]
    [SerializeField] private bool mostrarCuandoListo = false;
    [SerializeField] private Color colorRecargando = new Color(1f, 1f, 1f, 0.7f);
    [SerializeField] private Color colorListo = Color.cyan;

    [Header("LayerMask")]
    [SerializeField] private LayerMask layerMask;

    private WeaponModifierManager _mods;
    private FadeController _fadeController;
    private SpriteRenderer _shieldSprite;
    private Coroutine _fadeRoutine;

    private int blocksUsed;

    private float shieldTimer;
    private float cooldownTimer;
    private float cooldownTotal;   // duración total de la recarga actual (para calcular el relleno)

    private bool shieldActive;
    public bool IsBlocking => shieldActive;

    private readonly Collider2D[] _hits = new Collider2D[10];
    private readonly List<Collider2D> _explotionHits = new List<Collider2D>();
    private ContactFilter2D _explotionFilter;

    public WeaponType Type => WeaponType.Tan;

    void Awake()
    {
        _mods = FindFirstObjectByType<WeaponModifierManager>();
        _fadeController = GetComponent<FadeController>();
        _shieldSprite = shieldVisual != null ? shieldVisual.GetComponent<SpriteRenderer>() : null;

        // Explotion filter Configuration
        _explotionFilter = new ContactFilter2D();
        _explotionFilter.SetLayerMask(layerMask);
        _explotionFilter.useLayerMask = true;

        // El escudo empieza invisible (alpha 0). Se pone directo, sin depender del FadeController.
        PonerAlphaEscudo(0f);

        if (canvas == null && cooldownIndicator != null)
            canvas = cooldownIndicator.GetComponentInParent<Canvas>();

        if (cooldownIndicator != null)
            cooldownIndicator.gameObject.SetActive(false);
    }

    private float S(StatType stat, float baseValue) =>
        _mods != null ? _mods.GetFinalValue(WeaponType.Tan, stat, baseValue) : baseValue;

    // Dirección hacia donde apunta el escudo (eje X del objeto que rota)
    private Vector2 AimDirection()
    {
        Transform t = aimTransform != null
            ? aimTransform
            : (shieldVisual != null ? shieldVisual.transform : transform);
        return t.right;
    }

    // ───────────────────────── Fade del escudo ─────────────────────────

    private void PonerAlphaEscudo(float alpha)
    {
        if (_shieldSprite == null) return;

        Color c = _shieldSprite.color;
        c.a = alpha;
        _shieldSprite.color = c;
    }

    // Lleva el escudo hacia el alpha indicado (1 = visible, 0 = invisible).
    // Si ya había un fade en curso lo cancela, para que dos fades no se pisen
    // (p. ej. si el escudo termina mientras todavía estaba apareciendo).
    private void FadeEscudo(float alpha)
    {
        if (_shieldSprite == null) return;

        if (_fadeRoutine != null)
        {
            StopCoroutine(_fadeRoutine);
            _fadeRoutine = null;
        }

        // Sin FadeController o con tiempo 0: cambio instantáneo
        if (_fadeController == null || transitionEffectTime <= 0f)
        {
            PonerAlphaEscudo(alpha);
            return;
        }

        _fadeRoutine = StartCoroutine(_fadeController.Desvanecimiento(_shieldSprite, alpha, transitionEffectTime));
    }

    // Shield Input
    public void TryBlock()
    {
        if (shieldActive || cooldownTimer > 0f) return;

        shieldActive = true;
        blocksUsed = 0;
        shieldTimer = shieldDuration;

        FadeEscudo(1f);
    }

    void Update()
    {
        ActiveShield();
    }

    private void ActiveShield()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (!shieldActive) return;

        shieldTimer -= Time.deltaTime;
        int max = Mathf.RoundToInt(S(StatType.ProjectileCount, maxBlocks));

        Vector2 center = transform.position;
        Vector2 aimDir = AimDirection();
        float halfArc = blockAngle * 0.5f;

        int count = Physics2D.OverlapCircleNonAlloc(center, blockRadius, _hits);
        for (int i = 0; i < count && blocksUsed < max; i++)
        {
            if (!_hits[i].CompareTag("Proyectil")) continue;

            // Solo bloquea proyectiles que vienen dentro del arco del escudo
            if (blockAngle < 360f)
            {
                Vector2 haciaProyectil = (Vector2)_hits[i].transform.position - center;
                if (Vector2.Angle(aimDir, haciaProyectil) > halfArc) continue;
            }

            blocksUsed++;
            Destroy(_hits[i].gameObject);
        }

        if (shieldTimer <= 0f || blocksUsed >= max)
            EndShield();
    }

    private void EndShield()
    {
        shieldActive = false;

        // La recarga empieza cuando termina el escudo
        cooldownTotal = Mathf.Max(0.5f, S(StatType.Cooldown, blockCooldown));
        cooldownTimer = cooldownTotal;

        FadeEscudo(0f);
    }

    // ───────────────────────── Indicador de recarga ─────────────────────────

    // Se actualiza en LateUpdate para que corra DESPUÉS de que la cámara se mueva en ese frame
    void LateUpdate()
    {
        ActualizarIndicador();
    }

    private void ActualizarIndicador()
    {
        if (cooldownIndicator == null || cooldownFill == null) return;

        bool recargando = cooldownTimer > 0f && cooldownTotal > 0f;
        bool mostrar = recargando || (mostrarCuandoListo && !shieldActive);

        if (cooldownIndicator.gameObject.activeSelf != mostrar)
            cooldownIndicator.gameObject.SetActive(mostrar);

        if (!mostrar) return;

        // 0 = recién empezó la recarga, 1 = escudo listo
        cooldownFill.fillAmount = recargando
            ? 1f - Mathf.Clamp01(cooldownTimer / cooldownTotal)
            : 1f;
        cooldownFill.color = recargando ? colorRecargando : colorListo;

        SeguirPuntero();
    }

    private void SeguirPuntero()
    {
        if (canvas == null || Mouse.current == null) return;

        Vector2 screenPos = Mouse.current.position.ReadValue();

        // Screen Space - Overlay: la posición del UI son píxeles de pantalla,
        // no interviene ninguna cámara (es lo más estable)
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            cooldownIndicator.position = screenPos + indicatorOffset;
            return;
        }

        // Screen Space - Camera / World Space: depende de la cámara del canvas
        RectTransform canvasRect = (RectTransform)canvas.transform;
        Camera uiCam = canvas.worldCamera != null ? canvas.worldCamera : Camera.main;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, uiCam, out Vector2 local))
            cooldownIndicator.anchoredPosition = local + indicatorOffset;
    }

    // ───────────────────────── Explosión ─────────────────────────

    public IEnumerator ShieldExplotion()
    {
        if (S(StatType.ShieldCharge, 0) > 0)
        {
            int hitCount = Physics2D.OverlapCircle(transform.position, this.blockRadius, _explotionFilter, _explotionHits);

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D collider = _explotionHits[i];
                switch (collider.gameObject.tag)
                {
                    case "Proyectil":
                        Destroy(collider.gameObject);
                        break;
                    case "Enemy":
                        EnemyBase enemy = collider.GetComponentInParent<EnemyBase>();
                        if (enemy != null) enemy.TomarDaño(this.explotionDamage, this.transform.position);
                        break;
                }
            }
        }
        yield return null;
    }

    void OnDrawGizmosSelected()
    {
        Vector3 pos = transform.position;

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(pos, blockRadius);

        // Arco que cubre el escudo
        Vector3 aim = AimDirection();
        Vector3 bordeA = Quaternion.Euler(0f, 0f, blockAngle * 0.5f) * aim;
        Vector3 bordeB = Quaternion.Euler(0f, 0f, -blockAngle * 0.5f) * aim;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(pos, pos + bordeA * blockRadius);
        Gizmos.DrawLine(pos, pos + bordeB * blockRadius);
    }
}