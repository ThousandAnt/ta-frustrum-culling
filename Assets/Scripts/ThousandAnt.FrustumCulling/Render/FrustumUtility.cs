using System.Runtime.InteropServices;
using ThousandAnt.FrustumCulling.Collections;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace ThousandAnt.FrustumCulling.Render {

    [BurstCompile]
    public unsafe struct EmbedExtentsJob : IJob {

        public UnsafeReadonlyArray<Plane> Src;
        public float3 Extents;

        [WriteOnly]
        public NativeArray<float4> Dst;

        public void Execute() {
            for (int i = 0; i < Src.Length; i++) {
                // By doing the dot product between the plane's normal and the extents, we can determine whether 
                // each corner of the cube is within the frustum.
                // If the dot product yields a value less than 0, we know that the element is behind the plane.
                Dst[i] = new float4(
                    Src[i].normal, 
                    math.dot(Extents, math.abs(Src[i].normal)) + Src[i].distance);
            }
        }
    }

    // TODO: Check IJobParallelForDefer instead to populate the filtered matrices
    [BurstCompile]
    public unsafe struct PopulateFilteredMatricesJob : IJob {
        
        [WriteOnly]
        public NativeList<float4x4> Dst;

        [ReadOnly]
        public NativeList<int> FilteredIndices;

        [ReadOnly]
        public NativeArray<float4x4> Src;

        public void Execute() {
            for (int i = 0; i < FilteredIndices.Length; i++) {
                var idx = FilteredIndices[i];
                Dst.Add(Src[idx]);
            }
        }
    }

    [BurstCompile]
    public unsafe struct WriteToGpuBufferJob : IJob {

        [WriteOnly]
        public NativeArray<float4x4> Dst;

        [ReadOnly]
        public NativeList<int> FilteredIndices;

        [ReadOnly]
        public NativeArray<float4x4> Src;

        public void Execute() {
            for (int i = 0; i < FilteredIndices.Length; i++) {
                var idx = FilteredIndices[i];
                Dst[i] = Src[idx];
            }
        }
    }

    [BurstCompile]
    public unsafe struct ViewFrustumCullingFilterJob : IJobParallelForFilter {

        [ReadOnly]
        [DeallocateOnJobCompletion]
        public NativeArray<float4> Planes;

        [ReadOnly]
        public NativeArray<float4x4> Matrices;

        public bool Execute(int index) {
            var m = Matrices[index];
            
            for (int i = 0; i < 6; i++) {
                var plane = Planes[i];

                // Take the dot product of our embedded normal with the position and add the distance
                // If the value is less than 0, then the frustum is behind the plane.
                if (math.dot(plane.xyz, m.c3.xyz) + plane.w <= 0f) {
                    return false;
                }
            }

            return true;
        }
    }

    public static unsafe class FrustumUtility {

        public static readonly Plane[] Planes = new Plane[6];

        static GCHandle PlanesHandle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Setup() {
            var go = new GameObject("Frustum Initialization Proxy");
            go.AddComponent<FrustumInitializationProxy>();

            // Mark the go to not be destructable when we switch scenes if any.
            Object.DontDestroyOnLoad(go);
        }

        internal static void Initialize() {
            if (PlanesHandle.IsAllocated) {
                throw new System.InvalidOperationException("Cannot reallocated the PlanesHandle.");
            }

            // Pin the static content so we can get a pointer to it and pass it to jobs
            PlanesHandle = GCHandle.Alloc(Planes, GCHandleType.Pinned);
        }

        internal static void Release() {
            if (PlanesHandle.IsAllocated) {
                PlanesHandle.Free();
            }
        }

        internal static void SetFrustumPlanes(Camera cam) {
            GeometryUtility.CalculateFrustumPlanes(cam, Planes);
        }

        /// <summary>
        /// Main thread only utility. Grabs a pointer to the plane.
        /// </summary>
        public static UnsafeReadonlyArray<Plane> GetPlanesArray() {
            if (!PlanesHandle.IsAllocated) {
                throw new System.InvalidOperationException(
                    "PlanesHandle is not allocated. Make sure FrustumUtils.Initialized() is invoked!");
            }

            Plane* ptr = (Plane*)PlanesHandle.AddrOfPinnedObject().ToPointer();
            return new UnsafeReadonlyArray<Plane>(ptr, Planes.Length);
        }
    }
}
