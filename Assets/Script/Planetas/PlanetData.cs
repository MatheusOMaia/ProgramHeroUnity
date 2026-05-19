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
        public int face;
        public int x;
        public int y;
        public int prefabIndex;
    }

    public List<TileOverride> manualOverrides;
}