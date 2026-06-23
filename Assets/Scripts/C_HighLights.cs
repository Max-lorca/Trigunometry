using UnityEngine;

public class C_HighLights : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private Color colorBase = Color.white;
    [SerializeField] private Color colorBrillo = Color.yellow;
    [SerializeField] private float effectTime = 0.5f;

    private SpriteRenderer spriteRender;

    private void Awake()
    {
        spriteRender = GetComponent<SpriteRenderer>();
        spriteRender.color = colorBase;
    }

    private void Update()
    {
        if (GameManager.Instance == null)
            return;

        if (!GameManager.Instance.isTimeStopped)
        {
            spriteRender.color = colorBase;
            return;
        }

        float t = Mathf.PingPong(Time.unscaledTime, effectTime) / effectTime;

        spriteRender.color = Color.Lerp(colorBase, colorBrillo, t);
    }
}