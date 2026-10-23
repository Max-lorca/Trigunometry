using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponShoot : MonoBehaviour
{
    [SerializeField] public WeaponData currentWeapon;  // vuelve a WeaponData
    [SerializeField] private Tan_x shield;
    private SatoruChargeSystem chargeSystem;

    void Start()
    {
        chargeSystem = FindFirstObjectByType<SatoruChargeSystem>();
    }

    public void OnShoot(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || currentWeapon == null) return;
        if (shield != null && shield.IsBlocking) return;   // bloqueando = no dispara
        currentWeapon.TryShoot();
    }

    public void OnBlock(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && shield != null)
            shield.TryBlock();
    }

    public void OnHitEnemy()
    {
        if (chargeSystem != null)
            chargeSystem.AddCharge();
    }
}