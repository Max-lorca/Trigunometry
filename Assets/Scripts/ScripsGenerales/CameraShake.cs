using UnityEngine;
using Unity.Cinemachine;

public class CameraShake : MonoBehaviour
{
    [Header("Configuración default")]
    [SerializeField] private float duration, magnitude;
    [SerializeField] private CinemachineImpulseSource impulseSource;

    void Awake()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public void Shake(string type)
    {
        if (impulseSource == null) return;
        switch (type)
        {
            case "Default":
                impulseSource.ImpulseDefinition.ImpulseDuration = this.duration;
                impulseSource.GenerateImpulse(this.magnitude);
                break;
        }
    }
}