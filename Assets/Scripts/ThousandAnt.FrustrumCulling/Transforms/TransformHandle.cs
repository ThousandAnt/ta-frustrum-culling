using System;
using Unity.Mathematics;
using UnityEngine;

namespace ThousandAnt.Authoring {

    /// <summary>
    /// Transform Handle stores only the position and euler rotation. 
    /// Scale is assumed to be a uniform value.
    /// </summary>
    [Serializable]
    public struct TransformHandle {
        public float3 Position;
        public float3 Rotation;

        public float4x4 AsMatrix() {
            return float4x4.TRS(Position, quaternion.EulerXYZ(Rotation), new float3(1));
        }

        public static implicit operator TransformHandle(Transform transform) {
            return new TransformHandle {
                Position = transform.position,
                Rotation = transform.eulerAngles
            };
        }
    }
}
