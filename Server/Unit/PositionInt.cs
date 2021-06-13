using UnityEngine;

public class PositionInt
{
    private PositionInt()
    {
    }

    public PositionInt(int _x, int _y)
    {
        x = _x;
        y = _y;
    }
    public PositionInt(Vector2Int _pos)
    {
        x = _pos.x;
        y = _pos.y;
    }

    public int x;
    public int y;

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

    public Position ToPosition()
    {
        return new Position(x, y);
    }

    public static float Distance(PositionInt p1, PositionInt p2)
    {
        PositionInt v = p1 - p2;
        return v.Magnitude;
    }


    public static PositionInt operator +(PositionInt a, PositionInt b)
    {
        PositionInt pos = new PositionInt();
        pos.x = a.x + b.x;
        pos.y = a.y + b.y;
        return pos;
    }

    public static PositionInt operator -(PositionInt a, PositionInt b)
    {
        PositionInt pos = new PositionInt();
        pos.x = a.x - b.x;
        pos.y = a.y - b.y;
        return pos;
    }

    public static bool operator ==(PositionInt a, PositionInt b)
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

    public static bool operator !=(PositionInt a, PositionInt b)
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
        PositionInt pos = obj as PositionInt;
        if (x == pos.x && y == pos.y) return true;
        return false;
    }

    public override int GetHashCode()
    {
        return x.GetHashCode() ^ y.GetHashCode();
    }

    public override string ToString()
    {
        return "x_" + x + ":y_" + y;
    }
}
