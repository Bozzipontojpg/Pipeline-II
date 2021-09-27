using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode
    {
        NoiseMap,
        ColourMap,
        TopographicMap,
        Mesh
    }

    public DrawMode drawMode;

    public int mapChunkSize = 241;
    [Range(0,6)]
    public int levelOfDetail;

    public NoiseData noiseData;

    [Range(0,20)]
    public int blur;
    
    [HideInInspector]
    public bool noiseDataFoldout;
    
    public TerrainData terrainData;
    
    [HideInInspector]
    public bool terrainDataFoldout;
    
    public bool textureFilter;

    public bool autoUpdate;

    private Queue<MapThreadInfo<MapData>> mapThreadInfosQueue = new Queue<MapThreadInfo<MapData>>();

    void OnValuesUpdated()
    {
        if (!Application.isPlaying)
        {
            DrawMapInEditor();
        }
    }
    
    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData();
        
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap, textureFilter));
        }
        else if (drawMode == DrawMode.ColourMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap,mapChunkSize,mapChunkSize, textureFilter, blur));
        }
        else if (drawMode == DrawMode.TopographicMap)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, levelOfDetail),
                TextureGenerator.TextureFromColorMap(mapData.topoMap, mapChunkSize, mapChunkSize, textureFilter, blur));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, levelOfDetail),
                TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize, textureFilter, blur));
        }
    }

    public void RequestMapData(Action<MapData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(callback);
        };

        new Thread(threadStart).Start();
    }

    void MapDataThread(Action<MapData> callback)
    {
        MapData mapData = GenerateMapData();
        lock (mapThreadInfosQueue)
        {
            mapThreadInfosQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    void Update()
    {
        if(mapThreadInfosQueue.Count >0)
        {
            for (int i = 0; i < mapThreadInfosQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapThreadInfosQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    MapData GenerateMapData()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, noiseData.seed, noiseData.noiseScale, noiseData.octaves, noiseData.persistence, noiseData.lacunarity, noiseData.offset);

        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
        Color[] topoMap = new Color[mapChunkSize * mapChunkSize];

        /*
        List<float[,]> biomesNoise = new List<float[,]>();
        int f = 1;
        foreach (var biome in Biomes)
        {
            biomesNoise.Add(Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, noiseData.seed*f, noiseData.noiseScale, biome.biome.octaves, biome.biome.persistance, biome.biome.lacunarity, noiseData.offset));
            f++;
        }
        */
        
        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                float currentHeight = noiseMap[x, y];

                for (int i = 0; i < terrainData.Regions.Length; i++)
                {
                    if (currentHeight >= terrainData.Regions[i].height)
                    {
                        colorMap[y * mapChunkSize + x] = terrainData.colorGradient.Evaluate(currentHeight);
                        
                        break;
                    }
                }
                topoMap[y * mapChunkSize + x] = Color.HSVToRGB(1-(1*noiseMap[x, y]+0.2f),1,1);
            }
            
        }

        return new MapData(noiseMap, colorMap, topoMap);
    }
    
    private void OnValidate()
    {
        if (terrainData != null)
        {
            terrainData.onValuesUpdated -= OnValuesUpdated;
            terrainData.onValuesUpdated += OnValuesUpdated;
        }
        if (noiseData != null)
        {
            noiseData.onValuesUpdated -= OnValuesUpdated;
            noiseData.onValuesUpdated += OnValuesUpdated;
        }


    }

    struct MapThreadInfo<T>
    {
        public Action<T> callback;
        public T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}

public struct MapData
{
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;
    public readonly Color[] topoMap;

    public MapData(float[,] heightMap, Color[] colorMap, Color[] topoMap)
    {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
        this.topoMap = topoMap;
    }
}
