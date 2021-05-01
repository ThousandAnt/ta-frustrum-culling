using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace ThousandAnt.FrustumCulling.Render {

    public static class ShaderConstants {
        public static readonly int Matrices = Shader.PropertyToID("_Matrices");
    }

    public class IndirectRenderer : IDisposable {

        static readonly MaterialPropertyBlock TempBlock = new MaterialPropertyBlock();

        public float3 Extents => mesh.bounds.size / 2;

        ComputeBuffer[] argsBuffers;
        ComputeBuffer transformBuffer;

        readonly Mesh mesh;
        readonly Material[] materials;

        readonly uint[][] args;

        public IndirectRenderer(int maxObjects, Material[] materials, Mesh mesh) {
            this.mesh = mesh;
            this.materials = materials;

            args = new uint[mesh.subMeshCount][];

            for (int i = 0; i < args.Length; i++) {
                args[i] = new uint[5];
            }

            Initialize(maxObjects);
        }

        public NativeArray<float4x4> BeginDraw(int count) {
            return transformBuffer.BeginWrite<float4x4>(0, count);
        }

        public void EndDraw(int2 span) {
            transformBuffer.EndWrite<float4x4>(span.y - span.x);

            for (int i = 0; i < mesh.subMeshCount; i++) {
                // Set up the arguments
                args[i][0] = (uint)mesh.GetIndexCount(i);
                args[i][1] = (uint)(span.y - span.x);                           // Argument 1 is the total # of elements we want to render (we subtract the span)
                args[i][2] = (uint)mesh.GetIndexStart(i);
                args[i][3] = (uint)mesh.GetBaseVertex(i);

                argsBuffers[i].SetData(args[i]);

                Graphics.DrawMeshInstancedIndirect(
                    mesh,                                                       // The mesh we should draw
                    i,                                                          // The submesh index we need to draw
                    materials[i],                                               // The material that needs to be used to render the elements
                    new Bounds(Vector3.zero, new Vector3(500, 500, 500)),       // The area in which the element can appear in
                    argsBuffers[i],                                             // The indirect arguments which describes how many we are drawing
                    0,                                                          // The layer we should draw to
                    TempBlock,                                                  // Empty material block
                    UnityEngine.Rendering.ShadowCastingMode.On,                 // Draw shadows also
                    true);                                                      // Receive shadows too
            }
        }

        public void Draw(int2 span, NativeArray<float4x4> matrices) {
            transformBuffer.SetData(matrices);

            var length = span.y - span.x;

            for (int i = 0; i < mesh.subMeshCount; i++) {
                args[i][0] = (uint)mesh.GetIndexCount(i);
                args[i][1] = (uint)length;
                args[i][2] = (uint)mesh.GetIndexStart(i);
                args[i][3] = (uint)mesh.GetBaseVertex(i);

                argsBuffers[i].SetData(args[i]);

                // TODO: Support submeshes
                Graphics.DrawMeshInstancedIndirect(
                    mesh,
                    i, 
                    materials[i], 
                    new Bounds(Vector3.zero, new Vector3(500, 500, 500)),
                    argsBuffers[i],
                    0,
                    TempBlock,
                    UnityEngine.Rendering.ShadowCastingMode.On,
                    true);
            }
        }

        public void Dispose() {
            Release(true);
        }

        ~IndirectRenderer() {
            Dispose();
        }

        void Initialize(int count) {
            Release(false);

            argsBuffers = new ComputeBuffer[count];

            for (int i = 0; i < count; i++) {
                // Create the indirect arguments
                argsBuffers[i] = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
            }

            // Create the transform buffer.
            transformBuffer = new ComputeBuffer(count, UnsafeUtility.SizeOf<float4x4>(), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);

            for (int i = 0; i < materials.Length; i++) {
                // Set the material's ComputeBuffer
                materials[i].SetBuffer(ShaderConstants.Matrices, transformBuffer);
            }
        }

        void Release(bool releaseArgs) {
            transformBuffer?.Release();
            transformBuffer = null;
        
            if (releaseArgs) {
                for (int i = 0; i < argsBuffers.Length; i++) {
                    argsBuffers[i]?.Release();
                }
                argsBuffers = null;
            }
        }
    }
}
