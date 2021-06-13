using UnityEngine;

public class Position
{
    private Position()
    {
    }

    public Position(float _x, float _y)
    {
        x = _x;
        y = _y;
    }
    public Position(Vector2 _pos)
    {
        x = _pos.x;
        y = _pos.y;
    }

    public float x;
    public float y;

    /// <summary>
    /// ģ
    /// </summary>
    public float Magnitude
    {
        get
        {
            float X = x * x;
            float Y = y * y;
            return Mathf.Sqrt(X + Y);
        }
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, 0);
    }

    public Vector2 ToVector2()
    {
        return new Vector2(x, y);
    }

    public PositionInt ToPositionInt()
    {
        int ix = 0;
        int iy = 0;
        if (x > 0)
        {
            ix = Mathf.CeilToInt(x / 0.5f) / 2;
        }
        else if (x < 0)
        {
            ix = Mathf.FloorToInt(x / 0.5f) / 2;
        }

        if (y > 0)
        {
            iy = Mathf.CeilToInt(y / 0.5f) / 2;
        }
        else if (y < 0)
        {
            iy = Mathf.FloorToInt(y / 0.5f) / 2;
        }
        return new PositionInt(ix, iy);
    }

    public static float Distance(Position p1, Position p2)
    {
        Position v = p1 - p2;
        return v.Magnitude;
    }


    public static Position operator +(Position a, Position b)
    {
        Position pos = new Position();
        pos.x = a.x + b.x;
        pos.y = a.y + b.y;
        return pos;
    }

    public static Position operator -(Position a, Position b)
    {
        Position pos = new Position();
        pos.x = a.x - b.x;
        pos.y = a.y - b.y;
        return pos;
    }

    public static bool operator ==(Position a, Position b)
    {
        if (a is null && b is null)
        {
            return true;
        }
        if (!(a is null) && b is null)
        {
            return false;
        }
        if (a is null && !(b is null))
        {
            return false;
        }
        if (a.x == b.x && a.y == b.y) return true;
        return false;
    }

    public static bool operator !=(Position a, Position b)
    {
        if (a is null && b is null)
        {
            return false;
        }
        if (!(a is null) && b is null)
        {
            return true;
        }
        if (a is null && !(b is null))
        {
            return true;
        }
        if (a.x == b.x && a.y == b.y) return false;
        return true;
    }

    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        if (obj.GetType() != GetType()) return false;
        Position pos = obj as Position;
        if (x == pos.x && y == pos.y) return true;
        return false;
    }

    public override int GetHashCode()
    {
        return x.GetHashCode() ^ y.GetHashCode();
    }

    public override string ToString()
    {
        return "x:" + x + " : y:" + y;
    }
}
