using System;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType { Sin, Cos, Tan }
public enum StatType { Damage, Cooldown, ProjectileSpeed, Amplitude, Frequency, ProjectileCount, SpreadAngle, ShieldCooldown, MaxBlocks, ShieldRadius, ShieldCharge}
public enum ModOp { Add, Multiply }

[Serializable]
public struct StatModifier
{
    public WeaponType weapon;
    public StatType stat;
    public ModOp op;
    public float value;
}

public class WeaponModifierManager : MonoBehaviour
{
    private readonly List<StatModifier> modifiers = new();
    public event Action OnModifiersChanged;

    public void AddModifier(StatModifier mod)
    {
        modifiers.Add(mod);
        OnModifiersChanged?.Invoke();
    }

    // Fórmula: (base + suma de Add) * producto de Multiply
    public float GetFinalValue(WeaponType weapon, StatType stat, float baseValue)
    {
        float add = 0f;
        float mult = 1f;

        foreach (var m in modifiers)
        {
            if (m.weapon != weapon || m.stat != stat) continue;

            if (m.op == ModOp.Add) add += m.value;
            else mult *= m.value;
        }
        return (baseValue + add) * mult;
    }
}