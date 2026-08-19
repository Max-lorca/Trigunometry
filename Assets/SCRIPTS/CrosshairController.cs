using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class CrosshairController : MonoBehaviour
{
    [SerializeField] private Camera cam;
    void Start()
    {
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        // Plano donde está el juego (Z = 0)
        Plane plane = new Plane(Vector3.forward, Vector3.zero);

        if(plane.Raycast(ray, out float distance))
        {
            Vector3 mouseWorld = ray.GetPoint(distance);
            transform.position = mouseWorld;
        }
    }
}
