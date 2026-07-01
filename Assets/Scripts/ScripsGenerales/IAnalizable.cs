using UnityEngine;

public interface IAnalizable
{
    Transform AnalysisTransform { get; }
    string FuncionTrigonometrica { get; }
    float AnguloGrados { get; }
    float ValorCorrecto { get; }

    void OnSeleccionado();
    void OnAnalisisExitoso(float multiplicadorDano);
    void OnAnalisisFallido();
    void OnDeseleccionado();
    void RecibirDanoAnalisis(float daño);
}