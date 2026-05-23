using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class TileView : MonoBehaviour
{
    public CubeCoord coord;
    public CubeCoord Coord => coord;

    private Renderer rend;
    private Material materialInstance;

    void Awake()
    {
        rend = GetComponent<Renderer>();

        // Cria instância única do material (igual ao duplicate do Godot)
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
}