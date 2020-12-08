using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve)
    {
        var width = heightMap.GetLength(0);
        var height = heightMap.GetLength(1);

        var meshData = new MeshData(width, height);
        var vertexIndex = 0;
        var topLeftX = (width - 1) / -2f;
        var topLeftZ = (height - 1) / 2f;

        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier, topLeftZ - y);
            meshData.uvs[vertexIndex] = new Vector2(x / (float) width, y / (float) height);
            if (x < width - 1 && y < height - 1)
            {
                meshData.AddTriangle(vertexIndex, vertexIndex + width + 1, vertexIndex + width);
                meshData.AddTriangle(vertexIndex + width + 1, vertexIndex, vertexIndex + 1);
            }

            vertexIndex++;
        }

        return meshData;
    }
}

public class MeshData
{
    private int triangleIndex;
    private readonly int[] triangles;
    public Vector2[] uvs;
    public Vector3[] vertices;

    public MeshData(int meshWidth, int meshHeight)
    {
        vertices = new Vector3[meshWidth * meshHeight];
        uvs = new Vector2[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }

    public Mesh CreateMesh()
    {
        var mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }
}