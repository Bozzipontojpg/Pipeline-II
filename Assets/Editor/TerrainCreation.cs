using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

[CanEditMultipleObjects]
public class TerrainCreation : EditorWindow
{
	int GridResolution = 60;

	static float NoiseScale = 1.0f;
	static float MaxHeight = 100.0f;
	static float DistanceScale = 1.0f;
	static int Octavees = 8;

	static public MeshFilter MeshObject;

	string [] colorTypes =  new string[] {"None", "Color", "Texture"};
	int [] colorTypeIndexList = new int[] {1, 2, 3};

	string [] normalTypes =  new string[] {"Flat", "Smooth"};
	int [] normalTypesIndexList = new int[] {1, 2};

	static int NormalTypeIndex = 1;

	static int ColorTypeIndex = 1;

	static Color SandColor;
	static Color GrassColor;
	static Color SnowColor;

	static int SmoothStepsCount = 0;

	Texture2D noiseTex;

	Color[] pix;

	static AnimationCurve heigthAnimationCurve = new AnimationCurve();
	private static int levelOfDetail;

	private List<Vector3> vertices = new List<Vector3>();
	private List<int> triangles=new List<int>();
	private List<Vector2> uvs=new List<Vector2>();
	private List<Vector3> normals=new List<Vector3>();

	[MenuItem("Terrain/Create Terrain...")]
	static void Init()
	{
		EditorWindow.GetWindow<TerrainCreation>().Show();
	}

	public void OnGUI()
	{
		NoiseScale = EditorGUILayout.FloatField("Noise Scale", NoiseScale);
		
		GridResolution = EditorGUILayout.IntField("Grid Resolution", GridResolution);

		Octavees = EditorGUILayout.IntSlider("Number of Octavees", Octavees, 0, 8);

		if (GUILayout.Button ("Create New Texture"))
			CreateNoiseTexture();

		MaxHeight = EditorGUILayout.FloatField("Max Terrain Height", MaxHeight);

		DistanceScale = EditorGUILayout.FloatField("Distance of Grid vertex", DistanceScale);

		ColorTypeIndex = EditorGUILayout.IntPopup("Color Type", ColorTypeIndex, colorTypes, colorTypeIndexList);

		if (ColorTypeIndex == 2)
		{
			GrassColor = EditorGUILayout.ColorField ("Grass Color", GrassColor);
			SandColor = EditorGUILayout.ColorField ("Sand Color", SandColor);
			SnowColor = EditorGUILayout.ColorField ("Snow Color", SnowColor);
		}

		NormalTypeIndex = EditorGUILayout.IntPopup("Normal Type", NormalTypeIndex, normalTypes, normalTypesIndexList);

		SmoothStepsCount = EditorGUILayout.IntSlider("Number of smooth steps", SmoothStepsCount, 0, 30);

		heigthAnimationCurve = EditorGUILayout.CurveField("Height Curve", heigthAnimationCurve);
			
		levelOfDetail = EditorGUILayout.IntField("LOD", levelOfDetail);

		MeshObject = (MeshFilter) EditorGUILayout.ObjectField("Mesh Object", MeshObject, typeof(MeshFilter));

		if (GUILayout.Button("Generate Terrain"))
			CreateTerrain();
	}

	void CreateNoiseTexture()
	{
		noiseTex = new Texture2D(GridResolution, GridResolution);
		pix = new Color[GridResolution * GridResolution];

		float xOri = UnityEngine.Random.value * 100000.0f;
		float yOri = UnityEngine.Random.value * 100000.0f;

		float y = 0.0f;
		while (y < noiseTex.height)
		{
			float x = 0.0f;
			while (x < noiseTex.width)
			{
				float xCoord = xOri + x / noiseTex.width * NoiseScale + Mathf.Sin(y);
				float yCoord = yOri + y / noiseTex.height * NoiseScale;

				float sample = OctaveesNoise2D(xOri + x / noiseTex.width, yOri + y / noiseTex.height, Octavees, 1.0f, 0.75f);

				pix[(int) y * noiseTex.width + (int) x] = new Color(sample, sample, sample);
                
				x++;
            }

            y++;
        }

        noiseTex.SetPixels(pix);
        noiseTex.Apply();

		byte[] bytes = noiseTex.EncodeToPNG ();

		Debug.Log("Creating Terrain Texture: " + Application.dataPath + "/TerrainTexture.png");

		File.WriteAllBytes (Application.dataPath + "/TerrainTexture.png", bytes);

		AssetDatabase.ImportAsset("Assets/TerrainTexture.png");
	}

