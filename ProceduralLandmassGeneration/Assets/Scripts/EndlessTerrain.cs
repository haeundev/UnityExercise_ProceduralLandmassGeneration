using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    private const float viewerMoveThresholdForChunkUpdate = 25f;
    private const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;
    
    public LODInfo[] detailLevels;
    public static float maxViewDistance;

    public static Vector2 viewerPosition;
    private Vector2 viewerPositionOld;
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

        maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
        chunkSize = MapGenerator.MapChunkSize - 1;
        chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);
       
        UpdateVisibleChunks();
    }

    private void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
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
                terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial));
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
        private LODInfo[] detailLevels;
        private LODMesh[] lodMeshes;

        private MapGenerator.MapData mapData;
        private bool mapDataReceived;
        private int previousLODIndex = -1;
        
        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material)
        {
            this.detailLevels = detailLevels;

            this.position = coord * size;
            bounds = new Bounds(position, Vector3.one * size);
            var positionV3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("Terrain Chunk");
            _meshRenderer = meshObject.AddComponent<MeshRenderer>();
            _meshRenderer.material = material;
            _meshFilter = meshObject.AddComponent<MeshFilter>();
            meshObject.transform.position = positionV3;
            meshObject.transform.parent = parent;
            SetVisible(false);

            lodMeshes = new LODMesh[detailLevels.Length];
            for (var i = 0; i < detailLevels.Length; i++)
            {
                lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
            }

            mapGenerator.RequestMapData(position, OnMapDataReceived);
        }

        private void OnMapDataReceived(MapGenerator.MapData mapData)
        {
            this.mapData = mapData;
            mapDataReceived = true;

            Texture2D texture = TextureGenerator.TextureFromColorMap(mapData.colorMap, MapGenerator.MapChunkSize,
                MapGenerator.MapChunkSize);
            _meshRenderer.material.mainTexture = texture;
            
            UpdateTerrainChunk();
        }


        public void UpdateTerrainChunk()
        {
            if (mapDataReceived)
            {
                var viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));

                var visible = viewerDistanceFromNearestEdge <= maxViewDistance;

                if (visible)
                {
                    int lodIndex = 0;

                    for (int i = 0; i < detailLevels.Length - 1; i++)
                    {
                        if (viewerDistanceFromNearestEdge > detailLevels[i].visibleDistanceThreshold)
                        {
                            lodIndex = i + 1;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (lodIndex != previousLODIndex)
                    {
                        LODMesh lodMesh = lodMeshes[lodIndex];
                        if (lodMesh.hasMesh)
                        {
                            previousLODIndex = lodIndex;
                            _meshFilter.mesh = lodMesh.mesh;
                        }
                        else if (!lodMesh.hasRequestedMesh)
                        {
                            lodMesh.RequestMesh(mapData);
                        }
                    }
                }
                
                SetVisible(visible);
            }
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

    public class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        private int lod;
        private System.Action updateCallback;

        public LODMesh(int lod, System.Action updateCallback)
        {
            this.lod = lod;
            this.updateCallback = updateCallback;
        }

        private void OnMeshDataReceived(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            hasMesh = true;

            updateCallback();
        }

        public void RequestMesh(MapGenerator.MapData mapData)
        {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
        }
    }

    [System.Serializable]
    public struct LODInfo
    {
        public int lod;
        public float visibleDistanceThreshold;
    }
}