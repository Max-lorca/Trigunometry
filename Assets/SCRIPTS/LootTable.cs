using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "NuevaLootTable", menuName = "Juego/Loot Table")]
public class LootTable : ScriptableObject
{
    [Serializable]
    public struct Entry
    {
        public ModifierItemData item;
        public float weight;
    }
    [Range(0f,1f)] public float dropChance = 0.2f;
    public List<Entry> entries = new();

    public ModifierItemData Roll()
    {
        if (entries.Count == 0 || UnityEngine.Random.value > dropChance) return null;

        float total = 0f;
        foreach( var e in entries) total += e.weight;

        float r = UnityEngine.Random.value * total;
        foreach(var e in entries)
        {
            r -= e.weight;
            if (r <= 0f) return e.item;
        }
        return entries[entries.Count - 1].item;
    }
}
