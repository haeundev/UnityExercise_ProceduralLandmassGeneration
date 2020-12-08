using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public int mapWidth;
    public int mapHeight;
    public float noiseScale;

    public int octaves;
    public float persistence;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public bool autoUpdate;

    public void GenerateMap()
    {
        var noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistence, lacunarity, offset);

        var display = FindObjectOfType<MapDisplay>();
        display.DrawNoiseMap(noiseMap);
    }
}