using Assets.Scripts.Extensions;
using UnityEngine;

namespace Utilities
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshFilter))]
	public class SerializeMesh : MonoBehaviour
	{
		[SerializeField] Vector2[] uv;
		[SerializeField] Vector3[] verticies;
		[SerializeField] int[] triangles;
		[SerializeField] bool serialized = false;
		[SerializeField] Material srcMaterial;

		// Use this for initialization

		void Awake()
		{
			if (serialized)
			{
				Rebuild();
			}
		}

		void Start()
		{
		}

		public void Serialize()
		{
			var mesh = GetComponent<MeshFilter>().sharedMesh;

			if (mesh != null)
			{
				uv = mesh.uv;
				verticies = mesh.vertices;
				triangles = mesh.triangles;
			}


			var meshRenderer = GetComponent<MeshRenderer>();
			srcMaterial = meshRenderer.sharedMaterial;

			serialized = true;
		}

		public void Rebuild()
		{
			if (gameObject.GetComponent<MeshFilter>(out MeshFilter ourMesh))
			{
				ourMesh.sharedMesh = RebuildMesh();
			}

			if (gameObject.GetComponent(out MeshRenderer itsRenderer))
			{
				//itsRenderer.material = SpriteExploder.createFragmentMaterial(gameObject.transform.parent.gameObject);
			}
		}

		public Mesh RebuildMesh()
		{
			Mesh mesh = new Mesh();
			mesh.vertices = verticies;
			mesh.triangles = triangles;
			mesh.uv = uv;

			mesh.RecalculateNormals();
			mesh.RecalculateBounds();



			return mesh;
		}
	}

}