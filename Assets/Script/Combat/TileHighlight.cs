using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TileHighlight : MonoBehaviour
{
    public float size = 0.92f;
    public float lineWidth = 0.04f;

    private LineRenderer line;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();

        line.useWorldSpace = false;
        line.loop = true;
        line.positionCount = 4;

        line.startWidth = lineWidth;
        line.endWidth = lineWidth;

        line.numCornerVertices = 2;
        line.numCapVertices = 2;

        BuildSquare();
    }

    private void BuildSquare()
    {
        float h = size / 2f;

        line.SetPosition(0, new Vector3(-h, 0f, -h));
        line.SetPosition(1, new Vector3(h, 0f, -h));
        line.SetPosition(2, new Vector3(h, 0f, h));
        line.SetPosition(3, new Vector3(-h, 0f, h));
    }

    public void SetColor(Color color)
    {
        if (line == null)
            line = GetComponent<LineRenderer>();

        line.startColor = color;
        line.endColor = color;

        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = color;
        line.material = mat;
    }
}