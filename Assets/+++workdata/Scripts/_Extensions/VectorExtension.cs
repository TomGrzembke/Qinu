using UnityEngine;

public enum Axis
{
    X,
    Y,
    Z
}

public static class VectorExtension
{

    public static Vector3 NullZ(this Vector3 target)
    {
        target.z = 0;
        return target;
    }

    public static Vector2 RemoveZ(this Vector3 target)
    {
        return target;
    }

    public static Vector2 Add(this Vector3 target, float a, float b)
    {
        return target.RemoveZ() + new Vector2(a, b);
    }

    public static Vector3 Add(this Vector3 target, float a, float b, float c)
    {
        return target + new Vector3(a, b, c);
    }

    public static Vector2 Subtract(this Vector3 target, float a, float b)
    {
        return target.RemoveZ() - new Vector2(a, b);
    }

    public static Vector3 Subtract(this Vector3 target, float a, float b, float c)
    {
        return target - new Vector3(a, b, c);
    }

    public static Vector3 Round(this Vector3 target)
    {
        return new Vector3(Mathf.Ceil(target.x), Mathf.Ceil(target.y), Mathf.Ceil(target.z));
    }

    public static Vector2 RoundUp(this Vector2 target, float margin = 0)
    {
        if (target.x < margin && target.x > -margin)
            target.x = 0;

        if (target.y < margin && target.y > -margin)
            target.y = 0;


        if (target.x != 0)
            target.x = target.x > 0 ? Mathf.Ceil(target.x) : Mathf.Floor(target.x);

        if (target.y != 0)
            target.y = target.y > 0 ? Mathf.Ceil(target.y) : Mathf.Floor(target.y);

        return target;
    }

    public static Vector2 Clamp(this Vector2 target, float min, float max)
    {
        target.x = Mathf.Clamp(target.x, min, max);
        target.y = Mathf.Clamp(target.y, min, max);

        return target;
    }

    public static Vector3 Clamp(this Vector3 target, float min, float max)
    {
        target.x = Mathf.Clamp(target.x, min, max);
        target.y = Mathf.Clamp(target.y, min, max);
        target.z = Mathf.Clamp(target.z, min, max);

        return target;
    }

    public static Vector3 ChangeAxis(this Vector3 target, Axis axis, float value)
    {
        if (axis == Axis.X)
            target.x = value;
        else if (axis == Axis.Y)
            target.y = value;
        else if (axis == Axis.Z)
            target.z = value;

        return target;
    }

    public static Vector3 Add(this Vector3 target, Vector3 toAdd)
    {
        target = new(target.x + toAdd.x, target.y + toAdd.y, target.z + toAdd.z);
        return target;
    }

    public static bool IsBetween(this Vector2 target, Vector2 bottomLeft, Vector2 topRight)
    {
        Vector2 bottomRight = new(topRight.x, bottomLeft.y);
        Vector2 topLeft = new(bottomLeft.x, topRight.y);
        Vector2 nearestPoint = new();
        float nearestDistance = 100;

        CheckIfCloser(target, bottomLeft, ref nearestPoint, ref nearestDistance);
        CheckIfCloser(target, bottomRight, ref nearestPoint, ref nearestDistance);
        CheckIfCloser(target, topLeft, ref nearestPoint, ref nearestDistance);
        CheckIfCloser(target, topRight, ref nearestPoint, ref nearestDistance);

        if (nearestPoint == bottomLeft)
            if (target.x > nearestPoint.x && target.y > nearestPoint.y)
                return true;
            else
                return false;

        else if (nearestPoint == bottomRight)
            if (target.x < nearestPoint.x && target.y > nearestPoint.y)
                return true;
            else
                return false;

        else if (nearestPoint == topLeft)
            if (target.x > nearestPoint.x && target.y < nearestPoint.y)
                return true;
            else
                return false;

        else if (nearestPoint == topRight)
            if (target.x < nearestPoint.x && target.y < nearestPoint.y)
                return true;
            else
                return false;

        return false;
    }

    static void CheckIfCloser(Vector2 target, Vector2 point, ref Vector2 nearestPoint, ref float nearestDistance)
    {
        if (Vector2.Distance(target, point) < nearestDistance)
        {
            nearestDistance = Vector2.Distance(target, point);
            nearestPoint = point;
        }
    }
}