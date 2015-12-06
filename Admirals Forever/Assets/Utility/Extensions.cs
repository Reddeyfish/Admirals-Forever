using UnityEngine;
using System.Collections;

//single file for all utility extensions

//adds methods to add more options when switching transform parents

public static class TransformExtension
{

    public static void SetParent(this Transform transform, Transform parent, Vector3 newScale, bool worldPositionStays = false)
    {
        transform.SetParent(parent, worldPositionStays);
        transform.localScale = newScale;
    }

    public static void SetParentConstLocalScale(this Transform transform, Transform parent)
    {
        Vector3 localScale = transform.localScale;
        transform.SetParent(parent);
        transform.localScale = localScale;
    }

    public static Transform GetBaseParent(this Transform transform)
    {
        Transform result = transform;
        while (result.parent != null)
            result = result.parent;
        return result;
    }
}

//adds methods to ease conversion between world/screen space, and also with rotations

public static class VectorExtension
{
    public static Vector2 toWorldPoint(this Vector2 screenPoint)
    {
        return Camera.main.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, -Camera.main.transform.position.z));
    }

    public static Vector3 toWorldPoint(this Vector3 screenPoint)
    {
        return Camera.main.ScreenToWorldPoint(screenPoint);
    }

    public static float ToAngleRad(this Vector2 dir)
    {
        return Mathf.Atan2(dir.y, dir.x);
    }

    public static float ToAngle(this Vector2 dir)
    {
         return dir.ToAngleRad() * Mathf.Rad2Deg;
    }

    public static Quaternion ToRotation(this Vector2 dir)
    {
        return dir.ToRotation(Vector3.forward);
    }

    public static Quaternion ToRotation(this Vector2 dir, Vector3 axis)
    {
        return Quaternion.AngleAxis(dir.ToAngle(), axis);
    }

    public static Vector2 normal(this Vector2 dir)
    {
        return new Vector2(-dir.y, dir.x);
    }
}