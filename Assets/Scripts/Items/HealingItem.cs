using UnityEngine;

public class HealingItem : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private int healAmount = 1;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.3f;

    private Vector3 startPosition;
    private float time;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        time += Time.deltaTime;
        float yOffset = Mathf.Sin(time * bobSpeed) * bobHeight;
        transform.position = startPosition + new Vector3(0, yOffset, 0);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Algo tocó la curita: {other.gameObject.name}, Tag: {other.tag}");

        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.Heal(healAmount);
                Debug.Log($"❤️ Player curado! +{healAmount} vida");
                Destroy(gameObject);
            }
        }
    }
}