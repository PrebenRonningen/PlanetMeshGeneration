using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuadFace 
{
	Mesh mesh;
	int resolution; //subdivisions

	List<Vector3> vertices = new List<Vector3>();
	List<Vector2> texCoords = new List<Vector2>();
	List<int> indices = new List<int>();
	Vector3 localUp;
	Vector3 localForward;
	Vector3 localRight;

	public Vector3[] GetVerts()
	{
		return vertices.ToArray();
	}
	public QuadFace(Mesh mesh, int resolution, Vector3 localUp)
	{
		this.mesh = mesh;
		this.resolution = resolution;

		this.localUp = localUp;
		this.localForward = new Vector3( localUp.y, localUp.z, localUp.x );
		this.localRight = Vector3.Cross( this.localUp, localForward);
		this.mesh.Clear();
	}

	public static Vector3 GetBeterPoint(Vector3 p)
	{
		float x2 = p.x * p.x;
		float y2 = p.y * p.y;
		float z2 = p.z * p.z;
		float x1 = p.x * Mathf.Sqrt(1 - ( y2 + z2 ) / 2 + ( y2 * z2 ) / 3);
		float y1 = p.y * Mathf.Sqrt(1 - ( z2 + x2 ) / 2 + ( z2 * x2 ) / 3);
		float z1 = p.z * Mathf.Sqrt(1 - ( x2 + y2 ) / 2 + ( x2 * y2 ) / 3);
		return new Vector3(x1, y1, z1);
	}

	public static Vector2 CalculateTexCoords(Vector3 p )
	{
		Vector2 newUV = new Vector2();

		Vector3 nVec = p.normalized;
		newUV.x = ( Mathf.Atan2(nVec.z, nVec.x) / ( Mathf.PI * 2f ) ) + 0.5f;
		newUV.y = ( Mathf.Asin(nVec.y) / Mathf.PI ) + 0.5f;
		return newUV;
	}
	private void FindWrapedVerts()
	{
		Dictionary<int, int> checkedVert = new Dictionary<int, int>();
		List<int> tempIndices = new List<int>();

		for( int i = 0; i < indices.Count; i += 3 )
		{
			int v1 = indices[i];
			int v2 = indices[i + 1];
			int v3 = indices[i + 2];

			Vector3 A = new Vector3(texCoords[v1].x, texCoords[v1].y, 0);
			Vector3 B = new Vector3(texCoords[v2].x, texCoords[v2].y, 0);
			Vector3 C = new Vector3(texCoords[v3].x, texCoords[v3].y, 0);

			Vector3 texNormal = Vector3.Cross(B - A, C - A);


			if( texNormal.z > 0 )
				tempIndices.Add(i);
		}

		//int index = vertices.Count;
		for( int i = 0; i < tempIndices.Count; ++i )
		{
			int a = indices[tempIndices[i]];
			int b = indices[tempIndices[i] + 1];
			int c = indices[tempIndices[i] + 2];

			//Vector3 Apos = vertices[a];
			Vector2 Atex = texCoords[a];

			//Vector3 Bpos = vertices[b];
			Vector2 Btex = texCoords[b];

			//Vector3 Cpos = vertices[c];
			Vector2 Ctex = texCoords[c];

			if( Atex.x < 0.25f )
			{
				int newI;// = a;
				if( !checkedVert.TryGetValue(a, out newI) )
				{
					Atex.x += 1;
					vertices.Add(vertices[a]);
					texCoords.Add(Atex);
					checkedVert[a] = vertices.Count - 1;
					newI = vertices.Count - 1;
				}
				indices[tempIndices[i]] = newI;
			}
			if( Btex.x < 0.25f )
			{
				int newI;// = b;
				if( !checkedVert.TryGetValue(b, out newI) )
				{
					Btex.x += 1;
					vertices.Add(vertices[b]);
					texCoords.Add(Btex);
					checkedVert[b] = vertices.Count - 1;
					newI = vertices.Count - 1;
				}
				indices[tempIndices[i] + 1] = newI;
			}
			if( Ctex.x < 0.25f )
			{
				int newI;// = c;
				if( !checkedVert.TryGetValue(c, out newI) )
				{
					Ctex.x += 1;
					vertices.Add(vertices[c]);
					texCoords.Add(Ctex);
					checkedVert[c] = vertices.Count - 1;
					newI = vertices.Count - 1;
				}
				indices[tempIndices[i] + 2] = newI;
			}
		}
	}
	public void CreateFaces()
	{
		int triIndex = 0;
		int indice;
		for ( int y = 0; y < resolution; ++y)
		{
			for ( int x = 0; x < resolution; ++x)
			{
				indice = x + y * resolution;
				Vector2 progress = new Vector2(x, y) / ( resolution - 1 );
				Vector3 vPosition = localUp + localForward * (2 * progress.x - 1) + localRight * (2 * progress.y -1);

				vertices.Add(vPosition);
				texCoords.Add(CalculateTexCoords(vPosition));

				if(x != resolution - 1 && y != resolution - 1)
				{
					indices.Add(indice);
					indices.Add(indice + resolution + 1);
					indices.Add(indice + resolution);
					indices.Add(indice);
					indices.Add(indice + 1);
					indices.Add(indice + resolution + 1);
					
					triIndex += 6;
				}
			}
		}

		FindWrapedVerts();


		mesh.vertices = vertices.ToArray();
		mesh.uv = texCoords.ToArray();
		mesh.triangles = indices.ToArray();

	}
}

