using UnityEngine;

public class TurnIndicatorWorld : MonoBehaviour
{
    public float height = 1.4f;

    private Unit targetUnit;
    private CubePlanet planet;
    private Camera targetCamera;

    public void Setup(CubePlanet newPlanet, Camera newCamera)
    {
        planet = newPlanet;
        targetCamera = newCamera;
    }

    public void SetTarget(Unit unit)
    {
        targetUnit = unit;
        gameObject.SetActive(targetUnit != null);
    }

    private void LateUpdate()
    {
        if (targetUnit == null || planet == null)
            return;

        Vector3 normal = planet.GetFaceNormal(targetUnit.currentCoord.face).normalized;

        transform.position = targetUnit.transform.position + normal * height;

        FaceCamera(normal);
    }

    private void FaceCamera(Vector3 surfaceNormal)
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null)
            return;

        Vector3 toCamera = targetCamera.transform.position - transform.position;

        if (toCamera.sqrMagnitude < 0.001f)
            return;

        Vector3 forward = toCamera.normalized;

        Vector3 right = Vector3.Cross(surfaceNormal, forward);

        if (right.sqrMagnitude < 0.001f)
            right = targetCamera.transform.right;
        else
            right.Normalize();

        Vector3 visualUp = Vector3.Cross(forward, right).normalized;

        transform.rotation = Quaternion.LookRotation(
            -forward,
            visualUp
        );
    }
}