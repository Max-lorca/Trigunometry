using UnityEngine;

public class EnemyAnalizable : MonoBehaviour, IAnalizable
{
    [Header("Configuración Trigonométrica")]
    [SerializeField] private string funcionTrigonometrica = "sin"; // sin, cos, tan
    [SerializeField] private float anguloGrados = 45f;
    [SerializeField] private float valorCorrecto = 0.7071f; // sin(45°) = 0.7071

    [Header("Referencias")]
    [SerializeField] private MeleeEnemyController meleeEnemy;
    [SerializeField] private DistanceEnemyController distanceEnemy;

    [Header("Debilidades (Modo Satoru)")]
    [SerializeField] public float multiplicadorSeno = 1f;
    [SerializeField] public float multiplicadorCoseno = 1f;
    [SerializeField] public float multiplicadorTangente = 1f;

    public Transform AnalysisTransform => transform;
    public string FuncionTrigonometrica => funcionTrigonometrica;
    public float AnguloGrados => anguloGrados;
    public float ValorCorrecto => valorCorrecto;

    private bool isSelected = false;
    private float vidaActual;
    private float vidaMaxima = 100f;

    void Start()
    {
        if (meleeEnemy == null)
            meleeEnemy = GetComponent<MeleeEnemyController>();
        if (distanceEnemy == null)
            distanceEnemy = GetComponent<DistanceEnemyController>();

        vidaActual = vidaMaxima;
    }

    public void OnSeleccionado()
    {
        isSelected = true;
        Debug.Log($"🔍 Enemigo seleccionado: {gameObject.name}");
        // Efecto visual: resaltar al enemigo
        StartCoroutine(ResaltarEnemigo());
    }

    private System.Collections.IEnumerator ResaltarEnemigo()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color original = sr.color;
            sr.color = Color.yellow;
            yield return new WaitForSecondsRealtime(0.1f);
            sr.color = original;
        }
    }

    public void OnAnalisisExitoso(float multiplicadorDano)
    {
        Debug.Log($"✅ Análisis exitoso! Multiplicador: {multiplicadorDano}");

        // Obtener el multiplicador según el arma actual
        float multiplicadorArma = ObtenerMultiplicadorPorArma();
        float dañoFinal = 100f * multiplicadorDano * multiplicadorArma; // Muerte instantánea

        Debug.Log($"💥 Daño calculado: {dañoFinal} (Arma x{multiplicadorArma})");

        // Aplicar daño
        RecibirDanoAnalisis(dañoFinal);

        // Drop garantizado de 2 vidas
        TryDropHealingItems(2);
    }

    private float ObtenerMultiplicadorPorArma()
    {
        // Obtener el arma actual del jugador
        WeaponData armaActual = FindFirstObjectByType<WeaponShoot>()?.currentWeapon;
        if (armaActual == null) return 1f;

        // Determinar el multiplicador según el tipo de arma
        // NOTA: Como no tenemos WeaponType en WeaponData, usamos el nombre del GameObject
        string nombreArma = armaActual.gameObject.name.ToLower();

        if (nombreArma.Contains("seno") || nombreArma.Contains("sin"))
            return multiplicadorSeno;
        else if (nombreArma.Contains("coseno") || nombreArma.Contains("cos"))
            return multiplicadorCoseno;
        else if (nombreArma.Contains("tangente") || nombreArma.Contains("tan"))
            return multiplicadorTangente;

        return 1f;
    }

    public void OnAnalisisFallido()
    {
        Debug.Log($"❌ Análisis fallido para {gameObject.name}");
        // Feedback visual: flash rojo
        StartCoroutine(FlashRojo());
    }

    private System.Collections.IEnumerator FlashRojo()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color original = sr.color;
            sr.color = Color.red;
            yield return new WaitForSecondsRealtime(0.2f);
            sr.color = original;
        }
    }

    public void OnDeseleccionado()
    {
        isSelected = false;
        Debug.Log($"🔍 Enemigo deseleccionado: {gameObject.name}");
    }

    public void RecibirDanoAnalisis(float daño)
    {
        vidaActual -= daño;
        Debug.Log($"💥 Enemigo recibió {daño} de daño. Vida restante: {vidaActual}");

        if (vidaActual <= 0)
        {
            Muerte();
        }
    }

    private void Muerte()
    {
        Debug.Log($"💀 {gameObject.name} murió!");

        // Llamar al método de muerte del enemigo correspondiente
        if (meleeEnemy != null)
            meleeEnemy.TomarDaño(vidaMaxima);
        else if (distanceEnemy != null)
            distanceEnemy.TomarDaño(vidaMaxima);
        else
            Destroy(gameObject);
    }

    private void TryDropHealingItems(int cantidad)
    {
        Debug.Log($"📦 Drop garantizado de {cantidad} items de curación");
        // Buscar el prefab de curación y dropearlo
        GameObject healingPrefab = Resources.Load<GameObject>("HealingItemPrefab");
        if (healingPrefab != null)
        {
            for (int i = 0; i < cantidad; i++)
            {
                Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
                Instantiate(healingPrefab, transform.position + offset, Quaternion.identity);
            }
        }
    }
}