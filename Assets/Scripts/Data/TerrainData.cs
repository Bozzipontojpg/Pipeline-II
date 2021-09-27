using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Terrain Data/Terrain Data")]
public class TerrainData : UpdatebleData
{
    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;

    public float meshHeightMultiplier;
    
    public AnimationCurve meshHeightCurve;
    
    public Gradient colorGradient;
    public TerrainType[] Regions;
    private GradientColorKey[] colorKey;
    private GradientAlphaKey[] alphaKey;
    
    protected override void OnValidate()
    {
        colorKey = new GradientColorKey[Regions.Length];
        alphaKey = new GradientAlphaKey[Regions.Length];
        for (int i = 0; i < Regions.Length; i++)
        {
            colorKey[i].color = Regions[i].color;
            colorKey[i].time = Regions[i].height;
            
            alphaKey[i].alpha = 1.0f;
            alphaKey[i].time = Regions[i].height;
        }
        colorGradient.SetKeys(colorKey,alphaKey);
        
        base.OnValidate();
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    [Range(0,1)]
    public float height;
    public Color color;
}
