using UnityEngine;
using System.Collections.Generic;

public class EDropItem : MonoBehaviour
{
    public enum ModoDrop
    {
        TodasLasTablas,   // cada tabla se tira por separado: puede soltar varios items a la vez
        UnaTablaAlAzar    // se elige UNA tabla al azar y solo se tira esa: suelta como mucho 1 item
    }

    [Header("Loot")]
    [SerializeField] private List<LootTable> lootTable;
    [SerializeField] private GameObject pickupPrefab;

    [Header("Varias tablas")]
    [SerializeField] private ModoDrop modo = ModoDrop.TodasLasTablas;
    [Tooltip("Separación aleatoria entre pickups cuando cae más de un item, para que no queden apilados.")]
    [SerializeField] private float separacionDrops = 0.5f;

    [Header("Probabilidad")]
    [Tooltip("Probabilidad de que el enemigo intente soltar algo al morir (0 = nunca, 1 = siempre intenta).")]
    [SerializeField][Range(0f, 1f)] private float dropChance = 1f;

    [Header("Debug / Demostración")]
    [Tooltip("Garantiza que salga al menos un item (ignora Drop Chance y repite la tirada si no sale nada).")]
    [SerializeField] private bool forzarDrop = false;
    [SerializeField] private int maxReintentosForzado = 50;

    // Se reutiliza entre llamadas para no crear una lista nueva en cada muerte
    private readonly List<ModifierItemData> _items = new List<ModifierItemData>();

    public void Drop()
    {
        if (lootTable == null || lootTable.Count == 0 || pickupPrefab == null) return;

        if (!forzarDrop && Random.value > dropChance) return;

        TirarTablas(_items);

        // Forzado: se repite la tirada completa hasta que salga al menos un item
        if (forzarDrop)
        {
            int intentos = 0;
            while (_items.Count == 0 && intentos < maxReintentosForzado)
            {
                TirarTablas(_items);
                intentos++;
            }
        }

        for (int i = 0; i < _items.Count; i++)
            SoltarPickup(_items[i], i);
    }

    // Llena "resultado" con los items que salieron (puede quedar vacío)
    private void TirarTablas(List<ModifierItemData> resultado)
    {
        resultado.Clear();

        if (modo == ModoDrop.UnaTablaAlAzar)
        {
            LootTable elegida = ElegirTablaAlAzar();
            if (elegida != null)
            {
                ModifierItemData item = elegida.Roll();
                if (item != null) resultado.Add(item);
            }
            return;
        }

        // TodasLasTablas: cada una hace su propia tirada, y cada item que salga se suma
        foreach (LootTable tabla in lootTable)
        {
            if (tabla == null) continue; // hueco vacío en la lista del Inspector

            ModifierItemData item = tabla.Roll();
            if (item != null) resultado.Add(item);
        }
    }

    private LootTable ElegirTablaAlAzar()
    {
        int validas = 0;
        foreach (LootTable t in lootTable)
            if (t != null) validas++;

        if (validas == 0) return null;

        int objetivo = Random.Range(0, validas);
        foreach (LootTable t in lootTable)
        {
            if (t == null) continue;
            if (objetivo == 0) return t;
            objetivo--;
        }

        return null;
    }

    private void SoltarPickup(ModifierItemData item, int indice)
    {
        // Usa el prefab propio del item; si no tiene, el genérico
        GameObject prefab = item.pickupPrefab != null ? item.pickupPrefab : pickupPrefab;
        if (prefab == null)
        {
            Debug.LogWarning($"[EDropItem] '{item.itemName}' no tiene prefab propio y no hay Pickup Prefab genérico.");
            return;
        }

        // El primero cae en el punto exacto; los demás, con un pequeño desplazamiento
        Vector3 offset = indice == 0
            ? Vector3.zero
            : (Vector3)(Random.insideUnitCircle * separacionDrops);

        GameObject go = Instantiate(prefab, transform.position + offset, Quaternion.identity);

        GrantModifiers grant = go.GetComponent<GrantModifiers>();
        if (grant != null)
            grant.SetItem(item);
        else
            Debug.LogWarning($"[EDropItem] El prefab '{prefab.name}' no tiene GrantModifiers, el item no se asignó.");
    }
}