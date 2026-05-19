using System;

[Serializable]
public struct CubeCoord : IEquatable<CubeCoord>
{
    public int face;
    public int x;
    public int y;

    public CubeCoord(int face, int x, int y)
    {
        this.face = face;
        this.x = x;
        this.y = y;
    }

    public bool Equals(CubeCoord other)
    {
        return face == other.face && x == other.x && y == other.y;
    }

    public override bool Equals(object obj)
    {
        return obj is CubeCoord other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(face, x, y);
    }
}