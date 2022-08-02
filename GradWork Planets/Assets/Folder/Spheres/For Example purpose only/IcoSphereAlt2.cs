using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IoFace
{
	Mesh mesh;
	int edgeVertexCount;
	List<Vector3> vertices = new List<Vector3>();
	List<Vector2> texCoords = new List<Vector2>();
	List<int> indices = new List<int>();
	int maxNumVerts = 0;

	public Vector3[] GetVerts()
	{
		return vertices.ToArray();
	}
	public IoFace( Mesh mesh, List<Vector3> verts, int edgeVertexCount )
	{
		this.mesh = mesh;
		this.edgeVertexCount = edgeVertexCount;
		this.vertices = verts;

		this.maxNumVerts = ( ( edgeVertexCount + 3 ) * ( edgeVertexCount + 3 ) - ( edgeVertexCount + 3 ) ) / 2;

		this.mesh.Clear();
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

		for( int i = 0; i < tempIndices.Count; ++i )
		{
			int a = indices[tempIndices[i]];
			int b = indices[tempIndices[i] + 1];
			int c = indices[tempIndices[i] + 2];

			Vector2 Atex = texCoords[a];
			Vector2 Btex = texCoords[b];
			Vector2 Ctex = texCoords[c];

			if( Atex.x < 0.25f )
			{
				int newI;
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
				int newI;
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
				int newI;
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
		List<int[]> edges = new List<int[]> { new int[edgeVertexCount + 2], new int[edgeVertexCount + 2], new int[edgeVertexCount + 2] };

		for( int i = 0; i < 3; ++i )
		{
			Vector3 nVec = vertices[i].normalized;
			Vector2 newUV = new Vector2();
			newUV.x = ( Mathf.Atan2(nVec.z, nVec.x) / ( Mathf.PI * 2f ) ) + 0.5f;
			newUV.y = ( Mathf.Asin(nVec.y) / Mathf.PI ) + 0.5f;
			texCoords.Add(newUV);
		}

		for( int v = 0; v < 3; ++v )
		{
			Vector3 start = vertices[v];
			Vector3 end = vertices[( v + 1 ) % 3];

			edges[v][0] = v;

			for( int divisionIndex = 0; divisionIndex < edgeVertexCount; divisionIndex++ )
			{
				float t = ( divisionIndex + 1f ) / ( edgeVertexCount + 1f );
				edges[v][divisionIndex + 1] = vertices.Count;

				Vector3 newVec = Vector3.Lerp(start, end, t);

				vertices.Add(newVec);
				Vector2 newUV = new Vector2();

				Vector3 nVec = newVec.normalized;
				newUV.x = ( Mathf.Atan2(nVec.z, nVec.x) / ( Mathf.PI * 2f ) ) + 0.5f;
				newUV.y = ( Mathf.Asin(nVec.y) / Mathf.PI ) + 0.5f;
				texCoords.Add(newUV);
			}
			edges[v][edgeVertexCount + 1] = ( v + 1 ) % 3;
		}

		int[] verteMap = new int[maxNumVerts];
		int indexer = 0;
		verteMap[indexer++] = 0;
		System.Array.Reverse(edges[2]);

		for( int i = 1; i < edges[0].Length - 1; ++i )
		{
			verteMap[indexer++] = edges[0][i];

			Vector3 sideAVertex = vertices[edges[0][i]];
			Vector3 sideBVertex = vertices[edges[2][i]];

			int numinerPoints = i - 1;
			for( int j = 0; j < numinerPoints; ++j )
			{
				float t = ( j + 1f ) / ( numinerPoints + 1f );

				Vector3 newVec = Vector3.Lerp(sideAVertex, sideBVertex, t);

				vertices.Add(newVec);
				verteMap[indexer++] = vertices.Count -1;
				Vector2 newUV = new Vector2();

				Vector3 nVec = newVec.normalized;
				newUV.x = ( Mathf.Atan2(nVec.z, nVec.x) / ( Mathf.PI * 2f ) ) + 0.5f;
				newUV.y = ( Mathf.Asin(nVec.y) / Mathf.PI ) + 0.5f;
				texCoords.Add(newUV);
			}
			verteMap[indexer++] = edges[2][i];
		}

		for( int i = 0; i < edges[1].Length; ++i )
		{
			verteMap[indexer++] = edges[1][i];
		}

		int numRovs = edgeVertexCount + 1;
		for( int row = 0; row < numRovs; ++row )
		{
			int topVertex = ( ( row + 1 ) * ( row + 1 ) - row - 1 ) / 2;
			int bottomVertex = ( ( row + 2 ) * ( row + 2 ) - row - 2 ) / 2;

			int numTrianglesInRow = 1 + 2 * row;
			for( int column = 0; column < numTrianglesInRow; column++ )
			{
				int v0, v1, v2;

				if( column % 2 == 0 )
				{
					v0 = topVertex;
					v1 = bottomVertex;
					v2 = bottomVertex + 1;
					topVertex++;
					bottomVertex++;
				}
				else
				{
					v0 = topVertex - 1;
					v1 = bottomVertex;
					v2 = topVertex;
				}

				indices.Add(verteMap[v0]);
				indices.Add(verteMap[v1]);
				indices.Add(verteMap[v2]);
			}
		}

		FindWrapedVerts();

		mesh.vertices = vertices.ToArray();
		mesh.uv = texCoords.ToArray();
		mesh.triangles = indices.ToArray();
	}
}
public class IcoSphereAlt2 : MonoBehaviour
{
	public bool reset = false;

	// ------- Sphere Settings ------- //

	[Range(0, 200/*358*/)]
	public int edgeVertexCount = 0;

	[Range(1f, 100f)]
	public float radius = 10f;

	// ------- Shader Settings ------- //

	public ComputeShader shapeComputeShader;

	// ------- Noise Settings ------- //

	public bool useNoise = false;

	public enum NoiseType : int
	{
		None = 0,
		SimplexNoise = 1,
		RidgeNoise = 2
	}

	[SerializeField]
	public NoiseType[] noiseLayerType;

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

	public bool useMaterial = false;
	public Material mat = null;

	[SerializeField, HideInInspector]
	MeshFilter[] meshFilters;
	IcoFace[] icoFaces;

	int maxFaces = 20;

	private static float goldenRatio = ( 1f + Mathf.Sqrt(5f) ) / 2f;
	private static Vector3 diraction = new Vector3(1, goldenRatio, 0).normalized;

	Vector3[] vertices = new Vector3[] {
					new Vector3( -diraction.x,  diraction.y, 0),
					new Vector3(  diraction.x,  diraction.y, 0),
					new Vector3( -diraction.x, -diraction.y, 0),
					new Vector3(  diraction.x, -diraction.y, 0),

					new Vector3(  0, -diraction.x, diraction.y),
					new Vector3(  0,  diraction.x, diraction.y),
					new Vector3(  0, -diraction.x,-diraction.y),
					new Vector3(  0,  diraction.x,-diraction.y),

					new Vector3(  diraction.y, 0, -diraction.x),
					new Vector3(  diraction.y, 0,  diraction.x),
					new Vector3( -diraction.y, 0, -diraction.x),
					new Vector3( -diraction.y, 0,  diraction.x)
							};

	int[] indices = new int[] {
					0, 11, 5,
					0, 5 , 1,
					0, 1 , 7,
					0, 7 , 10,
					0, 10, 11,

					1 , 5 , 9,
					5 , 11, 4,
					11, 10, 2,
					10, 7 , 6,
					7 , 1 , 8,

					3, 9, 4,
					3, 4, 2,
					3, 2, 6,
					3, 6, 8,
					3, 8, 9,

					4, 9, 5,
					2, 4, 11,
					6, 2, 10,
					8, 6, 7,
					9, 8, 1
							};



	private void OnValidate()
	{
		Initialize();
		CreateSphere();
		CreateColor();
		ComputeProceduralPlanet();
		ComputeProceduralPlanetColor();
		return;
	}

	private void Initialize()
	{
		if( meshFilters == null || meshFilters.Length == 0 )
		{
			meshFilters = new MeshFilter[maxFaces];
		}
		icoFaces = new IcoFace[maxFaces];

		for( int i = 0; i < maxFaces; ++i )
		{
			if( meshFilters[i] == null )
			{
				GameObject gameObject = new GameObject("TriFace");
				gameObject.transform.parent = transform;

				gameObject.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));
				meshFilters[i] = gameObject.AddComponent<MeshFilter>();
				meshFilters[i].sharedMesh = new Mesh();
			}
			List<Vector3> verts = new List<Vector3> { vertices[indices[(i * 3)]], vertices[indices[( i * 3 ) + 1]], vertices[indices[( i * 3 ) + 2]] };

			icoFaces[i] = new IcoFace(meshFilters[i].sharedMesh, verts, edgeVertexCount);
		}
	}
	void CreateSphere()
	{
		foreach( IcoFace face in icoFaces )
		{
			face.CreateFaces();
		}
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

	// for single-mesh cube-Sphere
	//private float cubeResolution = 1;
	void ComputeProceduralPlanet()
	{
		if (shapeComputeShader == null)
			return;

		int maxLength = 0;
		for (int i = 0; i < maxFaces; ++i)
		{
			maxLength = Mathf.Max(maxLength, meshFilters[i].sharedMesh.vertices.Length);
		}

		ComputeBuffer computeVBuffer = new ComputeBuffer(maxLength, 12);
		ComputeBuffer computeHBuffer = new ComputeBuffer(maxLength, 12);

		ComputeBuffer computeNoiceSettingsBuffer = new ComputeBuffer(noiseSettings.Count, sizeof(float) * 14);
		ComputeBuffer computeNLTBuffer = new ComputeBuffer(noiseLayerType.Length, sizeof(int));

		int[] noiseLayerTypeBuffer = new int[noiseLayerType.Length];
		for (int nlt = 0; nlt < noiseLayerType.Length; ++nlt)
		{
			noiseLayerTypeBuffer[nlt] = (int)noiseLayerType[nlt];
		}

		computeNLTBuffer.SetData(noiseLayerTypeBuffer);
		computeNoiceSettingsBuffer.SetData(noiseSettings);

		shapeComputeShader.SetInt("noiseLayers", noiseSettings.Count);
		shapeComputeShader.SetFloat("radius", radius);
		shapeComputeShader.SetBool("useNoise", useNoise);
		// for single-mesh cube-Sphere
		// shapeComputeShader.SetFloat("resolution", cubeResolution);

		shapeComputeShader.SetBuffer(0, "noiseLayerType", computeNLTBuffer);
		shapeComputeShader.SetBuffer(0, "noiseSettings", computeNoiceSettingsBuffer);

		for (int i = 0; i < maxFaces; ++i)
		{
			Vector3[] allVerts = icoFaces[i].GetVerts();

			shapeComputeShader.SetFloat("numVertices", allVerts.Length);

			computeVBuffer.SetData(allVerts);

			shapeComputeShader.SetBuffer(0, "vertices", computeVBuffer);
			shapeComputeShader.SetBuffer(0, "heights", computeHBuffer);

			shapeComputeShader.Dispatch(0, 512, 1, 1);

			computeHBuffer.GetData(allVerts);

			meshFilters[i].sharedMesh.vertices = allVerts;
			meshFilters[i].sharedMesh.RecalculateBounds();
			meshFilters[i].sharedMesh.RecalculateNormals();
			meshFilters[i].sharedMesh.RecalculateTangents();
		}

		computeHBuffer.Release();
		computeNoiceSettingsBuffer.Release();
		computeNLTBuffer.Release();
		computeVBuffer.Release();
	}


	public ComputeShader colorComputeShader;
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

	void ComputeProceduralPlanetColor()
	{ 
		if (colorComputeShader == null || useMaterial || !freeUpdateColor)
		{
			return;
		}

		if (renderTexture == null || (textureSize != renderTexture.width) || freeUpdateColor)
		{
			renderTexture = new RenderTexture(textureSize, textureSize, 24);
			renderTexture.enableRandomWrite = true;
			renderTexture.Create();
		}

		int maxLength = 0;
		for (int i = 0; i < maxFaces; ++i)
		{
			maxLength = Mathf.Max(maxLength, meshFilters[i].sharedMesh.vertices.Length);
		}

		ComputeBuffer computeVBuffer = new ComputeBuffer(maxLength, 12);
		ComputeBuffer computeUVBuffer = new ComputeBuffer(maxLength, 8);

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
			Vector3[] allVerts = meshFilters[i].sharedMesh.vertices;
			Vector2[] alluvs = meshFilters[i].sharedMesh.uv;

			colorComputeShader.SetInt("numVertices", meshFilters[i].sharedMesh.vertices.Length);

			computeVBuffer.SetData(allVerts);
			computeUVBuffer.SetData(alluvs);

			colorComputeShader.SetBuffer(0, "vertices", computeVBuffer);
			colorComputeShader.SetBuffer(0, "uvs", computeUVBuffer);
		
			colorComputeShader.Dispatch(0, 512, 256, 1);
		}
		
		dest = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
		
		dest.filterMode = FilterMode.Bilinear;
		dest.Apply(false);
		Graphics.ConvertTexture(renderTexture, dest);
		
		for (int i = 0; i < maxFaces; ++i)
		{
			Material material = new Material(meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial);
			material.SetTexture("_MainTex", dest);
			material.SetFloat("_Glossiness", 0.2f);
			material.color = new Color(1f, 1f, 1f);
			meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial = material;
		}
		
		computeVBuffer.Release();
		renderTexture.Release();
		computeUVBuffer.Release();
	}
}

