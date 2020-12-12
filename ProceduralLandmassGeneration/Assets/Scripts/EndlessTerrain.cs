using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    public const float maxViewDistance = 450;

    public static Vector2 viewerPosition;
    private static MapGenerator mapGenerator;
    public Transform viewer;
    private int chunkSize;
    private int chunksVisibleInViewDistance;
    public Material mapMaterial;

    private readonly Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    private readonly List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    private void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
        chunkSize = MapGenerator.MapChunkSize - 1;
        chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);
    }

    private void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks();
    }

    private void UpdateVisibleChunks()
    {
        for (var i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        terrainChunksVisibleLastUpdate.Clear();

        var currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        var currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (var yOffset = -chunksVisibleInViewDistance; yOffset <= chunksVisibleInViewDistance; yOffset++)
        for (var xOffset = -chunksVisibleInViewDistance; xOffset <= chunksVisibleInViewDistance; xOffset++)
        {
            var viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

            if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
            {
                terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();

                if (terrainChunkDictionary[viewedChunkCoord].IsVisible())
                    terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
            }
            else
            {
                terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, transform, mapMaterial));
            }
        }
    }

    public class TerrainChunk
    {
        private MeshFilter _meshFilter;

        private MeshRenderer _meshRenderer;
        private Bounds bounds;
        private readonly GameObject meshObject;
        private readonly Vector2 position;

        public TerrainChunk(Vector2 coord, int size, Transform parent, Material material)
        {
            position = coord * size;
            bounds = new Bounds(position, Vector3.one * size);
            var positionV3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("Terrain Chunk");
            _meshRenderer = meshObject.AddComponent<MeshRenderer>();
            _meshRenderer.material = material;
            _meshFilter = meshObject.AddComponent<MeshFilter>();
            meshObject.transform.position = positionV3;
            meshObject.transform.parent = parent;
            SetVisible(false);

            mapGenerator.RequestMapData(OnMapDataReceived);
        }

        private void OnMapDataReceived(MapGenerator.MapData mapData)
        {
            print("Map Data Received");
            mapGenerator.RequestMeshData(mapData, OnMeshDataReceived);
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            _meshFilter.mesh = meshData.CreateMesh();
        }
        
        public void UpdateTerrainChunk()
        {
            var viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            var visible = viewerDistanceFromNearestEdge <= maxViewDistance;
            SetVisible(visible);
        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }
}