using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponShoot : MonoBehaviour
{
    [SerializeField] public WeaponData currentWeapon;

    public void OnShoot(InputAction.CallbackContext ctx)
    {
        if(ctx.performed && currentWeapon != null)
        {
            currentWeapon.TryShoot();
        }
    }
}
