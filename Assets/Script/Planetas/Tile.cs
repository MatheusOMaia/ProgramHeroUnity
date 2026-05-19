using System.Collections.Generic;

public class Tile
{
    public int face;
    public int x;
    public int y;

    public string tileType = "ground";
    public object occupant = null;

    public List<CubeCoord> neighbors = new List<CubeCoord>();

    public Tile(int face, int x, int y)
    {
        this.face = face;
        this.x = x;
        this.y = y;
    }
}