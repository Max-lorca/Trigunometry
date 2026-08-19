using JetBrains.Annotations;
using TMPro;
using UnityEngine;

public class PointsSystem : MonoBehaviour
{
    [SerializeField] private int points = 0;
    [SerializeField] private TextMeshPro pointText;
    public void AddPoints(int points)
    {
        this.points += points;
        ActualizarPuntaje();
    }
    public void ResetPoints()
    {
        points = 0;
        ActualizarPuntaje();
    }
    private void ActualizarPuntaje()
    {
        pointText.text = points.ToString();
    }
}
