using UnityEngine;
using Unity.Cinemachine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;
    [Header("Configuración default")]
    [SerializeField] private float duration, magnitude;
    [SerializeField] private float globalShakeForce = 1f; 
    [SerializeField] private CinemachineImpulseSource impulseSource;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
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
    public void CameraShakeImpulse(CinemachineImpulseSource impulseSource)
    {
        impulseSource.GenerateImpulseWithForce(globalShakeForce);
    }
}