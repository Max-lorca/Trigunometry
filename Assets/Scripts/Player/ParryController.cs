using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ParryController : MonoBehaviour
{
    [Header("Parry Configuración")] 
    [SerializeField] private float parryRad = 1.5f;
    [SerializeField] private float parryTime = 0.2f;
    [SerializeField] private float parryCooldown = 1f;

    [SerializeField] private Vector3 offset;
    // Le cambiamos el nombre para que quede claro que busca ataques enemigos
    [SerializeField] private LayerMask capaAtaquesEnemigos; 

    private bool canParry = true;
    
    // Corregido: Esto es una propiedad útil para que otros scripts sepan si está en modo parry
    public bool IsParrying { get; private set; } 
    
    private List<Collider2D> _hits = new List<Collider2D>();
    private Transform _puntoParry;
    private ContactFilter2D _filter;

    private void Start()
    {
        _puntoParry = this.transform;

        // Configurar el filtro una sola vez en el Start ahorra rendimiento
        _filter = new ContactFilter2D();
        _filter.SetLayerMask(capaAtaquesEnemigos);
        _filter.useLayerMask = true;
    }

    public void TryParry()
    {
        // CORRECCIÓN: Debe poder hacer parry si canParry es TRUE, no false.
        // Además evitamos que lo use si ya está haciendo un parry activo.
        if (canParry && !IsParrying)
        {
            StartCoroutine(ParryRoutine());
        }
    }

    private IEnumerator ParryRoutine()
    {
        canParry = false;
        IsParrying = true;

        // Limpiamos la lista anterior para evitar residuos del frame pasado
        _hits.Clear();

        // Realizamos el OverlapCircle sin generar basura en memoria
        int hitsCount = Physics2D.OverlapCircle(_puntoParry.position + offset, parryRad, _filter, _hits);

        // CORRECCIÓN: El bucle debe recorrer la cantidad real de impactos (hitsCount)
        for (int i = 0; i < hitsCount; i++)
        {
            // Seguridad: Si por alguna razón el elemento es nulo, lo saltamos
            if (_hits[i] == null) continue;

            switch (_hits[i].gameObject.tag)
            {
                case "Proyectil":
                    Debug.Log("¡Parry a un Proyectil!");
                    // Aquí destruyes el proyectil o le cambias de dirección
                    // Destroy(_hits[i].gameObject); 
                    break; 
                    
                case "Ataque":
                    Debug.Log("¡Parry a un Ataque Melee!");
                    // Aquí buscas el script del enemigo para interrumpirlo:
                    // Enemigo ataque = _hits[i].GetComponentInParent<Enemigo>();
                    // if(ataque != null) ataque.Interrumpir();
                    break;
            }
        }

        // El "tiempo activo" del parry suele ser corto (ej: 0.1s o un par de frames), 
        // pero por ahora desactivamos el estado de parry aquí.
        yield return new WaitForSeconds(parryTime); 
        IsParrying = false;

        // Esperamos el resto del cooldown para poder usarlo otra vez
        yield return new WaitForSeconds(parryCooldown - parryTime);
        canParry = true;
    }    

    // Para que puedas medir y calibrar el tamaño del Parry en la pestaña Scene
    private void OnDrawGizmosSelected()
    {
        if (_puntoParry == null) _puntoParry = this.transform;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(_puntoParry.position, parryRad);
    }
}