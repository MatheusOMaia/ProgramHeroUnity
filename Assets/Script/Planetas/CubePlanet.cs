using UnityEngine;
using System.Collections.Generic;

public class CubePlanet : MonoBehaviour
{
    public PlanetData planetData;

    private CubeTopology topology;

    private System.Random rng;

    private Dictionary<CubeCoord, int> overrideMap = new();

    private Dictionary<CubeCoord, GameObject> tileObjects = new();

    void Start()
    {
        if (planetData == null)
        {
            Debug.LogError("PlanetData não atribuído.");
            return;
        }

        rng = (planetData.randomSeed != 0)
            ? new System.Random(planetData.randomSeed)
            : new System.Random();

        BuildOverrideMap();

        topology = gameObject.AddComponent<CubeTopology>();
        topology.GenerateSurface(planetData.size);

        GenerateVisualPlanet();

        Debug.Log($"Planeta gerado: {planetData.planetName}");
    }

    // =========================
    // GERAR PLANETA
    // =========================
    void GenerateVisualPlanet()
    {
        foreach (var kv in topology.Tiles)
        {
            CubeCoord coord = kv.Key;

            GameObject prefab = GetPrefabForCoord(coord);

            if (prefab == null)
            {
                Debug.LogError($"Sem prefab para {coord.face},{coord.x},{coord.y}");
                continue;
            }

            Vector3 normal = FaceNormal(coord.face);
            float heightOffset = GetTileHeightOffset(coord);
            bool isDark = IsDarkChecker(coord);

            Vector3 pos = FaceToWorld(coord) + normal * heightOffset;

            GameObject tile = Instantiate(prefab, pos, Quaternion.identity, transform);

            tile.transform.rotation = GetRotation(coord.face);

            var view = tile.GetComponent<TileView>();
            if (view != null)
                view.Init(coord);
            view.SetupVisual(isDark, planetData.checkerDarkenAmount);
            tileObjects[coord] = tile;
        }
    }


    // PREFAB SELECTION
    GameObject GetPrefabForCoord(CubeCoord coord)
    {
        if (planetData.tilePrefabs.Count == 0)
            return null;

        if (overrideMap.ContainsKey(coord))
        {
            int index = overrideMap[coord];
            return planetData.tilePrefabs[Mathf.Clamp(index, 0, planetData.tilePrefabs.Count - 1)];
        }

        if (planetData.useRandomTiles)
        {
            int i = rng.Next(planetData.tilePrefabs.Count);
            return planetData.tilePrefabs[i];
        }

        return planetData.tilePrefabs[0];
    }

    void BuildOverrideMap()
    {
        overrideMap.Clear();

        foreach (var o in planetData.manualOverrides)
        {
            var coord = new CubeCoord(o.face, o.x, o.y);
            overrideMap[coord] = o.prefabIndex;
        }
    }

    // POSIÇÃO
    Vector3 FaceToWorld(CubeCoord c)
    {
        int s = planetData.size - 1;

        switch ((CubeTopology.Face)c.face)
        {
            case CubeTopology.Face.FRONT:
                return new Vector3(c.x, c.y, 0);

            case CubeTopology.Face.BACK:
                return new Vector3(s - c.x, c.y, s);

            case CubeTopology.Face.LEFT:
                return new Vector3(0, c.y, c.x);

            case CubeTopology.Face.RIGHT:
                return new Vector3(s, c.y, s - c.x);

            case CubeTopology.Face.TOP:
                return new Vector3(c.x, s, c.y);

            case CubeTopology.Face.BOTTOM:
                return new Vector3(c.x, 0, s - c.y);
        }

        return Vector3.zero;
    }

    Quaternion GetRotation(int face)
    {
        switch ((CubeTopology.Face)face)
        {
            case CubeTopology.Face.BOTTOM:
                return Quaternion.Euler(180, 0, 0);

            case CubeTopology.Face.FRONT:
                return Quaternion.Euler(-90, 0, 0);

            case CubeTopology.Face.BACK:
                return Quaternion.Euler(90, 0, 0);

            case CubeTopology.Face.LEFT:
                return Quaternion.Euler(0, 0, 90);

            case CubeTopology.Face.RIGHT:
                return Quaternion.Euler(0, 0, -90);
        }

        return Quaternion.identity;
    }

    Vector3 FaceNormal(int face)
    {
        switch ((CubeTopology.Face)face)
        {
            case CubeTopology.Face.FRONT: return Vector3.back;
            case CubeTopology.Face.BACK: return Vector3.forward;
            case CubeTopology.Face.LEFT: return Vector3.left;
            case CubeTopology.Face.RIGHT: return Vector3.right;
            case CubeTopology.Face.TOP: return Vector3.up;
            case CubeTopology.Face.BOTTOM: return Vector3.down;
        }

        return Vector3.up;
    }

    // VISUAL VARIAÇÕES
    bool IsDarkChecker(CubeCoord c)
    {
        return ((c.x + c.y) % 2) == 1;
    }

    float GetTileHeightOffset(CubeCoord c)
    {
        if (planetData.heightVariation <= 0)
            return 0;

        int seed = planetData.heightSeed
            + c.face * 73856093
            + c.x * 19349663
            + c.y * 83492791;

        System.Random r = new System.Random(seed);

        return (float)r.NextDouble() * planetData.heightVariation;
    }


    public Vector3 GetTileWorldPosition(CubeCoord c)
    {
        return FaceToWorld(c) + FaceNormal(c.face) * GetTileHeightOffset(c);
    }

    public GameObject GetTileObject(CubeCoord c)
    {
        return tileObjects[c];
    }

    public int GetPlanetSize()
    {
        if (planetData == null)
            return 1;

        return planetData.size;
    }


    public Vector3 GetPlanetCenterLocal()
    {
        if (planetData == null)
            return Vector3.zero;

        float center = (planetData.size - 1) / 2f;

        return new Vector3(
            center,
            center,
            center
        );
    }


    public Vector3 GetPlanetCenterWorld()
    {
        return transform.TransformPoint(GetPlanetCenterLocal());
    }
}