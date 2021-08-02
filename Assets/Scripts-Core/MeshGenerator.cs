using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Extensions;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
	// Start is called before the first frame update
	void Start()
	{

	}

	private void OnEnable()
	{
		if (gameObject.GetComponent<MeshFilter>(out MeshFilter ourMeshFilter))
		{
			Mesh newMesh = new Mesh();

			Vector3[] pts = new Vector3[] {
				new Vector3(0, 0),
				new Vector3(0, 20),
				new Vector3(20, 20),
				new Vector3(20, 0)
			};


			newMesh.vertices = pts;

			newMesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
			newMesh.uv = new Vector2[]
			{
				new Vector2(0f,0f),
				new Vector2(0f,1f),
				new Vector2(1f,1f),
				new Vector2(1f,0f)
			};

			ourMeshFilter.sharedMesh = newMesh;
		}

	}

	// Update is called once per frame
	void Update()
	{
		if (gameObject.GetComponent<MeshFilter>(out MeshFilter ourMeshFilter))
		{
			ourMeshFilter.mesh.vertices = ourMeshFilter.mesh.vertices.Select(inVector =>
			{
				return new Vector3(inVector.x + Random.Range(-.5f, .5f), inVector.y + Random.Range(-.5f, .5f), inVector.z);
			}).ToArray();
		}
	}
}
