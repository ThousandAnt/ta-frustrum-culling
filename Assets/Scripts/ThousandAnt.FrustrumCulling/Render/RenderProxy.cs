using ThousandAnt.FrustumCulling.Transforms;
using ThousandAnt.FrustumCulling.Transforms.Authoring;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

namespace ThousandAnt.FrustumCulling.Render {

    public class RenderProxy : MonoBehaviour {

#pragma warning disable CS0649
        // TODO: Support multiple render batches
        [SerializeField]
        RenderBatch batch;
#pragma warning restore CS0649

        // We store an array of matrices so we can pass this to the compute buffer
        NativeArray<float4x4> matrices;

        IndirectRenderer indirectRenderer;

        void OnEnable() {
            Assert.IsNotNull(batch, "A RenderBatch must exist...");

            indirectRenderer = new IndirectRenderer(batch.Transforms.Length, batch.Material, batch.Mesh);

            // Generate the matrices
            matrices = batch.Transforms.ToMatrixArray(Allocator.Persistent);
        }

        void OnDisable() {
            indirectRenderer.Dispose();

            if (matrices.IsCreated) {
                matrices.Dispose();
            }
        }

        void Update() {
            indirectRenderer.Draw(new int2(0, matrices.Length), matrices);
        }
    }
}
