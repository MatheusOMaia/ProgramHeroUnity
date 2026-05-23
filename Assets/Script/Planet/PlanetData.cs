using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "CubePlanet/PlanetData")]
public class PlanetData : ScriptableObject
{
    public string planetName = "Planet";

    public int size = 5;

    public int randomSeed = 0;

    public bool useRandomTiles = false;

    public float checkerDarkenAmount = 0.2f;

    public float heightVariation = 0.08f;
    public int heightSeed = 12345;

    public List<GameObject> tilePrefabs;

    [System.Serializable]
    public class TileOverride
    {
        public CubeTopology.Face face;
        public int x;
        public int y;
        public int prefabIndex;
    }

    public List<TileOverride> manualOverrides;

    [System.Serializable]
    public class UnitSpawn
    {
        public GameObject unitPrefab;

        public int face;
        public int x;
        public int y;

        public Unit.Team team = Unit.Team.Enemy;
    }
    
    public List<UnitSpawn> unitSpawns = new();
}