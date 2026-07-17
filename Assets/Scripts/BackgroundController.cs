using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    [HideInInspector] public float backgroundXVelocity;
    private Transform _actualPosition;

    private void Start()
    {
        _actualPosition = this.transform;
    }
    private void Update()
    {
        transform.position = new Vector3(_actualPosition.position.x * backgroundXVelocity, _actualPosition.position.y,
            _actualPosition.position.z) * Time.deltaTime;
    }
}
