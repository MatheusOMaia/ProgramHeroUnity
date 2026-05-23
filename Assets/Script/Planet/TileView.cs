using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class TileView : MonoBehaviour
{
    public CubeCoord coord;

    public CubeCoord Coord => coord;

    private Renderer rend;
    private Material materialInstance;

    private BoxCollider boxCollider;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        boxCollider = GetComponent<BoxCollider>();

        materialInstance = rend.material;
    }

    public void Init(CubeCoord c)
    {
        coord = c;
    }

    public void SetupVisual(bool isDark, float darkenAmount)
    {
        if (materialInstance == null)
            return;

        if (isDark)
        {
            float factor = 1f - darkenAmount;

            materialInstance.color = new Color(
                factor,
                factor,
                factor,
                1f
            );
        }
        else
        {
            materialInstance.color = Color.white;
        }
    }

    public Vector3 GetTopCenterWorld()
    {
        if (boxCollider != null)
        {
            Vector3 localTop = boxCollider.center + Vector3.up * (boxCollider.size.y * 0.5f);
            return transform.TransformPoint(localTop);
        }

        if (rend != null)
        {
            return transform.position + transform.up * (rend.bounds.size.y * 0.5f);
        }

        return transform.position + transform.up * 0.5f;
    }

    public Quaternion GetSurfaceRotation()
    {
        return transform.rotation;
    }

    public Vector3 GetSurfaceNormal()
    {
        return transform.up;
    }

    public void GetHighlightPose(
        float highlightHeight,
        float gap,
        out Vector3 position,
        out Quaternion rotation
    )
    {
        Vector3 normal = GetSurfaceNormal();

        position = GetTopCenterWorld() + normal * (highlightHeight * 0.5f + gap);
        rotation = GetSurfaceRotation();
    }
}