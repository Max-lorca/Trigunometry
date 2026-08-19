using UnityEngine;

/// <summary>
/// Contrato que debe cumplir cualquier enemigo "analizable" por el modo Satoru.
/// Simplificada: se quitaron ValorCorrecto y OnAnalisisFallido porque pertenecían
/// al viejo sistema de responder con un valor numérico (AnalysisModeController),
/// que ya no existe. Si en el futuro se reintroduce algún tipo de "fallo" al
/// disparar el arma incorrecta, se puede volver a agregar OnAnalisisFallido().
/// </summary>

public interface IAnalizable
{
    Transform AnalysisTransform { get; }
    string FuncionTrigonometrica { get; }
    float AnguloGrados { get; }

    // Pendiente de reconectar (outline con shader) - ver EnemyBase
    void OnSeleccionado();
    void OnDeseleccionado();

    void OnAnalisisExitoso(float multiplicadorDano);
    void RecibirDanoAnalisis(float daño);
}