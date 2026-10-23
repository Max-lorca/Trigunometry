using UnityEngine;
using System.Text;
using TMPro;

public class GrantModifiers : MonoBehaviour
{
    [SerializeField] private ModifierItemData item;
    [SerializeField] private GameObject pickupParticles;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private bool destroyAfterGrant = true;

    // TEXTO

    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;

    private CanvasGroup _canvasGroup;
    void Start() => Refresh();

    public void SetItem(ModifierItemData newItem)
    {
        item = newItem;
        Refresh();
    }
    private void Refresh()
    {
        if(item == null) return;
        if(nameText != null) nameText.text = item.itemName;
        if(descriptionText != null)
        {
            descriptionText.text = item.description + "\n" + BuildStatsText();
        }
    }
    private string BuildStatsText()
    {
        var sb = new StringBuilder();
        foreach( var m in item.modifiers)
        {
            string val = m.op == ModOp.Multiply ? $"x{m.value}" : (m.value >= 0 ? $"+{m.value}" : m.value.ToString());
            sb.AppendLine($"{m.weapon}: {m.stat} {val}");
        }
        return sb.ToString();
    }

    public void Grant(GameObject interactor)
    {
        var mods = interactor.GetComponentInParent<WeaponModifierManager>();
        if (mods == null) return;

        foreach (var mod in item.modifiers)
            mods.AddModifier(mod);

        if (pickupParticles != null)
            Instantiate(pickupParticles, transform.position, Quaternion.identity);

        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);

        if (destroyAfterGrant)
            Destroy(gameObject);
    }
}