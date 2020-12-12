using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve,
        int levelOfDetail)
    {
        AnimationCurve newHeightCurve = new AnimationCurve(heightCurve.keys);
        var width = heightMap.GetLength(0);
        var height = heightMap.GetLength(1);

        var vertexIndex = 0;
        var topLeftX = (width - 1) / -2f;
        var topLeftZ = (height - 1) / 2f;

        var meshSimplificationIncrement = levelOfDetail == 0 ? 1 : levelOfDetail * 2;
        var verticesPerLine = (width - 1) / meshSimplificationIncrement + 1;
        var meshData = new MeshData(verticesPerLine, verticesPerLine);

        for (var y = 0; y < height; y += meshSimplificationIncrement)
        for (var x = 0; x < width; x += meshSimplificationIncrement)
        {
            meshData.vertices[vertexIndex] = new Vector3(topLeftX + x,
                newHeightCurve.Evaluate(heightMap[x, y]) * heightMultiplier, topLeftZ - y);
            meshData.uvs[vertexIndex] = new Vector2(x / (float) width, y / (float) height);
            if (x < width - 1 && y < height - 1)
            {
                meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
                meshData.AddTriangle(vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);
            }

            vertexIndex++;
        }

        return meshData;
    }
}

public class MeshData
{
    private readonly int[] triangles;
    private int triangleIndex;
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