	public float OctaveesNoise2D(float x, float y, int octNum, float frq, float amp)
	{
		float gain = 1.0f;
		float sum = 0.0f;

		for (int i = 0; i < octNum; i++)
		{
			sum +=  Mathf.PerlinNoise(x * gain / frq, y * gain / frq) * amp / gain;
			gain *= 2.0f;
		}

		return sum;
	}

    void CreateTerrain()
    {
	    vertices.Clear();
	    uvs.Clear();
	    normals.Clear();
	    triangles.Clear();
	    
	    int height = GridResolution;
	    int width = GridResolution;
	    
	    float topLeftX = (width - 1) / -2f;
	    float topLeftZ = (height - 1) / 2f;
	    
	    int meshSimplificationIncrement = (levelOfDetail == 0)?1:levelOfDetail * 2;
	    int verticesPerLine = (width - 1) / meshSimplificationIncrement + 1;
	    
	    int vertexIndex = 0;
	    for (int y = 0; y < height; y+= meshSimplificationIncrement)
	    {
		    for (int x = 0; x < width; x+=meshSimplificationIncrement)
		    {
			    Color color = noiseTex.GetPixelBilinear(x, y);

			    vertices.Add(new Vector3(topLeftX + x, heigthAnimationCurve.Evaluate(color.grayscale * MaxHeight)*MaxHeight, topLeftZ - y));
			    uvs.Add(new Vector2(x / (float)width, y / (float)height));

			    if (x < width - 1 && y < height - 1)
			    {
				    AddTriangle(vertexIndex, vertexIndex+verticesPerLine+1, vertexIndex+verticesPerLine);
				    AddTriangle(vertexIndex+verticesPerLine+1, vertexIndex, vertexIndex+1);
			    }

			    vertexIndex++;
		    }
	    }

	    Mesh mesh = new Mesh();
	    mesh.vertices = vertices.ToArray();
	    mesh.triangles = triangles.ToArray();
	    mesh.uv = uvs.ToArray();
	    mesh.RecalculateNormals();

	    MeshObject.sharedMesh = mesh;
    }
    
    public void AddTriangle(int a, int b, int c)
    {
	    triangles.Add(a);
	    triangles.Add(b);
	    triangles.Add(c);
    }

    void CreateVertex()
    {
	    int height = GridResolution;
	    int width = GridResolution;
	    
	    vertices.Clear();
	    uvs.Clear();
	    normals.Clear();
	    
	    for (int z = 0; z < height; z++)
	    {
		    for (int x = 0; x < width; x++)
		    {
			    Vector3 vertex = new Vector3(x, 0, z);
			    Vector2 uv = new Vector2(x / height, z / width);

			    Color color = noiseTex.GetPixelBilinear(uv.x, uv.y);
			    vertex.y = color.r * MaxHeight;

			    vertices.Add(vertex);
			    uvs.Add(uv);
			    normals.Add(new Vector3(0, 0, 0));
		    }
	    }
    }

    void CreateTriangles()
    {
	    int height = GridResolution;
	    int width = GridResolution;

	    triangles.Clear();
	    
	    for (int z = 0; z < height-1; z++)
	    {
		    for (int x = 0; x < width-1; x++)
		    {
			    Vector3 normal;
			    Vector3 dir1, dir2;

			    dir1 = vertices[z * height + (x + 1)] - vertices[z * height + x];
			    dir2 = vertices[(z+1) * height + x ] - vertices[z * height + x];

			    normal = Vector3.Cross(dir1.normalized, dir2.normalized).normalized;
			    
			    triangles.Add(z * height+ x);
			    triangles.Add((z + 1) * height+ x);
			    triangles.Add(z * height+ (x + 1));
			    
			    triangles.Add(z * height+ x);
			    triangles.Add((z - 1) * height+ x);
			    triangles.Add(z * height+ (x - 1));

			    normals[z * height + x] += normal;
			    normals[(z+1) * height + x] += normal;
			    normals[z * height + (x+1)] += normal;
			    
		    }
	    }
    }

}