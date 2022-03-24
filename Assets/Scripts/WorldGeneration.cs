using UnityEngine;

public class WorldGeneration
{
    public static float size = 1000f;
    public static Vector3 GetCenterPoint()
    {
        return new Vector3(size / 2f, 0f, size / 2f);
    }
}
