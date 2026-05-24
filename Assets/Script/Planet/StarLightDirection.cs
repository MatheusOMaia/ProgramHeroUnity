using UnityEngine;

public class DirectionalLightFromStar : MonoBehaviour
{
    public Transform star;
    public Transform planetCenter;

    public bool invertDirection = false;

    private void Start()
    {
        AlignLight();
    }

    [ContextMenu("Align Light")]
    public void AlignLight()
    {
        if (star == null || planetCenter == null)
            return;

        Vector3 direction = planetCenter.position - star.position;

        if (invertDirection)
            direction = -direction;

        transform.rotation = Quaternion.LookRotation(direction.normalized);
    }
}