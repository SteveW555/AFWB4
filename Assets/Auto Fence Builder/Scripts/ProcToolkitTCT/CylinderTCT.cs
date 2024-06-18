using UnityEngine;

//namespace ProceduralToolkit.Samples.PrimitivesTCT
namespace ProceduralToolkitTCT
{
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class Cylinder : MonoBehaviour
    {
        public float radius = 1f;
        public int segments = 16;
        public float height = 2f;

        private void Start()
        {
            GetComponent<MeshFilter>().mesh = MeshDraftTCT.Cylinder(radius, segments, height).ToMesh();
        }
    }
}