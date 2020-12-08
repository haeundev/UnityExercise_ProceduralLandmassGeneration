using UnityEngine;
using Random = System.Random;

public static class Noise
{
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves,
        float persistence, float lacunarity, Vector2 offset)
    {
        var noiseMap = new float[mapWidth, mapHeight];

        var prng = new Random(seed);
        var octaveOffsets = new Vector2[octaves];
        for (var o = 0; o < octaves; o++)
        {
            var offsetX = prng.Next(-100000, 100000) + offset.x;
            var offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[o] = new Vector2(offsetX, offsetY);
        }

        if (scale <= 0)
            scale = 0.0001f;

        var maxNoiseHeight = float.MinValue;
        var minNoiseHeight = float.MaxValue;

        var halfWidth = mapWidth / 2f;
        var halfHeight = mapHeight / 2f;

        for (var y = 0; y < mapHeight; y++)
        for (var x = 0; x < mapWidth; x++)
        {
            float amplitude = 1;
            float frequency = 1;
            float noiseHeight = 0;

            for (var i = 0; i < octaves; i++)
            {
                var sampleX = (x-halfWidth) / scale * frequency + octaveOffsets[i].x;
                var sampleY = (y-halfHeight) / scale * frequency + octaveOffsets[i].y;
                var perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                noiseHeight += perlinValue * amplitude;
                amplitude *= persistence;
                frequency += lacunarity;
            }

            if (noiseHeight > maxNoiseHeight)
                maxNoiseHeight = noiseHeight;
            else if (noiseHeight < minNoiseHeight)
                minNoiseHeight = noiseHeight;

            noiseMap[x, y] = noiseHeight;
        }

        for (var y = 0; y < mapHeight; y++)
        for (var x = 0; x < mapWidth; x++)
            noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);


        return noiseMap;
    }
}