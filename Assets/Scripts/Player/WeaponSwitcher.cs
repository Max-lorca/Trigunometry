using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponSwitcher : MonoBehaviour
{
    [SerializeField] private List<GameObject> weaponsPrefabs;
    [SerializeField] private Transform weaponSpawn;
    [SerializeField] private TextMeshPro weaponText;

    private GameObject actualWeaponInstance;
    private WeaponShoot weaponShootInput;
    private FadeController fadeController;

    // Guarda la corrutina del fade
    private Coroutine fadeCoroutine;

    private void Start()
    {
        weaponShootInput = GetComponent<WeaponShoot>();
        fadeController = GetComponent<FadeController>();

        Color c = weaponText.color;
        c.a = 0f;
        weaponText.color = c;

        EquipWeapon(0);
    }

    private void EquipWeapon(int index)
    {
        if (actualWeaponInstance != null)
        {
            Destroy(actualWeaponInstance);
        }

        actualWeaponInstance = Instantiate(
            weaponsPrefabs[index],
            weaponSpawn.position,
            weaponSpawn.rotation,
            weaponSpawn);

        weaponShootInput.currentWeapon = actualWeaponInstance.GetComponent<WeaponData>();
        actualWeaponInstance.SetActive(true);
    }

    private void CambiarTexto(string nuevoTexto)
    {
        // Si había un fade en curso, lo detenemos
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        // Hacemos visible el texto nuevamente
        Color c = weaponText.color;
        c.a = 1f;
        weaponText.color = c;

        // Cambiamos el texto
        weaponText.text = nuevoTexto;

        // Iniciamos un nuevo fade
        fadeCoroutine = StartCoroutine(
            fadeController.DesvanecimientoTexto(weaponText, 0f, 1f));
    }

    public void Switch1(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        EquipWeapon(0);
        CambiarTexto("Sin(x)");
    }

    public void Switch2(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        EquipWeapon(1);
        CambiarTexto("Cos(x)");
    }

    public void Switch3(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        EquipWeapon(2);
        CambiarTexto("Tan(x)");
    }
}