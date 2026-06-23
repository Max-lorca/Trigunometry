using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class WeaponSwitcher : MonoBehaviour
{
    [SerializeField] private List<GameObject> weaponsPrefabs;
    [SerializeField] private Transform weaponSpawn;
    
    private GameObject actualWeaponInstance;
    private WeaponShoot weaponShootInput;
    void Start()
    {
        weaponShootInput = GetComponent<WeaponShoot>();
        EquipWeapon(0);
        actualWeaponInstance.SetActive(true);
    }
    private void EquipWeapon(int index)
    {
        if(actualWeaponInstance != null)
        {
            Destroy(actualWeaponInstance);
        }

        actualWeaponInstance = Instantiate(weaponsPrefabs[index], weaponSpawn.position, weaponSpawn.rotation, weaponSpawn);
        weaponShootInput.currentWeapon = actualWeaponInstance.GetComponent<WeaponData>();
        actualWeaponInstance.SetActive(true);
    }
    public void Switch1(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            EquipWeapon(0);
        }
    }
    public void Switch2(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            EquipWeapon(1);
        }
    }
    public void Switch3(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            EquipWeapon(2);
        }
    }
}
