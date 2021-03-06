using System;
using Unity.Mathematics;
using UnityEngine;

namespace ThousandAnt.FrustumCulling.Transforms {

    /// <summary>
    /// Transform Handle stores only the position and euler rotation. 
    /// Scale is assumed to be a uniform value.
    /// </summary>
    [Serializable]
    public struct TransformHandle {
        public float3 Position;
        public quaternion Rotation;
        public float3 Scale;

        public float4x4 AsMatrix() {
            return float4x4.TRS(Position, Rotation, Scale);
        }

        public static implicit operator TransformHandle(Transform transform) {
            return new TransformHandle {
                Position = transform.position,
                Rotation = transform.rotation,
                Scale    = transform.lossyScale
            };
        }
    }
}
