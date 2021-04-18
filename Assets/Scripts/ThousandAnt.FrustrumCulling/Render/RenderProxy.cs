using ThousandAnt.FrustumCulling.Transforms;
using ThousandAnt.FrustumCulling.Transforms.Authoring;
using Unity.Collections;
using Unity.Jobs;
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

        // -------------------------------------
        // Source data
        // -------------------------------------
        NativeArray<float4x4> matrices;

        // -------------------------------------
        // Filtered data
        // -------------------------------------
        NativeList<int> filteredIndices;
        NativeList<float4x4> filteredMatrices;

        IndirectRenderer indirectRenderer;
        JobHandle handle;

        void OnEnable() {
            Assert.IsNotNull(batch, "A RenderBatch must exist...");

            indirectRenderer = new IndirectRenderer(batch.Transforms.Length, batch.Material, batch.Mesh);

            // Generate the matrices
            matrices = batch.Transforms.ToMatrixArray(Allocator.Persistent);

            // Matrices.Length is the absolute worse case for allocation, so we preallocate to that size
            filteredIndices = new NativeList<int>(matrices.Length, Allocator.Persistent);
            filteredMatrices = new NativeList<float4x4>(matrices.Length, Allocator.Persistent);
        }

        void OnDisable() {
            // Complete all jobs before we dispose
            handle.Complete();

            // Do the actual disposal
            indirectRenderer.Dispose();

            if (matrices.IsCreated) {
                matrices.Dispose();
            }

            if (filteredIndices.IsCreated) {
                filteredIndices.Dispose();
            }

            if (filteredMatrices.IsCreated) {
                filteredMatrices.Dispose();
            }
        }

        void Update() {
            handle.Complete();

            // TODO: Ehh this is bad, I need a flag so I can properly end the write and not begin a new write
            if (filteredIndices.Length > 0) {
                indirectRenderer.EndDraw(new int2(0, filteredIndices.Length));
            }

            var contents = indirectRenderer.BeginDraw(matrices.Length);

            // Draw the content within view
            // indirectRenderer.Draw(new int2(0, filteredMatrices.Length), filteredMatrices.AsArray());

            // Reset the filters so we can reuse these buffers
            filteredIndices.Clear();
            filteredMatrices.Clear();

            var planes = new NativeArray<float4>(6, Allocator.TempJob);

            handle = new EmbedExtentsJob {
                Src     = FrustumUtils.GetPlanesArray(),
                Extents = indirectRenderer.Extents,
                Dst     = planes
            }.Schedule();

            handle = new ViewFrustumCullingFilterJob {
                Planes   = planes,
                Matrices = matrices
            }.ScheduleAppend(filteredIndices, matrices.Length, 32, handle);

            handle = new WriteToGpuBufferJob {
                Dst = contents,
                FilteredIndices = filteredIndices,
                Src = matrices
            }.Schedule(handle);
        }
    }
}
