using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HouseBuilderExtent

{
    public static Vector3 Round(this Vector3 v, float gridSize, Vector3 prefabSize)
    {
        if (Mathf.Approximately(gridSize, 0.5f))
        {
            v.x = Mathf.Round(v.x / gridSize) * gridSize;
            v.z = Mathf.Round(v.z / gridSize) * gridSize;
        }
        else
        {
            float xOffset = Mathf.Abs(v.x) % gridSize;
            float zOffset = Mathf.Abs(v.z) % gridSize;

            float halfSizeX = prefabSize.x / 2;
            float halfSizeZ = prefabSize.z / 2;

            v.x = v.x >= 0 ? Mathf.Round(v.x - xOffset) + halfSizeX : Mathf.Round(v.x + xOffset) - halfSizeX;
            v.z = v.z >= 0 ? Mathf.Round(v.z - zOffset) + halfSizeZ : Mathf.Round(v.z + zOffset) - halfSizeZ;
        }

        return v;
    }

    public static float AtLeast(this float f, float min)
    {
        return Mathf.Max(f, min);
    }
}