public class QuadSphere : MonoBehaviour
{
	public bool reset = false;

	[Range(2, 220)]
	public int resolution = 10;
	private int lastResolution = 10;

	[Range(1f, 100f)]
	public float radius = 10f;
	private float lastRadious = 10f;

	[Range(0f, 1f)]
	public float sphere = 1f;
	private float lastSphere = 1f;

	[Range(0f, 1f)]
	public float spread = 1f;
	private float lastSpread = 1f;

	// ------- Sphere Settings ------- //

	public ComputeShader colorComputeShader;
	// ------- Shader Settings ------- //

	public ComputeShader shapeComputeShader;

	public bool useHeightMap = false;
	private bool lastUseHeightMap = false;

	[Range(0f, 5f)]
	public float heightStrenght = 0f;
	private float lastHeightStrenght = 0f;

	public Texture2D heightMap;
	private Texture2D lastHeightMap;

	// ------- Noise Settings ------- //

	public bool useNoise = false;
	private bool lastUseNoise = false;

	public enum NoiseType : int
	{
		None = 0,
		SimplexNoise = 1,
		RidgeNoise = 2
	}

	[SerializeField]
	public NoiseType[] noiseLayerType;
	private int lastLayerTypeCount = 0;

	[System.Serializable]
	public struct NoiseSettings
	{
		[Range(0, 10)]
		public float layerCount;
		public float scale;
		public float frequency;
		public float freqMultiplier;
		public float amplitude;
		public float ampMultiplier;
		public Vector3 offset;
		public float minValue;
		public float vShift;
		public float ridgeWeight;
		public float ridgeWeightMultiplier;
		public float useFirstLayerAsMask;
	}

	[SerializeField]
	public List<NoiseSettings> noiseSettings = new List<NoiseSettings>();

	private int layerCount = 0;

	// ------- Noise Settings ------- //
	// ------- Shader Settings ------- //

	// ------- Material Settings ------- //

	public bool useMaterial = false;
	private bool lastUseMaterial = false;
	public Material mat = null;
	private Material lastMat = null;

	// ------- Material Settings ------- //

	[SerializeField, HideInInspector]
	MeshFilter[] meshFilters;
	QuadFace[] quadFaces;

	int maxFaces = 6;

	private bool shapeNeedsUpdate = true;
	private bool shaderNeedsUpdate = true;
	private bool colorsNeedUpdate = true;

	private void OnValidate()
	{
		if( quadFaces == null || reset )
		{
			quadFaces = null;
			OnPlanetUpdated();
			UpdateShaderSettingsValues();
			UpdateShapeSettingsValues();
		}

		shapeNeedsUpdate = ( resolution != lastResolution );
		if( shapeNeedsUpdate )
		{
			OnShapeUpdated();
			UpdateShapeSettingsValues();
		}

		shaderNeedsUpdate = ( heightStrenght != lastHeightStrenght ) || ( lastSphere != sphere ) || ( radius != lastRadious ) ||
					( lastUseHeightMap != useHeightMap ) || ( lastUseNoise != useNoise ) ||	( lastHeightMap != heightMap ) || 
					( layerCount != noiseSettings.Count ) || ( lastLayerTypeCount != noiseLayerType.Length ) || ( lastSpread != spread );
		//if( shaderNeedsUpdate )
		{
			ComputUpdate();
			UpdateShaderSettingsValues();
		}

		colorsNeedUpdate = ( lastUseMaterial != useMaterial ) || ( lastMat != mat );
		if( colorsNeedUpdate )
		{
			OnColorUpdated();
			UpdateColorSettingsValues();
		}
	}

