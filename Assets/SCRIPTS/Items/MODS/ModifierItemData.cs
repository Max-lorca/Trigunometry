using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NuevoModificador", menuName = "Juego/Modificador")]
public class ModifierItemData : ScriptableObject
{
    public string itemName = "Modificador";
    [TextArea] public string description;
    public Sprite icon;

    // Un item puede tener varios efectos (ventajas y desventajas a la vez)
    public List<StatModifier> modifiers = new();
}