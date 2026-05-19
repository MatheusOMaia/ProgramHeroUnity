using System.Collections.Generic;
using UnityEngine;

public class CubeTopology : MonoBehaviour
{
    public enum Face
    {
        TOP,
        LEFT,
        FRONT,
        RIGHT,
        BACK,
        BOTTOM
    }

    public int size = 5;

    private Dictionary<CubeCoord, Tile> tiles = new();
    public IReadOnlyDictionary<CubeCoord, Tile> Tiles => tiles;

    private readonly string[] directions = { "up", "down", "left", "right" };

    // GERAR SUPERFÍCIE
    public void GenerateSurface(int newSize)
    {
        size = newSize;
        tiles.Clear();

        foreach (Face face in System.Enum.GetValues(typeof(Face)))
        {
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    var coord = new CubeCoord((int)face, x, y);
                    tiles[coord] = new Tile((int)face, x, y);
                }
            }
        }

        BuildNeighbors();
    }


    // NEIGHBORS
    void BuildNeighbors()
    {
        foreach (var kv in tiles)
        {
            var coord = kv.Key;
            kv.Value.neighbors = GetNeighbors(coord);
        }
    }

    List<CubeCoord> GetNeighbors(CubeCoord c)
    {
        List<CubeCoord> n = new();

        var checks = new Dictionary<string, Vector2Int>
        {
            { "up", new Vector2Int(c.x, c.y - 1) },
            { "down", new Vector2Int(c.x, c.y + 1) },
            { "left", new Vector2Int(c.x - 1, c.y) },
            { "right", new Vector2Int(c.x + 1, c.y) }
        };

        foreach (var dir in checks)
        {
            var p = dir.Value;

            if (p.x >= 0 && p.x < size && p.y >= 0 && p.y < size)
            {
                n.Add(new CubeCoord(c.face, p.x, p.y));
            }
            else
            {
                n.Add(WrapEdge(c, dir.Key));
            }
        }

        return n;
    }


    // WRAP EDGE (CUBO)
    CubeCoord WrapEdge(CubeCoord c, string dir)
    {
        int face = c.face;
        int x = c.x;
        int y = c.y;

        switch ((Face)face)
        {
            case Face.FRONT:
                if (dir == "up") return new CubeCoord((int)Face.BOTTOM, x, size - 1);
                if (dir == "down") return new CubeCoord((int)Face.TOP, x, 0);
                if (dir == "left") return new CubeCoord((int)Face.LEFT, 0, y);
                if (dir == "right") return new CubeCoord((int)Face.RIGHT, size - 1, y);
                break;

            case Face.BACK:
                if (dir == "up") return new CubeCoord((int)Face.BOTTOM, size - 1 - x, 0);
                if (dir == "down") return new CubeCoord((int)Face.TOP, size - 1 - x, size - 1);
                if (dir == "left") return new CubeCoord((int)Face.RIGHT, 0, y);
                if (dir == "right") return new CubeCoord((int)Face.LEFT, size - 1, y);
                break;

            case Face.LEFT:
                if (dir == "up") return new CubeCoord((int)Face.BOTTOM, 0, size - 1 - x);
                if (dir == "down") return new CubeCoord((int)Face.TOP, 0, x);
                if (dir == "left") return new CubeCoord((int)Face.FRONT, 0, y);
                if (dir == "right") return new CubeCoord((int)Face.BACK, size - 1, y);
                break;

            case Face.RIGHT:
                if (dir == "up") return new CubeCoord((int)Face.BOTTOM, size - 1, x);
                if (dir == "down") return new CubeCoord((int)Face.TOP, size - 1, size - 1 - x);
                if (dir == "left") return new CubeCoord((int)Face.BACK, 0, y);
                if (dir == "right") return new CubeCoord((int)Face.FRONT, size - 1, y);
                break;

            case Face.TOP:
                if (dir == "up") return new CubeCoord((int)Face.FRONT, x, size - 1);
                if (dir == "down") return new CubeCoord((int)Face.BACK, size - 1 - x, size - 1);
                if (dir == "left") return new CubeCoord((int)Face.LEFT, y, size - 1);
                if (dir == "right") return new CubeCoord((int)Face.RIGHT, size - 1 - y, size - 1);
                break;

            case Face.BOTTOM:
                if (dir == "up") return new CubeCoord((int)Face.BACK, size - 1 - x, 0);
                if (dir == "down") return new CubeCoord((int)Face.FRONT, x, 0);
                if (dir == "left") return new CubeCoord((int)Face.LEFT, size - 1 - y, 0);
                if (dir == "right") return new CubeCoord((int)Face.RIGHT, y, 0);
                break;
        }

        Debug.LogError($"Wrap inválido: face={face} dir={dir}");
        return c;
    }


    // BFS PATHFINDING
    public List<CubeCoord> ShortestPath(CubeCoord start, CubeCoord target)
    {
        var queue = new Queue<CubeCoord>();
        var cameFrom = new Dictionary<CubeCoord, CubeCoord?>();

        queue.Enqueue(start);
        cameFrom[start] = null;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current.Equals(target))
                break;

            foreach (var n in tiles[current].neighbors)
            {
                if (!cameFrom.ContainsKey(n))
                {
                    cameFrom[n] = current;
                    queue.Enqueue(n);
                }
            }
        }

        if (!cameFrom.ContainsKey(target))
            return new List<CubeCoord>();

        return ReconstructPath(start, target, cameFrom);
    }

    List<CubeCoord> ReconstructPath(
        CubeCoord start,
        CubeCoord goal,
        Dictionary<CubeCoord, CubeCoord?> cameFrom)
    {
        List<CubeCoord> path = new();
        var current = goal;

        while (!current.Equals(start))
        {
            path.Insert(0, current);
            current = cameFrom[current].Value;
        }

        return path;
    }
}