using UnityEngine;

public class GrantModifiers : MonoBehaviour
{
    [SerializeField] private ModifierItemData item;
    [SerializeField] private GameObject pickupParticles;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private bool destroyAfterGrant = true;

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