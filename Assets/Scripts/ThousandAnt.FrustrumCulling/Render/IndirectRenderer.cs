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
                args[i][1] = (uint)(span.y - span.x);
                args[i][2] = (uint)mesh.GetIndexStart(i);
                args[i][3] = (uint)mesh.GetBaseVertex(i);

                argsBuffers[i].SetData(args[i]);

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

        public void Draw(int2 span, NativeArray<float4x4> matrices) {
            var data = transformBuffer.BeginWrite<float4x4>(span.x, span.y - span.x);

            unsafe {
                UnsafeUtility.MemCpy(data.GetUnsafePtr(), matrices.GetUnsafePtr(), matrices.Length * UnsafeUtility.SizeOf<float4x4>());
            }

            transformBuffer.EndWrite<float4x4>(matrices.Length);

            var length = span.y - span.x;

            for (int i = 0; i < mesh.subMeshCount; i++) {
                args[i][0] = (uint)mesh.GetIndexCount(i);
                args[i][1] = (uint)length;
                args[i][2] = (uint)mesh.GetIndexStart(i);
                args[i][3] = (uint)mesh.GetBaseVertex(i);

                argsBuffers[i].SetData(args[i]);
                // Debug.Log($"{i}: {materials[i]} {mesh} {mesh.GetSubMesh(i).vertexCount}, Args: {args[0]} {args[1]}, {args[2]}, {args[3]}, SUBMESH: {mesh.GetSubMesh(i).indexCount}, {mesh.GetSubMesh(i).indexStart} {mesh.GetSubMesh(i).baseVertex}, Actual Mesh: {mesh.GetIndexCount(i)}");

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

            // Update the argument buffer
            // TODO: Support multiple submeshes
            // uint indexCount = mesh != null ? mesh.GetIndexCount(0) : 0;
            // args[0] = indexCount;   // The first arg is the # of indices
            // args[1] = (uint)count;  // The second arg is the # of elements we want to render
            // argsBuffers.SetData(args);
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
