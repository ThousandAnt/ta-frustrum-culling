using System.Runtime.InteropServices;
using UnityEngine;

namespace ThousandAnt.FrustumCulling.Render {

    public static unsafe class FrustumUtils {

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
        public static Plane* GetPlanesPointer() {
            if (!PlanesHandle.IsAllocated) {
                throw new System.InvalidOperationException(
                    "PlanesHandle is not allocated. Make sure FrustumUtils.Initialized() is invoked!");
            }

            return (Plane*)PlanesHandle.AddrOfPinnedObject().ToPointer();
        }
    }
}
