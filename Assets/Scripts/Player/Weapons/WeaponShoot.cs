using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponShoot : MonoBehaviour
{
    [SerializeField] public WeaponData currentWeapon;
    [SerializeField] private SatoruChargeSystem chargeSystem; // 👈 NUEVO

    public void OnShoot(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && currentWeapon != null)
        {
            currentWeapon.TryShoot();
        }
    }

    // 👈 NUEVO: Notificar impacto
    public void OnHitEnemy()
    {
        if (chargeSystem != null)
        {
            chargeSystem.AddCharge();
        }
    }
}