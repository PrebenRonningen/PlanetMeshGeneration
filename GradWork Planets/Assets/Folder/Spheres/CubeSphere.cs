using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CubeSphere : MonoBehaviour
{

	private Vector2 CalculateTexCoords(Vector3 p)
	{
		Vector2 newUV = new Vector2();

		Vector3 nVec = p.normalized;
		newUV.x = (Mathf.Atan2(nVec.z, nVec.x) / (Mathf.PI * 2f)) + 0.5f;
		newUV.y = (Mathf.Asin(nVec.y) / Mathf.PI) + 0.5f;
		return newUV;
	}
	private Vector3 GetBeterPoint(Vector3 p, int resolution)
	{
		Vector3 v = new Vector3(p.x, p.y, p.z) * 2f / resolution;
		float x2 = v.x * v.x;
		float y2 = v.y * v.y;
		float z2 = v.z * v.z;

		float x1 = v.x * Mathf.Sqrt(1f - y2 / 2f - z2 / 2f + y2 * z2 / 3f);
		float y1 = v.y * Mathf.Sqrt(1f - z2 / 2f - x2 / 2f + z2 * x2 / 3f);
		float z1 = v.z * Mathf.Sqrt(1f - x2 / 2f - y2 / 2f + x2 * y2 / 3f);

		return new Vector3(x1, y1, z1);
	}
	private void FindWrapedVerts()
	{
		Dictionary<int, int> checkedVert = new Dictionary<int, int>();
		List<int> tempIndices = new List<int>();

		for (int i = 0; i < indices.Count; i += 3)
		{
			int v1 = indices[i];
			int v2 = indices[i + 1];
			int v3 = indices[i + 2];

			Vector3 A = new Vector3(texCoords[v1].x, texCoords[v1].y, 0);
			Vector3 B = new Vector3(texCoords[v2].x, texCoords[v2].y, 0);
			Vector3 C = new Vector3(texCoords[v3].x, texCoords[v3].y, 0);

			Vector3 texNormal = Vector3.Cross(B - A, C - A);


			if (texNormal.z > 0)
				tempIndices.Add(i);
		}

		//int index = vertices.Count;
		for (int i = 0; i < tempIndices.Count; ++i)
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

			if (Atex.x < 0.25f)
			{
				int newI;// = a;
				if (!checkedVert.TryGetValue(a, out newI))
				{
					Atex.x += 1;
					vertices.Add(vertices[a]);
					texCoords.Add(Atex);
					checkedVert[a] = vertices.Count - 1;
					newI = vertices.Count - 1;
				}
				indices[tempIndices[i]] = newI;
			}
			if (Btex.x < 0.25f)
			{
				int newI;// = b;
				if (!checkedVert.TryGetValue(b, out newI))
				{
					Btex.x += 1;
					vertices.Add(vertices[b]);
					texCoords.Add(Btex);
					checkedVert[b] = vertices.Count - 1;
					newI = vertices.Count - 1;
				}
				indices[tempIndices[i] + 1] = newI;
			}
			if (Ctex.x < 0.25f)
			{
				int newI;// = c;
				if (!checkedVert.TryGetValue(c, out newI))
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
	private void CreateVertices()
	{
		int edg = ((resolution * 3) - 3) * 4;
		int fac = 6 * ((resolution - 1) * (resolution - 1));
		float offset = (resolution / 2);
		float gap = 1;

		for (float y = -offset; y <= offset; y += gap)
		{
			for (float x = -offset; x <= offset; x += gap)
			{
				vertices.Add(new Vector3(x, y, -offset));
				texCoords.Add(CalculateTexCoords(vertices[vertices.Count - 1]));
			}
			for (float z = -offset + 1; z <= offset; z += gap)
			{
				vertices.Add(new Vector3(offset, y, z));
				texCoords.Add(CalculateTexCoords(vertices[vertices.Count - 1]));
			}
			for (float x = offset - 1; x >= -offset; x -= gap)
			{
				vertices.Add(new Vector3(x, y, offset));
				texCoords.Add(CalculateTexCoords(vertices[vertices.Count - 1]));
			}
			for (float z = offset - 1; z > -offset; z -= gap)
			{
				vertices.Add(new Vector3(-offset, y, z));
				texCoords.Add(CalculateTexCoords(vertices[vertices.Count - 1]));
			}
		}
		//top
		for (float z = -offset + 1; z < offset; z += gap)
		{
			for (float x = -offset + 1; x < offset; x += gap)
			{
				vertices.Add(new Vector3(x, offset, z));
				texCoords.Add(CalculateTexCoords(vertices[vertices.Count - 1]));
			}
		}
		// botom
		for (float z = -offset + 1; z < offset; z += gap)
		{
			for (float x = -offset + 1; x < offset; x += gap)
			{
				vertices.Add(new Vector3(x, -offset, z));
				texCoords.Add(CalculateTexCoords(vertices[vertices.Count - 1]));
			}
		}

		//for (int i = 0; i < vertices.Count; ++i)
		//{
		//	Vector3 finalPos2 = GetBeterPoint(vertices[i], resolution);
		//	Vector3 m = Vector3.Lerp(vertices[i] / resolution, Vector3.Lerp(vertices[i].normalized, finalPos2, spread), sphere);
		//	vertices[i] = m;
		//}
	}

	private void CreateTriangles()
	{
		int ring = (resolution * 2) * 2;
		int t = 0, v = 0;

		for (int y = 0; y < resolution; y++, v++)
		{
			for (int q = 0; q < ring - 1; q++, v++)
			{
				t = SetQuad(t, v, v + 1, v + ring, v + ring + 1);
			}
			t = SetQuad(t, v, v - ring + 1, v + ring, v + 1);
		}
		t = CreateTopFace(t, ring);
		CreateBottomFace(t, ring);
	}
	private int CreateBottomFace(int t, int ring)
	{
		int v = 1;
		int vMid = vertices.Count - (resolution - 1) * (resolution - 1);
		t = SetQuad(t, ring - 1, vMid, 0, 1);
		for (int x = 1; x < resolution - 1; x++, v++, vMid++)
		{
			t = SetQuad(t, vMid, vMid + 1, v, v + 1);
		}
		t = SetQuad(t, vMid, v + 2, v, v + 1);

		int vMin = ring - 2;
		vMid -= resolution - 2;
		int vMax = v + 2;

		for (int z = 1; z < resolution - 1; z++, vMin--, vMid++, vMax++)
		{
			t = SetQuad(t, vMin, vMid + resolution - 1, vMin + 1, vMid);
			for (int x = 1; x < resolution - 1; x++, vMid++)
			{
				t = SetQuad(t, vMid + resolution - 1, vMid + resolution, vMid, vMid + 1);
			}
			t = SetQuad(t, vMid + resolution - 1, vMax + 1, vMid, vMax);
		}

		int vTop = vMin - 1;
		t = SetQuad(t, vTop + 1, vTop, vTop + 2, vMid);
		for (int x = 1; x < resolution - 1; x++, vTop--, vMid++)
		{
			t = SetQuad(t, vTop, vTop - 1, vMid, vMid + 1);
		}
		t = SetQuad(t, vTop, vTop - 1, vMid, vTop - 2);

		return t;
	}
	private int CreateTopFace(int t, int ring)
	{
		int v = ring * resolution;
		for (int x = 0; x < resolution - 1; x++, v++)
		{
			t = SetQuad(t, v, v + 1, v + ring - 1, v + ring);
		}
		t = SetQuad(t, v, v + 1, v + ring - 1, v + 2);

		int vMin = ring * (resolution + 1) - 1;
		int vMid = vMin + 1;
		int vMax = v + 2;

		for (int z = 1; z < resolution - 1; z++, vMin--, vMid++, vMax++)
		{
			t = SetQuad(t, vMin, vMid, vMin - 1, vMid + resolution - 1);
			for (int x = 1; x < resolution - 1; x++, vMid++)
			{
				t = SetQuad(t, vMid, vMid + 1, vMid + resolution - 1, vMid + resolution);
			}
			t = SetQuad(t, vMid, vMax, vMid + resolution - 1, vMax + 1);
		}
		int vTop = vMin - 2;
		t = SetQuad(t, vMin, vMid, vTop + 1, vTop);
		for (int x = 1; x < resolution - 1; x++, vTop--, vMid++)
		{
			t = SetQuad(t, vMid, vMid + 1, vTop, vTop - 1);
		}
		t = SetQuad(t, vMid, vTop - 2, vTop, vTop - 1);
		return t;
	}
	private int SetQuad(int i, int v00, int v10, int v01, int v11)
	{
		indices.Add(v00);
		indices.Add(v01);
		indices.Add(v10);
		indices.Add(v10);
		indices.Add(v01);
		indices.Add(v11);
		return i + 6;
	}


	public bool reset = false;


	[Range(1, 52)]
	public int res = 6;
	private int resolution;
	private int lastResolution = 0;


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
	MeshFilter meshFilter;

	Mesh mesh;
	List<Vector3> vertices = new List<Vector3>();
	List<Vector2> texCoords = new List<Vector2>();
	List<int> indices = new List<int>();

	int maxFaces = 1;

	private bool shapeNeedsUpdate = true;
	private bool shaderNeedsUpdate = true;
	private bool colorsNeedUpdate = true;
	private void OnValidate()
	{
		resolution = res * 2;
		if (meshFilters == null || reset )
		{
		//	meshFilters = null;
			OnPlanetUpdated();
			UpdateShaderSettingsValues();
			UpdateShapeSettingsValues();
		}

		shapeNeedsUpdate = (resolution != lastResolution);
		if (shapeNeedsUpdate)
		{
			OnShapeUpdated();
			UpdateShapeSettingsValues();
		}

		shaderNeedsUpdate = (heightStrenght != lastHeightStrenght) || (lastSphere != sphere) || (radius != lastRadious) ||
					(lastUseHeightMap != useHeightMap) || (lastUseNoise != useNoise) ||	(lastHeightMap != heightMap) || 
					(layerCount != noiseSettings.Count) || (lastLayerTypeCount != noiseLayerType.Length) || (lastSpread != spread);
		//if( shaderNeedsUpdate )
		{
			ComputUpdate();
			UpdateShaderSettingsValues();
		}

		colorsNeedUpdate = (lastUseMaterial != useMaterial) || (lastMat != mat);
		if (colorsNeedUpdate)
		{
			OnColorUpdated();
			UpdateColorSettingsValues();
		}
	}

	private void Initialize()
	{
		if (meshFilters == null || meshFilters.Length == 0)
		{
			meshFilters = new MeshFilter[maxFaces];
		}

		if ( meshFilters[0] == null )
		{
			GameObject gameObject = new GameObject("CubeSphere");
			gameObject.transform.parent = transform;

			gameObject.AddComponent<MeshRenderer>();
			gameObject.GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));
			meshFilters[0] = gameObject.AddComponent<MeshFilter>();
			meshFilters[0].sharedMesh = mesh = new Mesh();
		}
		CreateSphere();

		if (shapeComputeShader == null)
			return;
		UpdateComputeShaderSettings();
	}

	private void CreateSphere()
    {
		vertices = new List<Vector3>();
		texCoords = new List<Vector2>();
		indices = new List<int>();

		if (mesh == null)
			meshFilters[0].sharedMesh = mesh = new Mesh();
		mesh.Clear();

		CreateVertices();
		CreateTriangles();
		FindWrapedVerts();

		mesh.vertices = vertices.ToArray();
		mesh.uv = texCoords.ToArray();
		mesh.triangles = indices.ToArray();
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
		for (int nt = 0; nt < noiseLayerType.Length; ++nt)
		{
			newNoiseLayerType[nt] = noiseLayerType[nt];
		}
		noiseLayerType = newNoiseLayerType;

		if (noiseLayerType.Length < noiseSettings.Count)
			noiseSettings.RemoveAt(noiseSettings.Count - 1);
		else if (noiseLayerType.Length > noiseSettings.Count)
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
		if (useMaterial)
		{
			foreach (MeshFilter mf in meshFilters)
			{
				mf.GetComponent<MeshRenderer>().sharedMaterial = mat;
			}
		}
		else
		{
			foreach (MeshFilter mf in meshFilters)
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
	public Texture2D dest;

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
		if (shapeComputeShader == null)
			return;

		float[] minMax = new float[2] { float.MaxValue, float.MinValue };

		int maxLength = 0;
		for (int i = 0; i < maxFaces; ++i)
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
		cubeSeg = resolution;
		shapeComputeShader.SetFloat("cubeSeg", cubeSeg);

		int[] noiseLayerTypeBuffer = new int[noiseLayerType.Length];
		for (int nt = 0; nt < noiseLayerType.Length; ++nt)
		{
			noiseLayerTypeBuffer[nt] = (int)noiseLayerType[nt];
		}
		computeNLTBuffer.SetData(noiseLayerTypeBuffer);

		shapeComputeShader.SetBuffer(0, "noiseLayerType", computeNLTBuffer);

		for (int i = 0; i < maxFaces; ++i)
		{
			Vector3[] allVerts = vertices.ToArray();
			Vector2[] alluvs = meshFilters[i].sharedMesh.uv;

			shapeComputeShader.SetFloat("numVertices", allVerts.Length);

			computeVBuffer.SetData(allVerts);
			computeUVBuffer.SetData(alluvs);
			computeMMBuffer.SetData(minMax);

			shapeComputeShader.SetBuffer(0, "vertices", computeVBuffer);
			shapeComputeShader.SetBuffer(0, "heights", computeHBuffer);
			shapeComputeShader.SetBuffer(0, "uvs", computeUVBuffer);
			shapeComputeShader.SetBuffer(0, "minMax", computeMMBuffer);

			shapeComputeShader.Dispatch(0, 1024, 1, 1);

			computeHBuffer.GetData(allVerts);
			computeMMBuffer.GetData(minMax);

			for (int u = 0; u < allVerts.Length; ++u)
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
		if (colorComputeShader == null || useMaterial || !freeUpdateColor)
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

		for (int i = 0; i < maxFaces; ++i)
		{
			colorComputeShader.SetInt("numVertices", meshFilters[i].sharedMesh.vertices.Length);

			computeUVBuffer.SetData(meshFilters[i].sharedMesh.uv);
			computeVBuffer.SetData(meshFilters[i].sharedMesh.vertices);
			colorComputeShader.SetBuffer(0, "vertices", computeVBuffer);
			colorComputeShader.SetBuffer(0, "uvs", computeUVBuffer);
			colorComputeShader.SetBuffer(0, "minMax", computeMMBuffer);
			colorComputeShader.Dispatch(0, 1024, 1024, 1);
		}

		dest = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);

		dest.filterMode = FilterMode.Bilinear;
		dest.Apply(false);
		Graphics.ConvertTexture(renderTexture, dest);

		for (int i = 0; i < maxFaces; ++i)
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

