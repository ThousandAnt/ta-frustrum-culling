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

        ComputeBuffer argsBuffer;
        ComputeBuffer transformBuffer;

        readonly Mesh mesh;
        readonly Material material;

        readonly uint[] args = { 0, 0, 0, 0, 0 };

        public IndirectRenderer(int maxObjects, Material material, Mesh mesh) {
            this.mesh = mesh;
            this.material = material;

            Initialize(maxObjects);
        }

        public NativeArray<float4x4> BeginDraw(int count) {
            return transformBuffer.BeginWrite<float4x4>(0, count);
        }

        public void EndDraw(int2 span) {
            transformBuffer.EndWrite<float4x4>(span.y - span.x);

            args[1] = (uint)(span.y - span.x);
            argsBuffer.SetData(args);

            Graphics.DrawMeshInstancedIndirect(
            mesh,
            0, 
            material, 
            new Bounds(Vector3.zero, new Vector3(500, 500, 500)),
            argsBuffer,
            0,
            TempBlock,
            UnityEngine.Rendering.ShadowCastingMode.On,
            true); 
        }

        public void Draw(int2 span, NativeArray<float4x4> matrices) {
            var data = transformBuffer.BeginWrite<float4x4>(span.x, span.y - span.x);

            unsafe {
                UnsafeUtility.MemCpy(data.GetUnsafePtr(), matrices.GetUnsafePtr(), matrices.Length * UnsafeUtility.SizeOf<float4x4>());
            }

            transformBuffer.EndWrite<float4x4>(matrices.Length);

            args[1] = (uint)(span.y - span.x);
            argsBuffer.SetData(args);

            Graphics.DrawMeshInstancedIndirect(
            mesh,
            0, 
            material, 
            new Bounds(Vector3.zero, new Vector3(500, 500, 500)),
            argsBuffer,
            0,
            TempBlock,
            UnityEngine.Rendering.ShadowCastingMode.On,
            true); 
        }

        public void Dispose() {
            Release(true);
        }

        ~IndirectRenderer() {
            Dispose();
        }

        void Initialize(int count) {
            Release(false);

            // Create the indirect arguments
            argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

            // Create the transform buffer.
            transformBuffer = new ComputeBuffer(count, UnsafeUtility.SizeOf<float4x4>(), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);

            // Set the material's ComputeBuffer
            material.SetBuffer(ShaderConstants.Matrices, transformBuffer);

            // Update the argument buffer
            // TODO: Support multiple submeshes
            uint indexCount = mesh != null ? mesh.GetIndexCount(0) : 0;
            args[0] = indexCount;   // The first arg is the # of indices
            args[1] = (uint)count;    // The second arg is the # of elements we want to render
            argsBuffer.SetData(args);
        }

        void Release(bool releaseArgs) {
            transformBuffer?.Release();
            transformBuffer = null;
        
            if (releaseArgs) {
                argsBuffer?.Release();
                argsBuffer = null;
            }
        }

    }
}
