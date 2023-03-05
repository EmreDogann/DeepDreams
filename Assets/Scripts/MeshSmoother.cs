using DeepDreams.Utils;
using UnityEngine;

namespace DeepDreams
{
    // From: https://italolelis.com/posts/unity3d-smooth-mesh/
    public class MeshSmoother : MonoBehaviour
    {
        private MeshFilter _meshfilter;
        private Mesh _mesh;
        private Vector3[] _vertices;
        private int[] _triangles;

        private readonly int[] subdivision = { 0, 2, 3, 4, 6, 8, 9, 12, 16, 18, 24 };

        [Header("Subdive Mesh")]
        [Tooltip("Divide meshes in submeshes to generate more triangles")]
        [Range(0, 10)]
        public int subdivisionLevel;

        [Tooltip("Repeat the process this many times")]
        [Range(0, 10)]
        public int timesToSubdivide;

        private void Start()
        {
            _meshfilter = GetComponent<MeshFilter>();
            _mesh = _meshfilter.mesh;
            _vertices = _mesh.vertices;
            _triangles = _mesh.triangles;

            for (int i = 0; i < timesToSubdivide; i++)
            {
                MeshHelper.Subdivide(_mesh, subdivision[subdivisionLevel]);
            }

            _meshfilter.mesh = _mesh;
            _vertices = _mesh.vertices;
        }
    }
}