	void Initialize()
	{
		if(meshFilters == null || meshFilters.Length == 0 )
		{
			meshFilters = new MeshFilter[maxFaces];
		}

		quadFaces = new QuadFace[maxFaces];

		Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

		for( int i = 0; i < maxFaces; ++i )
		{
			
			if(meshFilters[i] == null )
			{
				GameObject gameObject = new GameObject("SphereFace");
				gameObject.transform.parent = transform;
			
				gameObject.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));
				meshFilters[i] = gameObject.AddComponent<MeshFilter>();
				meshFilters[i].sharedMesh = new Mesh();
			}
			quadFaces[i] = new QuadFace(meshFilters[i].sharedMesh, resolution, directions[i]);
		}

		if( shapeComputeShader == null )
			return;
		UpdateComputeShaderSettings();
	}

	void CreateSphere()
	{
		foreach(QuadFace face in quadFaces)
		{
			face.CreateFaces();
		}
	}

	void UpdateShapeSettingsValues()
	{
		lastResolution = resolution;
	}

	void UpdateShaderSettingsValues()
	{
		lastRadious = radius;
		lastSphere = sphere;
		lastHeightStrenght = heightStrenght;
		lastUseHeightMap = useHeightMap;
		lastUseNoise = useNoise;
		lastHeightMap = heightMap;
		lastSpread = spread;
		UpdateLayerType();
		layerCount = noiseSettings.Count;
		lastLayerTypeCount = noiseLayerType.Length;
	}

	void UpdateLayerType()
	{
		NoiseType[] newNoiseLayerType = new NoiseType[noiseLayerType.Length];
		for( int nt = 0; nt < noiseLayerType.Length; ++nt )
		{
			newNoiseLayerType[nt] = noiseLayerType[nt];
		}
		noiseLayerType = newNoiseLayerType;

		if( noiseLayerType.Length < noiseSettings.Count )
			noiseSettings.RemoveAt(noiseSettings.Count - 1);
		else if( noiseLayerType.Length > noiseSettings.Count )
			noiseSettings.Add(noiseSettings.LastOrDefault());
	}
	void UpdateColorSettingsValues()
	{
		lastUseMaterial = useMaterial;
		lastMat = mat;
	}

	void OnPlanetUpdated()
	{
		Initialize();
		CreateSphere();
		CreateColor();
		ComputUpdate();
	}

	void OnShapeUpdated()
	{
		Initialize();
		CreateSphere();
		ComputUpdate();
	}
	void OnColorUpdated()
	{
		//Initialize();
		CreateColor();
	}

	void CreateColor()
	{
		if( useMaterial )
		{
			foreach( MeshFilter mf in meshFilters )
			{
				mf.GetComponent<MeshRenderer>().sharedMaterial = mat;
			}
		}
		else
		{
			foreach( MeshFilter mf in meshFilters )
			{
				Material newMeterial = new Material(Shader.Find("Standard"));
				newMeterial.SetFloat("_Glossiness", 0.1f);
				newMeterial.color = new Color(0.184f, 0.355f, 0.149f);
				mf.GetComponent<MeshRenderer>().sharedMaterial = newMeterial;
			}
		}
	}
	private void UpdateComputeShaderSettings()
	{
		shapeComputeShader.SetTexture(0, "heightMap", heightMap);
		shapeComputeShader.SetFloat("heightIntensity", heightStrenght);
		shapeComputeShader.SetFloat("radius", radius);
		shapeComputeShader.SetFloat("sphere", sphere);
		shapeComputeShader.SetBool("useHeightMap", useHeightMap);
		shapeComputeShader.SetBool("useNoise", useNoise);
	}



	public int textureSize = 256;
	public RenderTexture renderTexture;
	public Texture2D dest = null;

	public float pixelOffset = 1;
	[Range(0f, 1f)]
	public float lerpValue = 0;

	public float waterMax = 0f;
	public float grassLine = 72.9f;
	public float mountainLine = 75.25f;
	public float snowLine = 82f;
	public bool freeUpdateColor = false;

	private bool isCube = true;
	private float cubeSeg = 1;
	void ComputUpdate()
	{
		if( shapeComputeShader == null )
			return;

		float[] minMax = new float[2] { float.MaxValue, float.MinValue };

		int maxLength = 0;
		for( int i = 0; i < maxFaces; ++i )
		{
			maxLength = Mathf.Max(maxLength, meshFilters[i].sharedMesh.vertices.Length);
		}

		UpdateComputeShaderSettings();
		ComputeBuffer computeVBuffer = new ComputeBuffer(maxLength, 12);
		ComputeBuffer computeHBuffer = new ComputeBuffer(maxLength, 12);
		ComputeBuffer computeUVBuffer = new ComputeBuffer(maxLength, 8);
		ComputeBuffer computeNoiceSettingsBuffer = new ComputeBuffer(noiseSettings.Count, sizeof(float) * 14);
		ComputeBuffer computeNLTBuffer = new ComputeBuffer(noiseLayerType.Length, sizeof(int));
		ComputeBuffer computeMMBuffer = new ComputeBuffer(minMax.Length, sizeof(float) * minMax.Length);


		computeNoiceSettingsBuffer.SetData(noiseSettings);
		shapeComputeShader.SetBuffer(0, "noiseSettings", computeNoiceSettingsBuffer);
		shapeComputeShader.SetInt("noiseLayers", noiseSettings.Count);
		shapeComputeShader.SetFloat("lerpSpread", spread);
		shapeComputeShader.SetBool("isCube", isCube);
		shapeComputeShader.SetFloat("cubeSeg", cubeSeg);

		int[] noiseLayerTypeBuffer = new int[noiseLayerType.Length];
		for( int nt = 0; nt < noiseLayerType.Length; ++nt )
		{
			noiseLayerTypeBuffer[nt] = (int)noiseLayerType[nt];
		}
		computeNLTBuffer.SetData(noiseLayerTypeBuffer);

		shapeComputeShader.SetBuffer(0, "noiseLayerType", computeNLTBuffer);

		for( int i = 0; i < maxFaces; ++i )
		{
			Vector3[] allVerts = quadFaces[i].GetVerts();
			Vector2[] alluvs = meshFilters[i].sharedMesh.uv;

			shapeComputeShader.SetFloat("numVertices", allVerts.Length);

			computeVBuffer.SetData(allVerts);
			computeUVBuffer.SetData(alluvs);
			computeMMBuffer.SetData(minMax);

			shapeComputeShader.SetBuffer(0, "vertices", computeVBuffer);
			shapeComputeShader.SetBuffer(0, "heights", computeHBuffer);
			shapeComputeShader.SetBuffer(0, "uvs", computeUVBuffer);
			shapeComputeShader.SetBuffer(0, "minMax", computeMMBuffer);

			shapeComputeShader.Dispatch(0, 512, 1, 1);

			computeHBuffer.GetData(allVerts);
			computeMMBuffer.GetData(minMax);

			for(int u = 0; u < allVerts.Length; ++u) 
			{
				alluvs[u] = QuadFace.CalculateTexCoords(allVerts[u]);
			}

			meshFilters[i].sharedMesh.vertices = allVerts;
			meshFilters[i].sharedMesh.uv = alluvs;
			meshFilters[i].sharedMesh.RecalculateBounds();
			meshFilters[i].sharedMesh.RecalculateNormals();
			meshFilters[i].sharedMesh.RecalculateTangents();
		}
		computeHBuffer.Release();
		computeNoiceSettingsBuffer.Release();
		computeNLTBuffer.Release();


		// next step... COLOR!
		if( colorComputeShader == null || useMaterial || !freeUpdateColor )
		{
			computeVBuffer.Release();
			computeUVBuffer.Release();
			computeMMBuffer.Release();
			return;
		}

		if (renderTexture == null || (textureSize != renderTexture.width) || freeUpdateColor)
		{
			renderTexture = new RenderTexture(textureSize, textureSize, 24);
			renderTexture.enableRandomWrite = true;
			renderTexture.Create();
		}

		colorComputeShader.SetTexture(0, "Result", renderTexture);

		colorComputeShader.SetFloat("pxOffset", pixelOffset);
		colorComputeShader.SetFloat("waterMax", waterMax);
		colorComputeShader.SetFloat("grassLine", grassLine);
		colorComputeShader.SetFloat("mountainLine", mountainLine);
		colorComputeShader.SetFloat("snowLine", snowLine);
		colorComputeShader.SetFloat("radius", radius);
		colorComputeShader.SetFloat("resolution", renderTexture.width);
		colorComputeShader.SetFloat("lerpValue", lerpValue);

		for( int i = 0; i < maxFaces; ++i )
		{
			colorComputeShader.SetInt("numVertices", meshFilters[i].sharedMesh.vertices.Length);

			computeUVBuffer.SetData(meshFilters[i].sharedMesh.uv);
			computeVBuffer.SetData(meshFilters[i].sharedMesh.vertices);
			colorComputeShader.SetBuffer(0, "vertices", computeVBuffer);
			colorComputeShader.SetBuffer(0, "uvs", computeUVBuffer);
			colorComputeShader.SetBuffer(0, "minMax", computeMMBuffer);
			colorComputeShader.Dispatch(0, 1024, 512, 1);
		}

		dest = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);

		dest.filterMode = FilterMode.Bilinear;
		dest.Apply(false);
		Graphics.ConvertTexture(renderTexture, dest);

		for( int i = 0; i < maxFaces; ++i )
		{
			Material material = new Material(meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial);
			//	meshFilters[i].GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
			material.SetTexture("_MainTex", dest);
			material.SetFloat("_Glossiness", 0.2f);
			material.color = new Color(1f, 1f, 1f);
			meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial = material;
		}

		computeVBuffer.Release();
		renderTexture.Release();
		computeMMBuffer.Release();
		computeUVBuffer.Release();
	}
}
