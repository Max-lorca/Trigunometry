using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponShoot : MonoBehaviour
{
    [SerializeField] public WeaponData currentWeapon;
    private SatoruChargeSystem chargeSystem;

    void Start()
    {
        chargeSystem = FindFirstObjectByType<SatoruChargeSystem>();
    }

    public void OnShoot(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && currentWeapon != null)
        {
            currentWeapon.TryShoot();
        }
    }

    public void OnHitEnemy()
    {
        if (chargeSystem != null)
            chargeSystem.AddCharge();
    }
}