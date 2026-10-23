using System;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponSwitcher : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private List<GameObject> weaponsPrefabs;
    [SerializeField] private Transform weaponSpawn;
    [SerializeField] private TextMeshPro weaponText;
    [Header("Audio")]
    [SerializeField] private AudioClip switchWeaponAudio;
    private int actualIndex = 0;
    private AudioSource audioSource;
    private GameObject actualWeaponInstance;
    private WeaponShoot weaponShootInput;
    private FadeController fadeController;
    private Coroutine fadeCoroutine;
    private static readonly string[] analysisNames = { "Seno", "Coseno", "Tangente"};
    private static readonly string[] displayNames = {"Sin(x)", "Cos(x)", "Tan(x)"};
    private int analysisIndex = 0;

    void Start()
    {
        weaponShootInput = GetComponent<WeaponShoot>();
        fadeController = GetComponent<FadeController>();
        audioSource = GetComponent<AudioSource>();

        Color c = weaponText.color;
        c.a = 0f;
        weaponText.color = c;

        EquipWeapon(0);
    }

    private void EquipWeapon(int index)
    {
        if (actualWeaponInstance != null)
            Destroy(actualWeaponInstance);

        actualIndex = index;

        actualWeaponInstance = Instantiate(weaponsPrefabs[index], weaponSpawn.position, weaponSpawn.rotation, weaponSpawn);
        weaponShootInput.currentWeapon = actualWeaponInstance.GetComponent<WeaponData>();
        audioSource.PlayOneShot(switchWeaponAudio);
        actualWeaponInstance.SetActive(true);
    }

    private void CambiarTexto(string nuevoTexto)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        Color c = weaponText.color;
        c.a = 1f;
        weaponText.color = c;
        weaponText.text = nuevoTexto;

        fadeCoroutine = StartCoroutine(fadeController.DesvanecimientoTexto(weaponText, 0f, 1f));
    }

    // ==========================================
    // SWITCH CON INTEGRACIÓN AL MODO SATORU
    // ==========================================

    public void Switch1(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        // ✅ SI ESTÁ EN MODO SATORU → seleccionar arma para el PowerShot
        PowerShotSystem ps = FindFirstObjectByType<PowerShotSystem>();
        if (ps != null)
        {
            // Verificar si el Modo Satoru está activo
            if (ps.IsSatoruActive())
            {
                ps.SeleccionarArma("Seno");
                CambiarTexto("Seno");
                return;
            }
        }

        // ✅ FUERA DEL MODO SATORU → cambiar arma normalmente
        EquipWeapon(0);
        CambiarTexto("Sin(x)");
    }

    public void Switch2(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        PowerShotSystem ps = FindFirstObjectByType<PowerShotSystem>();
        if (ps != null && ps.IsSatoruActive())
        {
            ps.SeleccionarArma("Coseno");
            CambiarTexto("Coseno");
            return;
        }

        EquipWeapon(1);
        CambiarTexto("Cos(x)");
    }

    public void Switch3(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        PowerShotSystem ps = FindFirstObjectByType<PowerShotSystem>();
        if (ps != null && ps.IsSatoruActive())
        {
            ps.SeleccionarArma("Tangente");
            CambiarTexto("Tangente");
            return;
        }
    }
    
    public void SwitchRightGamepad(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) CycleWeapon(1);
    }

    public void SwitchLeftGamepad(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) CycleWeapon(-1);
    }

    private void CycleWeapon(int dir)
    {
        PowerShotSystem ps = FindFirstObjectByType<PowerShotSystem>();

        if (ps != null && ps.IsSatoruActive())
        {
            analysisIndex = (analysisIndex + dir + analysisNames.Length) % analysisNames.Length;
            ps.SeleccionarArma(analysisNames[analysisIndex]);
            CambiarTexto(analysisNames[analysisIndex]);
            return;
        }

        int count = weaponsPrefabs.Count;
        int next = (actualIndex + dir + count) % count;
        EquipWeapon(next);
        CambiarTexto(displayNames[next]);
    }
}