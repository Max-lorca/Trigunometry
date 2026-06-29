using UnityEngine;

/// <summary>
/// Cualquier enemigo que pueda ser "analizado" en el modo Análisis
/// (pausa de tiempo) debe implementar esta interfaz.
/// </summary>
public interface IAnalizable
{
    /// <summary>Transform usado por la UI/cámara/arma para apuntar al enemigo.</summary>
    Transform AnalysisTransform { get; }

    /// <summary>"sin", "cos" o "tan" — qué función se le pregunta al jugador.</summary>
    string FuncionTrigonometrica { get; }

    /// <summary>Ángulo del enemigo en grados (el dato visible para el jugador).</summary>
    float AnguloGrados { get; }

    /// <summary>Valor correcto que el jugador debe ingresar (ej: Mathf.Sin(angulo * Mathf.Deg2Rad)).</summary>
    float ValorCorrecto { get; }

    /// <summary>Se llama cuando el jugador hace clic y lo selecciona (para resaltar visualmente).</summary>
    void OnSeleccionado();

    /// <summary>Se llama cuando se deselecciona (por éxito, fallo, o timeout).</summary>
    void OnDeseleccionado();

    /// <summary>
    /// El jugador respondió correctamente dentro del tiempo límite.
    /// El propio enemigo es responsable de aplicar el daño bonus y
    /// forzar el drop garantizado (100%) de 2 vidas.
    /// </summary>
    void OnAnalisisExitoso(float multiplicadorDano);

    /// <summary>El jugador respondió mal o se le acabó el tiempo.</summary>
    void OnAnalisisFallido();

    /// <summary>
    /// El disparo bonus del modo Análisis impactó: el enemigo debe reenviar
    /// este daño a su propio sistema de vida (ej: llamando a TomarDaño internamente).
    /// </summary>
    void RecibirDanoAnalisis(float dano);
}