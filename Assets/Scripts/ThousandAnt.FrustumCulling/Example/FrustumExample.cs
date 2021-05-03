using Unity.Mathematics;
using UnityEngine;

namespace ThousandAnt.FrustumCulling.Example {

    public class FrustumExample : MonoBehaviour {

        public Camera Camera;
        public Color32 Valid;
        public Color32 Invalid;
        public Color32 FrustumColor;
        
        public Vector3 Position;
        public Material Material;
        public Mesh Mesh;

        MaterialPropertyBlock block;
        Plane[] planes;

        void Start() {
            block = new MaterialPropertyBlock();
            planes = new Plane[6];
        }

        void Update() {
            GeometryUtility.CalculateFrustumPlanes(Camera, planes);

            if (Camera == null || Mesh == null || Material == null) {
                return;
            }

            block.SetColor("_BaseColor", IsInViewFrustum() ? Valid : Invalid);

            Graphics.DrawMesh(
                Mesh, 
                Matrix4x4.TRS(Position, Quaternion.identity, Vector3.one), 
                Material, 
                0, 
                null, 
                0, 
                block);
        }


        bool IsInViewFrustum() {
            float3 extents = Mesh.bounds.size / 2;

            for (int i = 0; i < planes.Length; i++) {
                var plane = planes[i];
                
                var normal = plane.normal;
                var embedded = math.dot(extents, math.abs(plane.normal)) + plane.distance;

                if (math.dot(Position, normal) + embedded <= 0f) {
                    return false;
                }
            }

            return true;
        }
    }
}
