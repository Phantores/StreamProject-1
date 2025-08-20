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
}
