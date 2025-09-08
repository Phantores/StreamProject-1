using UnityEngine;

public static class VectorUtils
{
    public static Vector3 ReadPlanar(Vector2 input, bool y = true)
    {
        Vector3 output = Vector3.zero;
        if (!y){
            output = new Vector3(input.x, input.y, 0f);
        }
        else
        {
            output = new Vector3(input.x, 0f, input.y);
        }
        return output;
    }

    public static bool CappedSphereCheck(Vector3 target, Vector3 origin, float radius, float height)
    {
        Vector3 result = target - origin;
        if(result.sqrMagnitude >= radius*radius)
        {
            return true;
        } else if(Mathf.Abs(result.y) >= height)
        {
            return true;
        }

        return false;
    }

    public static Vector3 CappedSphereNormalize(Vector3 target, Vector3 origin, float radius, float height)
    {
        Vector3 result = target - origin;
        
        if( result.sqrMagnitude >= radius*radius)
        {
            result.Normalize();
            result *= radius;
        } else if(Mathf.Abs(result.y) >= height)
        {
            result.y = Mathf.Sign(result.y) * height;
        }

        return result;
    }

    public static Vector3 CappedSphereNormalize(Vector3 target, float radius, float height)
    {
        Vector3 result = target;

        if (result.sqrMagnitude >= radius * radius)
        {
            result.Normalize();
            result *= radius;
        }
        else if (Mathf.Abs(result.y) >= height)
        {
            result.y = Mathf.Sign(result.y) * height;
        }

        return result;
    }
}
