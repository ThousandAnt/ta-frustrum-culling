using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace ThousandAnt.Render {

    public static class ShaderConstants {
        public static readonly int Matrices = Shader.PropertyToID("_Matrices");
    }

    public class IndirectRenderer {

        ComputeBuffer argsBuffer;
        ComputeBuffer transformBuffer;

        readonly Mesh mesh;
        readonly Material material;

        readonly uint[] args = { 0, 0, 0, 0, 0 };

        public IndirectRenderer(int maxObjects, Material material, Mesh mesh) {
            this.mesh = mesh;
            this.material = material;
        }

        public void Draw(int2 span, NativeArray<float4x4> matrices) {
            throw new System.NotImplementedException();
        }

        ~IndirectRenderer() {
            argsBuffer.Release();
            transformBuffer.Release();
        }

        void InitializeBuffers(int max) {
            Release(false);

            // Create the transform buffer.
            transformBuffer = new ComputeBuffer(max, stride: Marshal.SizeOf<float4x4>());

            // Set the material's ComputeBuffer
            material.SetBuffer(ShaderConstants.Matrices, transformBuffer);

            // Update the argument buffer
            // TODO: Support multiple submeshes
            uint indexCount = mesh != null ? mesh.GetIndexCount(0) : 0;
            args[0] = indexCount;   // The first arg is the # of indices
            args[1] = (uint)max;    // The second arg is the # of elements we want to render
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