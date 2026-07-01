using UnityEngine;
using UnityEngine.InputSystem;

public class PowerShotInput : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PowerShotSystem powerShotSystem;
    [SerializeField] private WeaponShoot weaponShoot;

    void Awake()
    {
        if (powerShotSystem == null)
            powerShotSystem = GetComponent<PowerShotSystem>();

        if (weaponShoot == null)
            weaponShoot = GetComponent<WeaponShoot>();
    }

    public void OnPowerShot(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        // Intentar disparo potente primero
        if (powerShotSystem != null && powerShotSystem.TryPowerShot())
        {
            return;
        }

        // Disparo normal
        if (weaponShoot != null)
        {
            weaponShoot.OnShoot(ctx);
        }
    }
}