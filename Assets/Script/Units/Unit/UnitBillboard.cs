using UnityEngine;

public class UnitBillboard : MonoBehaviour
{
    [Header("References")]
    public Unit unit;
    public CubePlanet planet;
    public Transform visualPivot;
    public SpriteRenderer spriteRenderer;
    public Camera targetCamera;

    [Header("Sprite")]
    public float spriteWorldHeight = 1.2f;
    public bool autoSetupSpriteHeight = true;

    private void Awake()
    {
        if (unit == null)
            unit = GetComponent<Unit>();

        if (visualPivot == null)
            visualPivot = transform.Find("VisualPivot");

        if (spriteRenderer == null && visualPivot != null)
            spriteRenderer = visualPivot.GetComponentInChildren<SpriteRenderer>();

        if (targetCamera == null)
            targetCamera = Camera.main;

        if (planet == null)
            planet = FindFirstObjectByType<CubePlanet>();

        SetupSpritePivot();
    }

    private void LateUpdate()
    {
        UpdateBillboard();
    }

    private void SetupSpritePivot()
    {
        if (!autoSetupSpriteHeight)
            return;

        if (spriteRenderer == null)
            return;

        if (spriteRenderer.sprite == null)
            return;

        float currentHeight = spriteRenderer.sprite.bounds.size.y;

        if (currentHeight <= 0f)
            return;

        float scale = spriteWorldHeight / currentHeight;

        spriteRenderer.transform.localScale = Vector3.one * scale;

        // Como o pivot visual está no pé, o sprite sobe metade da altura.
        spriteRenderer.transform.localPosition = Vector3.up * (spriteWorldHeight / 2f);
    }

    private void UpdateBillboard()
    {
        if (unit == null || visualPivot == null || targetCamera == null || planet == null)
            return;

        Vector3 up = planet.GetFaceNormal(unit.currentCoord.face).normalized;

        Vector3 toCamera = targetCamera.transform.position - visualPivot.position;

        if (toCamera.sqrMagnitude < 0.001f)
            return;

        Vector3 forward = toCamera.normalized;

        Vector3 right = Vector3.Cross(up, forward);

        if (right.sqrMagnitude < 0.001f)
            right = targetCamera.transform.right;
        else
            right.Normalize();

        Vector3 visualUp = Vector3.Cross(forward, right).normalized;

        Quaternion rotation = Quaternion.LookRotation(
            -forward,
            visualUp
        );

        visualPivot.rotation = rotation;
    }
}